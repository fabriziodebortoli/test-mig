using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microarea.EasyBuilder;
using Microarea.EasyBuilder.Packager;
using Microarea.Library.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.NotificationManager;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Core.WebSockets;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Licence;
using Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls;
using Microarea.TaskBuilderNet.UI.WinControls;
using Microarea.TaskBuilderNet.UI.WinControls.Splashes;
using Microarea.TaskBuilderNet.UI.WinControls.AdvertisementRenderer;
using Microsoft.Win32;
using WeifenLuo.WinFormsUI.Docking;
using System.Net;

namespace Microarea.MenuManager
{
    /// <summary>
    /// Summary description for MenuMngForm.
    /// </summary>
    //============================================================================
    public partial class MenuMngForm : Form
	{
		private static string currentConfiguration = String.Empty;
		private static string token = string.Empty;
		private static int tbPort = -1;
		private static bool runWithoutMenu;
		private static bool clearCachedData;
        private static bool newMenu = true;
        private static int newMenuLocalPort = 0;
        

        private TbApplicationClientInterface tbAppClientInterface;
		private LoginManager currentLoginManager;
		private bool bLoginFailed;

		private string currentUser = String.Empty;
		private string currentCompany = String.Empty;
		private string currentPassword = String.Empty;
		private bool   currentWinNTAuthentication;
		private string currentPreferredLanguage = String.Empty;
		private string currentApplicationLanguage = String.Empty;
		private string installationName = String.Empty;

		private string userPaneString;

		private bool confirmClose = true;
		public bool closing;
		public bool starting;
        
        private bool reinitializingUI;
		private Point currentLocation;
		private Size currentClientSize;

		private IDiagnostic tbDiagnostic;
		private IDiagnostic maServerDiagnostic;
		private System.Timers.Timer messageTimer = new System.Timers.Timer();
		private ToolStripControlHost messagesToolStripControlHost;
		private PictureWithBalloon messagePictureBox;
		private AdvRendererManager advRendererManager;
		private Object lockTicket = new Object();
		private MenuEditorDockableForm menuDesignerForm;
		private ManualResetEvent changeLoginResetEvent = new ManualResetEvent(false);

		private bool isMainForm;
		private ApplicationLockToken lockToken;
		
		private bool? tabbedDocuments;
		string mainWindowTitle = null;
	
		private MenuBrowserDockableForm menuBrowserForm;

		private NotificationManager NotificationManager;
		public NotificationMenuItem NotificationBtn;

		PathFinder pathFinder;


        [DllImport("SHCore.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        private enum PROCESS_DPI_AWARENESS
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetProcessDPIAware();

        //------------------------------------------------------------------------------------------------------------------------------
        internal bool TabbedDocuments
		{
			get
			{
				if (tabbedDocuments == null)
				{
					PathFinder pf = new PathFinder("", "");// (PathFinder)tbAppClientInterface.PathFinder;
					SettingItem si = pf.GetSettingItem("Framework", "TBGenlib", "Forms", "TabbedDocuments");
					tabbedDocuments = si != null && si.Values[0].Equals("1");
				}
				return (bool)tabbedDocuments;

			}
		}
         static string iparam = null;
        static string itoken = null;
        static string tbLoaderParams = null;
        //---------------------------------------------------------------------
        //Iparam=imago://Document.ERP.Contacts.Documents.Contacts&IToken=AcxzmyscwozcilikjY2VacDGlZBzMXW9xfMnlSNPM7$u7ERcz2ZfruabqEBvItgmHAC6DlWo8OHFmvwNto4dAqd4LT9aT$ZGqn$KmLGiS97fq1TPAKqGJVBf9Ea3s64rYhiybDWsNzfTpbty4NRkkSSaBYXflk5zlH2m1tLQrnDmEtfVonHF7NOg82ewEYrMDy47t9AbG11Td6vkc
        private static void ParseArguments(string[] args)
		{
			currentConfiguration = String.Empty;

			foreach (string arg in args)
			{
				int equalPos = arg.IndexOf('=');
                if (equalPos > 0)
                {
                    try
                    {
                        string argumentValue = arg.Substring(equalPos + 1);
                        if (String.Compare(arg.Substring(0, equalPos), NameSolverStrings.TbConfiguration, true, CultureInfo.InvariantCulture) == 0)
                            currentConfiguration = argumentValue;

                        if (String.Compare(arg.Substring(0, equalPos), NameSolverStrings.TBPort, true, CultureInfo.InvariantCulture) == 0)
                            tbPort = Int32.Parse(argumentValue);

                        if (String.Compare(arg.Substring(0, equalPos), NameSolverStrings.AuthenticationToken, true, CultureInfo.InvariantCulture) == 0)
                            token = argumentValue;

                        if (String.Compare(arg.Substring(0, equalPos), NameSolverStrings.RunWithoutMenu, true, CultureInfo.InvariantCulture) == 0)
                            bool.TryParse(argumentValue, out runWithoutMenu);

                        if (String.Compare(arg.Substring(0, equalPos), NameSolverStrings.ClearCache, true, CultureInfo.InvariantCulture) == 0)
                            bool.TryParse(argumentValue, out clearCachedData);

                        //TODOLUCA, rimuovere una volta che il nuovo menu è stabile
                        if (String.Compare(arg.Substring(0, equalPos), "NewMenu", true, CultureInfo.InvariantCulture) == 0)
                            bool.TryParse(argumentValue, out newMenu);

                        if (String.Compare(arg.Substring(0, equalPos), "NewMenuLocalPort", true, CultureInfo.InvariantCulture) == 0)
                            int.TryParse(argumentValue, out newMenuLocalPort);
                        
                        if (String.Compare(arg.Substring(0, equalPos), "Iparam", true, CultureInfo.InvariantCulture) == 0)
                        {
                            //HACK: abbiamo verificato che nel passaggio di parametri ad un eseguibile di click once avviene un escape della stringa che sostituisce tutti i / in \\.
                            //Siccome per noi il parametro tb:// deve essere un uri valido allora lo riconvertiamo qui da tb:\\ in tb://
                            iparam = argumentValue.Replace("\\", "/").IsNullOrWhiteSpace() ? null : argumentValue;
                            // Debug.Fail("iparam:" + iparam);
                        }
                        if (String.Compare(arg.Substring(0, equalPos), "Itoken", true, CultureInfo.InvariantCulture) == 0)
                        {
                            itoken = argumentValue.IsNullOrWhiteSpace() ? null : argumentValue;
                            //Debug.Fail("itoken:" + itoken);
                        }
                        if (String.Compare(arg.Substring(0, equalPos), "tbLoaderParams", true, CultureInfo.InvariantCulture) == 0)
                        {
                            tbLoaderParams = argumentValue.IsNullOrWhiteSpace() ? null : argumentValue;
                        }
                    }
                    catch (Exception exc)
                    {
                        Debug.Fail("Error parsing arguments passed to MenuManager Main function: " + exc.Message);
                    }
                }
                else if (arg.StartsWith("TB://", StringComparison.InvariantCultureIgnoreCase))//oldmode
                    iparam = arg;


            }
		}


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		//--------------------------------------------------------------------------------
		[STAThread]
		static void Main(string[] args)
		{
            try
            {
                SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);
            }
            catch
            {
                try
                { // fallback, use (simpler) internal function
                    SetProcessDPIAware();
                }
                catch { }
            }

            Application.EnableVisualStyles();
            string[] myArgs = args;
            if (AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData != null
                && AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData.Length > 0)
            {
                myArgs = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
                myArgs = myArgs[0].Split('&');

            }
            else if (myArgs!= null && myArgs.Length>0 )
            {
                myArgs = myArgs[0].Split('&');
            }
            ParseArguments(myArgs);

			MenuMngForm myMenuManagerForm = null;

			try
			{
#if !DEBUG
				//tapullo temporaneo per ovviare ad un buco del tooltip (buco
				//del framework, non nostro) che non gestisce le cross thread operation
				//quando un tooltip attivo cerca di spostarsi su un controllo di un altro thread
				//esempio: vado sulla tab (thread di menumanager) e aspetto il tooltip
				//subito dopo sposto il mouse sull'icona del documento
				//(thread di documento, diverso) e il tooltip corrente cerca di 
				//adattarsi al nuovo controllo, ne chiede l-Handle e badabum!
				Control.CheckForIllegalCrossThreadCalls = false;
#endif
				if (clearCachedData)
				{
					string folder = BasePathFinder.BasePathFinderInstance.GetAppDataPath(false);
					if (Directory.Exists(folder))
						Directory.Delete(folder, true);
				}
                
				Microarea.TaskBuilderNet.Core.Generic.InstallationInfo.TestInstallation();
				myMenuManagerForm = new MenuMngForm(true);
          
				if (!myMenuManagerForm.IsDisposed) //potrebbe autochiudersi per ragioni varie (attivazione, database incopmleto, ecc.)
				{
					Thread.CurrentThread.Name = "MenuManager main thread";
					Application.Run(myMenuManagerForm);
				}
			}
			catch (ApplicationSemaphoreException exception)
			{
				MessageBox.Show(exception.Message, MenuManagerStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception exception)
			{
				if (myMenuManagerForm != null)
				{
					myMenuManagerForm.closing = true;

					//chiudo la splash e ridò il fuoco a myMenuManagerForm
					if (myMenuManagerForm.isMainForm)
						SplashStarter.Finish();
				}
				else
					SplashStarter.Finish();

				MessageBox.Show(String.Format(MenuManagerStrings.RunFormExceptionCloseAppWarningMsg, exception.Message));
			}
			finally
			{
				//must wait for TBLoad thread to terminate
				lock (typeof(TbApplicationClientInterface))
				{
					TbApplicationClientInterface.CloseTBApplication();
				}
			}
		}

		//--------------------------------------------------------------------------------
		public MenuMngForm(bool isMainForm)
		{
            this.isMainForm = isMainForm;
			//inizializza l'oggetto MenuMngForm (compreso il lock dell'applicazione per motivi legati al setup).
			InitializeApplication();

            try
            {
                WriteOnRegistryTBLauncherManagerKeys();
            }
            catch { }
            Application.Idle += Application_Idle;
		}

        //--------------------------------------------------------------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            

            //starts tb in a separate thread
            IntPtr handle = Handle;

            if (!StartTB(handle))
            {
                SplashStarter.Finish();
                CloseForm();
                return;
            }

            //verifica itoken e iparam
            //per imago qui bisogna gestire gli errori di login, il fatto che l'utente sia già connesso è gestito in automatico per imago con sovrascrittura automatica.
            //gli altri bisogna gestirli qui perchè se l'utente è già associato e viene fatta la login automatica trasparente non c'è la gestione dei messaggi della gestione classica html+js
            if (itoken != null  && currentLoginManager!= null && currentLoginManager.IsActivated("Erp", "imago"))
            {
               int result =  InternalSSOLogin(itoken);
                if (result == (int)LoginReturnCodes.ImagoUserAlreadyAssociated)
                {
                    DiagnosticViewer.ShowDiagnosticAndClear(maServerDiagnostic as Diagnostic);
                 
                }
                if (result == (int)LoginReturnCodes.InvalidSSOToken)
                {
                    DiagnosticViewer.ShowDiagnosticAndClear(maServerDiagnostic as Diagnostic);
                    return;//blocco, non si può fare nulla, è arrivato un token invalido
                }
            }

			//MOD Marco
			if (token == string.Empty)
				RunWebBrowser(itoken, iparam);

			else
			{

				if (!currentLoginManager.GetLoginInformation(token))
					Debug.Fail("GetLoginInformation fallita");
				// devo verificare che la cal associata a tale token sia una cal named o floating
				//per non permettere login da token di altro genere.
				if (currentLoginManager.ConfirmToken(token, ProcessType.MenuManager))
					OnLogged(token);

                if (!currentLoginManager.IsActivated("RanorexTestSupport", "RanorexTestSupport"))
                {
                    if (iparam != null)
                    {
                        MenuMngForm_TbNavigate(new TbNavigateEventArgs(iparam));
                        this.Activate();
                    }
                    else
                        RunWebBrowser();
                }
                //@@TODO in questo momento se c'èil Ranorex il menu web non parte, serve il vecchio
			}

            MenuMngDocumentContainer.WindowListZero += MenuMngDocumentContainer_WindowListZero;//todo non lo attacca nella runwebbrowser perchè di qui non la fa

            ////chiudo la splash
            if (isMainForm)
                SplashStarter.Finish();
        }


        //----------------------------------------------------------------------
        internal bool ValidateDomainCredentials(string user, string company)
        {
            try
            {
                //prendo i dati di user attualmente connesso a windows
                string fullname;
                string loginName = SystemInformation.UserName;
                string userDomainName = SystemInformation.UserDomainName;

                if (userDomainName.Length == 0)
                    fullname = Dns.GetHostName() + Path.DirectorySeparatorChar + loginName;
                else
                    fullname = userDomainName + Path.DirectorySeparatorChar + loginName;

                if (String.Compare(fullname, user, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return ExistCompanies(fullname, company);
                return false;

            }
            catch (Exception exc)
            {
                maServerDiagnostic.Set(DiagnosticType.Error, exc.ToString());
                return false;
            }
        }
        //---------------------------------------------------------------------
        private bool ExistCompanies(string user, string company = null)
        {
            string[] companies;
            try { companies = currentLoginManager.EnumCompanies(user); }
            catch (Exception exc)
            {
                maServerDiagnostic.Set(DiagnosticType.Error, exc.ToString());
                return false;
            }
            if (company.IsNullOrWhiteSpace()) //se non ho specificato nessuna company torno la sola esistenza di almeno una company
                return companies != null && companies.Length > 0;
            foreach (string s in companies)//altrimenti verifico che la company passata esista nella lista.
                if (string.Compare(company, s, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return true;
            return false;
        }

        //-----------------------------------------------------------------------
        private int InternalSSOLogin(string cryptedtoken)
        {
            //Eseguo la login sul server da cui ricevo la stringa di connessione al database
            int loginResult;
            try
            {
                loginResult = currentLoginManager.LoginViaInfinityToken(cryptedtoken, null, null, null);
                if (loginResult == 0)
                {
                    currentLoginManager.GetLoginInformation(currentLoginManager.AuthenticationToken);
                    currentLoginManager.IsWinNT(currentLoginManager.LoginId);
                    try {
                       
                        token = currentLoginManager.AuthenticationToken;


                        //qui controllo la login in sicurezza integrata
                        if (currentLoginManager.WinNTAuthentication && !ValidateDomainCredentials(currentLoginManager.UserName, currentLoginManager.CompanyName))
                        {
                            maServerDiagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, WebServicesWrapperStrings.ErrInvalidUser);
                            loginResult = (int)LoginReturnCodes.InvalidUserError;
                            currentLoginManager.LogOff(token);
                            return loginResult;
                        }
                        int loginid, companyid;
                        string userName, companyName;

                        bool bWebCompany;
                        // Chiamo la reload del menu
                        currentLoginManager.GetAuthenticationInformations(token, out loginid, out companyid, out bWebCompany);
                        currentLoginManager.GetAuthenticationNames(token, out userName, out companyName);

                        PathFinder pf = new PathFinder(companyName, userName);
                        string LogPath = pf.GetCustomCompanyPath() + "\\Log\\Imago.log";
                        System.Xml.XmlDocument json = NewMenuLoader.GetMenuXmlInfinity(
                                       userName,
                                       companyName,
                                       currentLoginManager.GetSystemDBConnectionString(token),
                                       LogPath ,
                                       loginid.ToString(),
                                       companyid.ToString());
                       
                        if (json != null && !json.InnerXml.IsNullOrEmpty())
                        {

                            using (MagoMenuParser pMenu = new MagoMenuParser(
                                 currentLoginManager.GetSystemDBConnectionString(currentLoginManager.AuthenticationToken),
                                  json.InnerXml,
                                  loginid.ToString(),
                                  companyid.ToString(),
                                  userName,
                                  companyName,
                                  "InternalSSOLogin"
                                  ))
                            {
                                bool bParse = pMenu.Parse();
                            }
                        }
                    }
                    catch (Exception e )
                    {
                        maServerDiagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "Unable to parse I.Mago Menu: " + e.Message);
                    }


                    return loginResult;
                }
                
            }
            catch (Exception exc)
            {
                maServerDiagnostic.Set(DiagnosticType.Error, string.Format(WebServicesWrapperStrings.ServerDown, exc.ToString()));
                return (int)LoginReturnCodes.Error;
            }

            switch (loginResult)
            {
                case (int)LoginReturnCodes.NoError:
                    return loginResult;

                case (int)LoginReturnCodes.AlreadyLoggedOnDifferentCompanyError:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.AlreadyLoggedOnDifferentCompanyError);
                    return loginResult;

                case (int)LoginReturnCodes.UserAlreadyLoggedError:
                    return loginResult;
                case (int)LoginReturnCodes.WebUserAlreadyLoggedError:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrWebUserAlreadyLogged);
                    return loginResult;

                case (int)LoginReturnCodes.NoCalAvailableError:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.NoCalAvailableError);
                    return loginResult;

                case (int)LoginReturnCodes.NoLicenseError:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrNoArticleFunctionality);
                    return loginResult;

                case (int)LoginReturnCodes.UserAssignmentToArticleFailure:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrUserAssignmentToArticle);

                    return loginResult;

                case (int)LoginReturnCodes.UserNotAllowed:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.UserNotAllowed);
                    return loginResult;


                case (int)LoginReturnCodes.ProcessNotAuthenticatedError:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrProcessNotAuthenticated);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidUserError:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrInvalidUser);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidProcessError:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrInvalidProcess);
                    return loginResult;

                case (int)LoginReturnCodes.LockedDatabaseError:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrLockedDatabase);
                    return loginResult;

                case (int)LoginReturnCodes.UserMustChangePasswordError:
                   // diagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrUserCannotChangePwdButMust);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidCompanyError:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrInvalidCompany);
                    return loginResult;

                case (int)LoginReturnCodes.ProviderError:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrProviderInfo);
                    return loginResult;

                case (int)LoginReturnCodes.ConnectionParamsError:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrConnectionParams);
                    return loginResult;

                case (int)LoginReturnCodes.CompanyDatabaseNotPresent:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.CompanyDatabaseNotPresent);
                    return loginResult;

                case (int)LoginReturnCodes.CompanyDatabaseTablesNotPresent:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.CompanyDatabaseTablesNotPresent);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidDatabaseForActivation:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.InvalidDatabaseForActivation);
                    return loginResult;

                case (int)LoginReturnCodes.WebApplicationAccessDenied:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.WebApplicationAccessDenied);
                    return loginResult;

                case (int)LoginReturnCodes.GDIApplicationAccessDenied:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.GDIApplicationAccessDenied);
                    return loginResult;

                case (int)LoginReturnCodes.LoginLocked:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.LoginLocked);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidDatabaseError:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.InvalidDatabaseError);
                    return loginResult;

                case (int)LoginReturnCodes.NoAdmittedCompany:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.NoAdmittedCompany);
                    return loginResult;

                case (int)LoginReturnCodes.NoOfficeLicenseError:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.NoOfficeLicenseError);
                    return loginResult;

                case (int)LoginReturnCodes.TooManyAssignedCAL:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.TooManyAssignedCAL);
                    return loginResult;

                case (int)LoginReturnCodes.NoDatabase:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.CompanyDatabaseNotPresent);
                    return loginResult;

                case (int)LoginReturnCodes.NoTables:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.CompanyDatabaseTablesNotPresent);
                    return loginResult;

                case (int)LoginReturnCodes.NoActivatedDatabase:
                    if (Functions.IsDebug() && currentLoginManager.IsDeveloperActivation())
                        break;

                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.InvalidDatabaseForActivation);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidModule:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.InvalidDatabaseError);
                    return loginResult;

                case (int)LoginReturnCodes.DBSizeError:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.DBSizeError);
                    return loginResult;

                case (int)LoginReturnCodes.SsoTokenError:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.SsoTokenError);
                    return loginResult;
                
                case (int)LoginReturnCodes.MoreThanOneSSOToken:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.MoreThanOneSSOToken);
                    return loginResult;

                case (int)LoginReturnCodes.InvalidSSOToken:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.InvalidSSOToken);
                    return loginResult;

                case (int)LoginReturnCodes.ImagoUserAlreadyAssociated:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.ImagoUserAlreadyAssociated);
                    return loginResult;

                case (int)LoginReturnCodes.SSOIDNotAssociated:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.SSOIDNotAssociated);
                    return loginResult;
                    
 case (int)LoginReturnCodes.ImagoCompanyNotCorresponding:
                    maServerDiagnostic.Set(DiagnosticType.Warning, WebServicesWrapperStrings.ImagoCompanyNotCorresponding);
                    return loginResult;
                default:
                    maServerDiagnostic.Set(DiagnosticType.Error, WebServicesWrapperStrings.ErrLoginFailed);
                    return loginResult;
            }
            
            return (int)LoginReturnCodes.Error;
        }

        CancellationTokenSource cts = new CancellationTokenSource();
		//--------------------------------------------------------------------------------
		void Application_Idle(object sender, EventArgs e)
		{
			Application.Idle -= Application_Idle;

			CancellationToken ct = cts.Token;

			Task.Factory.StartNew(() => InstallOrUpdatePayloads(ct), ct);
		}

		//--------------------------------------------------------------------------------
		void InstallOrUpdatePayloads(CancellationToken ct)
		{
			string payloadFile = null;

			try
			{
				Payload payload = null;
				string[] payloadFiles = Directory.GetFiles(AssemblyDirectory, "*.payload");
				for (int i = 0; i < payloadFiles.Length; i++)
				{
					if (ct.IsCancellationRequested)
						return;

					payloadFile = payloadFiles[i];

					payload = new Payload(payloadFile);

					if (!payload.IsValid)
					{
						maServerDiagnostic.Set(DiagnosticType.Error | DiagnosticType.LogInfo, String.Format("{0} - Payload {1} is not valid", installationName, payloadFile));
						continue;
					}

					payload.InstallOrUpdate(AssemblyDirectory);
				}
			}
			catch (Exception exc)
			{
				if (payloadFile != null)
					maServerDiagnostic.Set(DiagnosticType.Error | DiagnosticType.LogInfo, String.Format("{0} - Error installing {1} payload: {2}", installationName, payloadFile, exc.ToString()));
			}
		}

		//--------------------------------------------------------------------------------
		public static string AssemblyDirectory
		{
			get
			{
				return Functions.GetAssemblyPath(Assembly.GetExecutingAssembly());
			}
		}

        Image splashImage = null;
        //--------------------------------------------------------------------------------
        private void InitializeApplication()
		{
            //Ilaria/Luca: trick per far apparire subito prima della splash una finestra di attesa in caso non avessimo già disponibile il nome della solution master
            SplashManager.TemporarySplashForBrandLoading();

            maServerDiagnostic = new Microarea.TaskBuilderNet.Core.DiagnosticManager.Diagnostic(Diagnostic.EventLogName);//maserver generico

			// Impostazioni iniziali relative alla lingua: esse cambieranno, divenendo
			// definitive, solo dopo aver effettuato la login 
			currentPreferredLanguage = InstallationData.ServerConnectionInfo.PreferredLanguage;
			currentApplicationLanguage = InstallationData.ServerConnectionInfo.ApplicationLanguage;

			// l'impostazione della culture va effettuata prima di chiamare la InitializeComponent
			// (altrimenti la form non viene tradotta)
			DictionaryFunctions.SetCultureInfo(currentPreferredLanguage, currentApplicationLanguage);


			if (isMainForm) //per le login successive non devo mostrare splash
			{
                splashImage = InstallationData.BrandLoader.GetMenuManagerSplash();
                if (splashImage != null)
                    SplashStarter.Start(splashImage);
			}

			//Lock dell'applicazione per verificare e impedire eventualmente l'utilizzo di Mago durante un setup;
			lockToken = ApplicationSemaphore.Lock(BasePathFinder.BasePathFinderInstance.GetSemaphoreFilePath());

			currentLoginManager = new LoginManager();

			installationName = BasePathFinder.BasePathFinderInstance.Installation;

			StringLoader.EnableDictionaryCaching();

			// Required for Windows Form Designer support
			InitializeComponent();

			DefaultTheme_ThemeChanged(null, EventArgs.Empty);
			DefaultTheme.ThemeChanged += DefaultTheme_ThemeChanged;

			if (isMainForm)
			{
				ProductActivator activator = new ProductActivator(currentLoginManager);
                activator.Pee += Activator_Pee;
                activator.StopPee += Activator_StopPee;
                //prima di chiudere la splash mi calcolo i due bool
                bool activationOK = activator.ProductActivated();
				bool serverConnectionFileExists = File.Exists(InstallationData.ServerConnectionInfo.ServerConnectionFile);

				// se non ho ancora creato il ServerConnection.config  oppure c'è ma è incompleto (normale caso di installazione che salva il file solo con le lingue)
				// mostro il wizard di Quick Start, che e' in grado anche di attivare
				if (!File.Exists(InstallationData.ServerConnectionInfo.ServerConnectionFile) || String.IsNullOrEmpty(InstallationData.ServerConnectionInfo.SysDBConnectionString))
				{
					QuickStartWizard.QuickStartManager qsManager = new QuickStartWizard.QuickStartManager();
					// chiudo la splash
					SplashStarter.Finish();
					if (!qsManager.RunQuickStartWizard(currentLoginManager, this))
					{
						CloseForm();
						return; // ritorna false quando clicco su Cancel
					}
				}

				// se il prodotto non e' attivato propongo la finestra di attivazione (nel caso in cui sia saltata l'attivazione e ci sia un ServerConnection.config valido)
				if (!activator.ProductActivated() && !activator.ShowAboutAndActivate(this))
				{
					SplashStarter.Finish();
					CloseForm();
					return; // se l'attivazione non va a buon fine o l'utente ha cliccato su Cancel non procedo.
				}
			}

			AssignFormResources();

			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MenuMngForm_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MenuMngForm_DragEnter);

			this.CenterToScreen();
			currentLocation = this.Location;
        }


        //---------------------------------------------------------------------
        private void Activator_Pee(object sender, EventArgs e)
        {
            SplashStarter.Finish();
        }

        //---------------------------------------------------------------------
        private void Activator_StopPee(object sender, EventArgs e)
        {
            SplashStarter.Start(splashImage);
        }

        //---------------------------------------------------------------------
        void DefaultTheme_ThemeChanged(object sender, EventArgs e)
		{
			this.MenuMngDocumentContainer.Skin = new DockPanelSkin();
			this.MenuMngDocumentContainer.Skin.DockPaneStripSkin = new DockPaneStripThemeSkin();
			this.MenuMngDocumentContainer.Skin.DockPaneStripSkin.OverrideTheme();
		}

		//---------------------------------------------------------------------
		private void SetBalloonStatusStrip()
		{
			if (currentLoginManager == null || currentLoginManager.AuthenticationToken.IsNullOrWhiteSpace())
			{
				return;
			}
			bool messagePictureBoxCreated = false;
			if (this.messagePictureBox == null || this.messagePictureBox.IsDisposed)
			{
				this.messagePictureBox = new PictureWithBalloon();
				messagePictureBoxCreated = true;
				this.messagePictureBox.Name = "messagePictureBox";
			}
            this.messagePictureBox.Location = new Point(this.Size.Width - 42, this.Size.Height - 97);//lo mettiamo in basso a des
            this.messagePictureBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.messagePictureBox.Size = new Size(24, 24);
			this.messagePictureBox.Visible = false;
			this.messagePictureBox.BringToFront();

			if (!this.Controls.Contains(this.messagePictureBox))
				this.Controls.Add(this.messagePictureBox);

			if (messagePictureBoxCreated)
			{
				if (this.messagesToolStripControlHost != null)
				{
					if (!this.messagesToolStripControlHost.IsDisposed)
						this.messagesToolStripControlHost.Dispose();

					messagesToolStripControlHost = null;
				}
			}

			if (this.messagesToolStripControlHost == null || this.IsDisposed)
			{
				this.messagesToolStripControlHost = new ToolStripControlHost(messagePictureBox, "messagesToolStripStatusLabel");
				this.messagesToolStripControlHost.AutoSize = false;
				this.messagesToolStripControlHost.Size = new Size(50, 20);
				this.messagesToolStripControlHost.Visible = false;
			}

			if (!MenuFormStatusStrip.Items.Contains(this.messagesToolStripControlHost))
				MenuFormStatusStrip.Items.Add(this.messagesToolStripControlHost);
		}

		//---------------------------------------------------------------------
		private void DestroyBalloonStatusStrip()
		{
			if (this.messagePictureBox != null)
			{
				if (!this.messagePictureBox.IsDisposed)
					this.messagePictureBox.Dispose();

				if (this.Controls.Contains(this.messagePictureBox))
					this.Controls.Remove(this.messagePictureBox);

				this.messagePictureBox = null;
			}

			if (messagesToolStripControlHost != null)
			{
				if (!this.messagesToolStripControlHost.IsDisposed)
					this.messagesToolStripControlHost.Dispose();

				if (MenuFormStatusStrip.Items.Contains(this.messagesToolStripControlHost))
					MenuFormStatusStrip.Items.Remove(this.messagesToolStripControlHost);

				this.messagesToolStripControlHost = null;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool MultipleLoginAvailable
		{
			get
			{
				PathFinder pf = tbAppClientInterface.PathFinder as PathFinder;

				if (pf != null && pf.SingleThreaded)
					return false;

				SettingItem i = (tbAppClientInterface != null && pf != null)
					? pf.GetSettingItem("Framework", "TbGenlib", "Environment", "AllowMultipleLogins")
					: null;

				if (i == null)
					return false;

				return i.Values[0].Equals("1");
			}
		}

		//-------------------------------------------------------------------------------------------------------
		// Ho dovuto redefinire le propriet?di Location e ClientSize della Form:
		// Infatti, dopo aver effettuato la login, mutano le impostazioni relative alla
		// lingua (currentPreferredLanguaga) e, pertanto,
		// occorre necessariamente ricreare la form in modo che la localizzazione risulti
		// quella specificata per l'utente selezionato.
		// La InitializeComponents, richiamata dopo la login, sposta e ridimensiona la form. 
		// Le seguenti ridefinizioni delle proprietà di Location e Size evitano che ci?
		// accada.
		//-------------------------------------------------------------------------------------------------------
		public new Point Location
		{
			get
			{
				return base.Location;
			}

			set
			{
				// Devo evitare che la mia form venga riposizionata dalla InizializeComponents
				// quando sono costretta a chiamare tale funzione dopo la login (essendo
				// cambiate le lingue)
				if (reinitializingUI)
				{
					base.Location = currentLocation;
					return;
				}
				base.Location = value;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public new Size ClientSize
		{
			get
			{
				return base.ClientSize;
			}

			set
			{
                // Devo evitare che la mia form venga ridimensionata dalla InizializeComponents
                // quando sono costretta a chiamare tale funzione dopo la login (essendo
                // cambiate le lingue)

                Size formSize = value;
                if (reinitializingUI)
                {
                    formSize = currentClientSize;
                }

                // DPI screen scaling
                float dpiScale = 1;
                Graphics g = this.CreateGraphics();
                try
                {
                    dpiScale = g.DpiY / 96;
                }
                finally
                {
                    g.Dispose();
                }
                int w = (int)(formSize.Width * dpiScale);
                int h = (int)(formSize.Height * dpiScale);


                
                // Scaling DPI
                base.ClientSize = new Size(w, h);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SetCurrentLoginData(LoginManager aLoginManager)
		{
			currentLoginManager = aLoginManager;

			currentUser = (aLoginManager != null) ? aLoginManager.UserName : String.Empty;
			currentCompany = (aLoginManager != null) ? aLoginManager.CompanyName : String.Empty;
			currentPassword = (aLoginManager != null) ? aLoginManager.Password : String.Empty;
			currentWinNTAuthentication = (aLoginManager != null) ? aLoginManager.WinNTAuthentication : false;

			currentPreferredLanguage = (aLoginManager != null) ? aLoginManager.PreferredLanguage : String.Empty;
			currentApplicationLanguage = (aLoginManager != null) ? aLoginManager.ApplicationLanguage : String.Empty;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void AssignFormResources()
		{
			Icon ico = InstallationData.BrandLoader.GetTbAppManagerApplicationIcon();
			this.Icon  = (ico != null) ? ico :  MenuManagerStrings.MenuMngForm;
            //base.ClientSize = new Size(1000, 1000);
			SetBalloonStatusStrip();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void LoadMenuManagerIniSettings()
		{
			MenuFormIniSettings iniSettings = new MenuFormIniSettings();
			iniSettings.LoadFromConfiguration();

			if (iniSettings.FrameWidth > 0 && iniSettings.FrameHeight > 0)
				this.Size = new Size(iniSettings.FrameWidth, iniSettings.FrameHeight);

			//se durante il processo di caricamento ho mosso la finestra, non ripristino i settings salvati, ripristino solo lo stato massimizzato o meno
			if (this.Location == currentLocation)
			{
				WindowState = (FormWindowState)iniSettings.WindowState;
				return;
			}

			if (iniSettings.FrameXPos > 0 && iniSettings.FrameYPos > 0)
				this.Location = currentLocation = new Point(iniSettings.FrameXPos, iniSettings.FrameYPos);
			else
				this.CenterToScreen();

			WindowState = (FormWindowState)iniSettings.WindowState;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateUIAfterLogin()
		{
			MenuMngDocumentContainer.Init(tbAppClientInterface, this);
			
			mainWindowTitle = Text; //preserve old title (may have been changed by business logic)
          
			reinitializingUI = false;

			//Text = currentLoginManager.GetMasterProductBrandedName();//InstallationData.BrandLoader.GetBrandInfo().ProductTitle;

			StartMessagesTimer();
			LoadMenuManagerIniSettings();
		
			AssignFormResources();

			Text = mainWindowTitle;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EstablishConnectionToTB()
		{
			try
			{
				//must wait for TBLoad thread to terminate
				lock (typeof(TbApplicationClientInterface))
				{
					InitCurrentTBProcess();

					if (clearCachedData)
						tbAppClientInterface.ClearCache();
				}
			}
			catch (TbLoaderClientInterfaceException exception)
			{
				bLoginFailed = exception.LoginFailed;
				DisplayThreadSafeMessageBox(MenuManagerStrings.ConnectionToTBFailedMsg + " " + exception.ExtendedMessage);
			}
			catch (Exception e)
			{
				DisplayThreadSafeMessageBox(MenuManagerStrings.ConnectionToTBFailedMsg + " " + e.Message);
			}
			finally
			{

			}
		}

		//---------------------------------------------------------------------
		private static bool UseShellExecuteOnFileName(string fileName)
		{
			try
			{
				ProcessStartInfo pInfo = new ProcessStartInfo();
				//Set the file name member. 
				pInfo.FileName = fileName;
				pInfo.UseShellExecute = true;
				Process.Start(pInfo);
			}
			catch (Exception exception)
			{
				MessageBox.Show(String.Format(MenuManagerStrings.CommandNotFoundErrMsg, fileName, exception.Message));
				return false;
			}
			return true;
		}

		//---------------------------------------------------------------------
		private static bool ExecuteProcess(string exeFileName)
		{
			try
			{
				System.Diagnostics.Process exeProcess = new System.Diagnostics.Process();

				// Do not receive an event when the process exits.
				exeProcess.EnableRaisingEvents = false;

				exeProcess.StartInfo.FileName = exeFileName;
				// Start exe file and assign it to the process component.
				// The return value true indicates that a new process resource was started. 
				// If the process resource specified by the FileName member of the StartInfo property
				// is already running on the computer, no additional process resource is started.
				// Instead, the running process resource is reused and false is returned.

				return exeProcess.Start();
			}
			catch (InvalidOperationException exception)
			{
				// No file name was specified in the Process component's StartInfo. 
				// -or-
				// The ProcessStartInfo.UseShellExecute member of the StartInfo property is true while
				// ProcessStartInfo.RedirectStandardInput, ProcessStartInfo.RedirectStandardOutput, or
				// ProcessStartInfo.RedirectStandardError is true.
				MessageBox.Show(String.Format(MenuManagerStrings.ExecutionFailedErrMsg, exeFileName, exception.Message));
			}
			catch (Win32Exception exception)
			{
				// There was an error in opening the associated file.
				MessageBox.Show(String.Format(MenuManagerStrings.ExecutionFailedErrMsg, exeFileName, exception.Message));
			}
			return false;
		}

		//---------------------------------------------------------------------
		private bool ExecuteProcessByFileName(string exeFileName)
		{
			exeFileName = exeFileName.Replace("\r\n", " ");
			exeFileName = exeFileName.Trim();

			if (File.Exists(exeFileName))
			{
				if (Path.GetExtension(exeFileName).Equals(".exe", StringComparison.InvariantCultureIgnoreCase))
					return ExecuteProcess(exeFileName);

				return UseShellExecuteOnFileName(exeFileName);
			}

			NameSpace ns = new NameSpace(exeFileName);
			string fileName = pathFinder.GetFilename(ns, this.currentLoginManager.PreferredLanguage);

			if (File.Exists(fileName))
				return UseShellExecuteOnFileName(fileName);

			return ExecuteProcess(exeFileName); ;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private IntPtr GetThreadSafeFormHandle()
		{
			IntPtr handle = IntPtr.Zero;
			Invoke((ThreadStart)delegate { handle = this.Handle; });
			return handle;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void DisplayThreadSafeMessageBox(string messageToDisplay)
		{
			Invoke((ThreadStart)delegate { MessageBox.Show(this, messageToDisplay); });
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void InitCurrentTBProcess()
		{
			if (tbAppClientInterface == null)
            {
                if (this.maServerDiagnostic != null)
                {
                    maServerDiagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "'tbAppClientInterface' is null, cannot initialize anything");
                }
                return;
            }

			//imposto l'handle del manumanager a livello di application context, per avere i messaggi 
			//della login
			//Imposto sempre il menuHandle, anche se non siamo in TabbedDocuments, cosi l'organizer può ricevere i messaggi
			//di notifica dal c++
			tbAppClientInterface.MenuHandle = GetThreadSafeFormHandle();

			// se non ho l' ID del processo provo a vedere se ce n'è uno in ascolto 
			if (!tbAppClientInterface.Connected)
			{
				try
				{
					tbAppClientInterface.StartTbLoader(currentConfiguration, false, clearCachedData, tbPort);
				}
				catch (Exception exc)
				{
                    if (this.maServerDiagnostic != null)
                    {
                        maServerDiagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, MenuManagerStrings.InitTbLoginFailed + ": " + exc.ToString());
                    }

                    tbDiagnostic = tbAppClientInterface.GetApplicationContextDiagnostic(true);
					throw (new TbLoaderClientInterfaceException(MenuManagerStrings.InitTbLoginFailed));
				}
			}

			//invio a TB la stringa di connessione al database e i dati utente

			bLoginFailed = !tbAppClientInterface.InitTbLogin();
			tbDiagnostic = tbAppClientInterface.GetLoginContextDiagnostic(true);

			if (tbDiagnostic.TotalErrors > 0 || bLoginFailed)
			{
				maServerDiagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error,
						MenuManagerStrings.InitTbLoginFailed + Environment.NewLine +
						"'bLoginFailed' value: " + bLoginFailed + Environment.NewLine +
						(tbDiagnostic.TotalErrors > 0 ? "tbDiagnostic errors: " + tbDiagnostic.ToString() : "No errors in tbDiagnostic")
						);
				throw (new TbLoaderClientInterfaceException(MenuManagerStrings.InitTbLoginFailed));
			}

			//faccio l'append dei messaggi dell'application context
			//tbDiagnostic = tbAppClientInterface.GetApplicationContextDiagnostic(true);
			tbDiagnostic.Set(tbAppClientInterface.GetApplicationContextDiagnostic(true));

			if (!tbAppClientInterface.Connected || !tbAppClientInterface.Valid || tbDiagnostic.TotalErrors > 0)
            {
                if (this.maServerDiagnostic != null)
                {
                    maServerDiagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error,
                        MenuManagerStrings.InitTbLoginFailed + Environment.NewLine +
                        "'tbAppClientInterface.Connected' value: " + tbAppClientInterface.Connected + Environment.NewLine +
                        "'tbAppClientInterface.Valid' value: " + tbAppClientInterface.Valid + Environment.NewLine +
                        (tbDiagnostic.TotalErrors > 0 ? "tbDiagnostic errors: " + tbDiagnostic.ToString() : "No errors in tbDiagnostic")
                        );
                }
                throw (new TbLoaderClientInterfaceException(MenuManagerStrings.InitTbLoginFailed));
            }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void LogOff(string user, string company)
		{
			if (currentLoginManager != null)
				currentLoginManager.LogOff();

			currentLoginManager = null;
		}

		//----------------------------------------------------------------------------
		private void ShowHelpViewer()
		{
			HelpManager.CallOnlineHelp("RefGuide." + "Menu", currentPreferredLanguage);
		}

		////----------------------------------------------------------------------------
		//private void ChangeLoginInternal(bool unattended = false)
		//{
		//	if (!Monitor.TryEnter(typeof(MenuMngForm)))
		//		return;
		//	try
		//	{
		//		if (!tbAppClientInterface.CanChangeLogin(false))
		//		{
		//			MessageBox.Show(this, MenuManagerStrings.CannotChangeLoginMessageText, MenuManagerStrings.ChangeLoginErrorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
		//			return;
		//		}

		//		ReLoginDialog aReLoginDialog = new ReLoginDialog
		//			(
		//			currentUser,
		//			currentCompany,
		//			currentPassword);

		//		if (!unattended)
		//		{
		//			if (aReLoginDialog.ShowDialog(this) != DialogResult.OK)
		//			{
		//				//se l'utente è già stato sottoposto a logoff si ritroverà in uno stato catatonico....
		//				return;
		//			}
		//		}
		//		else
		//		{
		//			//Siccome è stata richiesta una re-login in unattended mode, dopo aver inizializzato
		//			//il ReLoginControl devo impostare nuovamente la password.
		//			//Infatti il ReLoginControl non ripresenta il campo Password compilato se la richiesta
		//			//di login non è unattended.
		//			aReLoginDialog.ReLoginControl.Init(true, false, currentUser, currentPassword, currentCompany);
		//			aReLoginDialog.ReLoginControl.Password = currentPassword;
		//		}

		//		string oldAuthenticationToken = currentLoginManager.AuthenticationToken;

		//		currentLoginManager.LogOff();

		//		if (!aReLoginDialog.Login())	//fallisce la login
		//		{
		//			ChangeLogin();
		//			return;
		//		}

		//		this.Cursor = Cursors.WaitCursor;
		//		this.Enabled = false;

		//		StopMessagesTimer();

		//		CompanyToolStripStatusLabel.Text = String.Empty;
		//		CompanyToolStripStatusLabel.Image = null;

		//		UserToolStripStatusLabel.Text = String.Empty;
		//		UserToolStripStatusLabel.Image = null;

		//		MenuFormStatusStrip.InfoText = MenuManagerStrings.ChangeLoginStatusBarMsg;

		//		CloseMenuWeb();

		//		userPaneString = MenuManagerStrings.UserStatusBarPanelText + currentUser;
		//		DateTime appDate = tbAppClientInterface.GetApplicationDate();
		//		int tbLoaderChangeLoginReturnCode = 0;
		//		Functions.DoParallelProcedure(() =>
		//		{
		//			tbLoaderChangeLoginReturnCode = tbAppClientInterface.ChangeLogin
		//				(
		//				oldAuthenticationToken,
		//				aReLoginDialog.AuthenticationToken,
		//				pathFinder = new PathFinder(aReLoginDialog.Company, aReLoginDialog.User),
		//				true
		//				);

		//			if (tbLoaderChangeLoginReturnCode == 0)
		//				tbAppClientInterface.SetApplicationDate(appDate);
		//		});

		//		if (tbLoaderChangeLoginReturnCode != 0)
		//		{
		//			ChangeLogin();
		//			return;
		//		}

		//		SetCurrentLoginData(aReLoginDialog.LoginManager);

		//		// l'impostazione della culture va effettuata prima di caricare il menù, ma dopo
		//		// aver chiamato la SetCurrentLoginData
		//		DictionaryFunctions.SetCultureInfo(currentPreferredLanguage, currentApplicationLanguage);

		//		RunWebBrowser();

		//		UpdateUIAfterLogin();
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show(this, ex.Message);
		//	}
		//	finally
		//	{
		//		this.Enabled = true;
		//		this.Cursor = Cursors.Default;
		//		StartMessagesTimer();
		//		Monitor.Exit(typeof(MenuMngForm));
		//		changeLoginResetEvent.Set();
		//	}
		//}
		//----------------------------------------------------------------------------
		private void ChangeLoginInternal(string token)
		{
			if (!Monitor.TryEnter(typeof(MenuMngForm)))
				return;
			try
			{

				string oldAuthenticationToken = currentLoginManager.AuthenticationToken;
				currentLoginManager.LogOff();
				currentLoginManager.GetLoginInformation(token);
				SetCurrentLoginData(currentLoginManager);
				StopMessagesTimer();

				DateTime appDate = tbAppClientInterface.GetApplicationDate();
				int tbLoaderChangeLoginReturnCode = 0;
				Functions.DoParallelProcedure(() =>
				{
					tbLoaderChangeLoginReturnCode = tbAppClientInterface.ChangeLogin
						(
						oldAuthenticationToken,
						token,
						pathFinder = new PathFinder(currentLoginManager.CompanyName, currentLoginManager.UserName),
						true
						);

					if (tbLoaderChangeLoginReturnCode == 0)
						tbAppClientInterface.SetApplicationDate(appDate);

					tbAppClientInterface.SetDocked(true);
				});

				if (tbLoaderChangeLoginReturnCode != 0)
				{
					//todo ilaria error?
					return;
				}

				// l'impostazione della culture va effettuata prima di caricare il menù, ma dopo
				// aver chiamato la SetCurrentLoginData
				DictionaryFunctions.SetCultureInfo(currentPreferredLanguage, currentApplicationLanguage);
                if (menuBrowserForm != null)
                    menuBrowserForm.ApplyResources();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message);
			}
			finally
			{
				this.Enabled = true;
				this.Cursor = Cursors.Default;
				StartMessagesTimer();
				Monitor.Exit(typeof(MenuMngForm));
				changeLoginResetEvent.Set();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool StartTB(IntPtr handle)
		{
			ITBApplicationProxy proxy = null;
			//Acquires a lock so as to prevent other threads to access
			//TBApplication until it has finished start up
			lock (typeof(TbApplicationClientInterface))
			{
				proxy = TbApplicationClientInterface.StartTBLoader
					(
					GetWcfBinding(),
					tbPort,
					BasePathFinder.BasePathFinderInstance.GetTBLoaderPath(),
					currentConfiguration,
					null,
					handle,
					false,
					false,
					false,
                    tbLoaderParams
					);
			}

			IDiagnostic diagnostic = null;
			if (proxy == null)
			{
				diagnostic = new Microarea.TaskBuilderNet.Core.DiagnosticManager.Diagnostic("StartTbloaderTask");
				diagnostic.SetError(MenuManagerStrings.ErrorStartingTb);
				DiagnosticViewer.ShowDiagnostic(diagnostic, OrderType.None);
				return false;
			}

			if (!proxy.Valid)
			{
				diagnostic = proxy.GetGlobalDiagnostic(true);
				if (diagnostic != null && (diagnostic.TotalErrors > 0 || diagnostic.TotalWarnings > 0 || diagnostic.TotalInformations > 0))
				{
					DiagnosticViewer.ShowDiagnostic(diagnostic, OrderType.None);
					return false;
				}
			}

			return true;
		}

        private bool logging = false;
		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			try
			{
				if (m.Msg == ExternalAPI.UM_IMMEDIATE_BALLOON)
				{
					//Chiamata asincrona per non fare chiamate web all'interno della WndProc.
					BeginInvoke(new MethodInvoker(new Action(() => ConsumeImmediateMessages())));
					return;
				}

				//è stato aggiunto un nuovo report o un nuovo documento
				//nella custom, devo rinfrescare i menu
				if (m.Msg == ExternalAPI.UM_REFRESH_USER_OBJECTS)
				{
					try
					{

						if (menuBrowserForm != null && pathFinder != null)
						{
							MenuInfo.CachedMenuInfos.Delete(pathFinder.User);
							menuBrowserForm.Reload();
						}

					}
					catch (Exception e)
					{
						Debug.Fail("MenuMngForm.RefreshUserObjects Error: " + e.ToString());
					}
					base.WndProc(ref m);
					return;
				}

				if (m.Msg == ExternalAPI.UM_GET_SOAP_PORT)
				{
					if (tbAppClientInterface == null)
						m.Result = IntPtr.Zero;
					else
						m.Result = new IntPtr(tbAppClientInterface.TbPort);
					return;
				}

				if (m.Msg == ExternalAPI.UM_SET_STATUS_BAR_TEXT)
				{
					MenuFormStatusStrip.InfoText = Marshal.PtrToStringAuto(m.WParam);
					return;
				}

				if (m.Msg == ExternalAPI.UM_CHOOSE_CUSTOMIZATION_CONTEXT)
				{
					ChooseCustomizationContextInternal();
				}

				if (m.Msg == ExternalAPI.UM_CHOOSE_CUSTOMIZATION_CONTEXT_AND_EASYBUILDERIT_AGAIN)
				{
					String nsDoc = Marshal.PtrToStringAuto(m.WParam);
					Marshal.FreeHGlobal(Marshal.StringToHGlobalAuto(nsDoc));
					ChooseCustomizationContextAndEasybuilderItAgain(nsDoc);
					return;
				}

				if (m.Msg == ExternalAPI.UM_CLEAR_STATUS_BAR)
				{
					MenuFormStatusStrip.InfoText = string.Empty;
					return;
				}

				if (m.Msg == ExternalAPI.UM_SET_MENU_WINDOW_TEXT)
				{
					mainWindowTitle = Text = Marshal.PtrToStringAuto(m.WParam);
					return;
				}

				if (m.Msg == ExternalAPI.UM_SET_USER_PANEL_TEXT)	//TODO ILARIA non lo si vuole piu`! 
					return;

				if (m.Msg == ExternalAPI.UM_ACTIVATE_MENU)
				{
					foreach (GenericDockableForm content in MenuMngDocumentContainer.Contents)
					{
						if (MenuManagerStrings.Home == content.Text)
						{
							content.Show();
							break;
						}
					}
					return;
				}
				if (m.Msg == ExternalAPI.UM_NEW_LOGIN)
				{
					NewLogin();
				}

				if (m.Msg == ExternalAPI.UM_ACTIVATE_INTERNET)
				{
					pingViaInternet();
					return;
				}

                if (m.Msg == ExternalAPI.UM_OPEN_CUSTOMIZATIONMANAGER)
                {
                    this.OpenCustomizationManagerToolStripMenuItem_Click(this, EventArgs.Empty);
                    return;
                }

				if (m.Msg == ExternalAPI.UM_OPEN_MENUEDITOR)
				{
					OpenMenuEditor();
					return;
				}

				if (m.Msg == ExternalAPI.UM_OPEN_URL)
				{
					string url = Marshal.PtrToStringAuto(m.WParam);
					string title = Marshal.PtrToStringAuto(m.LParam);
					if (!url.IsNullOrEmpty())
						OpenUrl(url, title);
					return;
				}

				if (m.Msg == ExternalAPI.UM_LOGIN_COMPLETED)
                {
                    logging = false;
					string authToken = NewMenuFunctions.GetMessageMarshal(Process.GetCurrentProcess().Handle, m);
					if (authToken.IsNullOrEmpty() || !OnLogged(authToken))
					{
						return;
					}
                    if (!iparam.IsNullOrEmpty())
                    {
                        MenuMngForm_TbNavigate(new TbNavigateEventArgs(iparam));
                        this.Activate();
                        iparam = null;
                        return;
                    }
                    //TESTImmediateBalloon1();
                }

                if (m.Msg == ExternalAPI.UM_SEND_CURRENT_TOKEN)
                {
                    string authToken = NewMenuFunctions.GetMessageMarshal(Process.GetCurrentProcess().Handle, m);
                    if (currentLoginManager == null)
                        currentLoginManager = new LoginManager();

                    //logout dal menu html, lascia il currentLoginManager in uno stato sbagliato, lo ripristino
                    if (currentLoginManager.LoginManagerState != LoginManagerState.Logged || authToken != currentLoginManager.AuthenticationToken)
                    {
                        currentLoginManager.GetLoginInformation(authToken);
                        SetCurrentLoginData(currentLoginManager);
                    }
                }

                if (m.Msg == ExternalAPI.UM_LOGGING)
                {
                    logging = true;
                }
                if (m.Msg == ExternalAPI.UM_LOGIN_INCOMPLETED)
                {
                    logging = false;
                }

				if (m.Msg == ExternalAPI.UM_RELOGIN_COMPLETED)
				{
                    logging = false;
					string authToken = NewMenuFunctions.GetMessageMarshal(Process.GetCurrentProcess().Handle, m);
					if (!authToken.IsNullOrEmpty())
					{
						ChangeLoginInternal(authToken);
					}
                    
                }

                if (m.Msg == ExternalAPI.UM_MAGO_LINKER)
                {
                    //maServerDiagnostic.Set(DiagnosticType.Error | DiagnosticType.LogInfo, "----UM_MAGO_LINKER START " + DateTime.Now.ToString("hhmmss.ffff"));


                    //Iparam=imago://Document.ERP.Contacts.Documents.Contacts&IToken=AcxzmyscwozcilikjY2VacDGlZBzMXW9xfMnlSNPM7$u7ERcz2ZfruabqEBvItgmHAC6DlWo8OHFmvwNto4dAqd4LT9aT$ZGqn$KmLGiS97fq1TPAKqGJVBf9Ea3s64rYhiybDWsNzfTpbty4NRkkSSaBYXflk5zlH2m1tLQrnDmEtfVonHF7NOg82ewEYrMDy47t9AbG11Td6vkc
                    string result = NewMenuFunctions.GetMessageMarshal(Process.GetCurrentProcess().Handle, m);

                    string[] args = result.Split('&');
                    ParseArguments(args);
                   
                        MenuMngForm_TbNavigate(new TbNavigateEventArgs(iparam));
                        this.Activate();
                  

                    //maServerDiagnostic.Set(DiagnosticType.Error | DiagnosticType.LogInfo, "----UM_MAGO_LINKER END " + DateTime.Now.ToString("hhmmss.ffff"));

                    maServerDiagnostic.Set(DiagnosticType.Error | DiagnosticType.LogInfo, "----UM_MAGO_LINKER END " + DateTime.Now.ToString("hhmmss.ffff"));

                    return;
                }

                //allows Microarea Localizer sending a message to obtain information about the active form
                // in order to locate the associated dictionary item
                if (LocalizerConnector.WndProc(ref m))
					return;

				if (MenuMngDocumentContainer != null && MenuMngDocumentContainer.HandleMessage(ref m))
					return;

				base.WndProc(ref m);
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				base.WndProc(ref m);
			}
		}


        //---------------------------------------------------------------------
        private string ValidateItoken( )
        {
          return  currentLoginManager.ValidateItoken(itoken);
        }

        //---------------------------------------------------------------------
        private void WriteOnRegistryTBLauncherManagerKeys()
        {
            string path = TBLauncherManagerPath();
            WriteOnRegistry(path, "TB");
            WriteOnRegistry(path, "IMago");
        }

        //-----------------------------------------------------------------------
        private void WriteOnRegistry(string path, string protocolName)
        {
            //non verifico l'esistenza perchè la cartella locale dove il programma è messo potrebbe variare in seguito ad aggionamenti.
            RegistryKey subKey1 = null;
            RegistryKey subKey11 = null;
            try
            {
                if (String.IsNullOrWhiteSpace(path)) return;

                subKey1 = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + protocolName);
                if (subKey1 == null) return;
                subKey1.SetValue("URL Protocol", "");
                subKey11 = subKey1.CreateSubKey(@"Shell\Open\Command");
                if (subKey11 == null) return;
                //null come nome  = (Default)
                subKey11.SetValue(null, path);
                //maServerDiagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, String.Format("{1} - Written registry keys in HKEY_CURRENT_USER\\Software\\Classes\\{0}", protocolName, installationName));
            }
            catch (Exception exc)
            {
                //Non traduco il messaggio di diagnostica, tanto finisce nell'event viewer e così è più semplice per noi capire se qualcosa è andato storto se il prodotto gira in lingue straniere
                maServerDiagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, String.Format("{1} - Error writing registry key in: HKEY_CURRENT_USER\\Software\\Classes\\{2}:  {0}", exc.ToString(), installationName, protocolName));

            }
            finally
            {
                //chiudo le chiavi
                if (subKey11 != null) subKey11.Close();
                if (subKey1 != null) subKey1.Close();
            }
            //runasadministrator
            RegistryKey k = null;
            try
            {
                string path2 = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath; //converte da url a path di file system
                string exepath = Path.Combine(Path.GetDirectoryName(path2), "TBLauncherManager.exe");
                if (!File.Exists(exepath)) return;
                if (exepath == null) return;
                k = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");
                if (k == null) return;

                k.SetValue(exepath, "~ RUNASADMIN");

            }
            catch (Exception exc)
            {
                maServerDiagnostic.Set(
                   DiagnosticType.LogInfo | DiagnosticType.Warning,
                   //Non traduco il messaggio di diagnostica, tanto finisce nell'event viewer e così è più semplice per noi capire se qualcosa è andato storto se il prodotto gira in lingue straniere
                   String.Format("{1} - Error writing registry key in: HKEY_CURRENT_USER\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers: {0}", exc.ToString(), installationName)
                   );
            }
            finally
            {
                //chiudo le chiavi
                if (k != null) k.Close();
            }
        }
		//---------------------------------------------------------------------------------
		private string TBLauncherManagerPath()
		{
			string mask = "\"{0}\" \"%1\"";
			string filePath = Path.Combine(Functions.GetExecutingAssemblyFolderPath(), "TBLauncherManager.exe");
			if (!File.Exists(filePath)) return null;
			return String.Format(mask, filePath);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CloseCustomizationContextInternal()
		{
			BaseCustomizationContext.CustomizationContextInstance.ChangeEasyBuilderApp(string.Empty, string.Empty);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool ChooseCustomizationContextInternal()
		{
			//Mi memorizza da parte il fatto che esista o meno una easyBuildeApp corrente
			//per capire se è già stato creato un contesto di customizzazione/standardizzazione.
			bool existsCurrentEasyBuilderApp = BaseCustomizationContext.CustomizationContextInstance.ExistsCurrentEasyBuilderApp;
			return ChooseCustomizationContext.Choose();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ChooseCustomizationContextAndEasybuilderItAgain(string nsDoc)
		{
			changeLoginResetEvent.Reset();
			//Mi memorizza da parte il fatto che esista o meno una easyBuildeApp corrente
			//per capire se è già stato creato un contesto di customizzazione/standardizzazione.
			bool existsCurrentEasyBuilderApp = BaseCustomizationContext.CustomizationContextInstance.ExistsCurrentEasyBuilderApp;
			if (!ChooseCustomizationContext.Choose())
				return;

			Task.Factory.StartNew(
				new Action(() =>
				{
					if (!existsCurrentEasyBuilderApp && BaseCustomizationContext.CustomizationContextInstance.IsCurrentEasyBuilderAppAStandardization)
					{
						if (!changeLoginResetEvent.WaitOne(20000))
							return;
					}
					int docHandle = 0;
					tbAppClientInterface.RunDocument(nsDoc, "", out docHandle);
					tbAppClientInterface.FireAction(docHandle, "EasyBuilderIt");
				})
				);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnActivated(EventArgs e)
		{
			//non devo fare postmessage, altrimenti non apre il radar alternativo sugli articoli (piu' in generale, la Activate arriva
			//quando il radar e' gia' aperto, e questo si richiude subito
			//E` simmetrico al metodo OnDeactivate
			if (MenuMngDocumentContainer != null)
				MenuMngDocumentContainer.ActivateAppFormWindow(true, false);

			base.OnActivated(e);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnDeactivate(EventArgs e)
		{
			//Mandiamo il messaggio di Applicazione Disattivata (WM_ACTIVATEAPP anziche` WM_ACTIVATE)
			//Risolve il seguente problema:
			//Con l' utilizzo dei controlli telerik avveniva che, se si apriva una tendina di una drop down e poi si faceva
			//click su un'altra applicazione, la tendina rimaneva aperta anche se MenuManager non era piu` l' applicazione attiva.
			if (MenuMngDocumentContainer != null)
				MenuMngDocumentContainer.ActivateAppFormWindow(false, false);

			base.OnDeactivate(e);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnLocationChanged(EventArgs e)
		{
			// Invoke base class implementation
			base.OnLocationChanged(e);

			if (!reinitializingUI)
				currentLocation = new Point(this.Location.X, this.Location.Y);
		}

		//-------------------------------------------------------------------------------------------------------
		protected override void OnSizeChanged(EventArgs e)
		{
            DocumentDockableForm activeDocument = MenuMngDocumentContainer.ActiveDocument as DocumentDockableForm;

            if (activeDocument != null)
            {
                ExternalAPI.SendMessage
                (
                    activeDocument.WindowHandle,
                    ExternalAPI.UM_MENU_MNG_RESIZING,
                    (IntPtr)Convert.ToInt16(true),
                    (IntPtr)Convert.ToInt16(true)
                );
            }

            // Invoke base class implementation
            base.OnSizeChanged(e);

			if (!reinitializingUI)
				currentClientSize = new Size(this.ClientSize.Width, this.ClientSize.Height);

        }

		// Se TB ?"chiudibile" lo si chiude senza bisogno di chiederne ulteriore conferma all'utente,  
		// cio?senza visualizzare in TBLoader un messaggio di richiesta relativo alla sua chiusura.
		// Altrimenti, se TB non viene chiuso, anche MenuManager deve restare aperto.
		// Se non esiste MenuManager (situazione atipica, causata da probabili malfunzionamenti) TB va
		// chiuso senza alcuna richiesta di conferma.
		// Se, invece, MenuManager esiste (e la richiesta di chiusura, ovviamente non proviene da lui) 
		// esso va attivato, inviandogli la richiesta di chiusura 
		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			if (starting || !Monitor.TryEnter(typeof(MenuMngForm)))
			{
				e.Cancel = true;
				return;
			}

            if (logging)
            {
              
                e.Cancel = true;
                return;
            }

			try
			{
                if (tbAppClientInterface != null && !tbAppClientInterface.OnBeforeCanCloseTB())
                {
                    e.Cancel = true;
                    Monitor.Exit(typeof(MenuMngForm));
                    return;
                }

				// Invoke base class implementation
				base.OnClosing(e);
				if (confirmClose)
				{
					AskClosingMenu ask = new AskClosingMenu(
						isMainForm ? MenuManagerStrings.CloseMenuManagerConfirmMsg : MenuManagerStrings.CloseLoginConfirmMsg,
						MenuMngDocumentContainer.DocumentStates
						);

					if (ask.ShowDialog(this) == DialogResult.No)
					{
						e.Cancel = true;
						return;
					}

					if (MenuMngDocumentContainer.DocumentStates != null)
					{
						//se non devo salvarmi i documenti aperti, li tolgo dalla lista prima di salvare
						if (!MenuMngDocumentContainer.DocumentStates.RestoreOpenDocuments)
							MenuMngDocumentContainer.DocumentStates.GetListAndClear();
						//salvo le posizioni dei documenti (per namespace) ed eventualmente i documenti aperti
						MenuMngDocumentContainer.DocumentStates.Save();
					}
				}

				if (tbAppClientInterface != null && tbAppClientInterface.Connected && tbAppClientInterface.Valid)
					Functions.DoParallelProcedure(() => { tbAppClientInterface.SilentCloseLoginDocuments(); });

				if (WindowState == FormWindowState.Minimized)
					WindowState = FormWindowState.Normal;

				if (tbAppClientInterface != null && tbAppClientInterface.Connected && tbAppClientInterface.Valid && !tbAppClientInterface.CanCloseTB())
				{
					MessageBox.Show(this, MenuManagerStrings.CannotCloseTBWarningMsg, MenuManagerStrings.CannotCloseTBWarningCaption);

					// Cancello l'evento di chiusura della form.
					e.Cancel = true;
					return;
				}

				closing = true;
			}
			finally
			{
				Monitor.Exit(typeof(MenuMngForm));
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnClosed(EventArgs e)
		{
			DefaultTheme.ThemeChanged -= DefaultTheme_ThemeChanged;

			if (currentLoginManager.IsUserLogged())
			{
				// devo salvare le dimensioni "normali" della form...
				FormWindowState currentWindowState = WindowState;
				if (currentWindowState != FormWindowState.Normal)
					WindowState = FormWindowState.Normal;


				// Salvo lo stato del men?per poterlo ripristinare al prossimo lancio
				MenuFormIniSettings iniSettingsToSave = new MenuFormIniSettings(this);

				iniSettingsToSave.WindowState = currentWindowState;
				iniSettingsToSave.SaveToConfiguration();
			}

			if (tbAppClientInterface != null && tbAppClientInterface.Connected && tbAppClientInterface.Valid)
				tbAppClientInterface.CloseLogin();

			LogOff(currentUser, currentCompany);

			// Invoke base class implementation
			base.OnClosed(e);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool OnLogged(string authToken = "")
		{
			if (authToken.IsNullOrEmpty())
				return false;

			if (currentLoginManager == null)
				currentLoginManager = new LoginManager();

            //rimuovo eventuali lock cadaveri da sessioni scadute
            LockManager lockManager = new LockManager(BasePathFinder.BasePathFinderInstance.LockManagerUrl);
            lockManager.RemoveUnusedLocks();

            //logout dal menu html, lascia il currentLoginManager in uno stato sbagliato, lo ripristino
            if (currentLoginManager.LoginManagerState != LoginManagerState.Logged || authToken != currentLoginManager.AuthenticationToken)
			{
				currentLoginManager.GetLoginInformation(authToken);
				SetCurrentLoginData(currentLoginManager);
			}

            /*if (!CheckDatabaseInternal())
			{
				currentLoginManager.LogOff();
				DiagnosticViewer.ShowDiagnostic(maServerDiagnostic);
				return false;
			}*/
            //TESTImmediateBalloon();
            DictionaryFunctions.SetCultureInfo(currentPreferredLanguage, currentApplicationLanguage);
			if (menuBrowserForm != null)
                menuBrowserForm.ApplyResources();

			reinitializingUI = true;
            			
			pathFinder = new PathFinder(currentCompany, currentUser);
			//Login step
			tbAppClientInterface = new TbApplicationClientInterface(currentConfiguration, pathFinder, tbPort, authToken, GetWcfBinding());
			
			Task establishConnectionToTBThread = new Task(new Action(EstablishConnectionToTB));
			establishConnectionToTBThread.Start();

			ManageMutex();

			while (!establishConnectionToTBThread.Wait(10))
				Application.DoEvents();

			if (tbDiagnostic != null && (tbDiagnostic.TotalErrors > 0 || tbDiagnostic.TotalWarnings > 0 || tbDiagnostic.TotalInformations > 0))
				DiagnosticViewer.ShowDiagnosticAndClear(tbDiagnostic as Diagnostic, OrderType.None);

			if (!tbAppClientInterface.Valid || bLoginFailed || !CUtility.IsLoginContextValid(currentLoginManager.AuthenticationToken))
			{
				//	LoginFacilities.SetRememberMe(Boolean.FalseString);  TODOLUCA, da fare???? 
				CloseForm();
				return false;
			}
			// l'impostazione della culture va effettuata prima di caricare il menù

			UpdateUIAfterLogin();

		
			new Thread(new ThreadStart(() => {
				if (BaseCustomizationContext.CustomizationContextInstance == null)
					return;

				object o = BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications;
			})).Start();

			MenuFormStatusStrip.Visible = false;
			userPaneString = MenuManagerStrings.UserStatusBarPanelText + currentUser;

            //--------------------------------------------------------------------------------------NotificationManager di Andrea D.
            //InitializeNotificationManager();
            //----------------------------------------------------------------------------------------------------------------------

            //RunStartupCommands();
            if (!itoken.IsNullOrEmpty() && menuBrowserForm != null)
                CloseMenuWeb();

			//se siamo in modalità test ranorex, lancia il menu vecchio e chiude quello nuovo (non utilizzabile)
			RunOldMenuForRanorex();

			return true;
		}

		//----------------------------------------------------------------------------------------------------------------------
		private void RunOldMenuForRanorex()
		{
			if (currentLoginManager.IsActivated("RanorexTestSupport", "RanorexTestSupport"))
			{
				//CloseMenuWeb();
				SetCurrentLoginData(currentLoginManager);
			
				MenuMngWinCtrlForm menuManagerWinCtrlForm = new MenuMngWinCtrlForm(pathFinder, currentLoginManager);
				menuManagerWinCtrlForm.Text = "Old Menu";
				menuManagerWinCtrlForm.RunCommand += MenuManagerWinCtrl_RunCommand;
				menuManagerWinCtrlForm.Show(MenuMngDocumentContainer);
			}
		}

		//----------------------------------------------------------------------------------------------------------------------
		private void MenuManagerWinCtrl_RunCommand(object sender, TbNavigateEventArgs e)
		{
			MenuMngForm_TbNavigate(e);
		}

		//----------------------------------------------------------------------------------------------------------------------
		private void ManageMutex()
		{
			Mutex menuMutex = null;
			try
			{
				Monitor.Enter(typeof(MenuMngForm));
				starting = true;

				//mutex name has to start with Global\ , so it is common to all sessions of terminal services
				string mutexName = "Global\\MenuManager" + installationName;
				bool mutexWasCreated;
				menuMutex = new Mutex(true, mutexName, out mutexWasCreated);
				// If this thread does not own the named mutex, it requests it by calling WaitOne.
				if (!mutexWasCreated)
					menuMutex.WaitOne(Timeout.Infinite, false);

				//mutex has to have no security, so it can be shared by all sessions of terminal services
				Functions.SetNoSecurityOnMutex(menuMutex);
			}
			finally
			{
				Monitor.Exit(typeof(MenuMngForm));
				if (menuMutex != null)
					menuMutex.ReleaseMutex();
				starting = false;
			}
		}
       

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CloseForm()
        {
           
			CloseMenuWeb();
			confirmClose = false;
			starting = false;

			this.DragDrop -= new System.Windows.Forms.DragEventHandler(this.MenuMngForm_DragDrop);
			this.DragEnter -= new System.Windows.Forms.DragEventHandler(this.MenuMngForm_DragEnter);
	
			Close();
		}
        
		//--------------------------------------------------------------------------------------------------------------------------------
		private static WCFBinding GetWcfBinding()
		{
			return tbPort > 0 ? WCFBinding.BasicHttp : WCFBinding.None;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CloseAllDocumentsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!CloseAllDocuments())
				MessageBox.Show(this, MenuManagerStrings.CannotCloseAllDocuments);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool CloseAllDocuments()
		{
			bool b = false;
			Functions.DoParallelProcedure(() => { b = tbAppClientInterface.SilentCloseLoginDocuments(); });
			return b;
		}

		//----------------------------------------------------------------------------
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			ShowHelpViewer();
			hevent.Handled = true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void NewLogin()
		{
			if (!MultipleLoginAvailable)
			{
				MessageBox.Show(this, MenuManagerStrings.NoMultipleLogin, MenuManagerStrings.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			string path = Assembly.GetExecutingAssembly().Location;
			Process.Start(path);
		}

		//---------------------------------------------------------------------
		private void StartMessagesTimer()
		{
			if (!messageTimer.Enabled)
			{
				if (!reinitializingUI)
				{
					SetBalloonStatusStrip();
				}

				//avvio il timer dei messaggi
				//messageTimer.Interval = 3000000;//50 minuti	
				//i messaggi devono essere richiesti più spesso perchè se ci sono disattivazioni da ping mostriamo il messaggio nel balloonn
				messageTimer.Interval = 300000;//5 minuti  

				messageTimer.Elapsed += new System.Timers.ElapsedEventHandler(ConsumeMessages);
				messageTimer.Start();
				try
				{
					new Task(new Action(() =>
					{
						Thread.Sleep(15000);

						// Aspettiamo 15 secondi affinchè MM sia completamente caricato.
						ConsumeMessages(null, null);
					})).Start();
				}
				catch (System.Security.SecurityException) { }
				catch (OutOfMemoryException) { }
			}
		}

		//---------------------------------------------------------------------
		private void StopMessagesTimer()
		{
			if (messageTimer.Enabled)
			{
				DestroyBalloonStatusStrip();

				messageTimer.Elapsed -= new System.Timers.ElapsedEventHandler(ConsumeMessages);
				messageTimer.Stop();
			}
		}

		//---------------------------------------------------------------------
		private void ConsumeMessages(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (messagePictureBox == null || messagePictureBox.balloonOpened) return;
			//sincronizzato per evitare che venga acceduto dal timer e dal change login in contemporanea
			lock (lockTicket)
			{
				IList validMessages = GetNewMessages();

				if (validMessages.Count > 0)//invoke xkè è un altro thread, quello del timer
					this.Invoke(new Action(() => SetStatusStripImage(validMessages)));
			}
		}

		//---------------------------------------------------------------------
		private Advertisement[] GetNewMessages()
		{
			//Chiedo a loginManager se ci sono nuovi messaggi per questo utente
			IAdvertisement[] newMessages = null;
			try
			{
				if (currentLoginManager != null)
					newMessages = currentLoginManager.GetMessages(currentLoginManager.AuthenticationToken);
			}
			catch
			{
				//Non do errore per non bloccare l'esecuzione di MenuManager
			}

			////modificato da andrea d. per aggiungere notifiche
			//var advList = new List<Advertisement>();
			//advList.AddRange(newMessages);
			//if(NotificationManager != null && NotificationManager.IsConnectedWithXSocket())
			//{
			//	foreach(var notify in NotificationManager.GetAllNotifications())
			//	{
			//		var adv				= new Advertisement();
			//		adv.Body			= new Advertisement.AdvertisementBody("", "", notify.Title);
			//		adv.HideDisclaimer	= true;
			//		adv.Historicize		= true;
			//		adv.ExpiryDate		= DateTime.Now.AddDays(1);

			//		adv.AutoClosingTime = 10000;
			//		advList.Add(adv);
			//	}
			//}
			//return advList.ToArray();
			////

			return FilterMessages(newMessages as Advertisement[]);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private Advertisement[] GetOldMessages()
		{
			//Chiedo a loginManager se ci sono vecchi messaggi per questo utente
			IAdvertisement[] oldmessages = null;
			try
			{
				if (currentLoginManager != null)
					oldmessages = currentLoginManager.GetOldMessages(currentLoginManager.AuthenticationToken);
			}
			catch
			{
				//Non do errore per non bloccare l'esecuzione di MenuManager
			}
			return FilterMessages(oldmessages as Advertisement[]);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private Advertisement[] FilterMessages(Advertisement[] messages)
		{
			return FilterExpiredMessages(messages);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private Advertisement[] FilterExpiredMessages(Advertisement[] messages)
		{
			List<Advertisement> validMessages = new List<Advertisement>();
			if (messages == null || messages.Length == 0)
				return validMessages.ToArray();

			foreach (Advertisement adv in messages)
				if (adv != null && !adv.Expired)//scarto i messaggi scaduti
					validMessages.Add(adv);
			return validMessages.ToArray();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SetStatusStripImage(IList validMessages, bool immediate = false)
		{
			if (validMessages == null || validMessages.Count == 0)
				return;

			if (advRendererManager != null && !advRendererManager.IsDisposed)
			{
				advRendererManager.MessageShowing -= new EventHandler<BalloonDataEventArgs>(MessagePictureBox_MessageShowing);
				//advRendererManager.RemovingBalloonRequired -= new AdvRendererManager.BalloonEventHandler(advRendererManager_RemovingBalloonRequired);//<--era commentata, modificato da andrea
				advRendererManager.Dispose();
			}

			advRendererManager = new AdvRendererManager();
			advRendererManager.MessageChanging += new EventHandler<BalloonDataEventArgs>(advRendererManager_MessageChanging);
			advRendererManager.CanSave = false;
			advRendererManager.AdvertisementRenderer = GetAdvRenderer();
			advRendererManager.MessageShowing += new EventHandler<BalloonDataEventArgs>(MessagePictureBox_MessageShowing);
			advRendererManager.RemovingBalloonRequired += new AdvRendererManager.BalloonEventHandler(advRendererManager_RemovingBalloonRequired);
			messagePictureBox.ContentManager = new AdvContentManager(advRendererManager);
			advRendererManager.Advertisements = validMessages;

			string tooltip =
				(validMessages.Count == 1) ?
				MenuManagerStrings.ANewMessage :
				String.Format(MenuManagerStrings.NewMessages, validMessages.Count);

			messagePictureBox.BringToFront();
			if (!immediate)
			{
				this.messagesToolStripControlHost.Visible = true;
				messagePictureBox.Show(tooltip);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void advRendererManager_RemovingBalloonRequired(string messageid)
		{
			currentLoginManager.DeleteMessageFromQueue(messageid);
		}

		//per ora commento tutto, Enrico non vuole che gli utenti abbiano il potere di disabilitarsi il balloon da soli
		//--------------------------------------------------------------------------------------------------------------------------------
		void advRendererManager_MessageChanging(object sender, BalloonDataEventArgs e)
		{
			//BalloonDataBag bag = e.BalloonData;

			//currentLoginManager.SetBalloonInfo(currentLoginManager.AuthenticationToken, bag.Type, bag.Block);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private BaseAdvRenderer GetAdvRenderer()
		{
			//non esiste più l'asinrona...
			bool isSynch = true;

			/*try
			{
				isSynch = currentLoginManager.IsSynchActivation();
			}
			catch
			{
				//Non do errore per non bloccare l'esecuzione di MenuManager
			}
			*/
			BaseAdvRenderer advRenderer = null;
			try
			{
				advRenderer = new HtmlAdvRenderer();
				((HtmlAdvRenderer)advRenderer).OnLine = isSynch;
				if (pathFinder != null)
					((HtmlAdvRenderer)advRenderer).LoginManagerUrl = pathFinder.LoginManagerUrl;

				((HtmlAdvRenderer)advRenderer).TbNavigate += new HtmlAdvRenderer.TbNavigateEventHandler(MenuMngForm_TbNavigate);

			}
			catch { advRenderer = new TextAdvRenderer(); }
			advRenderer.AutToken = currentLoginManager.AuthenticationToken;
			return advRenderer;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void MessagePictureBox_MessageShowing(object sender, BalloonDataEventArgs args)
		{
			//quando mi avvertono che i messaggi sono letti lo dico a loginmanager
			if (args != null && currentLoginManager != null)
			{
				try
				{
					//currentLoginManager.SetMessageRead(currentLoginManager.AuthenticationToken, args.BalloonData.Id);
				}
				catch
				{
					//Non do errore per non bloccare l'esecuzione di MenuManager
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void vm_RemovingBalloonRequired(string messageid)
		{
			currentLoginManager.DeleteMessageFromQueue(messageid);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void pingViaInternet()
		{
			MessageBoxWithIcon mswi = null;
			bool needed = true;
			if (currentLoginManager == null)
			{
				mswi = new MessageBoxWithIcon(MessageBoxWithIcon.IconType.Warning, MenuManagerStrings.LMNotAvailable, MenuManagerStrings.ValidateViaInternet);
				mswi.ShowDialog();
				return;
			}

			needed = currentLoginManager.PingNeeded(true);
			if (needed)
				mswi = new MessageBoxWithIcon(MessageBoxWithIcon.IconType.Error, MenuManagerStrings.ValidationFailed, MenuManagerStrings.ValidateViaInternet);
			else
				mswi = new MessageBoxWithIcon(MessageBoxWithIcon.IconType.OK, MenuManagerStrings.ValidationOK, MenuManagerStrings.ValidateViaInternet);

			mswi.ShowDialog();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void OpenReportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.DoParallelProcedure(() =>
			{
				tbAppClientInterface.RunFunction("Framework.TbWoormViewer.TbWoormViewer.ExecOpenReport");
			});

		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void NewReportToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Functions.DoParallelProcedure(() =>
			{
				tbAppClientInterface.RunFunction("Framework.TbWoormViewer.TbWoormViewer.ExecNewReport");
			});
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void NewBatchToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EBCreateNewDocument("Extensions.EasyBuilder.TbEasyBuilder.NewBatchDocument");
		}
		//--------------------------------------------------------------------------------------------------------------------------------
		private void NewDocumentToolStripMenuItem_Click(object sender, EventArgs e)
		{
			EBCreateNewDocument("Extensions.EasyBuilder.TbEasyBuilder.NewDocument");
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void EBCreateNewDocument(string ns)
		{
			if (IsLicenseForEasyBuilderVerified() && !BaseCustomizationContext.CustomizationContextInstance.NotAlone(NameSolverStrings.EasyStudioDesigner, 1, 0))
			{
				//Resetto il reset event per il cambio login nel caso mi serva qui sotto.
				changeLoginResetEvent.Reset();

				//Mi memorizza da parte il fatto che esista o meno una easyBuildeApp corrente
				//per capire se è già stato creato un contesto di customizzazione/standardizzazione.
				bool existsCurrentEasyBuilderApp = BaseCustomizationContext.CustomizationContextInstance.ExistsCurrentEasyBuilderApp;

				if (!BaseCustomizationContext.CustomizationContextInstance.ExistsCurrentEasyBuilderApp)
					return;

				Functions.DoParallelProcedure(
					() =>
					{
						//Se il contesto di customizzazione non c'era (quindi è stato creato adesso) e
						//quello che è stato creato adesso è una standardizzazione allora sicuramente, per via dell'attivazione,
						//è stato scatenato un cambio login per cui prima di lanciare il nuovo documento devo aspettare
						//che il cambio login sia terminato.
						if (!existsCurrentEasyBuilderApp && BaseCustomizationContext.CustomizationContextInstance.IsCurrentEasyBuilderAppAStandardization)
						{
							if (!changeLoginResetEvent.WaitOne(20000))
								return;
						}

						tbAppClientInterface.RunDocument(ns);
					}
				);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void OpenCustomizationManagerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//Puo' aprire il solution explorer solo se non ci sono documenti in editing di EasyBuilder
			if (tbAppClientInterface.GetOpenDocumentsInDesignMode() > 0)
			{
				MessageBox.Show(this, MenuManagerStrings.CustomizationManagerNotAllowed, MenuManagerStrings.Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			using (SolutionExplorer solutionExplorer = new SolutionExplorer(this.currentLoginManager))
			{
				solutionExplorer.ShowDialog(this);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool IsLicenseForEasyBuilderVerified()
		{
			if (tbDiagnostic == null)
				tbDiagnostic = new Microarea.TaskBuilderNet.Core.DiagnosticManager.Diagnostic("TBApplicationClientInterface");

			try
			{
				EBLicenseManager.DemandLicenseForEasyStudio(currentLoginManager.AuthenticationToken);
			}
			catch (NotEasyStudioDeveloperLicenseException exc)
			{
				tbDiagnostic.SetWarning(exc.Message);
				DiagnosticViewer.ShowDiagnostic(tbDiagnostic, DiagnosticType.Warning, this);
				return false;
			}
			catch (EasyStudioCalSoldOutLicenseException exc)
			{
				tbDiagnostic.SetWarning(exc.Message);
				DiagnosticViewer.ShowDiagnostic(tbDiagnostic, DiagnosticType.Warning, this);
				return false;
			}
			catch (Exception exc)
			{
				tbDiagnostic.SetError(String.Format("{0}{1}{2}", MenuManagerStrings.ErrorContactingLoginManager, Environment.NewLine, exc.ToString()));
				DiagnosticViewer.ShowDiagnostic(tbDiagnostic, DiagnosticType.Error, this);
				return false;
			}

			return true;
		}

		//gestione messaggi balloon immediati
		bool testballon = false;

		//--------------------------------------------------------------------------------------------------------------------------------
		private void TESTImmediateBalloon()
		{
			if (testballon)
			{
				TESTImmediateBalloon1();
				testballon = false;
			}

			else
			{
				TESTImmediateBalloon2();
				testballon = true;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void TESTImmediateBalloon1()
		{
			//PROBLEMA DEL PUNTO E VIRGOLA
			//Aggiungere reference a system.Web se si vuole usare encoding
			//string s = HttpUtility.UrlEncode("typecustsupp:3211264;custsupp:C<,/@&^~.:");

			//invio messaggio immediato!
			string bodyMessage = "<a href=\"TB://Document.ERP.Company.Documents.Company?CompanyId:1;\">COMPANY 1</a>"/* +
                "<br><a href=\"TB://Document.ERP.CustomersSuppliers.Documents.Customers?" + s + "\">Customer</a>"*/;

			currentLoginManager.AdvancedSendBalloon(
				currentLoginManager.AuthenticationToken,
				bodyMessage,
				DateTime.Now.AddYears(1),
				MessageType.PostaLite,
				null,
				MessageSensation.Information,
				true,
				true,
				10000);

			// mostro messaggi immediati senza necessità di cliccare la bustina
			ConsumeImmediateMessages();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void TESTImmediateBalloon2()
		{
			//PROBLEMA DEL PUNTO E VIRGOLA
			//Aggiungere reference a system.Web se si vuole usare encoding
			//string s = HttpUtility.UrlEncode("typecustsupp:3211264;custsupp:C<,/@&^~.:");

			//invio messaggio immediato!
			string bodyMessage = "<a href=\"TB://Document.ERP.CustomersSuppliers.Documents.Customers?typecustsupp:3211264;custsupp:0001;\">Customer 0001 </a>"/* +
                "<br><a href=\"TB://Document.ERP.CustomersSuppliers.Documents.Customers?" + s + "\">Customer</a>"*/;

			currentLoginManager.AdvancedSendBalloon(
				currentLoginManager.AuthenticationToken,
				bodyMessage,
				DateTime.Now.AddYears(1),
				MessageType.Updates,
				new string[] { currentLoginManager.UserName },
				MessageSensation.Error,
				false,
				true,
				5000);

			// mostro messaggi immediati senza necessità di cliccare la bustina
            //ConsumeImmediateMessages();
		}

		//---------------------------------------------------------------------
		private void ConsumeImmediateMessages()
		{
			if (messagePictureBox.balloonOpened) return;
			if (WindowState == FormWindowState.Minimized || !Visible)
				return;
			//sincronizzato per evitare che venga acceduto dal timer e dal change login in contemporanea
			lock (lockTicket)
			{
				IList validMessages = GetImmediateMessages();
				if (validMessages.Count > 0)//invoke xkè è un altro thread, quello del timer
					this.Invoke(new Action(() => ShowImmediateBalloon(validMessages)));
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ShowImmediateBalloon(IList validMessages)
		{
			SetStatusStripImage(validMessages, true);
			messagePictureBox.ShowImmediate();
		}

		//---------------------------------------------------------------------
		private Advertisement[] GetImmediateMessages()
		{
			//Chiedo a loginManager se ci sono nuovi messaggi per questo utente
			IAdvertisement[] newMessages = null;
			try
			{
				if (currentLoginManager != null)
					newMessages = currentLoginManager.GetImmediateMessages(currentLoginManager.AuthenticationToken);
			}
			catch
			{
				//Non do errore per non bloccare l'esecuzione di MenuManager
			}
			return FilterMessages(newMessages as Advertisement[]);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void MenuMngForm_TbNavigate(TbNavigateEventArgs e)
		{
            string error = ValidateItoken();
            if (!error.IsNullOrWhiteSpace())
            { 
                maServerDiagnostic.Set(DiagnosticType.Information | DiagnosticType.Error, error);
                DiagnosticViewer.ShowDiagnosticAndClear(maServerDiagnostic as Diagnostic);
                return;

            }

           //maServerDiagnostic.Set(DiagnosticType.Error| DiagnosticType.LogInfo, "----MenuMngForm_TbNavigate START "+DateTime.Now.ToString("hhmmss.ffff"));

            //  Debug.Fail("enames " + e.Namespace);

            //quindi apro i documenti della lista clonata
            //deve essere in un altro thread, altrimenti questo non risponde ai messaggi inviati
            //dal documento che si sta aprendo e va in deadlock
            if (e == null)
				return;

			if (e.Type == NameSpaceObjectType.Application)
			{
				string exeFile = e.Key.Substring(e.Key.IndexOf("=") + 1);
				Task t = new Task((Action)delegate { bool runOK = ExecuteProcessByFileName(exeFile); });
				t.Start();
			}
			else if (e.Type == NameSpaceObjectType.Document)
			{
				Task t = new Task((Action)delegate { bool runOK = tbAppClientInterface.RunDocument(e.Namespace, e.Key); });
				t.Start();
			}
			else if (e.Type == NameSpaceObjectType.Report)
			{
				Task t = new Task((Action)delegate { bool runOK = tbAppClientInterface.RunReport(e.Namespace, e.Key); });
				t.Start();
			}
			else if (e.Type == NameSpaceObjectType.Function)
			{
				Task t = new Task((Action)delegate { tbAppClientInterface.RunFunctionInNewThread(e.Namespace, e.Key); });
				t.Start();
			}
			else if (e.Type == NameSpaceObjectType.Text)
			{
				string txtFile = e.Key.Substring(e.Key.IndexOf("=") + 1);
				Task t = new Task((Action)delegate { bool runOK = tbAppClientInterface.RunTextEditor(txtFile); });
				t.Start();
			}

            //else if 
            //	(
            //		e.Type == NameSpaceObjectType.WordDocument || e.Type == NameSpaceObjectType.WordDocument2007 ||
            //		e.Type == NameSpaceObjectType.ExcelDocument || e.Type == NameSpaceObjectType.ExcelDocument2007 ||
            //		e.Type == NameSpaceObjectType.WordTemplate || e.Type == NameSpaceObjectType.WordTemplate2007 ||
            //		e.Type == NameSpaceObjectType.ExcelTemplate || e.Type == NameSpaceObjectType.ExcelTemplate2007
            //	)
            //{
            //	ProcessOfficeFile(e.Type, e.Namespace);
            //}
          //  maServerDiagnostic.Set(DiagnosticType.Error | DiagnosticType.LogInfo, "----MenuMngForm_TbNavigate END " + DateTime.Now.ToString("/////hhmmss.ffff"));

        }
        
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void MenuMngForm_DragDrop(object sender, DragEventArgs e)
		{
			string data = e.Data.GetData(typeof(string)) as string;
			if (data == null)
				return;
			try
			{
				Uri uri = new Uri(data);
				TbNavigateEventArgs args = new TbNavigateEventArgs(uri);
				MenuMngForm_TbNavigate(args);
				e.Effect = DragDropEffects.None;
			}
			catch { }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void MenuMngForm_DragEnter(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(typeof(string)))
			{
				e.Effect = DragDropEffects.None;
				return;
			}
			try
			{
				string data = e.Data.GetData(typeof(string)) as string;
				Uri uri = new Uri(data);
				TbNavigateEventArgs args = new TbNavigateEventArgs(uri);
				e.Effect = args.Type == NameSpaceObjectType.NotValid ? DragDropEffects.None : DragDropEffects.Copy;
			}
			catch
			{
				e.Effect = DragDropEffects.None;
			}

		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void RunWebBrowser(string ITokenforLogin = null, string TbLinkToOpen=null)
		{
			if (runWithoutMenu)
				return;

			menuBrowserForm = new MenuBrowserDockableForm(tbAppClientInterface == null ? "" : tbAppClientInterface.AuthenticationToken, newMenu, newMenuLocalPort);
            menuBrowserForm.ITokenforLogin = ITokenforLogin;
            menuBrowserForm.TbLinkToOpen = TbLinkToOpen;
            Icon ico = InstallationData.BrandLoader.GetTbAppManagerApplicationIcon();
			if (ico != null)
				menuBrowserForm.Icon = ico;

			menuBrowserForm.Name = "MenuWeb Form";
			menuBrowserForm.Text = MenuManagerStrings.Home;
			menuBrowserForm.Show(MenuMngDocumentContainer);

			//MenuMngDocumentContainer.ActiveDocumentChanged += MenuMngDocumentContainer_ActiveDocumentChanged;
			//MenuMngDocumentContainer.WindowListRemoved += MenuMngDocumentContainer_WindowListRemoved;
	   }

		//--------------------------------------------------------------------------------------------------------------------------------
		private void MenuMngDocumentContainer_WindowListZero(object sender, EventArgs e)
        {
               WindowState = FormWindowState.Minimized;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
       /* void MenuMngDocumentContainer_WindowListRemoved(object sender, EventArgs e)
		{
			ServerWebSocketConnector.PushToClients(currentLoginManager.AuthenticationToken, "DocumentListUpdated", "");

        }*/

		//--------------------------------------------------------------------------------------------------------------------------------
		/*void MenuMngDocumentContainer_ActiveDocumentChanged(object sender, EventArgs e)
		{
			if (MenuMngDocumentContainer.ActiveDocument == menuBrowserForm)
			{
				ServerWebSocketConnector.PushToClients(currentLoginManager.AuthenticationToken, "DocumentListUpdated", "");
			}
		}*/

		//--------------------------------------------------------------------------------------------------------------------------------
		public void CloseMenuWeb()
		{
			if (MenuMngDocumentContainer != null)
			{
				//MenuMngDocumentContainer.ActiveDocumentChanged -= MenuMngDocumentContainer_ActiveDocumentChanged;
				//MenuMngDocumentContainer.WindowListRemoved -= MenuMngDocumentContainer_WindowListRemoved;
			}

			if (menuBrowserForm != null)
			{
				menuBrowserForm.Dispose();
				menuBrowserForm = null;
			}
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void OpenUrl(string url, string title)
		{
			GenericBrowserDockableForm g = new GenericBrowserDockableForm(url);
			g.Text = title;
			g.Show(MenuMngDocumentContainer);
		}

		//------------------------------------------------------------------------------------------------------------------------------
		private void OpenMenuEditor()
		{
			if (IsLicenseForEasyBuilderVerified())
			{
				if (BaseCustomizationContext.CustomizationContextInstance.ExistsCurrentEasyBuilderApp)
				{
					if (menuDesignerForm != null && !menuDesignerForm.IsDisposed)
					{
						menuDesignerForm.CurrentUser = this.currentLoginManager.UserName;
						menuDesignerForm.Activate();
						return;
					}

					menuDesignerForm = new MenuEditorDockableForm(pathFinder, this.currentLoginManager);
					menuDesignerForm.FormClosed += menuDesignerForm_FormClosed;
					menuDesignerForm.Show(MenuMngDocumentContainer);
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void menuDesignerForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (menuDesignerForm == null)
				return;
			//Ricarico il menu solo se l'utente ha effettuato almeno una modifica.
			if (menuDesignerForm.AtLeastOnModificationOccurred)
			{
				Functions.ClearCachedData(pathFinder.User);
				if (menuBrowserForm != null)
					menuBrowserForm.Reload();
			}

			menuDesignerForm.FormClosed -= new FormClosedEventHandler(menuDesignerForm_FormClosed);
			menuDesignerForm = null;
		}

		#region Notification
		/// <summary>
		/// Aggiunge il pulsante delle notifiche al menu manager
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		private void SetNotificationBtn()
		{
			if (NotificationBtn == null)
			{
				NotificationBtn = new NotificationMenuItem();
				NotificationBtn.Anchor = AnchorStyles.Right;
				NotificationBtn.Visible = true;

				if (!MenuFormStatusStrip.Items.Contains(this.NotificationBtn))
				{
					MenuFormStatusStrip.Items.Add(this.NotificationBtn);
					//MenuFormStatusStrip.ShowItemToolTips = true; //per vedere il numero delle notifiche anche con un ToolTip
				}
			}
		}

		/// <summary>
		/// Questo metodo svolge il compito di: 
		/// - aggiungere il pulsante delle notifiche, 
		/// - instanziare il NotificationManager
		/// - registrarsi alle notifiche
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		private void InitializeNotificationManager()
		{
			SetNotificationBtn();

			if (NotificationBtn != null && NotificationManager == null)
			{
				int workerId = CUtility.GetWorkerId(this.currentLoginManager.AuthenticationToken);
				int companyId = CUtility.GetCompanyId(this.currentLoginManager.AuthenticationToken);

				NotificationManager = new NotificationManager(workerId, companyId, true);
				NotificationManager.ConnectionWithXSocketNotify += NotificationManager_ConnectionWithXSocketNotify;
				NotificationManager.Notify += NotificationManager_Notify;
				NotificationManager.MessageNotify += NotificationManager_MessageNotify;

				NotificationManager.SubscribeToTheNotificationServiceAsync();
			}

			UpdateStatus(NotificationManager.IsConnectedWithXSocket() ? ConnectionStatus.Connected : ConnectionStatus.Disconnected);
		}

		/// <summary>
		/// mi arriva una notifica dal notification manager. la notifica in questione, in realtà è la somma
		/// delle notifiche base provenienti da tutti i moduli
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------------------------------------------------------------------
		void NotificationManager_Notify(object sender, EventArgs e)
		{
			this.UpdateNotificationButton();
		}

		/// <summary>
		/// mi arriva una notifica di tipo testuale, utilizzata ad esempio per i test veloci
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------------------------------------------------------------------
		void NotificationManager_MessageNotify(object sender, NotificationMessageEventArgs e)
		{
			BaseNotificationModule.ShowMessage(e.Message);

			//mostro il baloon

			//var advList = new List<Advertisement>();
			//var adv = new Advertisement();
			//adv.Body = new Advertisement.AdvertisementBody(e.Message, "", "");
			//adv.HideDisclaimer = true;
			//adv.Historicize = false;
			//adv.Immediate = true;
			//adv.Sensation = MessageSensation.Warning;
			//adv.Type = MessageType.Advrtsm;
			//adv.AutoClosingTime = 10000;
			//advList.Add(adv);

			//ShowImmediateBalloon(advList);

			////invio messaggio al login manager

			//string bodyMessage = e.Message;
			//currentLoginManager.AdvancedSendBalloon(
			//	currentLoginManager.AuthenticationToken,
			//	bodyMessage,
			//	DateTime.Now.AddYears(1),
			//	MessageType.Advrtsm,
			//	new string[1] { currentLoginManager.UserName },
			//	MessageSensation.Information,
			//	true,
			//	true,
			//	10000);

			////mostro subito il messaggio senza aspettare il timer
			//ConsumeImmediateMessages();
			//ConsumeMessages(null, null);
			//this.UpdateNotificationButton();
		}

		/// <summary>
		/// Notifica di connessione al Notification Service via XSocket
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//--------------------------------------------------------------------------------------------------------------------------------
		private void NotificationManager_ConnectionWithXSocketNotify(object sender, ConnectionWithXSocketEventArgs e)
		{
			this.UpdateStatus(e.status);
		}

		/// <summary>
		/// verifico di star eseguendo il codice nel giusto thread
		/// </summary>
		/// <param name="c"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool ControlInvokeRequired(Control c, Action a)
		{
			if (c.InvokeRequired && c.IsHandleCreated && !c.IsDisposed)
				c.Invoke(new MethodInvoker(delegate { a(); }));
			else
				return false;
			return true;
		}

	
		/// <summary>
		/// Metodo per aggiornare il numero delle notifiche del pulsante
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public void UpdateNotificationButton()
		{
			if (ControlInvokeRequired(this, () => UpdateNotificationButton()))
				return;

			NotificationBtn.DropDownItems.Clear();

			var allNotifications = NotificationManager.GetAllNotifications();
			NotificationBtn.Number = allNotifications.Count;

			var notificationsGroups = new List<NotificationMenuItem>();
			var notificationDictionary = new Dictionary<NotificationType, NotificationMenuItem>();
			foreach (var notify in allNotifications)
			{
				////se non è già presente, aggiungo una voce menu nel dropDown principale per questo tipo di notifica
				if (!notificationDictionary.ContainsKey(notify.NotificationType))
					notificationDictionary.Add(notify.NotificationType, new NotificationMenuItem(ImageType.None) { Text = notify.NotificationType.ToString() });
				var notificationItem = new ToolStripMenuItem(notify.Title.LeftWithSuspensionPoints(50) + "\n" + notify.Description.LeftWithSuspensionPoints(50) + "\n" + notify.Date);
				notificationItem.Click += (sender, args) =>
				{
					notify.OnClickAction();
				};
				NotificationMenuItem notificationGroup = null;
				notificationDictionary.TryGetValue(notify.NotificationType, out notificationGroup);
				if (notificationGroup != null)
					notificationGroup.DropDownItems.Add(notificationItem);
			}
			foreach (var type in notificationDictionary.Keys)
			{
				NotificationMenuItem notificationGroup = null;
				notificationDictionary.TryGetValue(type, out notificationGroup);
				if (notificationGroup != null)
				{
					notificationGroup.Number = notificationGroup.DropDownItems.Count;
					notificationGroup.SetStatus(NotificationStatus.Connected);
					NotificationBtn.DropDownItems.Add(notificationGroup);
				}
			}

			//old style
			/*
			NotificationType prevType = NotificationType.Generic;
			bool AssignedType = false;
			foreach(var notify in allNotifications ?? new IGenericNotify[0])
			{
				//separo le notifiche in base al loro modulo di appartenenza
				if(!AssignedType || notify.NotificationType != prevType) 
				{
					//NotificationBtn.DropDownItems.Add(new ToolStripSeparator ());
					var typeMenuItem = new ToolStripMenuItem { Text = notify.NotificationType.ToString(), Enabled = false };
					NotificationBtn.DropDownItems.Add(typeMenuItem);
					prevType = notify.NotificationType;
					AssignedType = true;
				}

				var newButton = new ToolStripMenuItem(notify.Title.Left(40) + "\n" + notify.Description.Left(40) + "\n" + notify.Date);
				newButton.Click += (sender, args) =>
				{
					notify.OnClickAction();
				};
			 
				////per test
				//var imageStream = Assembly.GetAssembly(typeof(MenuMngWinCtrl)).GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.flag_20_black.png");
				//if(imageStream != null)
				//{
				//	System.Drawing.Image image = Image.FromStream(imageStream);
				//	if(image != null)
				//		newButton.Image = image;
				//}
				////fine test

				NotificationBtn.DropDownItems.Add(newButton);
			 
			}*/
			//aggiunto per problema con chiusura del dropdown al mouse leave -> evento lanciato anche quando ti muovi sulle frecce del dropdown
			//NotificationBtn.DropDown.MaximumSize = new Size(400, Screen.PrimaryScreen.WorkingArea.Height);
		}

		/// <summary>
		/// todoandrea: da rimuovere? brutto
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public void ShowReconnectButton()
		{
			if (ControlInvokeRequired(this, () => UpdateNotificationButton()))
				return;
			var newButton = new ToolStripMenuItem("ReConnect");
			NotificationBtn.DropDownItems.Clear();
			NotificationBtn.DropDownItems.Add(newButton);
			newButton.Font = new System.Drawing.Font("Calibri", 10, FontStyle.Bold);
			newButton.ForeColor = Color.Black;
			newButton.Click += (sender, args) =>
			{
				NotificationManager.SubscribeToTheNotificationServiceAsync();
			};
		}

		/// <summary>
		/// Aggiorno il pulsante in base allo stato della connessione con XSocket
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public void UpdateStatus(ConnectionStatus status)
		{
			if (ControlInvokeRequired(this, () => UpdateStatus(status)))
				return;

			switch (status)
			{
				case (ConnectionStatus.Connected):
					NotificationBtn.SetStatus(NotificationStatus.Connected);
					UpdateNotificationButton();
					break;

				case (ConnectionStatus.Disconnected):
					NotificationBtn.SetStatus(NotificationStatus.Disconnected);
					ShowReconnectButton();
					break;

				case (ConnectionStatus.IsConnecting):
					NotificationBtn.SetStatus(NotificationStatus.IsConnecting);
					ShowReconnectButton();
					break;
			}
		}

		#endregion


	}
}
