using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Web.Services.Protocols;
using System.Windows.Forms;

using Microarea.Console.Core.EventBuilder;
using Microarea.Console.Core.PlugIns;
using Microarea.Library.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Licence;
using Microarea.TaskBuilderNet.Licence.Licence.Forms;
using Microarea.TaskBuilderNet.UI.WinControls;
using Microarea.TaskBuilderNet.UI.WinControls.Splashes;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;

namespace Microarea.Console
{
	/// <summary>
	/// MicroareaConsole
	/// </summary>
	//=========================================================================
	public partial class MicroareaConsole : System.Windows.Forms.Form
	{
		#region Variables and Properties
		static bool schedulerIsRunning = false;
        WTETaskExecutionEngine taskExecutionEngine = null;

		// Icone di stato
		Icon startUpIcon;
		Icon loginManagerErrorIcon;
		Icon schedulerAgentIcon;
		Icon sqlServiceStopIcon;
		Icon sqlServicePauseIcon;
		Icon sqlServiceRunIcon;
		Icon sqlServiceUndefinedIcon;
		private ApplicationLockToken lockToken;
		// Panel
		StatusBarPanel sqlServicePanel = new StatusBarPanel();

		// Parametri di Stato della console
		private ConsoleStatus consoleStatus = new ConsoleStatus();

		// Array dei PlugIns Caricati dalla console (solo quelli attivati!)
		public ArrayList plugIns = new ArrayList();

		// Classe per la gestione degli eventi dalla console ai plugIns, e viceversa costruiti by reflection
		public EventsBuilder eventsBuilder = new EventsBuilder();

		// Parametri per la linea di comando
		public static string languageParam = String.Empty;
		public static string noResize = String.Empty; //se impostata su linea di comando non fa il resize delle form nella workingarea

		//True se la console viene runnata dal server, false altrimenti; siccome
		//in 3.0 la modalita' di esecuzione, da un punto di vista logico, e' sempre client, questo parametro andra' tolto
		public static bool runningFromServer = false;

		// Classe per la gestione della messaggistica
		private DiagnosticViewer diagnosticViewer = new DiagnosticViewer();

		private PathFinder pathFinder = new PathFinder(NameSolverStrings.AllCompanies, NameSolverStrings.AllUsers);

		private LoginManager loginManager = null;
		private LockManager lockManager = null;
		private ProductActivator activator = null;

		private int maxWidth = 0;
		private int maxHeight = 0;

		private string selectedAssembly = "";
		private bool exitWithoutAskMessage = false;

		//Gestione del Timer
		bool processing = false;
		System.Timers.Timer processingTimer = new System.Timers.Timer();

		private string serverName = String.Empty;
		private string instanceName = String.Empty;
		private SQLServerEdition sqlEdition = SQLServerEdition.Undefined;

		private string authenticationToken = String.Empty;
		private static bool canRunMicroareaConsole = true;

		//--------------------------------------------------------------------------------
		public Diagnostic Diagnostic { get { return consoleStatus.Diagnostic; } }
		#endregion

		#region Events and Delegates
		// Settaggi del formato di visualizzazione della Console (Icons, List, Details)
		//---------------------------------------------------------------------
		public delegate void ChangedView (object sender, View typeOfView);
		public event ChangedView OnChangedView;

		//Aggiorna i menu di contesto nei nodi degli utenti associati alla company
		//---------------------------------------------------------------------
		public delegate void UpdateTreeContextMenu(object sender);
		public event UpdateTreeContextMenu OnUpdateTreeContextMenu;

		//Shutdown della MicroareaConsole -------------------------------------
		public delegate bool ShutDownConsole ();
		public event ShutDownConsole OnShutDownConsole;

		//Inizializzazione PathFinder -----------------------------------------
		public delegate void InitPathFinder (PathFinder aPathFinder);
		public event InitPathFinder OnInitPathFinder;

		//Inizializzazione BrandLoader -----------------------------------------
		public delegate void InitBrandLoader (BrandLoader aBrandLoader);
		public event InitBrandLoader OnInitBrandLoader;

		//Inizializzazione LoginManager -----------------------------------------
		public delegate void InitLoginManager (LoginManager aLoginManager);
		public event InitLoginManager OnInitLoginManager;

		public event System.EventHandler OnSchedulerAgentStarted;
		public event System.EventHandler OnSchedulerAgentStopped;

		//Toolbar -------------------------------------------------------------
		public delegate void NewItem (object sender, System.EventArgs e);
		public event NewItem OnNewItem;
		public delegate void OpenItem (object sender, System.EventArgs e);
		public event OpenItem OnOpenItem;
		public delegate void SaveItem (object sender, System.EventArgs e);
		public event SaveItem OnSaveItem;
		public delegate void DeleteItem (object sender, System.EventArgs e);
		public event DeleteItem OnDeleteItem;
		public delegate void Connect (object sender, System.EventArgs e);
		public event Connect OnConnect;
		public delegate void Disconnect (object sender, System.EventArgs e);
		public event Disconnect OnDisconnect;
		public delegate void RefreshItem (object sender, System.EventArgs e);
		public event RefreshItem OnRefreshItem;
		public delegate void GetPlugInMessages (object sender, Diagnostic diagnosticPlugIns);
		public event GetPlugInMessages OnGetPlugInMessages;
		//eventi per i bottoni usati dal SecurityAdminPlugin -----------------
		public delegate void OtherObjects (object sender, System.EventArgs e);
		public event OtherObjects OnOtherObjects;
		public delegate void ShowIconSecurity (object sender, System.EventArgs e);
		public event ShowIconSecurity OnShowIconSecurity;
		public delegate void ShowFilterSecurity (object sender, System.EventArgs e);
		public event ShowFilterSecurity OnShowFilterSecurity;
		public delegate void SearchSecurityObjects (object sender, System.EventArgs e);
		public event SearchSecurityObjects OnSearchSecurityObjects;
		//eventi per il TaskSchedulerPlugin -----------------
		public delegate void NotifyEndedTask (int companyId, int loginId, out StringCollection messages);
		public event NotifyEndedTask OnNotifyEndedTask;

		public delegate void ShowAllGrants (object sender, System.EventArgs e);
		public event ShowAllGrants OnShowAllGrants;

		#endregion

		#region Static variables
		static string NameSpaceConsoleIcon = "Microarea.Console.Icons.Console.ico";
		static string NameSpaceStartUpIcon = "Microarea.Console.Icons.StartUp.ico";
		static string NameSpaceLoginManagerErrorIcon = "Microarea.Console.Icons.Disconnect.ico";
		static string NameSpaceSchedulerAgentIcon = "Microarea.Console.Icons.Task.ico";
		static string NameSpaceSqlServiceStopIcon = "Microarea.Console.Icons.SqlServiceStop.ico";
		static string NameSpaceSqlServicePauseIcon = "Microarea.Console.Icons.SqlServicePause.ico";
		static string NameSpaceSqlServiceRunIcon = "Microarea.Console.Icons.SqlServiceRun.ico";
		static string NameSpaceSqlServiceUndefinedIcon = "Microarea.Console.Icons.SqlServiceUndefined.ico";
		#endregion

		#region Costruttore - Inizializza lo stato della Console
		/// <summary>
		/// Costruttore
		/// Setta l'internazionalizzazione
		/// </summary>
		//---------------------------------------------------------------------
		public MicroareaConsole ()
		{
			try
            {   
                //Ilaria/Luca: trick per far apparire subito prima della splash una finestra di attesa in caso non avessimo già disponibile il nome della solution master
                SplashManager.TemporarySplashForBrandLoading();


                WaitingWindow ww = new WaitingWindow(String.Format(WinControlsStrings.LoadingConfiguration, InstallationData.InstallationName));
                ww.Show();

                BrandLoader.PreLoadMasterSolutionName();


                //Matteo/Luca: trick per far apparire subito la splash.
                //In questo modo non dobbiamo chiedere la country a LoginManager.
                //Ovviamente se la splash dipende dalla country questa soluzione non è accettabiile.
                Image splashImage = InstallationData.BrandLoader.GetConsoleSplash();
				if (splashImage != null)
                    SplashStarter.Start(splashImage);

				//Inizializzo e carico il pathFinder
				LoadPathFinder();

				lockToken = ApplicationSemaphore.Lock(pathFinder.GetSemaphoreFilePath());

				//Contatto il loginManager ed eventualmente setto lo stato
				if (!ContactLoginManager())
					consoleStatus.Status = StatusType.RemoteServerError;
				//Contatto il lockManager ed eventualmente setto lo stato
				if (!ContactLockManager())
					consoleStatus.Status = StatusType.RemoteServerError;

				TaskBuilderNet.Core.StringLoader.StringLoader.EnableDictionaryCaching();

				InitializeComponent();
				SetLinkToProducerSite();
			}
			catch (Exception ex)
			{
				LogException(ex);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
				canRunMicroareaConsole = false;
			}
		}

		//------------------------------------------------------------------------------
		private void SetLinkToProducerSite()
		{
			// 28/03/11: su richiesta di Fabrizio ho tolto Microarea S.p.A. e lasciato Microarea
			this.Text = string.Format("{0} {1}", NameSolverStrings.Microarea, this.Text);
			
			IBrandInfo brandInfo = InstallationData.BrandLoader.GetMainBrandInfo();
			if (brandInfo == null)
				return;
           
            this.Text = string.Format("{0} {1}", brandInfo.Company, this.Text);

			Image companyLogoImage = InstallationData.BrandLoader.GetCompanyLogo();
			if (companyLogoImage != null)
				this.BrandedLogoImage.Image = companyLogoImage;
 
			toolTip.SetToolTip(BrandedLogoImage, Strings.GoToProducerSite);
		}

		//---------------------------------------------------------------------
		private void BrandedLogoImage_Click(object sender, EventArgs e)
		{
           HelpManager.ConnectToProducerSite();
		}
		#endregion

		#region IsValidConnectionToSystemDb - Testa le info nel ServerConnection.config
		/// <summary>
		/// IsValidConnectionToSystemDb
		/// true se le info nel ServerConnection.config sono valide, false altrimenti
		/// </summary>
		//---------------------------------------------------------------------
		private bool IsValidConnectionToSystemDb()
		{
			bool connectionIsValid = false;

			if (string.IsNullOrEmpty(InstallationData.ServerConnectionInfo.SysDBConnectionString))
				return connectionIsValid;

			try
			{
				using (SqlConnection myConnection = new SqlConnection(InstallationData.ServerConnectionInfo.SysDBConnectionString))
				{
					myConnection.Open();
					connectionIsValid = true;
					//myConnection.ChangeDatabase(DatabaseLayerConsts.MasterDatabase);

					// chiedo l'edizione del database SQL
					sqlEdition = TBCheckDatabase.GetSQLServerEdition(myConnection);
				}
			}
			catch (SqlException)
			{
				connectionIsValid = false;
			}

			if (connectionIsValid)
			{
				// controllo lo stato del servizio del server del database di sistema (solo se non e' Azure)
				if (!consoleStatus.LicenceInformation.IsAzureSQLDatabase)
					CheckDatabaseServiceStatus();

				BuildPanels();
			}

			return connectionIsValid;
		}
		#endregion

		#region Funzioni di Inizializzazione

		#region SetCultureInformation
		/// <summary>
		/// SetCultureInformation
		/// </summary>
		//---------------------------------------------------------------------
		private void SetCultureInformation ()
		{
			//Internazionalizzazione
			try
			{
				//se ho già creato il file e non ho dato alcuna impostazione da riga di comando
				//leggo le impostazioni da serverConnection.config
				DictionaryFunctions.SetCultureInfo
					(
						InstallationData.ServerConnectionInfo.PreferredLanguage,
						InstallationData.ServerConnectionInfo.ApplicationLanguage
					);
				consoleStatus.Language = InstallationData.ServerConnectionInfo.PreferredLanguage;
				//non ho impostato nulla per quanto riguarda la culture
				consoleStatus.LocalCulture = Thread.CurrentThread.CurrentCulture.Name;
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message.ToString());
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotReadingConsoleLanguage);
				//default
				consoleStatus.Language = Thread.CurrentThread.CurrentUICulture.Name;
				consoleStatus.LocalCulture = Thread.CurrentThread.CurrentCulture.Name;
			}
		}
		#endregion

		#region InitTimer - Inizializza il timer con intervallo 500, e attacca l'evento elapsed
		/// <summary>
		/// InitTimer
		/// Inizializza il timer con intervallo 500, e attacca l'evento elapsed
		/// </summary>
		//---------------------------------------------------------------------
		private void InitTimer ()
		{
			//inizializzo il timer per la progressbar
			processingTimer.Enabled = true;
			processingTimer.AutoReset = true;
			processingTimer.Interval = 100;//500;
			processingTimer.Elapsed += new System.Timers.ElapsedEventHandler(processingTimer_Elapsed);
		}
		#endregion

		#region InitProgressBar - Inizializza la ProgressBar
		/// <summary>
		/// InitProgressBar
		/// Inizializza la ProgressBar ma non la mostra
		/// (Visible = false)
		/// </summary>
		//---------------------------------------------------------------------
		private void InitProgressBar ()
		{
			//Aggiungo la progressbar ma setto la visibility a false
			statusBarConsole.Controls.Clear();
			PlugInsProgressBar progressBarConsole = new PlugInsProgressBar();
			progressBarConsole.MinValue = 1;
			progressBarConsole.MaxValue = 100;
			progressBarConsole.StepValue = 1;
			progressBarConsole.CurrentValue = 1;
			progressBarConsole.SetProgressVisibility(false);
			progressBarConsole.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			StatusBarPanel progressPanel = this.statusBarConsole.Panels[0];
			progressBarConsole.Width = progressPanel.Width;
			progressBarConsole.Top = (this.statusBarConsole.Height - progressBarConsole.Height) / 2;

			this.statusBarConsole.Controls.Add(progressBarConsole);
		}
		#endregion

		#region processingTimer_Elapsed - Timer usato dalla progress bar
		/// <summary>
		/// processingTimer_Elapsed
		/// A ogni tick finchè processing = true esegue uno step della ProgressBar
		/// </summary>
		//---------------------------------------------------------------------
		private void processingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (processing)
			{
				if (statusBarConsole.Controls.Count > 0)
				{
					PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
					if (progressBar != null)
						progressBar.IncrementCyclicStep();
				}

				//Setto il cursore in stato di wait
				Cursor.Current = Cursors.WaitCursor;
				Cursor.Show();
			}
		}
		#endregion

		#region SetConsoleIcon -  Setta l'incona della Console caricandola dalle immagini embedded
		/// <summary>
		/// SetConsoleIcon
		/// Setta l'incona della Console caricandola dalle immagini embedded
		/// resource
		/// </summary>
		//---------------------------------------------------------------------
		private void SetConsoleIcon ()
		{
			Icon ico = InstallationData.BrandLoader.GetConsoleApplicationIcon();
			if (ico != null)
				this.Icon = ico;
			else
			{
				Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(NameSpaceConsoleIcon);
				if (iconStream != null)
				{
					System.Drawing.Icon icon = new Icon(iconStream);
					if (icon != null)
						this.Icon = icon;
				}
			}
		}

		/// <summary>
		/// SetIcons
		/// Setta le icone utilizzate per gli Stati della MicroareaConsole e cioè:
		/// - startUpIcon (quando non esiste il ServerConnection.config)
		/// - loginManagerErrorIcon (quando il web server non è disponibile)
		/// - schedulerAgentIcon (quanto è attivo lo scheduler agent)
		/// - sqlServiceStopIcon (quando il servizio sql è stoppato)
		/// - sqlServicePauseIcon (quanto il servizio sql è in pause)
		/// - sqlServiceRunIcon (quando il servizio sql è in running)
		/// </summary>
		//---------------------------------------------------------------------
		private void SetIcons ()
		{
			Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(NameSpaceStartUpIcon);
			if (iconStream != null)
				startUpIcon = new Icon(iconStream);

			iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(NameSpaceLoginManagerErrorIcon);
			if (iconStream != null)
				loginManagerErrorIcon = new Icon(iconStream);

			iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(NameSpaceSchedulerAgentIcon);
			if (iconStream != null)
				schedulerAgentIcon = new Icon(iconStream);

			iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(NameSpaceSqlServiceStopIcon);
			if (iconStream != null)
				sqlServiceStopIcon = new Icon(iconStream);

			iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(NameSpaceSqlServicePauseIcon);
			if (iconStream != null)
				sqlServicePauseIcon = new Icon(iconStream);

			iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(NameSpaceSqlServiceRunIcon);
			if (iconStream != null)
				sqlServiceRunIcon = new Icon(iconStream);

			iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(NameSpaceSqlServiceUndefinedIcon);
			if (iconStream != null)
				sqlServiceUndefinedIcon = new Icon(iconStream);
		}
		#endregion

		#region InitConsoleStatus - Inizializza lo status della console
		/// <summary>
		/// InitConsoleStatus
		/// Inizializza la classe per lo status della console e setta la DefaultView = Details
		/// </summary>
		//---------------------------------------------------------------------
		private void InitConsoleStatus ()
		{
			//pulisco la collezione degli utenti autenticati
			consoleStatus.AutenticatedUsers.Clear();
			consoleStatus.DefaultView = View.Details;
		}
		#endregion

		#region LoadPlugIns - Carica le dll dei plugIns
		/// <summary>
		/// LoadPlugIns
		/// Carica le dll dei plugIns, crea l'array dei plugIns così caricati tramite 
		/// Reflection si legge gli eventi e i relativi delegate ed esegue a runtime il +=.
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadPlugIns ()
		{
			//carico le dll dei plugsIns
			LoadDlls();
			Type consoleType = GetType().UnderlyingSystemType;
			//aggiungo alla collection anche la console
			plugIns.Add(this);
			//aggiungo gli eventuali metodi della console
			//che devono intercettare eventi dai PlugIn
			//a)leggo i metodi taggati con il custom attribute che devono
			//essere richiamati al fire dell'evento
			eventsBuilder.AddMethodsConsole(consoleType, plugIns);
			//b)Leggo gli eventi che i plugIns devono comunicare alla console
			//e/o ad altri plugIns
			eventsBuilder.AddEvents(this, consoleType);
			//c)eseguo il += tra i metodi e gli eventi letti precentemente
			eventsBuilder.BuildEvents(plugIns);
			//se eventBuilder ha generato degli errori li eredito
			if (eventsBuilder.Diagnostic.Error || eventsBuilder.Diagnostic.Warning)
				Diagnostic.Set(DiagnosticType.All, eventsBuilder.Diagnostic);
		}
		#endregion

		#region ReloadPlugIns - Ricarica i PlugIns che hanno bisogno di essere autenticati
		/// <summary>
		/// ReloadPlugIns
		/// Ricarico i PlugIns che hanno bisogno di essere autenticati
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnAfterCreateServerConnection")]
		public bool ReloadPlugIns (object sender)
		{
			//Verifico di non dover restartare lo SchedulerAgent
			CheckIfHasToRestartSchedulerAgent();

			if ((consoleStatus.Status & StatusType.StartUp) == StatusType.StartUp)
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
					consoleStatus.Status = StatusType.Administration | StatusType.RemoteServerError;
				else
					consoleStatus.Status = StatusType.Administration;

				if (consoleStatus.PlugInsAlsoToLoad.Count > 0)
				{
					OnSetProgressBarStep(sender, 2);
					OnSetProgressBarText(sender, Strings.Loading);
					OnSetProgressBarValue(sender, 1);
					OnSetProgressBarMaxValue(sender, 1000);
					OnEnableProgressBar(sender);
					LoadActivatedPlugIns();
					Type consoleType = GetType().UnderlyingSystemType;
					eventsBuilder.AddMethodsConsole(consoleType, plugIns);
					//eseguo il += tra i metodi e gli eventi letti precentemente
					eventsBuilder.BuildEvents(plugIns);
					//manda l'evento di inizializzazione di PathFinder ai
					//plugIn interessati ad usare il pathFinder
					if (ReInitPathFinder())
					{
						if (OnInitBrandLoader != null)
							OnInitBrandLoader(InstallationData.BrandLoader);
						if (OnInitPathFinder != null)
							OnInitPathFinder(pathFinder);
					}
					OnSetProgressBarText(sender, "");
					OnDisableProgressBar(sender);
					//se eventBuilder ha generato degli errori li eredito
					if (eventsBuilder.Diagnostic.Error || eventsBuilder.Diagnostic.Warning)
						Diagnostic.Set(DiagnosticType.All, eventsBuilder.Diagnostic);
				}
			}

			BuildPanels();
			Cursor.Current = Cursors.Default;
			return true;
		}
		#endregion

		#region InitMicroareaConsole - Inizializzazione della MicroareaConsole
		/// <summary>
		/// InitMicroareaConsole
		/// Inizializzazione della MicroareaConsole
		/// </summary>
		//---------------------------------------------------------------------
		private void InitMicroareaConsole ()
		{
			if (consoleFormPanel.Controls.Count > 0)
				consoleFormPanel.Controls.Clear();
			ConsoleForm consoleForm = new ConsoleForm();
			consoleForm.OnSelectedTreeNode += new ConsoleForm.SelectedTreeNode(OnAfterSelectedTreeNode);
			consoleForm.OnErrors += new ConsoleForm.Errors(OnAfterErrorsOnTree);
			consoleForm.TopLevel = false;
			consoleForm.Dock = DockStyle.Fill;
			consoleForm.TabStop = true;

			if (consoleForm.consoleTree.Nodes.Count > 0)
				consoleForm.consoleTree.Nodes.Clear();
			if (consoleForm.plugInWorkingArea.Controls.Count > 0)
				consoleForm.plugInWorkingArea.Controls.Clear();
			if (consoleForm.plugInBottomWorkingArea.Controls.Count > 0)
				consoleForm.plugInBottomWorkingArea.Controls.Clear();
			if (consoleForm.PlugIns != null && consoleForm.PlugIns.Count > 0)
				consoleForm.PlugIns.Clear();
			//Costruisce il primo nodo del tree
			consoleForm.consoleTree.Nodes.Add(BuildFirstNodeTree());
			consoleForm.consoleTree.ShowRootLines = false;
			//Aggiungo il tutto
			consoleFormPanel.Controls.Add(consoleForm);
			consoleForm.Show();
			this.Show();
		}
		#endregion

		#region InitMicroareaConsolePlugIns -  Inizializzazione della MicroareaConsole
		/// <summary>
		/// InitMicroareaConsolePlugIns
		/// Carico i PlugIns
		/// </summary>
		//---------------------------------------------------------------------
		private void InitMicroareaConsolePlugIns ()
		{
			ConsoleForm consoleForm = (ConsoleForm)consoleFormPanel.Controls[0];

			//Carica i plugIns,e i loro eventi
			LoadPlugIns();
			//La consoleForm che contiene gli oggetti tree e panel 
			//dalla console deve conoscere quali sono i PlugIns caricati,
			//quindi glieli passo
			consoleForm.PlugIns = plugIns;
			//seleziono il primo nodo, e lo espando
			consoleForm.consoleTree.SelectedNode = consoleForm.consoleTree.Nodes[0];
			consoleForm.consoleTree.ExpandAll();
		}
		#endregion

		#endregion

		#region Funzioni per la gestione della ProgressBar

		#region OnEnableProgressBar
		/// <summary>
		/// OnEnableProgressBar
		/// Abilita la PlugInsProgressBar e va partire il timer
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableProgressBar")]
		public void OnEnableProgressBar (object sender)
		{
			if (statusBarConsole.Controls.Count > 0)
			{
				processing = true;
				//mostro la progress bar
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
					progressBar.SetProgressVisibility(true);
				//parte il timer
				processingTimer.Start();
				//Setto il cursore in stato di Wait
				Cursor.Current = Cursors.WaitCursor;
				Cursor.Show();
			}
		}
		#endregion

		#region OnDisableProgressBar
		/// <summary>
		/// OnDisableProgressBar
		/// Disabilita la progressBar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableProgressBar")]
		public void OnDisableProgressBar (object sender)
		{
			if (statusBarConsole.Controls.Count > 0)
			{
				processing = false;
				//nascondo la progress bar
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
					progressBar.SetProgressVisibility(false);
				//blocco il timer
				processingTimer.Stop();
				//Setto il cursore normale
				Cursor.Current = Cursors.Default;
				Cursor.Show();
			}
		}
		#endregion

		#region OnSetProgressBarMaxValue
		/// <summary>
		/// OnSetProgressBarMaxValue
		/// Setta il valore max della progressBar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetProgressBarMaxValue")]
		public void OnSetProgressBarMaxValue (object sender, int max)
		{
			if (statusBarConsole.Controls.Count > 0)
			{
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
					progressBar.SetMaxValue(max);
			}
		}
		#endregion

		#region OnGetProgressBarMaxValue
		/// <summary>
		/// OnGetProgressBarMaxValue
		/// Restituisce il MaxValue della ProgressBar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnGetProgressBarMaxValue")]
		public int OnGetProgressBarMaxValue (object sender)
		{
			int maxValue = -1;
			if (statusBarConsole.Controls.Count > 0)
			{
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
					maxValue = progressBar.MaxValue;
			}
			return maxValue;
		}
		#endregion

		#region OnSetProgressBarMinvalue
		/// <summary>
		/// OnSetProgressBarMinValue
		/// Setta il valore min della progressBar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetProgressBarMinValue")]
		public void OnSetProgressBarMinValue (object sender, int min)
		{
			if (statusBarConsole.Controls.Count > 0)
			{
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
					progressBar.SetMinValue(min);
			}
		}
		#endregion

		#region OnGetProgressBarMinValue
		/// <summary>
		/// OnGetProgressBarMinValue
		/// Restituisce il MinValue della progressbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnGetProgressBarMinValue")]
		public int OnGetProgressBarMinValue (object sender)
		{
			int minValue = -1;
			if (statusBarConsole.Controls.Count > 0)
			{
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
					minValue = progressBar.MinValue;
			}
			return minValue;
		}
		#endregion

		#region OnSetProgressBarStep
		/// <summary>
		/// OnSetProgressBarStep
		/// Setta lo step della progressBar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetProgressBarStep")]
		public void OnSetProgressBarStep (object sender, int stepValue)
		{
			if (statusBarConsole.Controls.Count > 0)
			{
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
					progressBar.SetStep(stepValue);
			}
		}
		#endregion

		#region OnGetProgressBarStep
		/// <summary>
		/// OnGetProgressBarStep
		/// Restituisce lo Step impostato nella ProgressBar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnGetProgressBarStep")]
		public int OnGetProgressBarStep (object sender)
		{
			int stepValue = -1;
			if (statusBarConsole.Controls.Count > 0)
			{
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
					stepValue = progressBar.StepValue;
			}
			return stepValue;
		}
		#endregion

		#region OnSetProgressBarValue
		/// <summary>
		/// OnSetProgressBarValue
		/// Setta il Value della progressBar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetProgressBarValue")]
		public void OnSetProgressBarValue (object sender, int currentValue)
		{
			if (statusBarConsole.Controls.Count > 0)
			{
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
				{
					if (currentValue <= progressBar.MaxValue)
						progressBar.SetValue(currentValue);
					else
						progressBar.SetValue(progressBar.MaxValue);
				}
			}
		}
		#endregion

		#region OnGetProgressBarValue
		/// <summary>
		/// OnGetProgressBarValue
		/// Restituisce il Value della ProgressBar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnGetProgressBarValue")]
		public int OnGetProgressBarValue (object sender)
		{
			int currentValue = -1;
			if (statusBarConsole.Controls.Count > 0)
			{
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
					currentValue = progressBar.CurrentValue;
			}
			return currentValue;
		}
		#endregion

		#region OnSetProgressBarText
		/// <summary>
		/// OnSetProgressBarText
		/// Setta il messaggio da visualizzare accanto alla progressBar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetProgressBarText")]
		public void OnSetProgressBarText (object sender, string message)
		{
			if (statusBarConsole.Controls.Count > 0)
			{
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
					progressBar.SetText(message);
			}
		}
		#endregion

		#region OnSetIncrementCyclicStep
		/// <summary>
		/// OnSetIncrementCyclicStep
		/// Setta l'incremento ciclico della progressbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetProgressCyclicStep")]
		public void OnSetProgressCyclicStep ()
		{
			if (statusBarConsole.Controls.Count > 0)
			{
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
					progressBar.IncrementCyclicStep();
			}
		}
		#endregion

		#region OnPerformStepProgressBar
		/// <summary>
		/// OnPerformStepProgressBar
		/// aggiunge uno step alla progress bar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnPerformStepProgressBar")]
		public void OnPerformStepProgressBar()
		{
			if (statusBarConsole.Controls.Count > 0)
			{
				PlugInsProgressBar progressBar = (PlugInsProgressBar)statusBarConsole.Controls[0] as PlugInsProgressBar;
				if (progressBar != null)
					progressBar.PerformStepProgressBar();
			}
		}
		#endregion

		#endregion

		#region Funzioni di Eventi generici

		#region OnGetAuthenticationToken
		/// <summary>
		/// OnGetAuthenticationToken
		/// Evento per tutti i plugIn che abbiano la necessità di avere a disposizione il token
		/// di autenticazione per la sicurezza dei webservices.
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnGetAuthenticationToken")]
		public string GetAuthenticationToken ()
		{
			if (string.IsNullOrWhiteSpace(authenticationToken))
			{
				// se l'authenticationToken è vuoto visualizzo un messaggio
				Diagnostic.Set(DiagnosticType.Error, Strings.AuthenticationTokenNotValid);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}

			return authenticationToken;
		}
		#endregion

		#region OnChangeStatusBar -  Setto il Testo dalla Status Bar
		/// <summary>
		/// OnChangeStatusBar
		/// In risposta a tutti i PlugIn che effettuano il firing dell'evento OnChangeStatusBarHandle
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnChangeStatusBarHandle")]
		public void OnChangeStatusBar (object sender, DynamicEventsArgs e)
		{
			statusBarConsole.Text = (string)e.DataArgument;
		}
		#endregion

		#region OnPlugInLoad e OnPlugInUnLoad
		/// <summary>
		/// OnPlugInLoad
		/// In risposta a tutti i PlugIn che effettuano il firing dell'evento OnPlugInLoadHandle
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnPlugInLoadHandle")]
		public void OnPlugInLoad (object sender, DynamicEventsArgs e)
		{
			//per ora non faccio nulla, poi si vedrà
		}

		/// <summary>
		/// OnPlugInUnLoad
		/// In risposta a tutti i PlugIn che effettuano il firing dell'evento OnPlugInUnLoadHandle
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnPlugInUnLoadHandle")]
		public void OnPlugInUnLoad (object sender, DynamicEventsArgs e)
		{
			ClearAll();
			//per ora non faccio nulla, poi si vedrà
		}
		#endregion

		#region OnBeforeAddForm - prima di fare una Add di una form eventualmente fa il resize
		/// <summary>
		/// OnBeforeAddForm
		/// Funzione da utilizzarsi prima di fare una Add di una form alla console per effettuare, 
		/// se serve, il resize del panel
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnBeforeAddFormHandle")]
		public bool OnBeforeAddForm (object sender, int width, int height)
		{
			//se ho impostato il parametro noResize=true sulla linea di comando non effettuo il resize
			if (noResize.Length > 0 && String.Compare(noResize, bool.TrueString, true, CultureInfo.InvariantCulture) == 0)
				return true;

			maxWidth = Screen.PrimaryScreen.WorkingArea.Width - 10;
			maxHeight = Screen.PrimaryScreen.WorkingArea.Height - 10;

			if (consoleFormPanel.Controls.Count > 0 && consoleFormPanel.Controls[0] != null)
			{
				ConsoleForm consoleForm = (ConsoleForm)consoleFormPanel.Controls[0];
				//mi prendo le size
				Size consoleWorkingArea = consoleForm.plugInWorkingArea.ClientSize;

				//non devo fare niente
				if (width <= consoleWorkingArea.Width && height <= consoleWorkingArea.Height)
					return true;
				else
				{
					if (width > consoleWorkingArea.Width)
						this.Width = ((this.Width + width - consoleWorkingArea.Width) > maxWidth) ? maxWidth : this.Width + width - consoleWorkingArea.Width;
					if (height > consoleWorkingArea.Height)
						this.Height = ((this.Height + height - consoleWorkingArea.Height) > maxHeight) ? maxHeight : this.Height + height - consoleWorkingArea.Height;
				}
			}

			return true;
		}
		#endregion

		#region OnLoadedPlugIn - Se il plugIn specificato è caricato ritorna true, false altrimenti
		/// <summary>
		/// OnLoadedPlugIn
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnLoadedPlugInHandle")]
		public bool OnLoadedPlugIn (string nameOfPlugIn)
		{
			return (FindPlugIn(nameOfPlugIn) != null) ? true : false;
		}

		/// <summary>
		/// FindPlugIn
		/// Trova il plugIn nella collection di plugIn caricata in console
		/// </summary>
		/// <param name="assemblyType"></param>
		/// <returns>ritorna il plugIn della collection (in forma di object) o, in alternativa, null</returns>
		//---------------------------------------------------------------------
		private object FindPlugIn(string assemblyType)
		{
			for (int i = 0; i < plugIns.Count; i++)
			{
				if (String.Compare(plugIns[i].GetType().FullName, assemblyType, true, CultureInfo.InvariantCulture) == 0)
					return plugIns[i];
			}
			return null;
		}
		#endregion

		# region ConnectionStringChanged
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnConnectionStringChanged")]
		public void ConnectionStringChanged (object sender, DynamicEventsArgs e)
		{
			// after having used a new connection string, activation2 stuff have to be persisted
			// by reinitializing loginmanager
			try
			{
				//se il web server è in errore, non posso effettuare la ReInit del Loginmanager
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
					return;

				//si fa se la consoleStatus.Status != StatusType.RemoteServerError
				Cursor.Current = Cursors.WaitCursor;
				Cursor.Show();

				// inizializzo l'authenticationToken con il valore corrente della stringa di connessione
				// ad db di sistema. Alla Init del LoginManager passo invece la old string, xchè lui è sempre un colpo indietro
				authenticationToken = InstallationData.ServerConnectionInfo.SysDBConnectionString;
				int loginReturnCode = loginManager.Init(false, (string)e.DataArgument);

				//Non faccio la re-init anche di LockManager, perchè vi è un automatismo
				//per cui, se rinizializzo il login, si re-inizializza anche il lock Manager
				if (loginReturnCode != (int)LoginReturnCodes.NoError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, LoginManagerWrapperStrings.GetString(loginReturnCode));
					extendedInfo.Add(Strings.Source, String.Empty);
					extendedInfo.Add(Strings.DefinedInto, "LoginManager");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (ConnectionStringChanged)", ConstStrings.ApplicationName));
					extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnModifiedServerConnectionConfig)");
					Diagnostic.Set(DiagnosticType.Error, Strings.LoginManagerInit, extendedInfo);
					Debug.Fail(LoginManagerWrapperStrings.GetString(loginReturnCode));
					Debug.WriteLine(LoginManagerWrapperStrings.GetString(loginReturnCode));
				}

				// sends notification to interested plug-ins
				if (OnInitLoginManager != null)
					OnInitLoginManager(loginManager);
			}
			catch (WebException webExc)
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					if (webExc.Response != null)
					{
						HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
						Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
						webResponse.Close();
					}
					else
						Debug.Fail(webExc.Status.ToString());
				}

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (ConnectionStringChanged)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnModifiedServerConnectionConfig)");
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.LoginManagerInit, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (SoapException soapExc)
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
					Debug.Fail(soapExc.Message);

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (ConnectionStringChanged)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnModifiedServerConnectionConfig)");
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.LoginManagerInit, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (ConnectionStringChanged)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnModifiedServerConnectionConfig)");
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.LoginManagerInit, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			finally
			{
				Cursor.Current = Cursors.Default;
				Cursor.Show();
			}
		}
		# endregion

		# region Eventi dal SysAdmin per la MConsole
		///<summary>
		/// Evento inviato dal SysAdmin dopo che e' stato effettuato il log on / log off
		///</summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnEnableMenuAfterLogOperation")]
		public void EnableMenuAfterLogOperation(bool enable)
		{
			// abilito/disabilito il menu strip "Il mio Mago.net"
			showInfoToolStripMenuItem.Enabled = enable;
		}

		///<summary>
		/// Evento inviato dal SysAdmin dopo che si e' connesso al database di sistema
		///</summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "ConnectedToSysDB")]
		public void ConnectedToSysDB(object sender, System.EventArgs e)
		{
			// inizializzo l'authenticationToken con il valore corrente della stringa di connessione al db di sistema
			authenticationToken = InstallationData.ServerConnectionInfo.SysDBConnectionString;
		}
		#endregion

		#region Funzione per abilitare/disabilitare il TreeView della Console
		/// <summary>
		/// Abilita / Disabilita il treeView della Console 
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetConsoleTreeViewEnabled")]
		public void OnSetConsoleTreeViewEnabled(bool enable)
		{
			ConsoleForm consoleForm = (ConsoleForm)consoleFormPanel.Controls[0];
			if (consoleForm == null)
				return;

			TreeView consoleTree = (TreeView)consoleForm.consoleTree;
			if (consoleTree == null)
				return;

			SetConsoleTreeEnabled(consoleTree, enable);
		}

		//---------------------------------------------------------------------
		private void SetConsoleTreeEnabled(TreeView consoleTree, bool enable)
		{
			// InvokeRequired required compares the thread ID of the calling thread to the 
			// thread ID of the creating thread. If these threads are different, it returns true.
			if (consoleTree.InvokeRequired)
			{
				this.Invoke((Action)delegate { SetConsoleTreeEnabled(consoleTree, enable); });
				return;
			}

			consoleTree.Enabled = enable;
		}
		#endregion

		//---------------------------------------------------------------------
		protected override void WndProc (ref Message m)
		{
			//allows Microarea Localizer sending a message to obtain information about the active form
			// in order to locate the associated dictionary item
			if (Microarea.TaskBuilderNet.Core.StringLoader.LocalizerConnector.WndProc(ref m))
				return;

			base.WndProc(ref m);
		}
		#endregion

		#region Funzioni per lo Start/Stop/Restart (Quando cambia il serverconn.config)  dello SchedulerAgent

		#region OnStartSchedulerAgent
		/// <summary>
		/// OnStartSchedulerAgent
		/// Il TaskSchedulerPlugIn dice alla Console di startare lo scheduler Agent
		/// Se lo start ha successo, la console risponde con un OnSchedulerAgentStarted
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.TaskScheduler.TaskScheduler", "OnStartSchedulerAgent")]
		public void OnStartSchedulerAgent (object sender, string connectionString)
		{
			//se va tutto bene
			if (connectionString.Length > 0)
			{
				try
				{
					taskExecutionEngine = new WTETaskExecutionEngine(connectionString);
					taskExecutionEngine.TaskExecutionEnded += new WTETaskExecutionEngine.TaskExecutionEndedEventHandler(TaskExecutionEngine_TaskExecutionEnded);
					taskExecutionEngine.Start();
					schedulerIsRunning = true;
				}
				catch (ScheduledTaskException excScheduler)
				{
					Debug.Fail(excScheduler.Message);
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, excScheduler.ExtendedMessage);
					extendedInfo.Add(Strings.Source, excScheduler.Source);
					extendedInfo.Add(Strings.CalledBy, "TaskExecutionEngine (Stop)");
					extendedInfo.Add(Strings.CalledBy, "TaskSchedulerPlugIn (OnStopSchedulerAgent)");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnStopSchedulerAgent)", ConstStrings.ApplicationName));
					extendedInfo.Add(Strings.InnerException, excScheduler.InnerException != null ? excScheduler.InnerException.ToString() : string.Empty);
					Diagnostic.Set(DiagnosticType.Information, excScheduler.Message, extendedInfo);
				}
				catch (Exception exc)
				{
					Debug.Fail(exc.Message);
					schedulerIsRunning = false;
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, exc.Message);
					extendedInfo.Add(Strings.Source, exc.Source);
					extendedInfo.Add(Strings.CalledBy, "TaskExecutionEngine (Start)");
					extendedInfo.Add(Strings.CalledBy, "TaskSchedulerPlugIn (OnStartSchedulerAgent)");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnStartSchedulerAgent)", ConstStrings.ApplicationName));
					extendedInfo.Add(Strings.InnerException, exc.InnerException != null ? exc.InnerException.ToString() : string.Empty);
					Diagnostic.Set(DiagnosticType.Information, Strings.CannotStartSchedulerAgent, extendedInfo);
				}

				RefreshConsoleStatus();

				if (schedulerIsRunning)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.ServerName, taskExecutionEngine.DatabaseWorkStation);
					extendedInfo.Add(Strings.DatabaseName, taskExecutionEngine.DatabaseName);
					Diagnostic.Set(DiagnosticType.Information, Strings.SchedulerAgentIsStarted, extendedInfo);
					if (OnSchedulerAgentStarted != null)
						OnSchedulerAgentStarted(sender, null);
				}
			}
		}
		#endregion

		#region OnStopSchedulerAgent
		/// <summary>
		/// OnStopSchedulerAgent
		/// Il TaskSchedulerPlugIn dice alla Console di stoppare lo scheduler Agent
		/// Se lo stop ha successo, la console risponde con un OnSchedulerAgentStopped
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.TaskScheduler.TaskScheduler", "OnStopSchedulerAgent")]
		public void OnStopSchedulerAgent (object sender, System.EventArgs e)
		{
			if (taskExecutionEngine != null && taskExecutionEngine.Started)
			{
				try
				{
					taskExecutionEngine.Stop();
					schedulerIsRunning = false;
				}
				catch (ScheduledTaskException excScheduler)
				{
					Debug.Fail(excScheduler.Message);
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, excScheduler.ExtendedMessage);
					extendedInfo.Add(Strings.Source, excScheduler.Source);
					extendedInfo.Add(Strings.CalledBy, "TaskExecutionEngine (Stop)");
					extendedInfo.Add(Strings.CalledBy, "TaskSchedulerPlugIn (OnStopSchedulerAgent)");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnStopSchedulerAgent)", ConstStrings.ApplicationName));
					extendedInfo.Add(Strings.InnerException, excScheduler.InnerException != null ? excScheduler.InnerException.ToString() : string.Empty);
					Diagnostic.Set(DiagnosticType.Information, excScheduler.Message, extendedInfo);
				}
				catch (Exception exc)
				{
					Debug.Fail(exc.Message);
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, exc.Message);
					extendedInfo.Add(Strings.Source, exc.Source);
					extendedInfo.Add(Strings.CalledBy, "TaskExecutionEngine (Stop)");
					extendedInfo.Add(Strings.CalledBy, "TaskSchedulerPlugIn (OnStopSchedulerAgent)");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnStopSchedulerAgent)", ConstStrings.ApplicationName));
					extendedInfo.Add(Strings.InnerException, exc.InnerException != null ? exc.InnerException.ToString() : string.Empty);
					Diagnostic.Set(DiagnosticType.Information, Strings.CannotStopSchedulerAgent, extendedInfo);
				}

				RefreshConsoleStatus();
				if (!schedulerIsRunning)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.ServerName, taskExecutionEngine.DatabaseWorkStation);
					extendedInfo.Add(Strings.DatabaseName, taskExecutionEngine.DatabaseName);
					Diagnostic.Set(DiagnosticType.Information, Strings.SchedulerAgentIsStopped, extendedInfo);
					if (OnSchedulerAgentStopped != null)
						OnSchedulerAgentStopped(sender, null);
				}
			}
		}
		#endregion

		#region notifiche TaskExecutionEnded
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.TaskScheduler.TaskScheduler", "TaskExecutionEnded")]
		public void TaskExecutionEnded (object sender, WTETaskExecutionEngine.TaskExecutionEndedEventArgs e)
		{
			if (e == null || e.Task == null)
				return;

			OnTaskExecutionEnded(e.Task);
		}

		//---------------------------------------------------------------------
		private void TaskExecutionEngine_TaskExecutionEnded (object sender, WTETaskExecutionEngine.TaskExecutionEndedEventArgs e)
		{
			if (sender == null || sender != taskExecutionEngine || e == null || e.Task == null)
				return;

			OnTaskExecutionEnded(e.Task);
		}

		/// <summary>
		/// sparo un evento all'ApplicationDBAdmin per effettuare il controllo 
		/// 
		/// </summary>
		//---------------------------------------------------------------------
		private void OnTaskExecutionEnded (WTEScheduledTaskObj aTask)
		{
			// se non è un task di tipo Restore database non procedo
			if (!aTask.RestoreCompanyDB)
				return;

			StringCollection messages = new StringCollection();
			// sparo un evento all'ApplicationDBAdmin in modo da effettuare successivi controlli sul db appena ripristinato
			// 1. check esistenza TB_DBMark
			// 2. check tabelle in italiano o database in formato unicode
			if (OnNotifyEndedTask != null)
				OnNotifyEndedTask(aTask.CompanyId, aTask.LoginId, out messages);
		}
		#endregion

		#region CheckIfHasToRestartSchedulerAgent - Quando cambia il ServerConnection.config e il servizio è in stato di Running
		/// <summary>
		/// CheckIfHasToRestartSchedulerAgent
		/// Se la connessione cambia e il servizio è attivo bisogna stopparlo
		/// </summary>
		//---------------------------------------------------------------------
		public void CheckIfHasToRestartSchedulerAgent ()
		{
			if (taskExecutionEngine != null && taskExecutionEngine.Started)
			{
				string serverName = taskExecutionEngine.DatabaseWorkStation;
				string dbName = taskExecutionEngine.DatabaseName;
				string currentConn = String.Concat(InstallationData.ServerConnectionInfo.SysDBConnectionString);

				if (currentConn.IndexOf("Pooling") == -1)
					currentConn = String.Concat(InstallationData.ServerConnectionInfo.SysDBConnectionString, ";Pooling=false;");

				if ((currentConn.IndexOf(dbName) == -1) || (currentConn.IndexOf(serverName) == -1))
				{
					OnStopSchedulerAgent(this, new EventArgs());
					OnStartSchedulerAgent(this, currentConn);
				}
			}
		}
		#endregion

		#endregion

		#region Funzioni per la connessione al LoginManager

		#region LoadLicenceInfo - Informazioni sulla licenza installata
		/// <summary>
		/// LoadLicenceInfo
		/// Informazioni di licenza della corrente installazione
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadLicenceInfo()
		{
			consoleStatus.LicenceInformation.Edition			= GetEdition();
			consoleStatus.LicenceInformation.DBNetworkType		= GetDBNetworkType();
			consoleStatus.LicenceInformation.IsoState			= GetCountry();
			consoleStatus.LicenceInformation.IsSQL2012Allowed	= Sql2012Allowed();

			consoleStatus.LicenceInformation.IsAzureSQLDatabase = false;

			if (string.IsNullOrWhiteSpace(consoleStatus.LicenceInformation.Edition) ||
				consoleStatus.LicenceInformation.Edition == "Undefined" ||
				consoleStatus.LicenceInformation.DBNetworkType == DBNetworkType.Undefined ||
				string.IsNullOrWhiteSpace(consoleStatus.LicenceInformation.IsoState) ||
				consoleStatus.LicenceInformation.IsoState == "Undefined")
			{
				consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, Strings.ErrorFromLoginManager);
				extendedInfo.Add(Strings.Function, "LoadLicenceInfo");

				if (string.IsNullOrWhiteSpace(consoleStatus.LicenceInformation.Edition))
				{
					extendedInfo.Add(Strings.Function, "GetEdition");
					extendedInfo.Add(Strings.CalledBy, "LoginManager");
					extendedInfo.Add(Strings.CalledBy, "LoginManagerWrapper");
					extendedInfo.Add(Strings.CalledBy, ConstStrings.ApplicationName);
					Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateEdition, extendedInfo);
				}
				else if (consoleStatus.LicenceInformation.DBNetworkType == DBNetworkType.Undefined)
				{
					extendedInfo.Add(Strings.Function, "GetDBNetworkType");
					extendedInfo.Add(Strings.CalledBy, "LoginManager");
					extendedInfo.Add(Strings.CalledBy, "LoginManagerWrapper");
					extendedInfo.Add(Strings.CalledBy, ConstStrings.ApplicationName);
					Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateDatabaseType, extendedInfo);
				}
				else if (string.IsNullOrWhiteSpace(consoleStatus.LicenceInformation.IsoState))
				{
					extendedInfo.Add(Strings.Function, "GetCountry");
					extendedInfo.Add(Strings.CalledBy, "LoginManager");
					extendedInfo.Add(Strings.CalledBy, "LoginManagerWrapper");
					extendedInfo.Add(Strings.CalledBy, ConstStrings.ApplicationName);
					Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateIsoState, extendedInfo);
				}
			}
			showInfoToolStripMenuItem.Text = activator.GetProductName() + "...";
			if (pathFinder != null)
				pathFinder.Edition = consoleStatus.LicenceInformation.Edition;
		}
		#endregion

		#region GetCompanyDBIsInFreeState - Chiede a LoginManager se esistono utenti connessi al db di una certa azienda
		/// <summary>
		/// GetCompanyDBIsInFreeState
		/// Ogni plug-in puo' chiedere al LoginManager se esistono utenti connessi al db di una certa azienda
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnGetCompanyDBIsInFreeState")]
		public bool GetCompanyDBIsInFreeState (string companyId)
		{
			try
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
					extendedInfo.Add(Strings.Function, "LoginManager");
					extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetCompanyDBIsInFreeState)", ConstStrings.ApplicationName));
					extendedInfo.Add(Strings.CalledBy, "ApplicationDBAdmin (OnGetCompanyDBIsInFreeState)");
					Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
					//faccio ritornare true in modo che possa continuare (tanto, se il web è stoppato, nessuno è connesso
					//quindi il test può essere considerato superato, però dò dei messaggi nel viewer..
					return true;
				}
				return (loginManager.GetCompanyLoggedUsersNumber(Int32.Parse(companyId)) == 0);
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.CalledBy, "ApplicationDBAdmin (GetCompanyLoggedUsersNumber)");
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			return false;
		}
		#endregion

		#region GetDBNetworkType -  Ritorna il tipo di DBNetwork dell'installazione (Large - Small)
		/// <summary>
		/// GetDBNetworkType
		/// Ritorna il tipo di DBNetwork dell'installazione (Large - Small)
		/// Large: consente di utilizzare tutti i tipi di provider 
		/// ovvero SqlServer2000, MSDE2000, SqlServer2005, SqlServer Express Ed. 2005, Oracle
		/// </summary>
		//---------------------------------------------------------------------
		public DBNetworkType GetDBNetworkType ()
		{
			try
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
					extendedInfo.Add(Strings.Function, "LoginManager");
					extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetDBNetworkType)", ConstStrings.ApplicationName));
					extendedInfo.Add(Strings.CalledBy, "SysAdmin (GetDBNetworkType)");
					Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
					//errore non è possibile continuare x chi necessita di questa informazione
					return DBNetworkType.Undefined;
				}
				else
					return loginManager.GetDBNetworkType();
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "GetDBNetworkType");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetDBNetworkType)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (GetDBNetworkType)");
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateDatabaseType, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return DBNetworkType.Undefined;
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "GetDBNetworkType");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetDBNetworkType)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (GetDBNetworkType)");
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateDatabaseType, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return DBNetworkType.Undefined;
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.Function, "GetDBNetworkType");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetDBNetworkType)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (GetDBNetworkType)");
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateDatabaseType, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return DBNetworkType.Undefined;
			}
		}
		#endregion

		#region GetCountry -  Ritorna l'Iso Stato dell'installazione
		/// <summary>
		/// GetCountry
		/// Ritorna l' Iso Stato dell'installazione
		/// </summary>
		//---------------------------------------------------------------------
		public string GetCountry ()
		{
			try
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
					extendedInfo.Add(Strings.Function, "LoginManager");
					extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetCountry)", ConstStrings.ApplicationName));
					extendedInfo.Add(Strings.CalledBy, "SysAdmin (GetCountry)");
					Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
					//errore non è possibile continuare x chi necessita di questa informazione
					return String.Empty;
				}
				else
					return loginManager.GetCountry();
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "GetCountry");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetCountry)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (GetCountry)");
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateIsoState, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return String.Empty;
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "GetCountry");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetCountry)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (GetCountry)");
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateIsoState, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return String.Empty;
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.Function, "GetCountry");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetCountry)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (GetCountry)");
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateIsoState, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return String.Empty;
			}
		}
		#endregion

		#region GetEdition - ritorna l'edition del database di sistema
		/// <summary>
		/// GetEdition
		/// Ritorna l'edition dell'installazione Standard/Professional
		/// </summary>
		//---------------------------------------------------------------------
		public string GetEdition ()
		{
			try
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
					extendedInfo.Add(Strings.Function, "LoginManager");
					extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetEdition)", ConstStrings.ApplicationName));
					extendedInfo.Add(Strings.CalledBy, "SysAdmin (GetEdition)");
					Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
					//errore non è possibile continuare x chi necessita di questa informazione
					return String.Empty;
				}
				else
					return loginManager.GetEdition();
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "GetEdition");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetEdition)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (GetEdition)");
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateEdition, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return String.Empty;
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "GetEdition");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetEdition)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (GetEdition)");
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateEdition, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return String.Empty;
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.Function, "GetEdition");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetEdition)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (GetEdition)");
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateEdition, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return String.Empty;
			}
		}
		#endregion

		#region GetLoggedUsers - Il LoginManager restituisce gli utenti in quel momento connessi
		/// <summary>
		/// GetLoggedUsers
		/// La Console chiede al LoginManager se ci sono utenti connessi in quel momento all'applicazione
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnGetLoggedUsers")]
		public bool GetLoggedUsers (object sender)
		{
			try
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
					extendedInfo.Add(Strings.Function, "LoginManager");
					extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetLoggedUsers)", ConstStrings.ApplicationName));
					extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnGetLoggedUsers)");
					Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
					//faccio ritornare true in modo che possa continuare: se il web è stoppato, nessuno è connesso
					//quindi il test può essere considerato superato, però dò dei messaggi nel viewer..
					return false;
				}
				else
					return loginManager.GetLoggedUsersNumber() > 0;
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "GetLoggedUsersNumber");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetLoggedUsers)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnGetLoggedUsers)");
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateNumberOfLoggedUsers, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "GetLoggedUsersNumber");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetLoggedUsers)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnGetLoggedUsers)");
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateNumberOfLoggedUsers, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.Function, "GetLoggedUsersNumber");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (GetLoggedUsers)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnGetLoggedUsers)");
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeterminateNumberOfLoggedUsers, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			return false;
		}
		#endregion

		#region DeleteAssociation - Ogni volta che cancello una associazione utente/Azienda devo avvisare il LoginManager
		/// <summary>
		/// DeleteAssociation
		/// Ogni volta che cancello una associazione utente/Azienda devo avvisare il LoginManager
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnDeleteAssociationToLoginManager")]
		public bool OnDeleteAssociation (object sender, int loginId, int companyId)
		{
			if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
				extendedInfo.Add(Strings.Function, "LoginManager");
				extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnDeleteAssociation)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnDeleteAssociationToLoginManager)");
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				//faccio ritornare true in modo che possa continuare: se il web è stoppato, nessuno è connesso
				//quindi il test può essere considerato superato, però dò dei messaggi nel viewer..
				return true;
			}

			try
			{
				string token = GetAuthenticationToken();
				if (token.Length == 0)
					throw new Exception(Strings.AuthenticationTokenNotValid);
				return loginManager.DeleteAssociation(loginId, companyId, token);
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "DeleteAssociation");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnDeleteAssociation)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnDeleteAssociationToLoginManager)");
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeleteAssociation, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "DeleteAssociation");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnDeleteAssociation)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnDeleteAssociationToLoginManager)");
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeleteAssociation, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.Function, "DeleteAssociation");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnDeleteAssociation)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnDeleteAssociationToLoginManager)");
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeleteAssociation, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (System.Exception exc)
			{
				Diagnostic.Set(DiagnosticType.Error, exc.Message);
			}

			return false;
		}
		#endregion

		#region DeleteCompany - Ogni volta che cancello l'anagrafica di una azienda, devo avvisare il loginManager
		/// <summary>
		/// OnDeleteCompany
		/// Ogni volta che cancello l'anagrafica di una azienda, devo avvisare il loginManager
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnDeleteCompanyToLoginManager")]
		public bool OnDeleteCompany (object sender, int companyId)
		{
			if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
				extendedInfo.Add(Strings.Function, "LoginManager");
				extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnDeleteCompany)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnDeleteCompanyToLoginManager)");
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				//faccio ritornare true in modo che possa continuare: se il web è stoppato, nessuno è connesso
				//quindi il test può essere considerato superato, però dò dei messaggi nel viewer..
				return true;
			}

			try
			{
				string token = GetAuthenticationToken();
				if (token.Length == 0)
					throw new Exception(Strings.AuthenticationTokenNotValid);
				return loginManager.DeleteCompany(companyId, token);
			}
			catch (System.Net.WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "DeleteCompany");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnDeleteCompany)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnDeleteCompanyToLoginManager)");
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeleteCompany, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "DeleteCompany");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnDeleteCompany)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnDeleteCompanyToLoginManager)");
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeleteCompany, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.Function, "DeleteCompany");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (DeleteCompany)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnDeleteCompanyToLoginManager)");
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeleteCompany, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (System.Exception exc)
			{
				Diagnostic.Set(DiagnosticType.Error, exc.Message);
			}

			return false;
		}
		#endregion

		#region OnModifyCulture
		/// <summary>
		/// OnModifyCulture
		/// Ogni volta che modifico la culture dalla console, devo avvisare il loginManager
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnModifyCulture")]
		public void OnModifyCulture (object sender, string cultureUI, string culture)
		{
			if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
				extendedInfo.Add(Strings.Function, "LoginManager");
				extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnModifyCulture)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnModifyCulture)");
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
			}
		}
		#endregion

		#region DeleteUser - ogni volta che cancello un utente applicativo, devo avvisare il loginManager
		/// <summary>
		/// DeleteUser
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnDeleteUserToLoginManager")]
		public bool OnDeleteUser (object sender, int loginId)
		{
			if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
				extendedInfo.Add(Strings.Function, "LoginManager");
				extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnDeleteUser)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnDeleteUserToLoginManager)");
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				//faccio ritornare true in modo che possa continuare: se il web è stoppato, nessuno è connesso
				//quindi il test può essere considerato superato, però dò dei messaggi nel viewer..
				return true;
			}

			try
			{
				string token = GetAuthenticationToken();
				if (token.Length == 0)
					throw new Exception(Strings.AuthenticationTokenNotValid);
				return loginManager.DeleteUser(loginId, token);
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "OnDeleteUser");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnDeleteUser)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnDeleteUserToLoginManager)");
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeleteUser, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "OnDeleteUser");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnDeleteUser)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnDeleteUserToLoginManager)");
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeleteUser, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.Function, "OnDeleteUser");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnDeleteUser)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnDeleteUserToLoginManager)");
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotDeleteUser, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (System.Exception exc)
			{
				Diagnostic.Set(DiagnosticType.Error, exc.Message);
			}

			return false;
		}
		#endregion

		# region IsDemo
		/// <summary>
		/// IsDemo
		/// ritorna true se siamo in stato Demo o DemoWarning (chiede l'info al LoginManager)
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnIsDemo")]
		public bool IsDemo ()
		{
			// se sono in stato di errore non procedo a contattare il LoginManager e, nel dubbio, ritorno true
			if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
				extendedInfo.Add(Strings.Function, "LoginManager");
				extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				// errore non è possibile continuare x chi necessita di questa informazione
				// se LoginManager non risponde nel dubbio ritorno true e applico la restrizione
				return true;
			}

			return loginManager.IsDemo();
		}
		# endregion

		# region IsDistributor
		/// <summary>
		/// IsDistributor
		/// ritorna true se il Serial Number e' di tipo Distributor o DemoWarning (chiede l'info al LoginManager)
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnIsDistributor")]
		public bool IsDistributor ()
		{
			// se sono in stato di errore non procedo a contattare il LoginManager e, nel dubbio, ritorno true
			if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
				extendedInfo.Add(Strings.Function, "LoginManager");
				extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				// errore non è possibile continuare x chi necessita di questa informazione
				// se LoginManager non risponde nel dubbio ritorno true e applico la restrizione
				return true;
			}

			return loginManager.IsDistributor();
		}
		# endregion

		# region IsDevelopment
		/// <summary>
		/// ritorna true se il Serial Number e' di tipo Dvlp (chiede l'info al LoginManager)
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnIsDevelopment")]
		public bool IsDevelopment ()
		{
			// se sono in stato di errore non procedo a contattare il LoginManager e, nel dubbio, ritorno true
			if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
				extendedInfo.Add(Strings.Function, "LoginManager");
				extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				// errore non è possibile continuare x chi necessita di questa informazione
				// se LoginManager non risponde nel dubbio ritorno true e applico la restrizione
				return true;
			}

			return loginManager.IsDeveloperActivation();
		}
		# endregion

		#region IsActivated - True se la functionality dell'application è licenziata, false altrimenti
		/// <summary>
		/// IsActivated
		/// ritorna true se la functionality dell'application è licenziata, false altrimenti
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnIsActivated")]
		public bool IsActivated (string application, string functionality)
		{
			try
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
					extendedInfo.Add(Strings.Function, "LoginManager");
					extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (IsActivated)", ConstStrings.ApplicationName));
					Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
					//errore non è possibile continuare x chi necessita di questa informazione
					return false;
				}
				else
					return loginManager.IsActivated(application, functionality);
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "IsActivated");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (IsActivated)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				string message = String.Format(Strings.CannotDeterminateIfLicensed, functionality, application);
				Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return false;
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "IsActivated");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (IsActivated)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				string message = String.Format(Strings.CannotDeterminateIfLicensed, functionality, application);
				Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return false;
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.Function, "IsActivated");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (IsActivated)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				string message = String.Format(Strings.CannotDeterminateIfLicensed, functionality, application);
				Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return false;
			}
		}
		#endregion

		#region CanMigrate - ritorna true se e' possibile aggiornare un database pre-40
		/// <summary>
		/// CanMigrate
		/// ritorna true se e' possibile aggiornare un database pre-40
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnCanMigrate")]
		public bool CanMigrate()
		{
			try
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
					extendedInfo.Add(Strings.Function, "LoginManager");
					extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (CanMigrate)", ConstStrings.ApplicationName));
					Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
					//errore non è possibile continuare x chi necessita di questa informazione
					return false;
				}
				else
					return loginManager.CanMigrate();
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "CanMigrate");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (CanMigrate)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return false;
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "CanMigrate");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (CanMigrate)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return false;
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.Function, "IsActivated");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (IsActivated)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return false;
			}
		}
		#endregion

		#region HasUserAlreadyChangedPasswordToday - True se l'utente ha cambiato la pwd nella giornata odierna
		/// <summary>
		/// HasUserAlreadyChangedPasswordToday
		/// True se l'utente ha cambiato la pwd nella giornata odierna
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnHasUserAlreadyChangedPasswordToday")]
		public bool HasUserAlreadyChangedPasswordToday (string user)
		{
			try
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
					extendedInfo.Add(Strings.Function, "LoginManager");
					extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (HasUserAlreadyChangedPasswordToday)", ConstStrings.ApplicationName));
					Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
					//errore non è possibile continuare x chi necessita di questa informazione
					return false;
				}
				else
					return loginManager.HasUserAlreadyChangedPasswordToday(user);
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "HasUserAlreadyChangedPasswordToday");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (HasUserAlreadyChangedPasswordToday)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				string message = String.Format(Strings.CannotDeterminateIfUserHasChangedPwdToday, user);
				Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return false;
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "HasUserAlreadyChangedPasswordToday");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (HasUserAlreadyChangedPasswordToday)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				string message = String.Format(Strings.CannotDeterminateIfUserHasChangedPwdToday, user);
				Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return false;
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.Function, "HasUserAlreadyChangedPasswordToday");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (HasUserAlreadyChangedPasswordToday)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				string message = String.Format(Strings.CannotDeterminateIfUserHasChangedPwdToday, user);
				Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
				return false;
			}
		}
		#endregion

		#region OnTraceAction - Tracciatura (su MSD_Trace) di alcune attività di amministrazione utenti /database
		/// <summary>
		/// OnTraceAction
		/// Tracciatura (su MSD_Trace) di alcune attività di amministrazione utenti /database
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnTraceAction")]
		public void TraceAction (string company, string login, TraceActionType type, string processName)
		{
			try
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
					extendedInfo.Add(Strings.Function, "LoginManager");
					extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
					extendedInfo.Add(Strings.CalledBy, String.Format("{0} (TraceAction)", ConstStrings.ApplicationName));
					Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
					//errore nn è possibile continuare x chi necessita di questa informazione
					return;
				}
				else
					loginManager.TraceAction
					(
						company,
						login,
						type,
						processName,
						WindowsIdentity.GetCurrent().Name,
						Dns.GetHostName().ToUpper(CultureInfo.InvariantCulture)
					);
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "TraceAction");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (TraceAction)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				string message = String.Format(Strings.CannotTraceAction, TraceActionName.GetTraceVersionName(type), company, login);
				Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "TraceAction");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (TraceAction)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				string message = String.Format(Strings.CannotTraceAction, TraceActionName.GetTraceVersionName(type), company, login);
				Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (InvalidOperationException invalidExc)
			{
				Debug.Fail(invalidExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, invalidExc.Message);
				extendedInfo.Add(Strings.Source, invalidExc.Source);
				extendedInfo.Add(Strings.StackTrace, invalidExc.StackTrace);
				extendedInfo.Add(Strings.Function, "TraceAction");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (TraceAction)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.InnerException, invalidExc.InnerException != null ? invalidExc.InnerException.ToString() : string.Empty);
				string message = String.Format(Strings.CannotTraceAction, TraceActionName.GetTraceVersionName(type), company, login);
				Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
		}
		#endregion

		# region Sql2012Allowed
		/// <summary>
		/// Sql2012Allowed
		/// ritorna true se i serial number del Server sono validi per SqlServer 2012 (chiede l'info al LoginManager)
		/// </summary>
		//---------------------------------------------------------------------
		private bool Sql2012Allowed()
		{
			try
			{
				// se sono in stato di errore non procedo a contattare il LoginManager e, nel dubbio, ritorno true
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
					extendedInfo.Add(Strings.Function, "Sql2012Allowed");
					extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
					Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
					// errore non è possibile continuare x chi necessita di questa informazione
					// se LoginManager non risponde nel dubbio ritorno false e applico la restrizione
					return false;
				}

				return loginManager.Sql2012Allowed("sbirulino"); //token valido internamente
			}
			catch (Exception e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, e.Message);
				extendedInfo.Add(Strings.Source, e.Source);
				extendedInfo.Add(Strings.StackTrace, e.StackTrace);
				extendedInfo.Add(Strings.Function, "Sql2012Allowed");
				extendedInfo.Add(Strings.DefinedInto, "LoginManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (Sql2012Allowed)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (Sql2012Allowed)");
				extendedInfo.Add(Strings.InnerException, e.InnerException != null ? e.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				return false;
			}
		}
		# endregion

		#endregion

		#region Funzioni per la connessione al LockManager

		#region UnlockAllForUser - Quando cancello un utente applicativo devo avvisare il lockManager
		/// <summary>
		/// OnUnlockAllForUser
		/// Quando cancello un utente applicativo devo avvisare il lockManager
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnUnlockAllForUser")]
		public bool OnUnlockAllForUser (object sender, string userName)
		{
			if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLockManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
				extendedInfo.Add(Strings.Function, "LockManager");
				extendedInfo.Add(Strings.Library, "LockManagerWrapper");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnUnlockAllForUser)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnUnlockAllForUser)");
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLockManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				//faccio ritornare true in modo che possa continuare: se il web è stoppato, nessuno è connesso
				//quindi il test può essere considerato superato, però dò dei messaggi nel viewer..
				return true;
			}

			try
			{
				string token = GetAuthenticationToken();
				if (token.Length == 0)
					throw new Exception(Strings.AuthenticationTokenNotValid);
				return lockManager.UnlockAllForUser(userName, token);
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "UnlockAllForUser");
				extendedInfo.Add(Strings.DefinedInto, "LockManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnUnlockAllForUser)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnUnlockAllForUser)");
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotUnlockAllForUser, userName), extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "UnlockAllForUser");
				extendedInfo.Add(Strings.DefinedInto, "LockManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnUnlockAllForUser)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnUnlockAllForUser)");
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotUnlockAllForUser, userName), extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (System.Exception exc)
			{
				Diagnostic.Set(DiagnosticType.Error, exc.Message);
			}

			return false;
		}
		#endregion

		#region UnlockAllForCompanyDBName - Quando cancello il db di una azienda devo avvisare il lockManager
		/// <summary>
		/// OnUnlockAllForCompanyDBName
		/// Quando cancello il db di una azienda devo avvisare il lockManager
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnUnlockForCompanyDBName")]
		public bool OnUnlockAllForCompanyDBName (object sender, string companyDbName)
		{
			if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLockManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
				extendedInfo.Add(Strings.Function, "LockManager");
				extendedInfo.Add(Strings.Library, "LockManagerWrapper");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnUnlockAllForCompanyDBName)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnUnlockForCompanyDBName)");
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLockManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				//faccio ritornare true in modo che possa continuare: se il web è stoppato, nessuno è connesso
				//quindi il test può essere considerato superato, però dò dei messaggi nel viewer..
				return true;
			}

			try
			{
				string token = GetAuthenticationToken();
				if (token.Length == 0)
					throw new Exception(Strings.AuthenticationTokenNotValid);
				return lockManager.UnlockAllForCompanyDBName(companyDbName, token);
			}
			catch (WebException webExc)
			{
				if (webExc.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)webExc.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(webExc.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, webExc.Message);
				extendedInfo.Add(Strings.WebDescription, webExc.Response);
				extendedInfo.Add(Strings.Source, webExc.Source);
				extendedInfo.Add(Strings.StackTrace, webExc.StackTrace);
				extendedInfo.Add(Strings.Function, "OnUnlockAllForCompanyDBName");
				extendedInfo.Add(Strings.DefinedInto, "LockManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnUnlockAllForCompanyDBName)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnUnlockForCompanyDBName)");
				extendedInfo.Add(Strings.InnerException, webExc.InnerException != null ? webExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotUnlockAllForCompanyDb, companyDbName), extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (SoapException soapExc)
			{
				Debug.Fail(soapExc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, soapExc.Message);
				extendedInfo.Add(Strings.Source, soapExc.Source);
				extendedInfo.Add(Strings.StackTrace, soapExc.StackTrace);
				extendedInfo.Add(Strings.Function, "OnUnlockAllForCompanyDBName");
				extendedInfo.Add(Strings.DefinedInto, "LockManager");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnUnlockAllForCompanyDBName)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.CalledBy, "SysAdmin (OnUnlockForCompanyDBName)");
				extendedInfo.Add(Strings.InnerException, soapExc.InnerException != null ? soapExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotUnlockAllForCompanyDb, companyDbName), extendedInfo);
				if ((consoleStatus.Status & StatusType.RemoteServerError) != StatusType.RemoteServerError)
				{
					consoleStatus.Status = consoleStatus.Status | StatusType.RemoteServerError;
					BuildPanels();
				}
			}
			catch (System.Exception exc)
			{
				Diagnostic.Set(DiagnosticType.Error, exc.Message);
			}

			return false;
		}
		#endregion

		#endregion

		#region Funzioni per la gestione degli utenti autenticati dalla Console
		/// <summary>
		/// IsUserAuthenticated
		/// Richiede alla Console se l'utente specificato è stato già autenticato
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnIsUserAuthenticated")]
		public bool IsUserAuthenticated (string login, string password, string serverName)
		{
			try
			{
				return consoleStatus.IsUserAuthenticated(login, password, serverName);
			}
			catch (Exception err)
			{
				Debug.Fail(err.Message);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotDeterminateIfUserIsAuthenticated, login));
				return false;
			}
		}

		/// <summary>
		/// AddUserAuthenticated
		/// Aggiunge l'utente specificato alla lista degli utenti autenticati della Console
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnAddUserAuthenticated")]
		public void AddUserAuthenticated (string login, string password, string serverName, DBMSType dbType)
		{
			try
			{
				consoleStatus.AddAuthenticatedUser(login, password, serverName, dbType);
			}
			catch (Exception err)
			{
				Debug.Fail(err.Message);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotAddUserToAuthenticatedUsers, login));
			}
		}

		/// <summary>
		/// OnGetUserAuthenticatedPwd
		/// Richiede alla Console la pwd dell'utente già autenticato
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnGetUserAuthenticatedPwd")]
		public string GetUserAuthenticatedPwd (string login, string serverName)
		{
			string password = String.Empty;
			try
			{
				password = consoleStatus.GetAuthenticatedUserPwd(login, serverName);
			}
			catch (Exception err)
			{
				Debug.Fail(err.Message);
				Diagnostic.Set(DiagnosticType.Error, err.Message);
			}
			return password;
		}
		#endregion

		#region Funzioni sui pulsanti della Toolbar

		#region Abilito/disabilito/click bottone di New
		/// <summary>
		/// OnEnableNewToolBarButton
		/// Abilito il bottone New della Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableNewToolBarButton")]
		public void OnEnableNewToolBarButton (object sender, System.EventArgs e)
		{
			this.newToolStripButton.Enabled = true;
			this.newToolStripButton.Visible = true;
		}

		/// <summary>
		/// OnDisableNewToolBarButton
		/// Disabilito il bottone New della Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableNewToolBarButton")]
		public void OnDisableNewToolBarButton (object sender, System.EventArgs e)
		{
			this.newToolStripButton.Enabled = false;
			this.newToolStripButton.Visible = false;
		}

		/// <summary>
		/// OnSetNewToolBarButtonPushed
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetNewToolBarButtonPushed")]
		public void OnSetNewToolBarButtonPushed (object sender, bool isPushed)
		{
			this.newToolStripButton.Checked = isPushed;
		}

		//---------------------------------------------------------------------
		private void newToolStripButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (OnNewItem != null)
					OnNewItem(sender, e);
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, newToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		#region Abilito/disabilito/click bottone di Open
		/// <summary>
		/// OnEnableOpenToolBarButton
		/// Abilito il bottone Open della Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableOpenToolBarButton")]
		public void OnEnableOpenToolBarButton (object sender, System.EventArgs e)
		{
			this.openToolStripButton.Enabled = true;
			this.openToolStripButton.Visible = true;
		}

		/// <summary>
		/// OnDisableOpenToolBarButton
		/// Disabilito il bottone Open della Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableOpenToolBarButton")]
		public void OnDisableOpenToolBarButton (object sender, System.EventArgs e)
		{
			this.openToolStripButton.Enabled = false;
			this.openToolStripButton.Visible = false;
		}

		/// <summary>
		/// OnSetOpenToolBarButtonPushed
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetOpenToolBarButtonPushed")]
		public void OnSetOpenToolBarButtonPushed (object sender, bool isPushed)
		{
			this.openToolStripButton.Checked = isPushed;
		}

		//---------------------------------------------------------------------
		private void openToolStripButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (OnOpenItem != null)
					OnOpenItem(sender, e);
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, openToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		#region Abilito/disabilito/click bottone di Save
		/// <summary>
		/// OnEnableSaveToolBarButton
		/// Abilito il bottone Save della Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableSaveToolBarButton")]
		public void OnEnableSaveToolBarButton (object sender, System.EventArgs e)
		{
			this.saveToolStripButton.Enabled = true;
			this.saveToolStripButton.Visible = true;
		}

		/// <summary>
		/// OnDisableSaveToolBarButton
		/// Disabilito il bottone Save della Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableSaveToolBarButton")]
		public void OnDisableSaveToolBarButton (object sender, System.EventArgs e)
		{
			this.saveToolStripButton.Enabled = false;
			this.saveToolStripButton.Visible = false;
		}

		/// <summary>
		/// OnSetSaveToolBarButtonPushed
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetSaveToolBarButtonPushed")]
		public void OnSetSaveToolBarButtonPushed (object sender, bool isPushed)
		{
			this.saveToolStripButton.Checked = isPushed;
		}

		//---------------------------------------------------------------------
		private void saveToolStripButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (OnSaveItem != null)
					OnSaveItem(sender, e);
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, saveToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		#region Abilito/disabilito/click bottone di Delete
		/// <summary>
		/// OnEnableDeleteToolBarButton
		/// Abilito il bottone Delete della Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableDeleteToolBarButton")]
		public void OnEnableDeleteToolBarButton (object sender, System.EventArgs e)
		{
			this.deleteToolStripButton.Enabled = true;
			this.deleteToolStripButton.Visible = true;
		}

		/// <summary>
		/// OnDisableDeleteToolBarButton
		/// Disabilito il bottone Delete della Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableDeleteToolBarButton")]
		public void OnDisableDeleteToolBarButton (object sender, System.EventArgs e)
		{
			this.deleteToolStripButton.Enabled = false;
			this.deleteToolStripButton.Visible = false;
		}

		/// <summary>
		/// OnSetDeleteToolBarButtonPushed
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetDeleteToolBarButtonPushed")]
		public void OnSetDeleteToolBarButtonPushed (object sender, bool isPushed)
		{
			this.deleteToolStripButton.Checked = isPushed;
		}

		//---------------------------------------------------------------------
		private void deleteToolStripButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (OnDeleteItem != null)
					OnDeleteItem(sender, e);
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, deleteToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		#region Abilita/disabilita/click bottone di altri oggetti
		/// <summary>
		/// OnEnableOtherObjectsToolBarButton
		/// Abilita il bottone di altri oggetti sotto sicurezza
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableOtherObjectsToolBarButton")]
		public void OnEnableOtherObjectsToolBarButton (object sender, System.EventArgs e)
		{
			this.otherObjectsToolStripButton.Enabled = true;
			this.otherObjectsToolStripButton.Visible = true;
		}

		/// <summary>
		/// OnDisableOtherObjectsToolBarButton
		/// Disabilita il bottone di altri oggetti sotto sicurezza
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableOtherObjectsToolBarButton")]
		public void OnDisableOtherObjectsToolBarButton (object sender, System.EventArgs e)
		{
			this.otherObjectsToolStripButton.Enabled = false;
			this.otherObjectsToolStripButton.Visible = false;
		}

		/// <summary>
		/// OnSetOtherObjectsToolBarButtonPushed
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetOtherObjectsToolBarButtonPushed")]
		public void OnSetOtherObjectsToolBarButtonPushed (object sender, bool isPushed)
		{
			this.otherObjectsToolStripButton.Checked = isPushed;
		}

		//---------------------------------------------------------------------
		private void otherObjectsToolStripButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (OnOtherObjects != null)
					OnOtherObjects(sender, e);
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, otherObjectsToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		#region Abilita/disabilita/click bottone di Security
		/// <summary>
		/// OnEnableShowSecurityIconsToolBarButton
		/// Abilita il bottone nella toolbar per il plugIn della Security
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableShowSecurityIconsToolBarButton")]
		public void OnEnableShowSecurityIconsToolBarButton (object sender, System.EventArgs e)
		{
			this.showSecurityIconsToolStripButton.Enabled = true;
			this.showSecurityIconsToolStripButton.Visible = true;
		}

		/// <summary>
		/// OnDisableShowSecurityIconsToolBarButton
		/// Disabilita il bottone nella toolbar per il plugIn di Security
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableShowSecurityIconsToolBarButton")]
		public void OnDisableShowSecurityIconsToolBarButton(object sender, System.EventArgs e)
		{
			this.showSecurityIconsToolStripButton.Enabled = false;
			this.showSecurityIconsToolStripButton.Visible = false;
		}

		/// <summary>
		/// OnSetShowSecurityIconsToolBarButtonPushed
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetShowSecurityIconsToolBarButtonPushed")]
		public void OnSetShowSecurityIconsToolBarButtonPushed (object sender, bool isPushed)
		{
			this.showSecurityIconsToolStripButton.Checked = isPushed;
		}

		//---------------------------------------------------------------------
		private void showSecurityIconsToolStripButton_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (OnShowIconSecurity != null)
					OnShowIconSecurity(sender, e);
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, showSecurityIconsToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		#region Abilita/disabilita/click bottone di Cerca oggetti security
		/// <summary>
		/// OnEnableFindSecurityObjectsToolBarButton
		/// Abilita il bottone nella toolbar per il Cerca oggetti security
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableFindSecurityObjectsToolBarButton")]
		public void OnEnableFindSecurityObjectsToolBarButton(object sender, System.EventArgs e)
		{
			this.SearchSecurityObjectsToolStripButton.Enabled = true;
			this.SearchSecurityObjectsToolStripButton.Visible = true;
		}

		/// <summary>
		/// OnDisableFindSecurityObjectsToolBarButton
		/// Disabilita il bottone nella toolbar per il Cerca oggetti security
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableFindSecurityObjectsToolBarButton")]
		public void OnDisableFindSecurityObjectsToolBarButton (object sender, System.EventArgs e)
		{
			this.SearchSecurityObjectsToolStripButton.Enabled = false;
			this.SearchSecurityObjectsToolStripButton.Visible = false;
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetFindSecurityObjectsToolBarButtonPushed")]
		public void OnSetFindSecurityObjectsToolBarButtonPushed(object sender, bool isPushed)
		{
			this.SearchSecurityObjectsToolStripButton.Checked = isPushed;
		}

		//---------------------------------------------------------------------
		private void SearchSecurityObjectsToolStripButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (OnSearchSecurityObjects != null)
					OnSearchSecurityObjects(sender, e);
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, SearchSecurityObjectsToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		# endregion

		# region Abilita/disabilita/click bottone Mostra tutti i permessi della Security
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnabledShowAllGrantsToolBarButtonPushed")]
		public void OnEnabledShowAllGrantsToolBarButtonPushed (object sender, System.EventArgs e)
		{
			btShowAllGrants.Enabled = true;
			btShowAllGrants.Visible = true;
		}

		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableShowAllGrantsToolBarButtonPushed")]
		public void OnDisableShowAllGrantsToolBarButtonPushed (object sender, System.EventArgs e)
		{
			btShowAllGrants.Enabled = false;
			btShowAllGrants.Visible = false;
		}

		//---------------------------------------------------------------------
		private void btShowAllGrants_Click(object sender, EventArgs e)
		{
			if (OnShowAllGrants != null)
				OnShowAllGrants(sender, e);
		}
		#endregion

		#region Abilito/disabilito/click bottone per i Filtri della Security
		/// <summary>
		/// OnEnableApplySecurityFilterToolBarButton
		/// Abilita il bottone di OSL Filter
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableApplySecurityFilterToolBarButton")]
		public void OnEnableApplySecurityFilterToolBarButton (object sender, System.EventArgs e)
		{
			this.applySecurityFilterToolStripButton.Enabled = true;
			this.applySecurityFilterToolStripButton.Visible = true;
		}
		/// <summary>
		/// OnDisableApplySecurityFilterToolBarButton
		/// Disabilita il bottone di OSL Filter
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableApplySecurityFilterToolBarButton")]
		public void OnDisableApplySecurityFilterToolBarButton (object sender, System.EventArgs e)
		{
			this.applySecurityFilterToolStripButton.Enabled = false;
			this.applySecurityFilterToolStripButton.Visible = false;
		}

		/// <summary>
		/// OnSetApplySecurityFilterToolBarButtonPushed
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetApplySecurityFilterToolBarButtonPushed")]
		public void OnSetApplySecurityFilterToolBarButtonPushed (object sender, bool isPushed)
		{
			this.applySecurityFilterToolStripButton.Checked = isPushed;
		}

		//---------------------------------------------------------------------
		private void applySecurityFilterToolStripButton_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				if (OnShowFilterSecurity != null)
					OnShowFilterSecurity(sender, e);
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, applySecurityFilterToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		#region Abilito/disabilito/click bottone Connect
		/// <summary>
		/// OnEnableConnectToolBarButton
		/// Abilito il bottone Connect della Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableConnectToolBarButton")]
		public void OnEnableConnectToolBarButton (object sender, System.EventArgs e)
		{
			this.connectToolStripButton.Enabled = true;
			this.connectToolStripButton.Visible = true;
			this.disconnectToolStripButton.Enabled = false;
			this.disconnectToolStripButton.Visible = false;
		}

		/// <summary>
		/// OnDisableConnectToolBarButton
		/// Disabilito il bottone Connect della Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableConnectToolBarButton")]
		public void OnDisableConnectToolBarButton (object sender, System.EventArgs e)
		{
			this.connectToolStripButton.Enabled = false;
			this.connectToolStripButton.Visible = false;
			this.disconnectToolStripButton.Enabled = true;
			this.disconnectToolStripButton.Visible = true;
		}

		/// <summary>
		/// OnSetConnectToolBarButtonPushed
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetConnectToolBarButtonPushed")]
		public void OnSetConnectToolBarButtonPushed (object sender, bool isPushed)
		{
			this.connectToolStripButton.Checked = isPushed;
		}

		//---------------------------------------------------------------------
		private void connectToolStripButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (OnConnect != null)
					OnConnect(sender, e);
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, connectToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		#region Abilito/disabilito/click bottone Disconnect
		/// <summary>
		/// OnEnableDisconnectToolBarButton
		/// Abilito il bottone Connect della Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableDisconnectToolBarButton")]
		public void OnEnableDisconnectToolBarButton (object sender, System.EventArgs e)
		{
			this.connectToolStripButton.Enabled = false;
			this.connectToolStripButton.Visible = false;
			this.disconnectToolStripButton.Enabled = true;
			this.disconnectToolStripButton.Visible = true;
		}

		/// <summary>
		/// OnDisableDisconnectToolBarButton
		/// Abilito il bottone Connect della Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableDisconnectToolBarButton")]
		public void OnDisableDisconnectToolBarButton (object sender, System.EventArgs e)
		{
			this.connectToolStripButton.Enabled = false;
			this.connectToolStripButton.Visible = false;
			this.disconnectToolStripButton.Enabled = false;
			this.disconnectToolStripButton.Visible = true;
		}

		/// <summary>
		/// OnSetDisconnectToolBarButtonPushed
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetDisconnectToolBarButtonPushed")]
		public void OnSetDisconnectToolBarButtonPushed (object sender, bool isPushed)
		{
			this.disconnectToolStripButton.Checked = isPushed;
		}

		//---------------------------------------------------------------------
		private void disconnectToolStripButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (OnDisconnect != null)
					OnDisconnect(sender, e);
				ClearAll();
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, disconnectToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		#region Abilito/disabilito/click bottone di Help
		/// <summary>
		/// OnEnableHelpToolBarButton
		/// Abilita il bottone di Help
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableHelpToolBarButton")]
		public void OnEnableHelpToolBarButton (object sender, System.EventArgs e)
		{
			this.helpToolStripButton.Enabled = true;
			this.helpToolStripButton.Visible = true;
		}

		/// <summary>
		/// OnDisableHelpToolBarButton
		/// Disabilita il bottone di Help
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableHelpToolBarButton")]
		public void OnDisableHelpToolBarButton (object sender, System.EventArgs e)
		{
			this.helpToolStripButton.Enabled = false;
			this.helpToolStripButton.Visible = false;
		}

		/// <summary>
		/// OnSetHelpToolBarButtonPushed
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetHelpToolBarButtonPushed")]
		public void OnSetHelpToolBarButtonPushed (object sender, bool isPushed)
		{
			this.helpToolStripButton.Checked = isPushed;
		}

		//---------------------------------------------------------------------
		private void helpToolStripButton_Click(object sender, EventArgs e)
		{
			try
			{
				ShowMenu(sender, e, false);
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, helpToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		#region METODO DA TOGLIERE! Gestione toolbar dell'auditing:@@BAUZI da eliminare quando è possibile modificare i plugins che effettuno il fire dell'evento

		/// <summary>
		/// OnEnableQueryToolBarButton
		/// Abilita il bottone di Query
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableQueryToolBarButton")]
		public void OnEnableQueryToolBarButton (object sender, System.EventArgs e)
		{
		}

		/// <summary>
		/// OnDisableQueryToolBarButton
		/// Disabilita il bottone di Query
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableQueryToolBarButton")]
		public void OnDisableQueryToolBarButton (object sender, System.EventArgs e)
		{
		}

		#endregion

		#region Abilito/disabilito/click bottone di Refresh
		/// <summary>
		/// OnEnableRefreshToolBarButton
		/// Abilita il bottone di refresh
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableRefreshToolBarButton")]
		public void OnEnableRefreshToolBarButton (object sender, System.EventArgs e)
		{
			this.refreshToolStripButton.Enabled = true;
			this.refreshToolStripButton.Visible = true;
		}
		/// <summary>
		/// OnDisableRefreshToolBarButton
		/// Disabilita il bottone di Refresh
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableRefreshToolBarButton")]
		public void OnDisableRefreshToolBarButton (object sender, System.EventArgs e)
		{
			this.refreshToolStripButton.Enabled = false;
			this.refreshToolStripButton.Visible = false;
		}

		/// <summary>
		/// OnSetRefreshToolBarButtonPushed
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnSetRefreshToolBarButtonPushed")]
		public void OnSetRefreshToolBarButtonPushed (object sender, bool isPushed)
		{
			this.refreshToolStripButton.Checked = isPushed;
		}

		//---------------------------------------------------------------------
		private void refreshToolStripButton_Click(object sender, EventArgs e)
		{
			try
			{
				if (OnRefreshItem != null)
				{
					OnRefreshItem(sender, e);
					RefreshConsoleStatus();
				}
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, refreshToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		# region Mostra/nasconde il tree della Console
		//--------------------------------------------------------------------------------
		private void hideTreeToolStripButton_Click(object sender, EventArgs e)
		{
			if (hideTreeToolStripButton.Checked)
			{
				ConsoleTreeHandler(this, new EnabledConsoleElementEventArgs(false));
				hideTreeToolStripButton.CheckState = CheckState.Checked;
			}
			else
			{
				ConsoleTreeHandler(this, new EnabledConsoleElementEventArgs(true));
				hideTreeToolStripButton.CheckState = CheckState.Unchecked;
			}
		}
		# endregion

		# region Mostra le info di licenze e, su click del pulsante apposito, registra il programma
		//--------------------------------------------------------------------------------
        private void activateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClientStub c = null;
            try
            {
                c = new ClientStub(BasePathFinder.BasePathFinderInstance);
            }
            catch (Exception exc)
            {
                Diagnostic.Set(DiagnosticType.Error, Strings.ExceptionOccurred, exc.Message);
                return;
            }
            c.Register();
        }

		//--------------------------------------------------------------------------------
		private void showInfoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				LicensedConfigurationForm lcForm = new LicensedConfigurationForm
					(
					new ClientStub(BasePathFinder.BasePathFinderInstance),
					activator.GetProductName(),
                    true
					);
				lcForm.StartPosition = FormStartPosition.CenterParent;

				// mostro la form con la griglia dei seriali e le info utente
				DialogResult rs = lcForm.ShowDialog(this);

				// se la registrazione e' andata a buon fine chiedo se si vuole restartare la console
				if (rs == DialogResult.OK)
				{
					if (DiagnosticViewer.ShowQuestion(Strings.ReloadAdminConsole, Strings.ReloadAdminConsoleTitle) == DialogResult.No)
						return;

					ProcessStartInfo psi = new ProcessStartInfo();
					psi.FileName = Application.ExecutablePath;

					// se la stringa al ServerConnection.config esiste imposto l'argomento per l'autologin al processo
					if (!string.IsNullOrWhiteSpace(InstallationData.ServerConnectionInfo.SysDBConnectionString))
						psi.Arguments = "/autologin yes";

					// lancio una nuova AdminConsole (senza fare comparire la form di Login)
					Process.Start(psi);

					// killo il processo corrente
					Process.GetCurrentProcess().Kill();
				}
			}
			catch (Exception exc)
			{
				Diagnostic.Set(DiagnosticType.Error, Strings.ExceptionOccurred, exc.Message);
				return;
			}
		}
		# endregion
		#endregion

		#region Funzioni per Abilitare / Disabilitare voci di menù dei PlugIns

		#region Abilito/disabilito la voce di menù del PlugIn specificata da textMenu
		/// <summary>
		/// OnEnablePlugInMenu
		/// Abilita la voce di Menù specificata tra i Menù dei PlugIns
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnablePlugInMenuHandle")]
		public void OnEnablePlugInMenu(string textMenu, System.EventArgs e)
		{
			for (int i = 0; i < mainMenuConsole.Items.Count; i++)
			{
				if (mainMenuConsole.Items[i] == null || !(mainMenuConsole.Items[i] is ToolStripMenuItem) ||
					!((ToolStripMenuItem)mainMenuConsole.Items[i]).HasDropDownItems)
					continue;

				Type menuType = mainMenuConsole.Items[i].GetType();
				for (int j = 0; j < ((ToolStripMenuItem)mainMenuConsole.Items[i]).DropDownItems.Count; j++)
				{
					string currentTextMenu = ((ToolStripMenuItem)mainMenuConsole.Items[i]).DropDownItems[j].Text;
					if (String.Compare(currentTextMenu, textMenu, true, CultureInfo.InvariantCulture) == 0)
					{
						((ToolStripMenuItem)mainMenuConsole.Items[i]).DropDownItems[j].Enabled = true;
						break;
					}
				}
			}
		}

		/// <summary>
		/// OnDisablePlugInMenu
		/// Disabilita la voce di Menù specificata tra i Menù dei PlugIns
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisablePlugInMenuHandle")]
		public void OnDisablePlugInMenu (string textMenu, System.EventArgs e)
		{
			for (int i = 0; i < mainMenuConsole.Items.Count; i++)
			{
				if (mainMenuConsole.Items[i] == null || !(mainMenuConsole.Items[i] is ToolStripMenuItem) || 
					!((ToolStripMenuItem)mainMenuConsole.Items[i]).HasDropDownItems)
					continue;

				Type menuType = mainMenuConsole.Items[i].GetType();
				for (int j = 0; j < ((ToolStripMenuItem)mainMenuConsole.Items[i]).DropDownItems.Count; j++)
				{
					string currentTextMenu = ((ToolStripMenuItem)mainMenuConsole.Items[i]).DropDownItems[j].Text;
					if (String.Compare(currentTextMenu, textMenu, true, CultureInfo.InvariantCulture) == 0)
					{
						((ToolStripMenuItem)mainMenuConsole.Items[i]).DropDownItems[j].Enabled = false;
						break;
					}
				}
			}
		}
		#endregion

		#region Abilito/disabilito un intero gruppo di menù
		/// <summary>
		/// OnEnableGroupPlugInMenu
		/// Abilito un intero gruppo di menù
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnEnableGroupPlugInMenuHandle")]
		public void OnEnableGroupPlugInMenu (string textMenu, System.EventArgs e)
		{
			for (int i = 0; i < mainMenuConsole.Items.Count; i++)
			{
				Type menuType = mainMenuConsole.Items[i].GetType();
				if (String.Compare(mainMenuConsole.Items[i].Text, textMenu, true, CultureInfo.InvariantCulture) == 0)
				{
					mainMenuConsole.Items[i].Enabled = true;
					break;
				}
			}
		}

		/// <summary>
		/// OnDisableGroupPlugInMenu
		/// Disabilito un intero gruppo di menù
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnDisableGroupPlugInMenuHandle")]
		public void OnDisableGroupPlugInMenu (string textMenu, System.EventArgs e)
		{
			for (int i = 0; i < mainMenuConsole.Items.Count; i++)
			{
				Type menuType = mainMenuConsole.Items[i].GetType();
				if (String.Compare(mainMenuConsole.Items[i].Text, textMenu, true, CultureInfo.InvariantCulture) == 0)
				{
					mainMenuConsole.Items[i].Enabled = false;
					break;
				}
			}
		}
		#endregion

		#endregion

		#region BuildFirstNodeTree - Costruisce il primo nodo del tree
		/// <summary>
		/// BuildFirstNodeTree
		/// Costruisce il primo nodo nel Tree
		/// </summary>
		//---------------------------------------------------------------------
		protected PlugInTreeNode BuildFirstNodeTree ()
		{
			PlugInTreeNode rootNode = new PlugInTreeNode(Strings.LblFirstNodeTree);
			rootNode.ImageIndex = PlugInTreeNode.GetDefaultImageIndex;
			rootNode.SelectedImageIndex = PlugInTreeNode.GetDefaultImageIndex;
			rootNode.AssemblyName = Assembly.GetEntryAssembly().GetName().Name;
			rootNode.AssemblyType = GetType();
			return rootNode;
		}
		#endregion

		#region ReInitPathFinder - Reinizializzazione del PathFinder
		/// <summary>
		/// ReInitPathFinder
		/// </summary>
		//---------------------------------------------------------------------
		private bool ReInitPathFinder ()
		{
			//if (!pathFinder.Init())
			//{
			//    ExtendedInfo extendedInfo = new ExtendedInfo();
			//    extendedInfo.Add(Strings.Description, pathFinder.Diagnostic.ToString());
			//    extendedInfo.Add(Strings.Function, "Init");
			//    extendedInfo.Add(Strings.DefinedInto, "PathFinder");
			//    extendedInfo.Add(Strings.Library, "NameSolver");
			//    extendedInfo.Add(Strings.CalledBy, String.Format("{0} (ReInitPathFinder)", ConstStrings.ApplicationName));
			//    Diagnostic.Set(DiagnosticType.Error, Strings.PathFinderInit, extendedInfo);
			//    return false;
			//}

			runningFromServer = pathFinder.RunAtServer;

			return true;
		}
		#endregion

		#region LoadPathFinder - Inizializza pathFinder
		/// <summary>
		/// LoadPathFinder
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadPathFinder ()
		{
			runningFromServer = pathFinder.RunAtServer;

			if (!string.IsNullOrEmpty(pathFinder.ServerConnectionFile))
				SetCultureInformation();
			else
				Diagnostic.Set(DiagnosticType.Information, Strings.CannotFoundedServerConnection);
		}
		#endregion

		#region ContactLoginManager - Contatta il LoginManager
		/// <summary>
		/// ContactLoginManager
		/// </summary>
		//---------------------------------------------------------------------
		private bool ContactLoginManager ()
		{
			try
			{
				loginManager = new LoginManager(pathFinder.LoginManagerUrl, InstallationData.ServerConnectionInfo.WebServicesTimeOut);
				activator = new ProductActivator(loginManager);
			}
			catch (WebException err)
			{
				if (err.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)err.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(err.Status.ToString());

				LogException(err);
				return false;
			}
			catch (Exception ex)
			{
				LogException(ex);
				return false;
			}
			//ora provo a contattare il loginManager - Mi server per capire in che stato lavorerà la console
			//se la chiamata fallisce significa che il WebService non è disponibile (ovvero è stoppato, o 
			//ha qualche problema), pertanto la console entrerà in stato di Errore e caricherà solo quei 
			//PlugIns che non hanno bisogno di essere autenticati
			try
			{
				loginManager.IsAlive();
			}
			catch (WebException err)
			{
				if (err.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)err.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(err.Status.ToString());

				LogException(err);
				return false;
			}
			catch (Exception ex)
			{
				LogException(ex);
				return false;
			}
			//è andato tutto bene
			return true;
		}

		//---------------------------------------------------------------------
		private void LogException (Exception err)
		{
			ExtendedInfo extendedInfo = new ExtendedInfo();
			extendedInfo.Add(Strings.Description, err.Message);
			extendedInfo.Add(Strings.Source, err.Source);
			extendedInfo.Add(Strings.StackTrace, err.StackTrace);
			extendedInfo.Add(Strings.InnerException, err.InnerException != null ? err.InnerException.ToString() : string.Empty);
			Diagnostic.Set(DiagnosticType.Error, err.Message, extendedInfo);

		}
		#endregion

		#region ContactLockManager - Contatto del LockManager
		/// <summary>
		/// ContactLockManager
		/// </summary>
		//---------------------------------------------------------------------
		private bool ContactLockManager ()
		{
			try
			{
				lockManager = new LockManager(pathFinder.LockManagerUrl);
			}
			catch (WebException err)
			{
				if (err.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)err.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(err.Status.ToString());
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, err.Message);
				extendedInfo.Add(Strings.Source, err.Source);
				extendedInfo.Add(Strings.Function, "LockManager Constructor");
				extendedInfo.Add(Strings.Library, "LockManagerWrapper");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (ContactLockManager)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.StackTrace, err.StackTrace);
				extendedInfo.Add(Strings.InnerException, err.InnerException != null ? err.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, Strings.LockManagerInit, extendedInfo);
				return false;
			}
			//ora provo a contattare il loginManager - Mi server per capire in che stato lavorerà la console
			//se la chiamata fallisce significa che il WebService non è disponibile (ovvero è stoppato, o 
			//ha qualche problema), pertanto la console entrerà in stato di Errore e caricherà solo quei 
			//PlugIns che non hanno bisogno di essere autenticati
			try
			{
				lockManager.IsAlive();
			}
			catch (WebException err)
			{
				if (err.Response != null)
				{
					HttpWebResponse webResponse = (HttpWebResponse)err.Response;
					Debug.Fail((webResponse.StatusDescription.Length > 0) ? webResponse.StatusDescription : webResponse.StatusCode.ToString());
					webResponse.Close();
				}
				else
					Debug.Fail(err.Status.ToString());

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLockManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
				extendedInfo.Add(Strings.WebDescription, err.Message);
				extendedInfo.Add(Strings.Source, err.Source);
				extendedInfo.Add(Strings.Function, "LockManager");
				extendedInfo.Add(Strings.Library, "LockManagerWrapper");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (ContactLockManager)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.StackTrace, err.StackTrace);
				extendedInfo.Add(Strings.InnerException, err.InnerException != null ? err.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLockManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				return false;
			}
			catch (SoapException soapEx)
			{
				Debug.Fail(soapEx.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLockManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
				extendedInfo.Add(Strings.WebDescription, soapEx.Message);
				extendedInfo.Add(Strings.Source, soapEx.Source);
				extendedInfo.Add(Strings.Function, "LockManager");
				extendedInfo.Add(Strings.Library, "LockManagerWrapper");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (ContactLockManager)", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.StackTrace, soapEx.StackTrace);
				extendedInfo.Add(Strings.InnerException, soapEx.InnerException != null ? soapEx.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLockManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
				return false;
			}

			//è andato tutto bene
			return true;
		}
		#endregion

		#region LoadDlls - Carico le Dll dei PlugIns
		//---------------------------------------------------------------------
		private string[] GetPlugins ()
		{
			return Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.Plugin.*.dll");
		}

		//---------------------------------------------------------------------
		private string GetPluginNameFromFile (string assemblyFile)
		{
			string file = Path.GetFileNameWithoutExtension(assemblyFile);
			int idx = file.LastIndexOf(".");
			if (idx != -1)
			{
				idx++;
				file = file.Substring(idx, file.Length - idx);
			}

			return file;
		}

		/// <summary>
		/// LoadDlls
		/// carico le dll dei plugIns
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadDlls ()
		{
			if (pathFinder == null)
				return;

			foreach (string assemblyFile in GetPlugins())
			{
				string pluginName = GetPluginNameFromFile(assemblyFile);

				try
				{
					Assembly assembly = Assembly.LoadFrom(assemblyFile);

					//leggo i custom attribute
					//Utilizzato al momento per Security e TaskScheduler
					Object[] activated = assembly.GetCustomAttributes(typeof(Activated), false);

					//Se il plugIn deve richiedere l'attivazione, e l'applicationServer non è running oppure il
					//plugIn non ha la cal, il plugIn in question non viene caricato, e la console passa
					//a quello successivo
					if (activated.Length > 0)
					{
						try
						{
							if (
								((Activated)activated[0]).IsActivatedValue &&
								(consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError
								)
								continue;

							if (
								((Activated)activated[0]).IsActivatedValue &&
								!loginManager.IsActivated(DatabaseLayerConsts.MicroareaConsole, pluginName)
								)
							{
								//se sono in fase di inizializzazione, il test non ha senso,
								//in quanto i PlugIns sottoposti ad autenticazione attualmente
								//non sono disponibili (quindi gli skippo)
								if ((consoleStatus.Status & StatusType.StartUp) != StatusType.StartUp)
								{
									//Plug in non attivato
									string message = String.Format(Strings.PlugInNotActivated, pluginName);
									ExtendedInfo extendedInfo = new ExtendedInfo();
									extendedInfo.Add(Strings.Function, "LoadDlls");
									extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
									if (String.Compare(pluginName, "SysAdmin", true, CultureInfo.InvariantCulture) != 0)
										Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo);
									else
										Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
								}
								else
									consoleStatus.PlugInsAlsoToLoad.Add(pluginName);
								continue;
							}
						}
						catch (Exception err)
						{
							string message = String.Format(Strings.CannotLoadingPlugIn, pluginName);
							ExtendedInfo extendedInfo = new ExtendedInfo("Message", err.Message);
							extendedInfo.Add(Strings.Description, err.Message);
							extendedInfo.Add(Strings.Function, "LoadDlls");
							extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
							extendedInfo.Add(Strings.Source, err.Source);
							extendedInfo.Add(Strings.StackTrace, err.StackTrace);
							extendedInfo.Add(Strings.InnerException, err.InnerException != null ? err.InnerException.ToString() : string.Empty);
							if (String.Compare(pluginName, "SysAdmin", true, CultureInfo.InvariantCulture) != 0)
								Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo);
							else
								Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							Debug.Fail(err.Message);
							continue;
						}
					}

					if (!LoadPluginDependencies(assembly))
						break;
				}
				catch (Exception e)
				{
					string message = String.Format(Strings.CannotLoadingPlugIn, pluginName);
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, e.Message);
					extendedInfo.Add(Strings.Function, "LoadDlls");
					extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
					extendedInfo.Add(Strings.Source, e.Source);
					extendedInfo.Add(Strings.StackTrace, e.StackTrace);
					extendedInfo.Add(Strings.InnerException, e.InnerException != null ? e.InnerException.ToString() : string.Empty);
					Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo);
				}
			}
		}

		//---------------------------------------------------------------------
		private Type GetPluginType (Assembly asm)
		{
			List<Type> types = new List<Type>();
			foreach (Type t in asm.GetExportedTypes())
				if (t.IsSubclassOf(typeof(PlugIn)))
					types.Add(t);

			if (types.Count == 0)
				throw new ApplicationException(string.Format(
					Strings.NoPluginClass,
					asm.FullName,
					typeof(PlugIn).FullName));

			if (types.Count > 1)
				throw new ApplicationException(string.Format(
					Strings.TooManyPluginClasses,
					asm.FullName,
					typeof(PlugIn).FullName));

			return types[0];
		}

		//---------------------------------------------------------------------
		private bool LoadPluginDependencies (Assembly assembly)
		{
			Object[] isPlugIn = assembly.GetCustomAttributes(typeof(IsPlugIn), false);
			//se è un plug 
			if (isPlugIn.Length <= 0)
				return true;
			Object[] dependedFromPlugIn = assembly.GetCustomAttributes(typeof(DependencyFromPlugIn), false);

			//va tutto bene, quindi procede
			if (((IsPlugIn)isPlugIn[0]).IsPlugInValue)
			{
				// se non è ancora caricato
				if (!IsLoadedBefore(GetPluginType(assembly).FullName))
				{
					//dipende da qualche altro plugIn ?
					if (dependedFromPlugIn.Length > 0)
					{
						//si
						string dependecyType = ((DependencyFromPlugIn)dependedFromPlugIn[0]).AssemblyDependency;
						//è già stato caricato? se si non faccio niente, se no prima  carico la sua dipendenza
						if (!IsLoadedBefore(dependecyType))
						{
							string assemblyNameLoad = FindDependencyDll(dependecyType);
							Assembly assemblyLoad = null;

							assemblyLoad = Assembly.LoadFrom(assemblyNameLoad);

							Object[] attribute = assemblyLoad.GetCustomAttributes(typeof(IsPlugIn), false);
							if (attribute.Length > 0)
							{
								bool isPlugInLoad = ((IsPlugIn)attribute[0]).IsPlugInValue;
								if (isPlugInLoad)
									LoadPlugIn(assemblyLoad, GetPluginType(assemblyLoad));
							}
						}
					}
					//no, la dipendenza è già stata caricata, oppure non esiste dipendenza, quindi procedo
					LoadPlugIn(assembly, GetPluginType(assembly));
				}
			}
			return true;
		}

		/// <summary>
		/// IsLoadedBefore
		/// Vado a controllare nell'array dei PlugIns già caricati, se il tipo corrente esiste - se non esiste, 
		/// significa che è da caricare
		/// </summary>
		//---------------------------------------------------------------------
		private bool IsLoadedBefore(string type)
		{
			foreach (object plugin in plugIns)
			{
				if (plugin.GetType().FullName == type)
					return true;
			}
			return false;
		}

		/// <summary>
		/// FindDependencyDll
		/// Dato un plugIn, ne ritorna la dipendenza (se esiste)
		/// </summary>
		//---------------------------------------------------------------------
		private string FindDependencyDll(string nameDependency)
		{
			if (pathFinder == null)
				return string.Empty;

			string name = nameDependency.Substring(nameDependency.LastIndexOf(".") + 1);
			foreach (string assemblyFile in GetPlugins())
			{
				string pluginName = GetPluginNameFromFile(assemblyFile);
				if (String.Compare(pluginName, name, true, CultureInfo.InvariantCulture) == 0)
					return assemblyFile;
			}

			return string.Empty;
		}
		#endregion

		#region LoadPlugIn e LoadActivatedPlugIns - Carica i PlugIn, i suoi eventi, se deve essere autenticato, etc.
		/// <summary>
		/// LoadPlugIn
		/// Per ogni plugIn, attivo l'oggetto, caricandolo in un array di PlugIn, carico gli eventi e 
		/// gli eventuali metodi (se c'è il custom attribute impostato) che devono "rispondere" a specificati altri eventi 
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadPlugIn (Assembly assembly, Type assemblyType)
		{
			//istanzio l'oggetto  di tipo assemblyType
			Object obj = Activator.CreateInstance(assemblyType, null);

			//aggiungo il plugIns alla lista dei plugIn caricati
			plugIns.Add(obj);
			Type definedType = assembly.GetType(assemblyType.FullName);
			if (definedType != null)
			{
				eventsBuilder.AddMethods(definedType, plugIns);
				eventsBuilder.AddEvents(obj, definedType);
			}

			//carico il plugIn invocando il metodo LoadPlugIn, definito a livello di interfaccia
			MethodInfo loadPlugIn = assemblyType.GetMethod("Load");

			ConsoleForm consoleForm = (ConsoleForm)consoleFormPanel.Controls[0];

			//ConsoleGUIObjects
			ConsoleGUIObjects consoleGUIObjects = new ConsoleGUIObjects();
			consoleGUIObjects.MenuConsole = mainMenuConsole;
			consoleGUIObjects.TreeConsole = consoleForm.consoleTree;
			consoleGUIObjects.WkgAreaConsole = consoleForm.plugInWorkingArea;
			consoleGUIObjects.BottomWkgAreaConsole = consoleForm.plugInBottomWorkingArea;

			//ConsoleEnvironmentInfo
			ConsoleEnvironmentInfo consoleEnvironmentInfo = new ConsoleEnvironmentInfo();
			consoleEnvironmentInfo.ConsoleStatus = consoleStatus.Status;
			consoleEnvironmentInfo.RunningFromServer = runningFromServer;
			consoleEnvironmentInfo.IsLiteConsole = string.Compare(Assembly.GetExecutingAssembly().ManifestModule.Name, "AdministrationConsoleLite.exe", StringComparison.InvariantCultureIgnoreCase) == 0;

			Object[] paramLoad = new Object[3];
			paramLoad[0] = consoleGUIObjects;
			paramLoad[1] = consoleEnvironmentInfo;
			//Informazioni di licenza
			paramLoad[2] = consoleStatus.LicenceInformation;

			//@@TODOMICHI DA TOGLIERE 3.0
			Object[] folderName = assembly.GetCustomAttributes(typeof(PlugInFolderName), false);

			try
			{
				loadPlugIn.Invoke(obj, paramLoad);

				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Configuration, "");
				extendedInfo.Add(Strings.RunningFrom, (runningFromServer) ? Strings.ServerPlugIn : Strings.ClientPlugIn);
				Diagnostic.Set(DiagnosticType.Information, String.Format(Strings.PlugInLoaded, definedType.Name), extendedInfo);

				if (folderName != null && folderName.Length > 0) //@@TODOMICHI DA TOGLIERE 3.0
				{
					Diagnostic.Set
						(
						DiagnosticType.Information,
						string.Format("Plugin {0} is loaded from folder {1}", definedType.Name, ((PlugInFolderName)folderName[0]).GetPlugInFolderName),
						extendedInfo
						);
				}
			}
			catch (TargetException targetExc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Function, "Load");
				extendedInfo.Add(Strings.DefinedInto, definedType.Name);
				extendedInfo.Add(Strings.CalledBy, String.Format("LoadPlugIn ({0})", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.Description, targetExc.Message);
				extendedInfo.Add(Strings.Source, targetExc.Source);
				extendedInfo.Add(Strings.StackTrace, targetExc.StackTrace);
				extendedInfo.Add(Strings.InnerException, targetExc.InnerException != null ? targetExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Warning, String.Format(Strings.PlugInLoadFailed, definedType.Name), extendedInfo);
			}
			catch (ArgumentException argumentExc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Function, "Load");
				extendedInfo.Add(Strings.DefinedInto, definedType.Name);
				extendedInfo.Add(Strings.CalledBy, String.Format("LoadPlugIn ({0})", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.Description, argumentExc.Message);
				extendedInfo.Add(Strings.Source, argumentExc.Source);
				extendedInfo.Add(Strings.InnerException, argumentExc.InnerException != null ? argumentExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Warning, String.Format(Strings.PlugInLoadFailed, definedType.Name), extendedInfo);
			}
			catch (TargetInvocationException targetInvocationExc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Function, "Load");
				extendedInfo.Add(Strings.DefinedInto, definedType.Name);
				extendedInfo.Add(Strings.CalledBy, String.Format("LoadPlugIn ({0})", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.Description, targetInvocationExc.Message);
				extendedInfo.Add(Strings.Source, targetInvocationExc.Source);
				extendedInfo.Add(Strings.InnerException, targetInvocationExc.InnerException != null ? targetInvocationExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Warning, String.Format(Strings.PlugInLoadFailed, definedType.Name), extendedInfo);
			}
			catch (TargetParameterCountException parameterCountExc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Function, "Load");
				extendedInfo.Add(Strings.DefinedInto, definedType.Name);
				extendedInfo.Add(Strings.CalledBy, String.Format("LoadPlugIn ({0})", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.Description, parameterCountExc.Message);
				extendedInfo.Add(Strings.Source, parameterCountExc.Source);
				extendedInfo.Add(Strings.InnerException, parameterCountExc.InnerException != null ? parameterCountExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Warning, String.Format(Strings.PlugInLoadFailed, definedType.Name), extendedInfo);
			}
			catch (System.MethodAccessException methodAccessExc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Function, "Load");
				extendedInfo.Add(Strings.DefinedInto, definedType.Name);
				extendedInfo.Add(Strings.CalledBy, String.Format("LoadPlugIn ({0})", ConstStrings.ApplicationName));
				extendedInfo.Add(Strings.Description, methodAccessExc.Message);
				extendedInfo.Add(Strings.Source, methodAccessExc.Source);
				extendedInfo.Add(Strings.InnerException, methodAccessExc.InnerException != null ? methodAccessExc.InnerException.ToString() : string.Empty);
				Diagnostic.Set(DiagnosticType.Warning, String.Format(Strings.PlugInLoadFailed, definedType.Name), extendedInfo);
			}
		}

		/// <summary>
		/// LoadActivatedPlugIns
		/// Carica i PlugIns che devono essere autenticati dal LoginManager
		/// </summary>
		//---------------------------------------------------------------------
		private void LoadActivatedPlugIns()
		{
			if (pathFinder == null)
				return;

			foreach (string assemblyFile in GetPlugins())
			{
				string pluginName = GetPluginNameFromFile(assemblyFile);

				if (!consoleStatus.PlugInsAlsoToLoad.Contains(pluginName))
					continue;

				try
				{
					Assembly assembly = Assembly.LoadFrom(assemblyFile);
					//leggo i custom attribute
					//Utilizzato al momento per Security e TaskScheduler
					Object[] activated = assembly.GetCustomAttributes(typeof(Activated), false);

					//Se il plugIn deve richiedere l'attivazione, e l'applicationServer non è running oppure il
					//plugIn non ha la cal, il plugIn in question non viene caricato, e la console passa
					//a quello successivo
					if (activated.Length > 0)
					{
						try
						{
							if (
								((Activated)activated[0]).IsActivatedValue &&
								(consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError
								)
								continue;

							if (
								((Activated)activated[0]).IsActivatedValue &&
								!loginManager.IsActivated(DatabaseLayerConsts.MicroareaConsole, pluginName)
								)
							{
								//Plug in non attivato
								string message = String.Format(Strings.PlugInNotActivated, pluginName);
								ExtendedInfo extendedInfo = new ExtendedInfo();
								extendedInfo.Add(Strings.Function, "LoadDlls");
								extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
								if (String.Compare(pluginName, "SysAdmin", true, CultureInfo.InvariantCulture) != 0)
									Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo);
								else
									Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
								continue;
							}

							if (!LoadPluginDependencies(assembly))
								break;
						}
						catch (Exception err)
						{
							string message = String.Format(Strings.CannotLoadingPlugIn, pluginName);
							ExtendedInfo extendedInfo = new ExtendedInfo("Message", err.Message);
							extendedInfo.Add(Strings.Description, err.Message);
							extendedInfo.Add(Strings.Function, "LoadDlls");
							extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
							extendedInfo.Add(Strings.Source, err.Source);
							extendedInfo.Add(Strings.StackTrace, err.StackTrace);
							extendedInfo.Add(Strings.InnerException, err.InnerException != null ? err.InnerException.ToString() : string.Empty);
							if (String.Compare(pluginName, "SysAdmin", true, CultureInfo.InvariantCulture) != 0)
								Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo);
							else
								Diagnostic.Set(DiagnosticType.Error, message, extendedInfo);
							Debug.Fail(err.Message);
							continue;
						}
					}
				}
				catch (Exception e)
				{
					string message = String.Format(Strings.CannotLoadingPlugIn, pluginName);
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, e.Message);
					extendedInfo.Add(Strings.Function, "LoadDlls");
					extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
					extendedInfo.Add(Strings.Source, e.Source);
					extendedInfo.Add(Strings.StackTrace, e.StackTrace);
					extendedInfo.Add(Strings.InnerException, e.InnerException != null ? e.InnerException.ToString() : string.Empty);
					Diagnostic.Set(DiagnosticType.Warning, message, extendedInfo);
				}
			}
		}
		#endregion

		#region Main
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		//---------------------------------------------------------------------
		[STAThread]
		static void Main (string[] args)
		{
			try
			{
				Application.EnableVisualStyles();
				Application.DoEvents();

				languageParam = CommandLineParam.GetCommandLineParameterValue(NameSolverStrings.TbLanguage);
				noResize = CommandLineParam.GetCommandLineParameterValue(NameSolverStrings.NoResize);
				//#if !DEBUG 
				Application.ThreadException += new ThreadExceptionEventHandler(OnMicroareaConsoleThreadException);
				AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnMicroareaConsoleUnhandledException);
				//#endif
				MicroareaConsole console = new MicroareaConsole();

				if (canRunMicroareaConsole)
					Application.Run(console);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				Debug.WriteLine(exc.Message + "\r\n" + exc.StackTrace);
			}
		}
		#endregion

		#region OnMicroareaConsoleThreadException - Cattura qualunque Exception non gestita
		/// <summary>
		/// OnMicroareaConsoleThreadException
		/// Tutto ciò che non è gestito finisce qua
		/// </summary>
		//---------------------------------------------------------------------
		private static void OnMicroareaConsoleThreadException (object sender, ThreadExceptionEventArgs eArgs)
		{
			ExceptionBox.ShowExceptionBox(sender, eArgs.Exception);
		}

		/// <summary>
		/// OnMicroareaConsoleThreadException
		/// Tutto ciò che non è gestito finisce qua
		/// </summary>
		//---------------------------------------------------------------------
		private static void OnMicroareaConsoleUnhandledException(object sender, UnhandledExceptionEventArgs eArgs)
		{
			ExceptionBox.ShowExceptionBox(sender, eArgs.ExceptionObject.ToString());
		}
		#endregion

		#region Visualizza / Nasconde oggetti della Console (tree, toolbar...)

		/// <summary>
		/// OnShowLoginAdvancedOptions
		/// Ritorna il valore della checkbox nella finestra 'Customize', in modo da mostrare
		/// o nascondere le opzioni avanzate sugli utenti associati ad un'azienda 
		/// [cambio associazioni, creazione nuova login, manutenzione login]
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnShowLoginAdvancedOptions")]
		public bool ShowLoginAdvancedOptions()
		{
			return consoleStatus.IsVisibleLoginAdvancedOptions;
		}

		/// <summary>
		///	Click su windows per customizzare il layout della console
		/// </summary>
		//---------------------------------------------------------------------
		private void customizeMenuItem_Click (object sender, System.EventArgs e)
		{
			CustomizeView customizeView = new CustomizeView(consoleStatus);
			customizeView.OnEnabledConsoleTree += new CustomizeView.EnabledConsoleTree(ConsoleTreeHandler);
			customizeView.OnEnabledConsoleToolbar += new CustomizeView.EnabledConsoleToolbar(ConsoleToolbarHandler);
			customizeView.OnEnabledConsoleStatusBar += new CustomizeView.EnabledConsoleStatusBar(ConsoleStatusBarHandler);
			customizeView.OnEnabledConsoleMenuPlugIn += new CustomizeView.EnabledConsoleMenuPlugIn(ConsoleMenuPlugInHandler);
			customizeView.OnEnabledLoginAdvancedOptions += new CustomizeView.EnabledLoginAdvancedOptions(EnabledLoginAdvancedOptions);
			customizeView.ShowDialog();
		}

		/// <summary>
		/// Handle per visualizzare/nascondere le opzioni avanzate per le login
		/// </summary>
		//---------------------------------------------------------------------
		protected void EnabledLoginAdvancedOptions(object sender, EnabledConsoleElementEventArgs e)
		{
			consoleStatus.IsVisibleLoginAdvancedOptions = e.IsVisible;

			// manda un evento che viene intercettato dal SysAdmin e aggiunge/toglie i MenuItem dal 
			// menu di contesto degli utenti associati all'azienda
			if (OnUpdateTreeContextMenu != null)
				OnUpdateTreeContextMenu(sender);
		}

		/// <summary>
		/// Handle per visualizzare/nascondere il Tree
		/// </summary>
		//---------------------------------------------------------------------
		protected void ConsoleTreeHandler(object sender, EnabledConsoleElementEventArgs e)
		{
			ConsoleForm consoleForm = (ConsoleForm)consoleFormPanel.Controls[0];
			TreeView consoleTree = (TreeView)consoleForm.consoleTree;
			consoleTree.Visible = e.IsVisible;
			consoleStatus.IsVisibleConsoleTree = e.IsVisible;
		}

		/// <summary>
		/// ConsoleToolbarHandler
		/// Handle per visualizzare/nascondere la Toolbar
		/// </summary>
		//---------------------------------------------------------------------
		protected void ConsoleToolbarHandler (object sender, EnabledConsoleElementEventArgs e)
		{
			this.consoleToolStrip.Visible = e.IsVisible;
			consoleStatus.IsVisibleStandardToolbarConsole = e.IsVisible;
			ConsoleForm consoleForm = (ConsoleForm)consoleFormPanel.Controls[0];
		}

		/// <summary>
		/// ConsoleStatusBarHandler
		/// Handle per visualizzare/nascondere la Status Bar; se la status bar
		/// è presente oppure no, modifico le impostazione del panel e di conseguenza,
		/// quelle della ConsoleForm(tree+workingarea+workingbottomarea)contenuto in essa
		/// </summary>
		//---------------------------------------------------------------------
		protected void ConsoleStatusBarHandler (object sender, EnabledConsoleElementEventArgs e)
		{
			statusBarConsole.Visible = e.IsVisible;
			consoleStatus.IsVisibleStatusBarConsole = e.IsVisible;
			ConsoleForm consoleForm = (ConsoleForm)consoleFormPanel.Controls[0];
		}

		/// <summary>
		/// ConsoleStandardMenuHandler
		/// Handle per visualizzare/nascondere le voci standard di menu (View e Menu)
		/// </summary>
		//---------------------------------------------------------------------
		protected void ConsoleStandardMenuHandler (object sender, EnabledConsoleElementEventArgs e)
		{
			viewMenuItem.Visible = e.IsVisible;
			consoleStatus.IsVisibleStandardMenuConsole = e.IsVisible;
		}

		/// <summary>
		/// ConsoleMenuPlugInHandler
		/// Handle per visualizzare/nascondere le voci di menù relative ai plugIns
		/// </summary>
		//---------------------------------------------------------------------
		protected void ConsoleMenuPlugInHandler (object sender, EnabledConsoleElementEventArgs e)
		{
			for (int i = 0; i < mainMenuConsole.Items.Count; i++)
			{
				Type menuType = mainMenuConsole.Items[i].GetType();
				if (String.Compare(menuType.FullName, "Microarea.Console.Core.PlugIns.PlugMenuItem", true, CultureInfo.InvariantCulture) == 0)
					mainMenuConsole.Items[i].Visible = e.IsVisible;
			}
			consoleStatus.IsVisibleMenuPlugIn = e.IsVisible;
		}
		#endregion

		#region Funzioni di Uscita dalla MicroareaConsole
		//---------------------------------------------------------------------
		private bool ExitConsole (bool exitApplication)
		{
			bool confirmExitFromPlugIn = true;

			if (OnShutDownConsole != null)
			{
				try
				{
					for (int i = 0; i < OnShutDownConsole.GetInvocationList().Length; i++)
					{
						System.IAsyncResult a = BeginInvoke(OnShutDownConsole.GetInvocationList()[i], null);
						if (a != null)
							confirmExitFromPlugIn = confirmExitFromPlugIn && Convert.ToBoolean(EndInvoke(a));
					}
				}
				catch (Exception exc)
				{
					Debug.Fail(exc.Message);
				}

				if (!confirmExitFromPlugIn)
					return false;
			}

			if (!exitWithoutAskMessage)
			{
				//chiedo conferma della chiusura
				diagnosticViewer.Message = Strings.LblExit;
				diagnosticViewer.Title = Text;
				diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
				diagnosticViewer.ShowIcon = MessageBoxIcon.Question;
				diagnosticViewer.DefaultButton = MessageBoxDefaultButton.Button2;
				DialogResult exitConfirm = diagnosticViewer.Show();
				if (exitConfirm == DialogResult.No)
					return false;

				//se lo scheduler sta runnundo chiedo se si vuole uscire comunque
				if (schedulerIsRunning)
				{
					diagnosticViewer.Message = Strings.AskStopSchedulerAgent;
					diagnosticViewer.Title = Strings.LblExit;
					diagnosticViewer.ShowButtons = MessageBoxButtons.YesNo;
					diagnosticViewer.ShowIcon = MessageBoxIcon.Question;
					diagnosticViewer.DefaultButton = MessageBoxDefaultButton.Button2;
					DialogResult exitConfirmStopSchedulerAgent = diagnosticViewer.Show();
					if (exitConfirmStopSchedulerAgent == DialogResult.No)
						return false;
				}
			}

			if (exitApplication)
			{
				Application.Exit();
				ClearAll();
			}

			//chiude tb qualora fosse stato aperto dallo Scheduler
			TbApplicationClientInterface.CloseTBApplication();

			return true;
		}

		/// <summary>
		/// Uscita della console (Da Menù - Voce Esci)
		/// PUNTO DI USCITA DALLA CONSOLE
		/// </summary>
		//---------------------------------------------------------------------
		private void exitMenuItem_Click (object sender, System.EventArgs e)
		{
			ExitConsole(true);
		}

		/// <summary>
		/// Uscita dalla Console, con eventuale richiesta di conferma all'utente
		/// Se exitWithoutAskMessage = true chiedo conferma all'utente prima di chiudere l'AC
		/// exitWithoutAskMessage = false quando c'è già una istanza attiva della console,
		/// e pertanto devo chiuderla immediatamente, senza chiedere nessuna conferma.
		/// </summary>
		//---------------------------------------------------------------------
		private void MicroareaConsole_Closing (object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !ExitConsole(false);
		}

		/// <summary>
		/// Uscita della console (Dal pulsante della toolbar Esci)
		/// PUNTO DI USCITA DALLA CONSOLE
		/// </summary>
		//---------------------------------------------------------------------
		private void exitToolStripButton_Click(object sender, EventArgs e)
		{
			ExitConsole(true);
		}
		#endregion

		#region Settaggi del formato di visualizzazione della Console (Icons, List, Details)
		/// <summary>
		/// setta la visualizzazione a Icone(grandi)
		/// </summary>
		//---------------------------------------------------------------------
		private void largeIconsMenu_Click (object sender, System.EventArgs e)
		{
			consoleStatus.DefaultView = View.LargeIcon;
			((ToolStripMenuItem)sender).Checked = true;
			listMenu.Checked = false;
			smallIconsMenu.Checked = false;
			detailMenu.Checked = false;
			if (OnChangedView != null)
				OnChangedView(sender, View.LargeIcon);
		}

		/// <summary>
		/// setta la visualizzazione a Icone(piccole)
		/// </summary>
		//---------------------------------------------------------------------
		private void smallIconsMenu_Click (object sender, System.EventArgs e)
		{
			consoleStatus.DefaultView = View.SmallIcon;
			((ToolStripMenuItem)sender).Checked = true;
			largeIconsMenu.Checked = false;
			listMenu.Checked = false;
			detailMenu.Checked = false;
			if (OnChangedView != null)
				OnChangedView(sender, View.SmallIcon);
		}

		/// <summary>
		/// Setta la visualizzazione a List
		/// </summary>
		//---------------------------------------------------------------------
		private void listMenu_Click (object sender, System.EventArgs e)
		{
			consoleStatus.DefaultView = View.List;
			((ToolStripMenuItem)sender).Checked = true;
			largeIconsMenu.Checked = false;
			smallIconsMenu.Checked = false;
			detailMenu.Checked = false;
			if (OnChangedView != null)
				OnChangedView(sender, View.List);
		}

		/// <summary>
		/// Setta la visualizzazione a Detail
		/// </summary>
		//---------------------------------------------------------------------
		private void detailMenu_Click (object sender, System.EventArgs e)
		{
			consoleStatus.DefaultView = View.Details;
			((ToolStripMenuItem)sender).Checked = true;
			largeIconsMenu.Checked = false;
			smallIconsMenu.Checked = false;
			listMenu.Checked = false;
			if (OnChangedView != null)
				OnChangedView(sender, View.Details);
		}
		#endregion

		#region aboutMenu_Click - Finestra di about
		/// <summary>
		/// Finesta di about
		/// </summary>
		//---------------------------------------------------------------------
		private void aboutMenu_Click (object sender, System.EventArgs e)
		{
			AboutBox aboutBox = new AboutBox(runningFromServer, consoleStatus.LicenceInformation);
			aboutBox.ShowDialog(this);
			LoadLicenceInfo();
		}
		#endregion

		#region GetConsoleStatus - Restituisce lo Stato di Console
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnGetConsoleStatus")]
		public StatusType GetConsoleStatus ()
		{
			return consoleStatus.Status;
		}
		#endregion

		#region RefreshConsoleStatus - Esegue il Refresh anche sullo stato della console (e nel caso mostra le icone)
		/// <summary>
		/// overload RefreshConsoleStatus
		/// </summary>
		//---------------------------------------------------------------------
		public void RefreshConsoleStatus ()
		{
			RefreshConsoleStatus(String.Empty);
		}
		/// <summary>
		/// RefreshConsoleStatus
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("Microarea.Console.Plugin.SysAdmin.SysAdmin", "OnRefreshConsoleStatus")]
		public void RefreshConsoleStatus (string fullServerName)
		{
			Cursor.Current = Cursors.WaitCursor;
			Cursor.Show();
			//Inizializzo lo stato 
			consoleStatus.Status = StatusType.None;

			this.LoadLicenceInfo();

			if (consoleStatus.LicenceInformation.Edition.Length == 0 ||
				consoleStatus.LicenceInformation.DBNetworkType == DBNetworkType.Undefined ||
				consoleStatus.LicenceInformation.IsoState.Length == 0)
				consoleStatus.Status = StatusType.RemoteServerError;

			//verifico l'esistenza del ServerConnection.config
			if (pathFinder.ServerConnectionFile.Length == 0)
				consoleStatus.Status = consoleStatus.Status | StatusType.StartUp;
			else
				if (!File.Exists(pathFinder.ServerConnectionFile))
					consoleStatus.Status = consoleStatus.Status | StatusType.StartUp;
				else
					if (!IsValidConnectionToSystemDb())
						consoleStatus.Status = consoleStatus.Status | StatusType.StartUp;
					else
						consoleStatus.Status = consoleStatus.Status | StatusType.Administration;

			if (schedulerIsRunning)
				consoleStatus.Status = consoleStatus.Status | StatusType.SchedulerAgentIsRunning;

			if (fullServerName.Length > 0)
			{
				serverName = instanceName = String.Empty;
				serverName = fullServerName.Split(Path.DirectorySeparatorChar)[0];
				if (fullServerName.Split(Path.DirectorySeparatorChar).Length > 1)
					instanceName = fullServerName.Split(Path.DirectorySeparatorChar)[1];
			}

			BuildPanels();
			Cursor.Current = Cursors.Default;
			Cursor.Show();
		}
		#endregion

		#region ClearAll - Pulisco l'array degli utenti autenticati e dò un msg
		/// <summary>
		/// ClearAll
		/// Pulisco l'array degli utenti autenticati e dò un msg
		/// </summary>
		//---------------------------------------------------------------------
		private void ClearAll ()
		{
			consoleStatus.AutenticatedUsers.Clear();
			consoleStatus.Diagnostic.Set(DiagnosticType.Information, Strings.LogOutFromMicroareaConsole);
		}
		#endregion

		#region ViewMessages - Visualizza i messaggi della console e dei PlugIns (se ce ne sono)
		//---------------------------------------------------------------------
		private void showMessagesToolStripButton_Click(object sender, EventArgs e)
		{
			try
			{
				ViewMessages();
			}
			catch (Exception exc)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, exc.Message);
				extendedInfo.Add(Strings.Source, exc.Source);
				extendedInfo.Add(Strings.DefinedInto, ConstStrings.ApplicationName);
				extendedInfo.Add(Strings.StackTrace, exc.StackTrace);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.PressConsoleToolbar, showMessagesToolStripButton.ToolTipText), extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}

		/// <summary>
		/// ViewMessages
		/// Visualizza i messaggi della console e dei PlugIns (se ce ne sono)
		/// </summary>
		//---------------------------------------------------------------------
		private void ViewMessages ()
		{
			Diagnostic plugInsDiagnostic = new Diagnostic("PlugInsDiagnostic");
			if (OnGetPlugInMessages != null)
				OnGetPlugInMessages(this, plugInsDiagnostic);
			//merge tra plugIns diagnostic e il diagnostic della console
			plugInsDiagnostic.Set(Diagnostic);

			if (Diagnostic.Error || Diagnostic.Information || Diagnostic.Warning)
				DiagnosticViewer.ShowDiagnostic(plugInsDiagnostic);
		}
		#endregion

		#region OnAfterSelectedTreeNode - Imposto opportunamente il namespace per l'help ogni volta che seleziono un nodo sul tree
		/// <summary>
		/// OnAfterSelectedTreeNode
		/// Imposto opportunamente il namespace per l'help (che punta all'help del nodo selezionato)oppure a quello generale
		/// </summary>
		//---------------------------------------------------------------------
		private void OnAfterSelectedTreeNode (string assemblyName, System.EventArgs e)
		{
			selectedAssembly = assemblyName;

			if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, String.Format(Strings.CannotContactLoginManagerDesc, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)));
				extendedInfo.Add(Strings.Function, "LoginManager");
				extendedInfo.Add(Strings.Library, "LoginManagerWrapper");
				extendedInfo.Add(Strings.CalledBy, String.Format("{0} (OnAfterSelectedTreeNode)", ConstStrings.ApplicationName));
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotContactLoginManager, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture)), extendedInfo);
			}
		}
		#endregion

		#region OnAfterErrorsOnTree - Errori che si verificano cliccando sui nodi dei tree
		/// <summary>
		/// OnAfterErrorsOnTree
		/// Errori che si verificano cliccando sui nodi dei tree (ConsoleForm)
		/// Li eredito nel diagnostico di MicroareaConsole
		/// </summary>
		//---------------------------------------------------------------------
		public void OnAfterErrorsOnTree (Diagnostic diagnosticTree)
		{
			if (diagnosticTree != null)
				Diagnostic.Set(diagnosticTree);
		}
		#endregion

		#region MicroareaConsole_Load - Inizializza lo stato della Console (nodi, caricamento PlugIns..)
		/// <summary>
		/// Inizializza lo stato della Console, costruisce il primo nodo nel tree, 
		/// carica le dll e aggancia gli eventi tra plugIn
		/// </summary>
		//---------------------------------------------------------------------------------
		private void MicroareaConsole_Load (object sender, System.EventArgs e)
		{
			try
			{
				// Inizializza il timer per la progress bar
				InitTimer();
				// Inizializza e carica la ProgressBar nella StatusBar
				InitProgressBar();
				// Setta l'incona della MicroareaConsole
				SetConsoleIcon();
				// Setta le icone per gli stati della MicroareaConsole
				SetIcons();

				// disabilito il bottone di connect e la voce di menù
				OnDisableConnectToolBarButton(sender, e);
				OnDisableDisconnectToolBarButton(sender, e);

				//Inizializzo e mostro la console
				InitMicroareaConsole();
				Application.DoEvents();

				// Stabilisco lo status della MicroareaConsole
				// Se è StartUp sto creando il db di sistema, in quanto non esiste il file ServerConnection.config in Custom
				// In caso contrario sono Administration
				if (!File.Exists(pathFinder.ServerConnectionFile))
					consoleStatus.Status = consoleStatus.Status | StatusType.StartUp;
				else
					if (!IsValidConnectionToSystemDb())
						consoleStatus.Status = consoleStatus.Status | StatusType.StartUp;
					else
						consoleStatus.Status = consoleStatus.Status | StatusType.Administration;

				// Info di licenza che saranno propagate poi ai vari PlugIns (se errore setto la stato a HTTPError)
				if (activator.ProductActivated())
					LoadLicenceInfo();
				else
				// se non è attivato devo mostrare 
				// o la casella per inserire il serial number s enon trovo neanche un licensed di prodotto master già fatto
				// o, se trovo il licensed di prodotto master, la ex-wua con le griglie di inserimento dati.
				{
					//chiudo la splash
					SplashStarter.Finish();

					if (!activator.ShowAboutAndActivate(this)) // close the Console, without asking the user
					{
						this.exitWithoutAskMessage = true;
						this.Close();
						return;
					}
					else
					{
						consoleStatus.Status = consoleStatus.Status & ~StatusType.RemoteServerError;
						LoadLicenceInfo();
					}
				}

				// controllo lo stato del servizio del server del database di sistema (solo se non e' su Azure)
				if (!consoleStatus.LicenceInformation.IsAzureSQLDatabase)
					CheckDatabaseServiceStatus();

				//costruisco i panel in base allo stato
				if (consoleStatus.Status != StatusType.Administration)
					BuildPanels();

				Application.DoEvents();

				OnSetProgressBarStep(sender, 4);
				OnSetProgressBarText(sender, Strings.Loading);
				OnSetProgressBarValue(sender, 3);
				OnSetProgressBarMaxValue(sender, 100);
				OnEnableProgressBar(sender);

				// Inizializza le variabili di stato della MicroareaConsole
				InitConsoleStatus();

				// Carico i PlugIns
				InitMicroareaConsolePlugIns();

				// manda l'evento di inizializzazione di BrandLoader ai plugIn interessati ad usare il brandLoader
				OnInitBrandLoader?.Invoke(InstallationData.BrandLoader);
				// manda l'evento di inizializzazione di PathFinder ai plugIn interessati ad usare il pathFinder
				OnInitPathFinder?.Invoke(pathFinder);
				// manda l'evento di inizializzazione di LoginManager ai plugIn interessati ad usare il LoginManager
				OnInitLoginManager?.Invoke(loginManager);

				// disabilito la progress bar
				OnSetProgressBarText(sender, String.Empty);
				OnSetProgressBarValue(sender, 100);
				OnDisableProgressBar(sender);
				// chiudo la splash
				SplashStarter.Finish();

				//Se ci sono errori o Warnings
				if (Diagnostic.Error)
					DiagnosticViewer.ShowDiagnostic(Diagnostic);

				//abilito il bottone di connect e la voce di menu
				OnEnableDisconnectToolBarButton(sender, e);
				OnEnableConnectToolBarButton(sender, e);

				if (OnConnect != null)
					OnConnect(sender, e);
			}
			catch (Exception ex)
			{
				LogException(ex);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
				Close();
			}
		}
		#endregion

		#region BuildPanels - Costruisce i Panel nella StatusBar (l'unico obbligatorio è quello della progressBar)
		/// <summary>
		/// BuildPanels
		/// Costruisce i Pannelli nella StatusBar
		/// l'unico obbligatorio è quello della progressBar, gli altri identificano gli stati di 
		/// errore e/o di inizializzazione della console
		/// </summary>
		//---------------------------------------------------------------------
		private void BuildPanels ()
		{
			//l'unico pannello che esiste sempre è quello della progressBar
			StatusBarPanel progressPanel = this.statusBarConsole.Panels[0];
			//tutti gli altri Panel li devo costruire in base allo stato
			statusBarConsole.Panels.Clear();

			progressPanel.AutoSize = StatusBarPanelAutoSize.Spring;
			statusBarConsole.Panels.Add(progressPanel);

			if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
			{
				StatusBarPanel consoleStatusPanel = new StatusBarPanel();
				consoleStatusPanel.Text = Strings.RemoteServerError;
				consoleStatusPanel.ToolTipText = String.Format(Strings.RemoteServerErrorToolTip, pathFinder.RemoteWebServer.ToUpper(CultureInfo.InvariantCulture));
				consoleStatusPanel.Icon = loginManagerErrorIcon;
				consoleStatusPanel.AutoSize = StatusBarPanelAutoSize.Contents;
				statusBarConsole.Panels.Add(consoleStatusPanel);
			}
			if ((consoleStatus.Status & StatusType.StartUp) == StatusType.StartUp)
			{
				StatusBarPanel startUpConsole = new StatusBarPanel();
				startUpConsole.Text = Strings.StartUp;
				startUpConsole.ToolTipText = Strings.StartUpToolTip;
				startUpConsole.Icon = startUpIcon;
				startUpConsole.AutoSize = StatusBarPanelAutoSize.Contents;
				statusBarConsole.Panels.Add(startUpConsole);
			}
			if ((consoleStatus.Status & StatusType.SchedulerAgentIsRunning) == StatusType.SchedulerAgentIsRunning)
			{
				StatusBarPanel schedulerAgentPanel = new StatusBarPanel();
				schedulerAgentPanel.Text = Strings.SchedulerAgentRunning;
				schedulerAgentPanel.ToolTipText = Strings.SchedulerAgentIsStarted;
				schedulerAgentPanel.Icon = schedulerAgentIcon;
				schedulerAgentPanel.AutoSize = StatusBarPanelAutoSize.Contents;
				statusBarConsole.Panels.Add(schedulerAgentPanel);
			}

			//prendo l'elemento corrente fra i servizi disponibili (in realtà ce ne è sempre UNO!)
			DatabaseService currentService = null;
			if (consoleStatus.SqlServiceDatabase.Count == 1)
				currentService = (DatabaseService)consoleStatus.SqlServiceDatabase[0];

			if (currentService == null)
				return;

			string serviceName = currentService.ServiceName;
			string computerName = currentService.ComputerName;
			string activationDatabase = currentService.ActivationDatabase;

			if ((currentService.ServiceStatus & StatusType.SqlServiceRun) == StatusType.SqlServiceRun)
			{
				sqlServicePanel.Text = String.Empty;
				sqlServicePanel.ToolTipText = String.Format(Strings.RunningDatabase, activationDatabase, computerName, serviceName);
				sqlServicePanel.Icon = sqlServiceRunIcon;
				sqlServicePanel.AutoSize = StatusBarPanelAutoSize.Contents;
				statusBarConsole.Panels.Add(sqlServicePanel);
			}
			else if ((currentService.ServiceStatus & StatusType.SqlServicePause) == StatusType.SqlServicePause)
			{
				sqlServicePanel.Text = String.Empty;
				sqlServicePanel.ToolTipText = String.Format(Strings.PausingDatabase, activationDatabase, computerName, serviceName);
				sqlServicePanel.Icon = sqlServicePauseIcon;
				sqlServicePanel.AutoSize = StatusBarPanelAutoSize.Contents;
				statusBarConsole.Panels.Add(sqlServicePanel);
			}
			else if ((currentService.ServiceStatus & StatusType.SqlServiceStop) == StatusType.SqlServiceStop)
			{
				sqlServicePanel.Text = String.Empty;
				sqlServicePanel.ToolTipText = String.Format(Strings.StoppingDatabase, activationDatabase, computerName, serviceName);
				sqlServicePanel.Icon = sqlServiceStopIcon;
				sqlServicePanel.AutoSize = StatusBarPanelAutoSize.Contents;
				statusBarConsole.Panels.Add(sqlServicePanel);
			}
			else if ((currentService.ServiceStatus & StatusType.SqlServiceUndefined) == StatusType.SqlServiceUndefined)
			{
				sqlServicePanel.Text = String.Empty;
				sqlServicePanel.ToolTipText = Strings.UndefinedDatabaseState;
				sqlServicePanel.Icon = sqlServiceUndefinedIcon;
				sqlServicePanel.AutoSize = StatusBarPanelAutoSize.Contents;
				statusBarConsole.Panels.Add(sqlServicePanel);
			}
		}
		#endregion

		#region statusBarConsole_DrawItem
		/// <summary>
		/// statusBarConsole_DrawItem
		/// Il pannello della status bar che contiene la progressbar viene sempre riscritto a seconda della 
		/// dimensione (che dipende se esistono altri panel nella status bar oppure no)
		/// </summary>
		//---------------------------------------------------------------------
		private void statusBarConsole_DrawItem (object sender, System.Windows.Forms.StatusBarDrawItemEventArgs sbdevent)
		{
			if (sbdevent.Panel == this.progressBarPanel)
			{
				if (statusBarConsole.Controls.Count > 0)
					((PlugInsProgressBar)statusBarConsole.Controls[0]).Bounds = sbdevent.Bounds;
			}
		}
		#endregion

		#region Funzioni di Help

		#region helpMenu_Click - Simulo la pressione del tasto F1
		/// <summary>
		/// helpMenu_Click
		/// Simulo la pressione del tasto F1
		/// </summary>
		//---------------------------------------------------------------------
		private void ShowMenu (object sender, System.EventArgs e, bool onLine)
		{
			try
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, Strings.ErrorFromLoginManager);
					extendedInfo.Add(Strings.Function, "helpMenu_Click");
					extendedInfo.Add(Strings.CalledBy, ConstStrings.ApplicationName);
					Diagnostic.Set(DiagnosticType.Error, Strings.CannotInvokeHelp, extendedInfo);
					DiagnosticViewer.ShowDiagnostic(Diagnostic);
				}
				else
				{

					string searchParameter = 
						(string.Compare(selectedAssembly, GetType().Assembly.GetName().Name, StringComparison.InvariantCultureIgnoreCase) == 0)
						? GetType().FullName
						: GetPluginType(Assembly.Load(selectedAssembly)).FullName;
					if (consoleFormPanel.Controls.Count > 0)
					{
						ConsoleForm consoleForm = (ConsoleForm)consoleFormPanel.Controls[0];
						Panel workingArea = consoleForm.plugInWorkingArea;
						if (workingArea.Controls.Count > 0)
							searchParameter = workingArea.Controls[0].GetType().FullName;
					}

					string lang = InstallationData.ServerConnectionInfo.PreferredLanguage;
					string edition = loginManager.GetEdition();

					searchParameter = "RefGuide." + searchParameter;
					HelpManager.CallOnlineHelp(searchParameter, lang);
				}
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, Strings.ErrorFromLoginManager);
				extendedInfo.Add(Strings.LblError, exc.Message);
				extendedInfo.Add(Strings.Function, "helpMenu_Click");
				extendedInfo.Add(Strings.CalledBy, ConstStrings.ApplicationName);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotInvokeHelp, extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}

		//---------------------------------------------------------------------
		private void helpMenu_Click (object sender, System.EventArgs e)
		{
			ShowMenu(sender, e, false);
		}
		#endregion

		#region OnOpenHelpFromPopUp
		/// <summary>
		/// OnOpenHelpFromPopUp
		/// Richiamo l'help dalle PopUp dei plugIns
		/// </summary>
		//---------------------------------------------------------------------
		[AssemblyEvent("All", "OnCallHelpFromPopUp")]
		public void OnOpenHelpFromPopUp (object sender, string nameSpace, string searchParameter)
		{
			try
			{
				if ((consoleStatus.Status & StatusType.RemoteServerError) == StatusType.RemoteServerError)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(Strings.Description, Strings.ErrorFromLoginManager);
					extendedInfo.Add(Strings.Function, "OnOpenHelpFromPopUp");
					extendedInfo.Add(Strings.CalledBy, nameSpace);
					extendedInfo.Add(Strings.CalledBy, ConstStrings.ApplicationName);
					Diagnostic.Set(DiagnosticType.Error, Strings.CannotInvokeHelp, extendedInfo);
					DiagnosticViewer.ShowDiagnostic(Diagnostic);
				}
				else
				{
					string preferredLanguage = InstallationData.ServerConnectionInfo.PreferredLanguage;
					string edition = loginManager.GetEdition();

					HelpManager.CallOnlineHelp(searchParameter, preferredLanguage);
				}
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(Strings.Description, Strings.ErrorFromLoginManager);
				extendedInfo.Add(Strings.LblError, exc.Message);
				extendedInfo.Add(Strings.Function, "OnOpenHelpFromPopUp");
				extendedInfo.Add(Strings.CalledBy, nameSpace);
				extendedInfo.Add(Strings.CalledBy, ConstStrings.ApplicationName);
				Diagnostic.Set(DiagnosticType.Error, Strings.CannotInvokeHelp, extendedInfo);
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		#endregion

		#region Gestione dei servizi di Database (ServiceManager)
		/// <summary>
		/// statusBarConsole_PanelClick
		/// </summary>
		//---------------------------------------------------------------------
		private void statusBarConsole_PanelClick (object sender, System.Windows.Forms.StatusBarPanelClickEventArgs e)
		{
			if (e.StatusBarPanel == sqlServicePanel)
			{
				ServiceManager serviceManager = new ServiceManager(consoleStatus.SqlServiceDatabase);
				serviceManager.OnChangedState += new EventHandler(serviceManager_OnChangedState);
				serviceManager.ShowDialog();
			}
		}

		/// <summary>
		/// serviceManager_OnChangedState
		/// Lo stato di un servizio è stato modificato
		/// </summary>
		//---------------------------------------------------------------------
		private void serviceManager_OnChangedState (object sender, System.EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			Cursor.Show();

			ServiceController serviceChanged = (ServiceController)sender;
			for (int i = 0; i < this.consoleStatus.SqlServiceDatabase.Count; i++)
			{
				DatabaseService current = (DatabaseService)this.consoleStatus.SqlServiceDatabase[i];
				if (String.Compare(current.ServiceDisplayName, serviceChanged.DisplayName, true, CultureInfo.InvariantCulture) == 0)
				{
					StatusType newState;
					if (serviceChanged.Status == ServiceControllerStatus.Running)
						newState = StatusType.SqlServiceRun;
					else if (serviceChanged.Status == ServiceControllerStatus.Paused)
						newState = StatusType.SqlServicePause;
					else if (serviceChanged.Status == ServiceControllerStatus.Stopped)
						newState = StatusType.SqlServiceStop;
					else
						newState = StatusType.SqlServiceUndefined;

					((DatabaseService)this.consoleStatus.SqlServiceDatabase[i]).ServiceStatus = newState;
				}
			}

			CheckDatabaseServiceStatus();
			BuildPanels();
			Cursor.Current = Cursors.Default;
			Cursor.Show();
		}

		//---------------------------------------------------------------------
		private StatusType SetServiceStatus (ServiceControllerStatus status)
		{
			StatusType type = StatusType.None;

			switch (status)
			{
				case ServiceControllerStatus.Running:
					type = StatusType.SqlServiceRun; break;

				case ServiceControllerStatus.Paused:
					type = StatusType.SqlServicePause; break;

				case ServiceControllerStatus.Stopped:
					type = StatusType.SqlServiceStop; break;

				default:
					type = StatusType.SqlServiceUndefined; break;
			}

			return type;
		}

		/// <summary>
		/// CheckDatabaseServiceStatus
		/// carica i servizi attivi sul server specificato nella finestra di login e verifica che sia attivo
		/// quello di sql server (a seconda dell'istanza selezionata)
		/// </summary>
		//---------------------------------------------------------------------
		private void CheckDatabaseServiceStatus ()
		{
			if (string.IsNullOrEmpty(serverName))
				serverName = Dns.GetHostName().ToUpper(CultureInfo.InvariantCulture);

			try
			{
				if (SyntaxCheck.CheckMachineName(serverName))
				{
					consoleStatus.SqlServiceDatabase.Clear();

					IPHostEntry hostEntry = Dns.GetHostEntry(serverName);

					ServiceController[] services = ServiceController.GetServices(serverName);

					foreach (ServiceController service in services)
					{
						if (service.DisplayName.IndexOf("SQL") == -1)
							continue;

						DatabaseService dbService = new DatabaseService();
						dbService.ServiceName = service.ServiceName;
						dbService.ServiceDisplayName = service.DisplayName;
						dbService.ComputerName = string.IsNullOrEmpty(instanceName) ? serverName : Path.Combine(serverName,instanceName);
						dbService.ServiceStatus = SetServiceStatus(service.Status);
						dbService.ServiceController = service;

						// se l'istanza è vuota mostro solo i servizi di SQL Server
						if (string.IsNullOrEmpty(instanceName))
						{
							// servizio Sql Server 2000
							if (service.DisplayName.StartsWith("MSSQLSERVER"))
							{
								dbService.ActivationDatabase = "SQLServer2000";
								consoleStatus.SqlServiceDatabase.Add(dbService);
								continue;
							}

							// servizio Sql Server 2005
							if (String.Compare(service.DisplayName, "SQL Server (MSSQLSERVER)", StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								// devo discriminare in base alla versione, perche' il servizio di una default instance
								// di SQL Express ha il medesimo nome servizio di SQL Server 2005
								switch (sqlEdition)
								{
									case SQLServerEdition.SqlServer2005:
										dbService.ActivationDatabase = "SQLServer2005";
										break;
									case SQLServerEdition.SqlServer2008:
										dbService.ActivationDatabase = "SQLServer2008";
										break;
									case SQLServerEdition.SqlServer2012:
										dbService.ActivationDatabase = "SQLServer2012";
										break;
                                    case SQLServerEdition.SqlServer2014:
                                        dbService.ActivationDatabase = "SQLServer2014";
                                        break;
									case SQLServerEdition.SqlServer2016:
										dbService.ActivationDatabase = "SQLServer2016";
										break;
									case SQLServerEdition.SqlExpress2005:
									case SQLServerEdition.SqlExpress2008:
									case SQLServerEdition.SqlExpress2012:
                                    case SQLServerEdition.SqlExpress2014:
									case SQLServerEdition.SqlExpress2016:
										dbService.ActivationDatabase = "SQLServer Express Ed.";
										break;
								}
								consoleStatus.SqlServiceDatabase.Add(dbService);
								continue;
							}
						}
						else
						{
							// servizio MSDE 2000
							if (String.Compare(service.DisplayName, String.Concat("MSSQL$", instanceName), StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								dbService.ActivationDatabase = "MSDE";
								consoleStatus.SqlServiceDatabase.Add(dbService);
								continue;
							}

							// servizio Sql Server Express Edition 2005
							if (String.Compare(service.DisplayName, String.Concat("SQL Server (", instanceName, ")"), StringComparison.InvariantCultureIgnoreCase) == 0)
							{
								dbService.ActivationDatabase = "SQLServer Express Ed.";
								consoleStatus.SqlServiceDatabase.Add(dbService);
								continue;
							}
						}
					}
				}
			}
			catch (Exception)
			{
			}

			if (consoleStatus.SqlServiceDatabase.Count == 0)
				consoleStatus.SqlServiceDatabase.Add(new DatabaseService());
		}
		#endregion

		#region computerManagement_Click - Avvio di 'Gestione Computer'
		/// <summary>
		/// computerManagement_Click
		/// Esegue Computer management
		/// </summary>
		//---------------------------------------------------------------------
		private void computerManagement_Click (object sender, System.EventArgs e)
		{
			string computerManagementCommand = "compmgmt.msc"; //si trova in \%WINDIR%\SYSTEM32 

			try
			{
				Process.Start(computerManagementCommand);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message, exc.InnerException.Message);
				Diagnostic.Set(DiagnosticType.Error, String.Format(Strings.CannotExecuteProcess, computerManagementCommand));
				DiagnosticViewer.ShowDiagnostic(Diagnostic);
			}
		}
		#endregion

		# region ProxySettings
		//--------------------------------------------------------------------------------
		private void proxySettingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SetProxySetting(ProxyFirewallManager.Show(GetProxySetting()));
		}

		//---------------------------------------------------------------------
		private ProxySettings GetProxySetting()
		{
			return loginManager.GetProxySettings();
		}

		//---------------------------------------------------------------------
		private void SetProxySetting(ProxySettings proxySettings)
		{
			loginManager.SetProxySettings(proxySettings);
		}		
		# endregion
		
		/// <summary>
		/// Dispose
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose (bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (lockToken != null)
					lockToken.Dispose();
			}
			if (taskExecutionEngine != null)
				taskExecutionEngine.Dispose();
			base.Dispose(disposing);
		}
	}
}
