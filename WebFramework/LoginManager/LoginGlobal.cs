using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SecurityLayer;
using Microarea.TaskBuilderNet.Core.SecurityLayer.SecurityLightObjects;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Activation;
using Microarea.TaskBuilderNet.Licence.Licence;
using Microarea.TaskBuilderNet.Licence.Licence.ConfigurationInfoProvider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Timers;
using System.Web;
using System.Xml;

namespace Microarea.WebServices.LoginManager
{
    //=========================================================================
    [Flags]
    public enum MobileCal { 
        None=0, 
        WMS=1,
        Manufacturing=2
    }

	/// <summary>
	/// Callse che contiene l'istanza statica dell'applicazione
	/// </summary>
	//=========================================================================
	internal class LoginApplication
	{
		private static LoginEngine1 loginEngine = null;
		/// <summary>
		/// Referenza all'applicazione, la prima volta che la si utilizza chiama in automatico il costruttore statico
		/// </summary>
		internal static LoginEngine1 LoginEngine { get { return loginEngine; } }

		/// <summary>
		/// Costruttore statico
		/// </summary>
		//---------------------------------------------------------------------------
		static LoginApplication()
		{
			loginEngine = new LoginEngine1();
		}

        /// <summary>
        /// classe istanziata da ApplicationServer che contiene membri statici relativi 
        /// a login manager, apre una sola connessione al db osl quando si connette il primo client
        /// e la chiude nella dispose
        /// </summary>
        //=========================================================================
        internal class LoginEngine1 : IDisposable
        {
            [DllImport("iphlpapi.dll", ExactSpelling = true)]
            internal static extern int SendARP(int DestIP, int SrcIP, [Out] byte[] pMacAddr, ref int PhyAddrLen);

            #region costanti
            /// <summary>
            /// Numero di chiamate isactivated che scatenano un controllo.
            /// </summary>
            private const int maxIsActivated = 100;
            private const int timeout = 15000;
            private const int minInterval = 600000;
            private const int maxInterval = 1200000;
            private readonly DateTime DefaultMinDateTime = new DateTime(1799, 12, 31, CultureInfo.InvariantCulture.Calendar);
            private readonly DateTime DefaultMaxDateTime = new DateTime(2208, 12, 31, CultureInfo.InvariantCulture.Calendar);
            #endregion

            #region membri privati
            private ContractDataBag contractData = new ContractDataBag();
            private SqlConnection SysDBConnection = new SqlConnection();
            private ActivationObject activationManager = null;
            private Hashtable activatedList = null;
            internal ActivationObject ActivationManager { get { return activationManager; } }
            internal Diagnostic diagnostic = new Diagnostic("LoginManager");    //Gestione errori
                                                                                /// <summary>
                                                                                /// timer che cadenzia i check
                                                                                /// </summary>
            //private		System.Timers.Timer	timer					= new System.Timers.Timer();
            /// <summary>
            /// timer che cadenzia il reperimento dei messaggi dal server microarea
            /// </summary>s
            private System.Timers.Timer messageTimer = new System.Timers.Timer();
            /// <summary>
            /// timer che decide quando fare il primo ping
            /// </summary>
            private System.Timers.Timer firstPingTimer = new System.Timers.Timer();
            //private		CounterManager		counterManager			= null;
            //private		CounterManager		CounterManager			{ get { return counterManager; } }
            /// <summary>
            /// numero di isactivated effettuate finora
            /// </summary>
            private int isActivatedCount = 0;
            private AuthenticationSlots authenticationSlots = null;
            string sessionGUID = string.Empty;
            private bool securityLightEnabled = false;
            private bool waiting = true;//dice che sono in attesa di fare un ping di successo per poter dire di essere attivato.
            private int failedPing = 0;//volte che il ping riprova (max4)
            private int failedPingViaSMS = 0;//volte che il ping riprova senza bloccarsi via sms, corrisponde ai tentativi sbagliati che gli concediamo (max4)
            internal InstallationVersion InstallationVer { get { return BasePathFinder.BasePathFinderInstance.InstallationVer; } }
            //{0}:Major,{1}:Minor,{2}:ServicePack,{3}:Build date(YYYYMMDD).
            private const string loginManagerVersionMask = "{0}.{1} ({2})";
            private MessageQueue messagesQueue;
            private MessagesPersister messagesPersister;
            private string connectionString = null;
            private string configurationHash = "LaMiaLibertaCominciaDoveFinisceLaVostra";
            int maxpingFailed = 3;//zerobased
            int maxpingFailedViaSMS = 20;//zerobased

            internal bool RefreshStatus = true;//nome offuscato

            MluExpiryDateCodeManager cm;
            int pingViaSMSCode = 0;

            private enum PingTimeEnum { Start, Short, Temp, Long, Temp1 };
            private PingTimeEnum pingTime = PingTimeEnum.Start;
            private PingTimeEnum PingTime { get { return pingTime; } set { pingTime = value; SetTimer(); } }

            /// il valore della data di scadenza dell'mlu viene inviato a mago tramite ping. viene salvato criptato nello userinfo.
            /// In caso di ping con SMS viene inserito nel codice di risposta tale data di scadenza e  il sistema la salva subito nello user info 
            /// e contemporaneamente se lo tiene in memoria. Viene acceduto quindi il dato in memoria e viene anche risalvato sulle userinfo alla chiusura di lginmanager, 
            /// per evitare che la gente sgamata cancelli il valore dalle dalle userinfo.
            /// Se sgamano tutto possono cancellare il valore e andare per un'ora ( prima che il programma si blocchi per il ping)
            //---------------------------------------------------------------------------
            private DateTime mluExpiredDate = DateTimeFunctions.MaxValue;
            //---------------------------------------------------------------------------
            public DateTime MluExpiredDate
            {
                get
                {
                    try
                    {
                        if (mluExpiredDate == DateTimeFunctions.MaxValue)//lo rileggo dalle userinfo solo se non l'ho mai letto, se no me lo tengo in memoria.
                            mluExpiredDate = activationManager.User.GetMluDate(out mluCancelled);
                    }
                    catch (Exception exc)
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, exc.Message);//" - Error 13322 - Error evaluating operation date: "+ ProductHashCode
                        mluExpiredDate = DateTimeFunctions.MinValue;//non sarà valida nessuna data
                    }
                    return mluExpiredDate;
                }
            }

            //booleano che completa l'indormazione della data, la data mi viene comunque inviata,se mlu disdettato  è la data scadenza, altrimenti è una data futura random.
            //in caso di data futura però io non so se è una data futura random con mlu ok o se è una data di scadenza dell'mlu futura ( mlu disdettato ma deve ancora scadere).
            bool mluCancelled = false;

            #endregion

            #region Proprietà
            internal bool ActivationManagerInitialized { get { return true; } }
            internal bool SqlConnectionOpened { get { return ((SysDBConnection.State & ConnectionState.Open) == ConnectionState.Open); } }
            internal bool SecurityLightEnabled { get { return securityLightEnabled; } }
            internal string LoginManagerVersion { get { return InstallationVer.Version; } }
            #endregion

            #region Costruttore

            //---------------------------------------------------------------------------
            internal LoginEngine1()
            {
            }

            #endregion

            #region Funzioni per il lock delle risorse

            string locker = "No locking method";
            //---------------------------------------------------------------------------
            internal bool TryLockResources()
            {
                try
                {
                    if (!Monitor.TryEnter(this, timeout))
                    {
                        diagnostic.Set
                            (DiagnosticType.LogInfo | DiagnosticType.Warning,
                            string.Format("Failed to lock LoginManager after {0} milliseconds. Method: {1}", timeout, locker));
                        return false;
                    }

                }
                catch (Exception exc)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "TryLockResources: " + exc.ToString());
                    throw exc;
                } try
                {

                    locker = (new StackTrace()).GetFrame(1).GetMethod().Name;
                }
                catch { locker = "Unknown method"; }
                return true;
            }
            //---------------------------------------------------------------------------
            internal void ReleaseResources()
            {
                try
                {
                    Monitor.Exit(this);
                    locker = "No locking method";
                }
                catch (Exception exc)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "ReleaseResources: " + exc.ToString());
                    throw exc;
                }
            }

            #endregion

            #region connessione al db di sistema
            //---------------------------------------------------------------------------
            internal bool IsValidTokenForConsole(string authenticationToken)
            {
                return authenticationToken == connectionString;
            }

            //---------------------------------------------------------------------------
            internal string GetSystemDBConnectionString(string authenticationToken)
            {
                if (String.IsNullOrEmpty(connectionString) ||
                    (!IsValidTokenForConsole(authenticationToken) && !IsValidToken(authenticationToken)))
                    return string.Empty;

                return connectionString;
            }

            //----------------------------------------------------------------------
            private bool OpenSysDBConnection()
            {
                if (SqlConnectionOpened)
                    return true;

                if (String.IsNullOrWhiteSpace(connectionString))
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, LoginManagerStrings.SysDBConnectionStringEmpty);
                    return false;
                }

                try
                {
                    SysDBConnection.ConnectionString = connectionString;
                    SysDBConnection.Open();
                    //diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "E' stata aperta una connessione al sysdb");
                    return true;
                }
                catch (SqlException exception)
                {
                    CloseSysDBConnection();
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, string.Format(LoginManagerStrings.ErrDBConnectionFailed, exception.Message));
                    Debug.WriteLine(string.Format("LoginGlobal:OpenSysDBConnection - Error: {0} Number: {1}", exception.Message, exception.Number.ToString()));
                    return false;
                }
            }

            //----------------------------------------------------------------------
            private void CloseSysDBConnection()
            {
                try
                {
                    if (SqlConnectionOpened)
                    {
                        SysDBConnection.Close();
                        SysDBConnection.Dispose();
                    }

                    SysDBConnection.ConnectionString = string.Empty;
                    //diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "E' stata chiusa una connessione al sysdb");
                }
                catch (SqlException exception)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "CloseSysDBConnection: " + exception.Message);
                }
            }

            /// <summary>
            /// Chiude la connessione al database condivisa da tutti i thread
            /// </summary>
            //-----------------------------------------------------------------------
            public void Dispose()
            {
                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, LoginManagerStrings.Ending);

                if (firstPingTimer != null)
                {
                    if (firstPingTimer.Enabled)
                        firstPingTimer.Stop();

                    firstPingTimer.Dispose();
                    firstPingTimer = null;
                }

                if (messageTimer != null)
                {
                    if (messageTimer.Enabled)
                        messageTimer.Stop();

                    messageTimer.Dispose();
                    messageTimer = null;
                }
                messagesPersister.StopListeningTo(messagesQueue);
                messagesPersister.Dispose();
                messagesQueue.Dispose();

                CloseSysDBConnection();
                GC.SuppressFinalize(this);
            }

            #endregion

            #region inizializzazione

            //---------------------------------------------------------------------------
            internal void LogInitEventArgs(InitEventArgs initEventArgs)
            {
                if (initEventArgs == null || string.IsNullOrWhiteSpace(initEventArgs.ToString()))
                    return;

                Microarea.TaskBuilderNet.Core.Generic.ProcessInfo pi = new Microarea.TaskBuilderNet.Core.Generic.ProcessInfo();

                diagnostic.Set(
                 DiagnosticType.LogInfo | DiagnosticType.Information,
                 String.Format(
                 "{1}{0}{0}{2}",
                 Environment.NewLine,
                 initEventArgs.ToString(),
                 pi.ToString())
                 );
            }

            //---------------------------------------------------------------------------
            internal string GetSMSHeaderImage()
            {
                //recupera la header image da  da re alla pagina dell'sms, passandola come stringa encodata base 64 da mettere nel  src
                try
                {
                    string key = GetBrandedKey("HeaderSMS");
                    if (String.IsNullOrWhiteSpace(key)) return string.Empty;
                    NameSpace n = new NameSpace(key);
                    if (n == null) return string.Empty;

                    string path = BasePathFinder.BasePathFinderInstance.GetImagePath(n);
                    if (String.IsNullOrWhiteSpace(path) || !File.Exists(path))
                        return string.Empty;

                    string pre = "data:image/png;base64,";

                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                    // Create a byte array of file stream length
                    byte[] imageData = new byte[fs.Length];

                    //Read block of bytes from stream into the byte array
                    fs.Read(imageData, 0, System.Convert.ToInt32(fs.Length));

                    //Close the File Stream
                    fs.Close();

                    if (imageData == null || imageData.Length == 0) return string.Empty;
                    string encoded = Convert.ToBase64String(imageData);
                    return pre + encoded;
                }
                catch { }

                return string.Empty;
            }


            //---------------------------------------------------------------------------
            internal void SetCulture()
            {
                try
                {
                    string cui = InstallationData.ServerConnectionInfo.PreferredLanguage;
                    string c = InstallationData.ServerConnectionInfo.ApplicationLanguage;

                    if (!String.IsNullOrEmpty(c))
                        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(c);

                    if (!String.IsNullOrEmpty(cui))
                        Thread.CurrentThread.CurrentUICulture = new CultureInfo(cui);
                }
                catch
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Impossible set culture to LoginManager");
                }
            }

            //torna l'hash risultante dalla somma degli hash della lista di moduli attivati. 
            //questo copre tutte le casistiche, anche in demo e dvlp e rnfs 
            //dove la lista d moduli attivati cambia 
            //ma non cambia la chiave di attivazione (perchè un solo serial attiva tutto)
            //-----------------------------------------------------------------
            internal string GetConfigurationHash()
			{
				return configurationHash;
			}

            //-----------------------------------------------------------------------
            internal bool VerifyDBSize()
            {

                bool nonfare = LoginApplication.LoginEngine.IsActivatedInternal("ClientNet", "DBNoLimit");
                bool fare = GetDBNetworkType() == DBNetworkType.Small;
                return fare && !nonfare;

            }
            //---------------------------------------------------------------------------
            internal int Init(bool reboot, InitEventArgs initEventArgs)
            {

                if (!TryLockResources())
                    return (int)LoginReturnCodes.BusyResourcesError;
                try
                {
                    //waiting = true;//lascio che rimanga quello che è, in modo che se lm parte è a true (default), se ha appena fatto un ping o una registrazione è impostato correttamente 
                    pingViaSMSCode = 0;
                    //devo reinizializzare l'oggetto statico per rileggere le modifiche
                    BasePathFinder.ClearBasePathFinderInstance();
                    InstallationData.Clear();
                    //imposto la cultura letta dal serverconnectinconfig
                    SetCulture();
                    diagnostic.Installation = BasePathFinder.BasePathFinderInstance.Installation;
                    LogInitEventArgs(initEventArgs);

                    #region inizializzazione MessagesQueue

                    messagesPersister = new MessagesPersister
                        (
                        BasePathFinder.BasePathFinderInstance.GetMessagesQueuePath(),
                        new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
                        );
                    try
                    {
                        messagesQueue = messagesPersister.Load() as MessageQueue;
                        //ripulisco dei messaggi che non devono essere mostrati perchè scadono col riavvio

                        if (messagesQueue != null) messagesQueue.PurgeOnRestart();
                    }
                    catch
                    {
                        messagesQueue = null;
                        try { File.Delete(BasePathFinder.BasePathFinderInstance.GetMessagesQueuePath()); }
                        // in caso di eccezione durante la lettura dei messaggi cancelliamo il file eliminando eventuali messaggi vecchi\non leggibili
                        catch { }
                    }
                    if (messagesQueue == null)
                        messagesQueue = new MessageQueue();
                    messagesPersister.ListenTo(messagesQueue);

                    #endregion

                    #region Inizializzazione Activation Manager
                    if (!InitActivation())
                        return (int)LoginReturnCodes.NoLicenseError;
                    actstate = activationManager.GetState();
                    configurationHash = activationManager.GetConfigurationHash();
                    if (!IsActivated())
                        return (int)LoginReturnCodes.UnregisteredProduct;
                    activatedList = activationManager.GetActivatedList();
                    diagnostic.WriteChildDiagnostic(activationManager.Diagnostic, true);
                    #endregion
                    
                    #region CONNECTIONSTRING
                    CloseSysDBConnection();

                    //Inizializzo la variabile che contiene la stringa di connessione decriptata 
                    //e la tiene in memoria sempre, rinfrescandola ad ogni init, 
                    //in modo da non doverla decriptare ad ogni richiesta
                    connectionString = InstallationData.ServerConnectionInfo.SysDBConnectionString;
                    if (String.IsNullOrEmpty(connectionString))
                        return (int)LoginReturnCodes.MissingConnectionString;

                    if (!OpenSysDBConnection())
                        return (int)LoginReturnCodes.SysDBConnectionFailure;
                    #endregion

                    #region PING Timer initialization
                    //stoppo se fosse già inizializzato
                    if (firstPingTimer.Enabled)
                    {
                        firstPingTimer.Stop();
                        firstPingTimer.Elapsed -= new ElapsedEventHandler(FirtPingTimer_Elapsed);//perchè faccio questo non me lo ricordo...
                    }
                    firstPingTimer.Elapsed += new ElapsedEventHandler(FirtPingTimer_Elapsed);
                    //restarto col timer impostato correttamente
                    SetTimer();

                    #endregion

                    #region timermesaggi
                    //avvio il timer dei messaggi
                    if (messageTimer.Enabled)
                    {
                        messageTimer.Stop();
                        messageTimer.Elapsed -= new ElapsedEventHandler(GetAdvertisement);
                    }
                    Random r = new Random();
                    messageTimer.Interval = r.Next(GetMilliSeconds(0, 21, 0), GetMilliSeconds(0, 27, 0));
                    // 24 ore: coloro che lasciano il server di mago sempre acceso faranno 1 chiamata al giorno
                    //modifica del 28/3/12(da 8 ore a 24) perchè 3 chiamate al giorno erano un po'troppe ed inutili per il carico di ballon che in realtà è piccolo
                    // coloro che lo accendono e spengono faranno una chiamata al giorno.
                    messageTimer.Elapsed += new ElapsedEventHandler(GetAdvertisement);
                    messageTimer.Start();

                    try
                    {
                        new Thread(new ThreadStart(GetFirstAdvertisement)).Start();
                    }
                    catch (System.Security.SecurityException) { }
                    catch (OutOfMemoryException) { }
                    #endregion

                    #region Slot init
                    int slotsRes = -1;
                    try
                    {
                        authenticationSlots = AuthenticationSlots.Create(activationManager);
                        slotsRes = authenticationSlots.Init(activationManager, SysDBConnection, diagnostic);
                    }
                    catch (Exception exc)
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "Init error: " + exc.ToString());
                    }
                    if (slotsRes != (int)LoginReturnCodes.NoError)
                        return slotsRes;
                    #endregion


                    ClearMobileCalAssignment();

                    #region LoginMngSession
                    //Scrivo il file con il token di sessione utilizzato dai client che fanno login in sicurezza integrata
                    //devono leggere il file e passare il contenuto nel parametro password del metodo login
                    try
                    {
                        using (StreamWriter sr = new StreamWriter(BasePathFinder.BasePathFinderInstance.GetLoginMngSessionFile(), false, System.Text.Encoding.UTF8))
                        {
                            sessionGUID = Guid.NewGuid().ToString();
                            sr.Write(sessionGUID);
                        }
                    }
                    catch (Exception exc)
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "LMSession error: " + exc.Message);
                    }

                    #endregion

                    //imposto dati per far risultare mlu a posto almeno fino al primo ping
                    contractData.ContractData = ContractData.ContractExpiration;
                    contractData.PaymentRenewalDate = DateTimeFunctions.MaxValue;

                    securityLightEnabled = IsActivatedInternal("MicroareaConsole", "SecurityLight") &&
                                            SecurityLightManager.ExistAccessRights(this.SysDBConnection);

                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, LoginManagerStrings.InitSuccess);

                    // se EasyAttachment e' attivato allora inizializzo EasyAttachmentSync web service
                    if (IsActivatedInternal(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment))
                    {
                        EasyAttachmentSync eaSync = new EasyAttachmentSync(BasePathFinder.BasePathFinderInstance.EasyAttachmentSyncUrl);

                        try
                        {
                            if (eaSync.Init("{2E8164FA-7A8B-4352-B0DC-479984070507}"))
                                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, LoginManagerStrings.EASyncInitSuccess);
                        }
                        catch (Exception exc)
                        {
                            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, string.Format(LoginManagerStrings.EASyncInitError, exc.ToString()));
                        }
                    }

                    // se DataSynchronizer e' attivato, allora inizializzo il relativo web service
                    if (IsActivatedInternal(NameSolverStrings.Extensions, DatabaseLayerConsts.DataSynchroFunctionality))
                    {
                        try
                        {
                            DataSynchronizer dSync = new DataSynchronizer(BasePathFinder.BasePathFinderInstance.DataSynchronizerUrl);
                            List<DataSynchroDatabaseInfo> dataSynchroDbList = GetDataSynchroDatabasesInfo();

                            foreach (DataSynchroDatabaseInfo elem in dataSynchroDbList)
                            {
                                if (dSync.Init(elem.LoginName, elem.LoginPassword, elem.LoginWindowsAuthentication, elem.CompanyName))
                                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, LoginManagerStrings.DSyncInitSuccess);
                            }
                        }
                        catch (Exception exc)
                        {
                            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, string.Format(LoginManagerStrings.DSyncInitError, exc.ToString()));
                        }
                    }

                    WakeUpTbSender();

                    return (int)LoginReturnCodes.NoError;
                }
                catch (Exception exc)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "Init error: " + exc.ToString());
                    return (int)LoginReturnCodes.Error;
                }
                finally
                {
                    ReleaseResources();
                }
            }

            //---------------------------------------------------------------------
            private void ClearMobileCalAssignment()
            {

                MobileCal mc = MobileCal.None;
                if (!IsActivatedInternal("ERP", "WMSMobileLicence"))
                    mc |= MobileCal.WMS;
                if (!IsActivatedInternal("ERP", "ManufacturingMobileLicence"))
                    mc |= MobileCal.Manufacturing;
                authenticationSlots.ClearMobileCalAssignment(mc);
            }

            ///<summary>
            /// Sia MailConnector che EasyAttachment svegliano il TbSender
            ///</summary>
            //---------------------------------------------------------------------
            private void WakeUpTbSender()
            {
                try
                {
                    if (
                        IsActivatedInternal(NameSolverStrings.Extensions, "MailConnector") ||
                        IsActivatedInternal(NameSolverStrings.Extensions, DatabaseLayerConsts.EasyAttachment)
                        )
                    {
                        TbSenderWrapper tbSender = new TbSenderWrapper(BasePathFinder.BasePathFinderInstance.TbSenderUrl);
                        tbSender.WakeUp();
                    }
                }
                catch { }
            }

            //---------------------------------------------------------------------------
            internal bool InitActivation()
            {
                try
                {
                    IConfigurationInfoProvider provider = new FSProviderForInstalled(BasePathFinder.BasePathFinderInstance);
                    provider.AddFunctional = true;

                    activationManager = new ActivationObject(provider);

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    if (activationManager == null)
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, ex.ToString());
                    else
                    {
                        // TODO - implementare diagnostica in provider e fargli segnalare errore di Init()
                        if (!diagnostic.WriteChildDiagnostic(activationManager.Diagnostic, true))
                            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, LoginManagerStrings.ErrInitAcivationMng);
                    }
                    return false;

                }
                return true;
            }

            //---------------------------------------------------------------------------
            internal void RefreshSecurityStatus()
            {
                securityLightEnabled = IsActivatedInternal("MicroareaConsole", "SecurityLight") &&
                                        SecurityLightManager.ExistAccessRights(this.SysDBConnection);
            }

            #endregion

            #region funzioni non esposte

            /// <summary>
            /// data l'ID di una company verifica il blocco o meno del database aziendale
            /// da parte del plug-in DBAdmin della console per operazioni di aggiornamento
            /// </summary>
            /// <param name="companyId"></param>
            /// <returns></returns>
            //-----------------------------------------------------------------------
            private bool IsLockedDatabase(int companyID)
            {
                if (!OpenSysDBConnection())
                    return false;

                bool updating = false;
                string query = "SELECT Updating FROM MSD_Companies WHERE CompanyId = @CompanyID";

                SqlCommand aSqlCommand = new SqlCommand();
                SqlDataReader aSqlDataReader = null;

                try
                {
                    aSqlCommand.CommandText = query;
                    aSqlCommand.Connection = SysDBConnection;
                    aSqlCommand.Parameters.AddWithValue("@CompanyID", companyID);
                    aSqlDataReader = aSqlCommand.ExecuteReader();

                    if (aSqlDataReader.Read())
                        updating = (bool)aSqlDataReader[LoginManagerStrings.Updating];

                    return updating;
                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsLockedDatabase: " + err.Message);
                    return false;
                }
                finally
                {
                    if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
                        aSqlDataReader.Close();
                }
            }

            //-----------------------------------------------------------------------
            private bool IsValidCompany(int companyID)
            {
                if (!OpenSysDBConnection())
                    return false;

                bool valid = false;
                string query = "SELECT IsValid FROM MSD_Companies WHERE CompanyId = @CompanyId";

                SqlCommand aSqlCommand = new SqlCommand();
                SqlDataReader aSqlDataReader = null;

                try
                {
                    aSqlCommand.CommandText = query;
                    aSqlCommand.Connection = SysDBConnection;
                    aSqlCommand.Parameters.AddWithValue("@CompanyId", companyID);
                    aSqlDataReader = aSqlCommand.ExecuteReader();

                    if (aSqlDataReader.Read())
                        valid = (bool)aSqlDataReader[LoginManagerStrings.IsValid];

                    return valid;
                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsValidCompany: " + err.Message);
                    return false;
                }
                finally
                {
                    if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
                        aSqlDataReader.Close();
                }
            }

            /// <summary>
            /// se l'Edition è Standard, fa una query sul db di sistema ed estrae le prime 2 aziende (come accade anche
            /// lato Administration Console). Poi fa il match tra l'azienda scelta e quelle estratte ed eventualmente
            /// blocca la connessione al db aziendale.
            /// </summary>
            /// <returns></returns>
            //---------------------------------------------------------------------------
            private bool IsAdmittedCompany(string companyName)
            {
                if (!OpenSysDBConnection())
                    return false;

                string query = "SELECT MSD_Companies.Company, MSD_Providers.Provider  " +
                        "FROM   MSD_Companies, MSD_Providers " +
                        "WHERE  MSD_Companies.ProviderId = MSD_Providers.ProviderId " +
                        "ORDER  BY MSD_Companies.Company";

                SqlCommand aSqlCommand = new SqlCommand();
                SqlDataReader aSqlDataReader = null;

                try
                {
                    aSqlCommand.CommandText = query;
                    aSqlCommand.Connection = SysDBConnection;
                    aSqlDataReader = aSqlCommand.ExecuteReader();

                    // scorro il reader solo nelle prime due righe estratte, che identificano le aziende alle quali mi posso connettere
                    for (int i = 0; i < 2; i++)
                    {
                        aSqlDataReader.Read();
                        if (string.Compare(aSqlDataReader["Company"].ToString(), companyName, true, CultureInfo.InvariantCulture) == 0)
                            return true;
                    }
                }
                catch (Exception e)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsAdmittedCompany: " + e.Message);
                }
                finally
                {
                    if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
                        aSqlDataReader.Close();
                }

                return false;
            }

            /*//---------------------------------------------------------------------------
			internal bool ReloadGlobalSettings()
			{
				Type t = GetParametersMngClass("ProtocolManager");
				if (t == null)
					return false;

				try
				{
					return (bool) t.InvokeMember(
						"ReloadGlobalSettings", 
						BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, 
						null, 
						null, 
						null);
				}
				catch(Exception e)
				{
					diagnostic.Set(DiagnosticType.Error, "Error : " + ErrorMessage.PongReflectionError + " " + e.Message);
					return false;
				}
			
			}*/

            #endregion //funzioni non esposte

            #region funzioni per login e logout

            //----------------------------------------------------------------------
            internal bool ValidateDomainCredentials(string user, string pwd)
            {
                string[] splitString = { "\\" };
                bool valid = false;
                string domain = string.Empty;
                try
                {
                    if (user.IndexOf("\\") > 0)
                    {
                        string[] tokens = user.Split(splitString, StringSplitOptions.None);
                        domain = tokens[0];
                        user = tokens[1];
                    }
                    if (domain.Length == 0 || string.Compare(domain, System.Environment.MachineName, true) == 0)
                    {
                        domain = System.Environment.MachineName;
                        using (PrincipalContext context = new PrincipalContext(ContextType.Machine, domain))
                            valid = context.ValidateCredentials(user, pwd, ContextOptions.Negotiate);
                    }
                    else
                    {
                        using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domain))
                            valid = context.ValidateCredentials(user, pwd, ContextOptions.Negotiate);
                    }
                }
                catch (Exception exc)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "ValidateDomainCredentials: " + exc.Message);
                    valid = false;
                }

                return valid;
            }
            //----------------------------------------------------------------------
            internal int ValidateUser
                (
                string userName,
                string password,
                bool winNTAuthentication,
                out int loginId,
                out StringCollection userCompanies,
                out bool userCannotChangePassword,
                out bool userMustChangePassword,
                out DateTime expiredDatePassword,
                out bool passwordNeverExpired,
                out bool expiredDateCannotChange
                )
            {
                loginId = -1;

                userCompanies = null;
                userCannotChangePassword = false;
                userMustChangePassword = false;
                expiredDatePassword = DefaultMaxDateTime;
                passwordNeverExpired = false;
                expiredDateCannotChange = false;

                if (!OpenSysDBConnection())
                    return (int)LoginReturnCodes.SysDBConnectionFailure;

                bool wnta = false;
                bool webLogin = false;
                bool gdiLogin = true;

                if (!GetLoginId(userName, out loginId, out wnta, out webLogin, out gdiLogin))
                    return (int)LoginReturnCodes.InvalidUserError;

                if (winNTAuthentication != wnta)
                    return (int)LoginReturnCodes.AuthenticationTypeError;

                if (IsLoginLocked(loginId))
                    return (int)LoginReturnCodes.LoginLocked;

                //Verifico che la password inserita sia valida
                if (!IsValidUser(loginId, userName, password, winNTAuthentication))
                {
                    AddWrongPwdLoginCount(loginId);
                    return (int)LoginReturnCodes.InvalidUserError;
                }
                else
                    ClearWrongPwdLoginCount(loginId);

                userCompanies = EnumCompaniesById(loginId);

                if (
                    !GetPasswordInfo
                    (
                    loginId,
                    out userCannotChangePassword,
                    out userMustChangePassword,
                    out expiredDatePassword,
                    out passwordNeverExpired,
                    out expiredDateCannotChange
                    )
                    )
                    return (int)LoginReturnCodes.UserMustChangePasswordError;

                return (int)LoginReturnCodes.NoError;
            }

            //			//-----------------------------------------------------------------------
            //			internal int SsoLogin(LoginProperties loginProperties)
            //			{
            //				if (loginProperties == null)
            //					return (int)LoginReturnCodes.SsoTokenEmpty;

            //				string ssoToken = loginProperties.SsoToken;

            //				if (ssoToken.IsNullOrWhiteSpace())
            //					return (int)LoginReturnCodes.SsoTokenEmpty;

			//-----------------------------------------------------------------------
			internal int Login
				(
				ref string userName,
				ref string companyName,
				string password,
				string askingProcess,
				string macIp,
				bool overWriteLogin,
				out bool admin,
				out string authenticationToken,
				out int companyId,
				out string dbName,
				out string dbServer,
				out int providerId,
				out bool security,
				out bool auditing,
				out bool useKeyedUpdate,
				out bool transactionUse,
				out string preferredLanguage,
				out string applicationLanguage,
				out string providerName,
				out string providerDescription,
				out bool useConstParameter,
				out bool stripTrailingSpaces,
				out string providerCompanyConnectionString,
				out string nonProviderCompanyConnectionString,
				out string dbUser,
				out string activationDB//sempre string.empty?????
				)
			{
				admin = false;
				authenticationToken = string.Empty;
				companyId = -1;
				dbName = string.Empty;
				dbServer = string.Empty;
				providerId = -1;
				security = false;
				auditing = false;
				useKeyedUpdate = false;
				transactionUse = false;
				preferredLanguage = string.Empty;
				applicationLanguage = string.Empty;
				providerName = string.Empty;
				providerDescription = string.Empty;
				useConstParameter = false;
				stripTrailingSpaces = false;
				providerCompanyConnectionString = string.Empty;
				nonProviderCompanyConnectionString = string.Empty;
				dbUser = string.Empty;
				activationDB = string.Empty;

                if (!IsActivated())
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "Login: UnregisteredProduct" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                    return (int)LoginReturnCodes.UnregisteredProduct;
                }

                if (!OpenSysDBConnection())
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "Login: OpenSysDBConnection failed" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                    TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    return (int)LoginReturnCodes.SysDBConnectionFailure;
                }

                int loginResult = 0;
                try
                {
                    if (!IsUserAssociatedToCompany(userName, companyName))
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: IsUserAssociatedToCompany" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.InvalidUserError;
                    }

                    int loginId = -1;
                    bool winNTAuth = false;

                    bool webLogin = false;
                    bool gdiLogin = true;
                    bool concurrent = false;

                    if (!GetLoginId(userName, out loginId, out winNTAuth, out webLogin, out gdiLogin, out concurrent))
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: GetLoginId" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.InvalidUserError;
                    }

                    //l'utente non può connettersi ne via web ne gdi
                    if (!webLogin && !gdiLogin && !concurrent)
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: !webLogin && !gdiLogin" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.InvalidUserError;
                    }

                    if (IsLoginLocked(loginId))
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: IsLoginLocked" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.LoginLocked;
                    }

                    if (askingProcess == ProcessType.SchedulerAgent || askingProcess == ProcessType.SchedulerManager)
                    {
                        if (sessionGUID != password.Reverse())
                        {
                            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: Scheduler login failed" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                            return (int)LoginReturnCodes.InvalidUserError;
                        }
                    }
                    else
                    {
                        //Verifico che la password inserita sia = a quella scritta nel db
                        if (!IsValidUser(loginId, userName, password, winNTAuth))
                        {
                            AddWrongPwdLoginCount(loginId);
                            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: IsValidUser" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                            TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                            return (int)LoginReturnCodes.InvalidUserError;
                        }
                    }

                    ClearWrongPwdLoginCount(loginId);

                    DateTime ExpiredDatePassword = DefaultMinDateTime;
                    bool UserMustChangePassword = false;
                    bool UserCannotChangePassword = false;
                    bool PasswordNeverExpired = true;
                    bool ExpiredDateCannotChange = true;

                    if (!
                        GetPasswordInfo
                        (
                        loginId,
                        out UserCannotChangePassword,
                        out UserMustChangePassword,
                        out ExpiredDatePassword,
                        out PasswordNeverExpired,
                        out ExpiredDateCannotChange
                        )
                        )
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: GetPasswordInfo" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.UserMustChangePasswordError; ;
                    }

                    if (UserMustChangePassword)
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: UserMustChangePassword" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.UserMustChangePasswordError;
                    }

                    companyId = GetCompanyID(companyName);
                    if (companyId == -1)
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: Invalid company" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.InvalidCompanyError;
                    }

                    admin = IsAdmin(loginId, companyId);

                    GetUserLanguages(loginId, companyId, out preferredLanguage, out applicationLanguage);

                    // Verifico se l'azienda scelta non sia lockata dal processo di aggiornamento
                    // del database aziendale da parte della console
                    if (IsLockedDatabase(companyId))
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: IsLockedDatabase" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.LockedDatabaseError;
                    }

                    // Se l'azienda risulta Invalida non faccio proseguire. Lo stato di invalido è messo dal migrationkit se 
                    // il database risulta da migrare
                    if (!IsValidCompany(companyId))
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: IsValidCompany" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.InvalidDatabaseError;
                    }

                    // se l'Edition è Standard verifico se mi posso connettere all'azienda selezionata 
                    // (ovvero se è tra le prime due aziende censite e gestite dalla Console)
                    if (string.Compare(GetEdition(), NameSolverStrings.StandardEdition, true, CultureInfo.InvariantCulture) == 0)
                        if (!IsAdmittedCompany(companyName))
                        {
                            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: IsAdmittedCompany" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                            TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                            return (int)LoginReturnCodes.NoAdmittedCompany;
                        }

                    int port = 0;
                    if (!GetCompanyInfo(companyId, out companyName, out dbName, out dbServer, out providerId, out port))
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: GetCompanyInfo" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.InvalidCompanyError;
                    }

                    bool rowSecurity = false;
                    bool dataSynchro = false;
                    if (!GetOslStatus(companyId, out security, out auditing, out rowSecurity, out dataSynchro))
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: GetOslStatus" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.InvalidCompanyError;
                    }

                    bool useUnicode = false;
                    if (!GetConnectionParams
                        (
                        companyId,
                        out useKeyedUpdate,
                        out transactionUse,
                        out useUnicode
                        )
                        )
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: GetConnectionParams" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.ConnectionParamsError;
                    }

                    if (!GetProviderInfo
                        (
                        providerId,
                        out providerName,
                        out providerDescription,
                        out useConstParameter,
                        out stripTrailingSpaces
                        )
                        )
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: GetConnectionParams" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                        TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return (int)LoginReturnCodes.ProviderError;
                    }

                    providerCompanyConnectionString = GetCompanyConnectionString(loginId, companyId, true, dbName, dbServer, providerName, port);
                    nonProviderCompanyConnectionString = GetCompanyConnectionString(loginId, companyId, false, dbName, dbServer, providerName, port);
                    dbUser = GetDBUser(nonProviderCompanyConnectionString);

                    loginResult = authenticationSlots.Login(loginId, userName, macIp, companyId, companyName, askingProcess, webLogin, gdiLogin, concurrent, overWriteLogin, out authenticationToken);
                }
                catch (Exception err)
                {
                    TraceAction(companyName, userName, TraceActionType.LoginFailed, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "Login: " + err.Message + string.Format(" User:{0}, Company:{1}", userName, companyName));
                    return (int)LoginReturnCodes.GenericLoginFailure;
                }

                if (loginResult == (int)LoginReturnCodes.NoError)
                {
                    //login avvenuta con successo
                    //salvo la company nell'array delle company usate per poterla inviare col ping al server -> dalla 3.5 me lo fa mago con la P.I.
                    // if (!String.IsNullOrWhiteSpace(companyName) && !usedCompanies.Contains(companyName)) usedCompanies.Add(companyName);
                    TraceAction(companyName, userName, TraceActionType.Login, askingProcess, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    if (InstallationData.ServerConnectionInfo.EnableLMVerboseLog)//loggo solo se verboso
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, "Login: success" + string.Format(" User:{0}, Company:{1}", userName, companyName));
                    //Prendo il nome utente e company come sono stati inseriti nel db (col case)
                    GetAuthenticationNames(authenticationToken, out userName, out companyName);

                }
                else
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Login: authenticationSlots.Login result = " + loginResult + string.Format(" User:{0}, Company:{1}", userName, companyName));

                RefreshSecurityStatus();//aggiunto qui perchè prima veniva fatto dal client al  termine della login in logincontrol senza apparente motivo che fosse la, anche se quel valore
                return loginResult;
            }

            //-----------------------------------------------------------------------
            internal bool RefreshWMSSlot(string authenticationToken)
            {
                AuthenticationSlot s = authenticationSlots.GetSlot(authenticationToken, true);
                if (s == null) return false;
                s.IsAliveWMS = true;
                return true;

            }

            //--------------------------------------------------------------------------			
            internal void Logout(string authenticationToken)
            {
                if (string.IsNullOrWhiteSpace(authenticationToken))
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "Logout: empty token.");
                    return;
                }
                string userName = string.Empty;
                string companyName = string.Empty;
                GetAuthenticationNames(authenticationToken, out userName, out companyName);

                TraceAction(companyName, userName, TraceActionType.Logout, string.Empty, string.Empty, HttpContext.Current.Request.UserHostAddress);
                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(companyName) && InstallationData.ServerConnectionInfo.EnableLMVerboseLog)//loggo solo se verboso
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, string.Format("Logout user={0} company={1}", userName, companyName));

                authenticationSlots.Logout(authenticationToken);
            }

            //---------------------------------------------------------------------------
            internal int ChangePassword(string userName, string oldPassword, string newPassword)
            {
                if (!OpenSysDBConnection())
                {
                    TraceAction(string.Empty, userName, TraceActionType.ChangePasswordFailed, string.Empty, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    return (int)LoginReturnCodes.SysDBConnectionFailure;
                }

                int loginId = -1;
                bool wna = false;
                bool webLogin = false;
                bool gdiLogin = true;

                if (!GetLoginId(userName, out loginId, out wna, out webLogin, out gdiLogin))
                {
                    TraceAction(string.Empty, userName, TraceActionType.ChangePasswordFailed, string.Empty, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    return (int)LoginReturnCodes.InvalidUserError;
                }

                if (IsLoginLocked(loginId))
                {
                    TraceAction(string.Empty, userName, TraceActionType.LoginFailed, string.Empty, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    return (int)LoginReturnCodes.LoginLocked;
                }

                //verifico che la vecchia password sia buona
                if (!IsValidUser(loginId, userName, oldPassword, false))
                {
                    TraceAction(string.Empty, userName, TraceActionType.ChangePasswordFailed, string.Empty, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    return (int)LoginReturnCodes.InvalidUserError;
                }

                if (HasUserAlreadyChangedPasswordToday(userName))
                {
                    TraceAction(string.Empty, userName, TraceActionType.ChangePasswordFailed, string.Empty, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    return (int)LoginReturnCodes.PasswordAlreadyChangedToday;
                }

                //prendo le informazioni relative al cambio pwd dal db
                bool userCannotChangePassword = false;
                bool userMustChangePassword = false;
                DateTime expiredDatePassword = DefaultMinDateTime;
                bool passwordNeverExpired = true;
                bool expiredDateCannotChange = false;

                if (newPassword.Length < InstallationData.ServerConnectionInfo.MinPasswordLength)
                {
                    TraceAction(string.Empty, userName, TraceActionType.ChangePasswordFailed, string.Empty, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    return (int)LoginReturnCodes.PasswordTooShortError;
                }

                //errore!
                if (!
                    GetPasswordInfo
                    (
                    loginId,
                    out userCannotChangePassword,
                    out userMustChangePassword,
                    out expiredDatePassword,
                    out passwordNeverExpired,
                    out expiredDateCannotChange
                    )
                    )
                {
                    TraceAction(string.Empty, userName, TraceActionType.ChangePasswordFailed, string.Empty, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    return (int)LoginReturnCodes.InvalidUserError;
                }

                //l'utente non può cambiare la pwd
                if (userCannotChangePassword)
                {
                    TraceAction(string.Empty, userName, TraceActionType.ChangePasswordFailed, string.Empty, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    return (int)LoginReturnCodes.CannotChangePasswordError;
                }

                //la password è spirata e non può essere cambiata
                if (expiredDateCannotChange && expiredDatePassword < DateTime.Today)
                {
                    TraceAction(string.Empty, userName, TraceActionType.ChangePasswordFailed, string.Empty, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    return (int)LoginReturnCodes.PasswordExpiredError;
                }

                //aggiorno i dati di pwd
                string query = "UPDATE MSD_Logins SET Password = @password, " +
                                "UserMustChangePassword = @MustChange, " +
                                "ExpiredDatePassword = @ExpiredDatePassword WHERE LoginId = @LoginID";

                SqlCommand aSqlCommand = new SqlCommand();

                try
                {
                    aSqlCommand.Connection = SysDBConnection;
                    aSqlCommand.CommandText = query;

                    aSqlCommand.Parameters.AddWithValue("@password", Microarea.TaskBuilderNet.Core.Generic.Crypto.Encrypt(newPassword));
                    DateTime newExpiredDatePassword = DateTime.Now.AddDays(InstallationData.ServerConnectionInfo.PasswordDuration);
                    aSqlCommand.Parameters.AddWithValue("@MustChange", false);
                    aSqlCommand.Parameters.AddWithValue("@ExpiredDatePassword", newExpiredDatePassword.Date);
                    aSqlCommand.Parameters.AddWithValue("@LoginID", loginId);

                    aSqlCommand.ExecuteNonQuery();
                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "ChangePassword: " + err.Message);
                    TraceAction(string.Empty, userName, TraceActionType.ChangePasswordFailed, string.Empty, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    return (int)LoginReturnCodes.InvalidUserError;
                }

                TraceAction(string.Empty, userName, TraceActionType.ChangePassword, string.Empty, string.Empty, HttpContext.Current.Request.UserHostAddress);

                return (int)LoginReturnCodes.NoError;
            }

            //--------------------------------------------------------------------------
            internal bool CanTokenRunTB(string token)
            {
                return GetCalType(token) != LoginSlotType.Invalid;
            }

            //--------------------------------------------------------------------------
            internal LoginSlotType GetCalType(string token)
            {
                if (String.IsNullOrEmpty(token))
                    return LoginSlotType.Invalid;

                AuthenticationSlot ast = authenticationSlots.GetSlot(token);
                if (ast == null)
                    return LoginSlotType.Invalid;

                return ast.CalType;
            }

            //--------------------------------------------------------------------------
            internal LoginSlotType GetSlotType(string token)
            {
                if (String.IsNullOrEmpty(token))
                    return LoginSlotType.Invalid;

                AuthenticationSlot ast = authenticationSlots.GetSlot(token);
                if (ast == null)
                    return LoginSlotType.Invalid;

                return ast.SlotType;
            }

            //----------------------------------------------------------------------
            internal MobileCal ConsumeMobileCal(string authenticationToken)
            {
                MobileCal mc = MobileCal.None;

                if (
                    authenticationToken == null ||
                    authenticationToken.Trim().Length == 0 ||
                    !IsValidToken(authenticationToken)
                    )
                    return MobileCal.None;

                AuthenticationSlot slot = authenticationSlots.GetSlot(authenticationToken);
                if (slot == null)
                    return MobileCal.None;

                //Qui devo verificare se l'utente è stato associato.se non ci sono associazioni allora come adesso
                //se almeno uno dei due lo è allora si va con le info del db

                mc = authenticationSlots.ConsumeMobileCal(slot.LoginID, authenticationToken, slot.MACAddress);
                return mc;
            }


            #endregion

            #region funzioni per il lock degli utenti causa errata login
            //----------------------------------------------------------------------
            private int GetLoginLockCount(int loginID)
            {
                if (!OpenSysDBConnection())
                    return -1;

                string query = "SELECT LoginFailedCount FROM MSD_Logins WHERE LoginId = @loginID";

                int loginFailedCount = 0;

                SqlCommand aSqlCommand = new SqlCommand();
                aSqlCommand.Parameters.AddWithValue("@loginID", loginID);
                SqlDataReader reader = null;
                try
                {
                    aSqlCommand.CommandText = query;
                    aSqlCommand.Connection = SysDBConnection;

                    reader = aSqlCommand.ExecuteReader();
                    if (reader.Read())
                        loginFailedCount = (int)reader["LoginFailedCount"];
                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetLoginLockCount: " + err.Message);
                    return -1;
                }
                finally
                {
                    if (reader != null && !reader.IsClosed)
                        reader.Close();
                }

                return loginFailedCount;
            }

            //----------------------------------------------------------------------
            private bool IsLoginLocked(int loginID)
            {
                if (!OpenSysDBConnection())
                    return false;

                //Prendo lo LoginId dell'utente
                string query = "SELECT Locked FROM MSD_Logins WHERE LoginId = @loginID";

                bool locked = false;
                SqlCommand aSqlCommand = new SqlCommand();
                SqlDataReader reader = null;
                try
                {
                    aSqlCommand.CommandText = query;
                    aSqlCommand.Parameters.AddWithValue("@loginID", loginID);
                    aSqlCommand.Connection = SysDBConnection;

                    reader = aSqlCommand.ExecuteReader();
                    if (reader.Read())
                        locked = (bool)reader["Locked"];
                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsLoginLocked: " + err.Message);
                    return true;
                }
                finally
                {
                    if (reader != null && !reader.IsClosed)
                        reader.Close();
                }

                return locked;
            }

            //----------------------------------------------------------------------
            private bool SetLoginLocked(int loginID, bool locked)
            {
                if (!OpenSysDBConnection())
                    return false;

                if (locked)
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "SetLoginLocked: LoginID:" + loginID.ToString());

                string query = "UPDATE MSD_Logins SET Locked = @Locked WHERE LoginId = @LoginId";

                SqlCommand aSqlCommand = new SqlCommand();
                try
                {
                    aSqlCommand.CommandText = query;
                    aSqlCommand.Connection = SysDBConnection;
                    aSqlCommand.Parameters.AddWithValue("@Locked", locked ? "1" : "0");
                    aSqlCommand.Parameters.AddWithValue("@LoginId", loginID);

                    aSqlCommand.ExecuteNonQuery();
                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "SetLoginLocked: " + err.Message);
                    return false;
                }
                finally
                {

                }

                return true;
            }

            //----------------------------------------------------------------------
            private bool SetLoginLockCount(int loginID, int loginLockCount)
            {
                if (!OpenSysDBConnection())
                    return false;

                string query = "UPDATE MSD_Logins SET LoginFailedCount = @loginCount WHERE LoginId = @loginID";

                SqlCommand aSqlCommand = new SqlCommand();
                try
                {
                    aSqlCommand.CommandText = query;
                    aSqlCommand.Connection = SysDBConnection;
                    aSqlCommand.Parameters.AddWithValue("@loginCount", loginLockCount);
                    aSqlCommand.Parameters.AddWithValue("@loginID", loginID);

                    aSqlCommand.ExecuteNonQuery();
                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "SetLoginLockCount: " + err.Message);
                    return false;
                }
                finally
                {

                }

                return true;
            }

            //----------------------------------------------------------------------
            private void AddWrongPwdLoginCount(int loginId)
            {
                if (!OpenSysDBConnection())
                    return;

                int loginLockCount = GetLoginLockCount(loginId);

                if (loginLockCount >= InstallationData.ServerConnectionInfo.MaxLoginFailed)
                    return;

                loginLockCount++;

                SetLoginLockCount(loginId, loginLockCount);

                if (loginLockCount >= InstallationData.ServerConnectionInfo.MaxLoginFailed)
                    SetLoginLocked(loginId, true);
            }

            //----------------------------------------------------------------------
            private void ClearWrongPwdLoginCount(int loginId)
            {
                SetLoginLockCount(loginId, 0);
                SetLoginLocked(loginId, false);
            }

            #endregion

            #region funzioni legate all'attivazione

            //----------------------------------------------------------------------
            internal bool IsSynchActivationInternal()
            {
                return true;
                //non esist più l'asincrona!
                //string globalFile = pathFinder.GetGlobalSettingsFile();
                //GlobalSettings globalSettings = GlobalSettings.GetGlobalSettings(globalFile);

                ////Se il file GlobalSettings.xml non esiste allora è sincrona
                //if (globalSettings == null)
                //    return true;

                ////Se non è specificato nulla al suo interno allora è sincrona
                //if (globalSettings.CommunicationSettings == null)
                //    return true;

                //return globalSettings.CommunicationSettings.IsSynch;
            }

            //----------------------------------------------------------------------
            internal bool IsDeveloperActivationInternal()
            {
                return activationManager.IsDevelopmentSuite();
            }

            //----------------------------------------------------------------------
            /// <summary>
            /// Se nell'attivazione sono presenti serial number di dbplaceholder non 2012
            /// allora NON è possibile usare sql 2012, 
            /// se sono presenti seriali 2012 
            /// o se NON sono presenti del tutto seriali dbplaceholder 
            /// allora è possibile usare sql 2012
            /// se ha i serili è a posto con la licenza 
            /// se non li ha potrebbe non essere a posto
            /// a meno che non li abbia comprati da terze parti .
            /// </summary>
            /// <returns></returns>
            internal bool Sql2012Allowed(string authToken)
            {
                //verifica token in realtà superflua...
                if (authToken != "sbirulino" && !IsValidTokenForConsole(authToken) && !IsValidToken(authToken))
                    return false;

                return activationManager.Sql2012Allowed();
            }

            //----------------------------------------------------------------------
            internal SerialNumberType GetSerialNumberType()
            {
                return activationManager.GetSerialNumberType();
            }

            //----------------------------------------------------------------------
            internal int GetTokenProcessType(string token)
            {
                return authenticationSlots.GetTokenProcessType(token);
            }

            //----------------------------------------------------------------------
            internal string GetMobileToken(string token, int loginType)
            {
                //questo metodo prende un token e deve assicurarsi che sia valido e  del wms
                //una volta verificato ciò, si esegue una login su uno slot speciale (infinito non conteggiato da nessuno, inizialmente va bene quello dello scheduler)
                //restituisce un token valido a tutti gli effetti e che viene purgato dopo i canonici 5 minuti di inattivita.
                //in caso venga richiamato il metodo con lo stesso logintype restituirò lo stesso token.
                //in caso la richiesta venga rilasciata da un type non ancora esistente viene rilasciato un nuovo token.
                //Il login type è il corrispondente dell'askingprocess, ma quello usato qui è una suddivisione ulteriore degli asking process standard

                //(wms, manuf, warman)
                if (String.IsNullOrWhiteSpace(token) || !IsValidToken(token, null, ProcessType.WMS))
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetMobileToken error: token is not valid.");
                    return null;
                }

                authenticationSlots.FreePendingMobileSlot();
                AuthenticationSlot s = authenticationSlots.GetSlot(token, true);
                if (s == null)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetMobileToken error: token does not exist.");
                    return null;
                }
                //login
                string newauthenticationToken = null;
                int loginResult = authenticationSlots.Login(s.LoginID, s.LoginName, "", s.CompanyID, s.CompanyName, GetProcessType(loginType), false, false, false, false, out newauthenticationToken);
                if (loginResult != 0)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetMobileToken result: " + loginResult);
                    return null;
                }
                return newauthenticationToken;
            }

            //----------------------------------------------------------------------
            private string GetProcessType(int loginType)
            {
                switch (loginType)
                {
                    case 1:
                        return ProcessType.InvisibleWMS;
                    case 2:
                        return ProcessType.InvisibleWARMAN;//"InvisibleWARMAN";
                    case 3:
                        return ProcessType.InvisibleMAN;
                }
                return null;
            }

            //----------------------------------------------------------------------
            internal bool IsCalAvailableInternal(string authenticationToken, string application, string functionality)
            {
                if (
                    !OpenSysDBConnection() ||
                    application == null ||
                    application == string.Empty ||
                    functionality == string.Empty ||
                    functionality == null ||
                    activationManager == null
                    )
                    return false;

                //se mi è stato passato un token di autenticazione, verifico che sia
                //associato ad utente loginato.
                int loginId = -1;
                int companyId = -1;
                bool unnamedCal = false;
                if (
                    authenticationToken != string.Empty &&
                    !GetAuthenticationInformations(authenticationToken, out loginId, out companyId, out unnamedCal)
                    )
                    return false;

                ArticleInfo ai = null;

                try
                {
                    ai = activationManager.GetArticleByFunctionality(application, functionality);
                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsCalAvailable: " + err.Message);
                    return false;
                }

                if (ai == null)
                    return false;

                //se l'articolo non ha cal non devo associarlo all'utente, basta
                //la presenza per validare la richiesta
                if (!ai.HasCal())
                    return true;
                //if(ai.IsBackModule)

                //if (unnamedCal)
                //{
                //    diagnostic.Set
                //        (
                //        DiagnosticType.LogInfo | DiagnosticType.Error, 
                //        "IsCalAvailable: " + string.Format(LoginManagerStrings.CalWithoutAuthentication, functionality)
                //        );
                //    return false;
                //}

                if (authenticationToken == string.Empty || authenticationToken == null)
                {
                    diagnostic.Set
                        (
                        DiagnosticType.LogInfo | DiagnosticType.Error,
                        "IsCalAvailable: " + string.Format(LoginManagerStrings.CalWithoutAuthentication, functionality)
                        );
                    return false;
                }

                //anomalia assegnazione cal ent
                authenticationSlots.licut = (functionality == Microarea.TaskBuilderNet.Licence.Licence.XmlSyntax.Consts.UserLicence);


                int res = authenticationSlots.AssignUserToArticle(loginId, authenticationToken, ai);

                //int res =0;
                //if (!(activationManager.GetEdition() == Edition.Enterprise && functionality == Microarea.TaskBuilderNet.Licence.Licence.XmlSyntax.Consts.UserLicence))
                // res = authenticationSlots.AssignUserToArticle(loginId, authenticationToken, ai);

                if (res != (int)LoginReturnCodes.NoError)
                {
                    //string descri = string.Empty;
                    //diagnostic.Set
                    //	(
                    //	DiagnosticType.LogInfo | DiagnosticType.Error, 
                    //	"IsCalAvailable: " + string.Format(LoginManagerStrings.ErrAssignUserToArticle, GetLoginName(loginId, out descri), ai.Name)
                    //	);
                    return false;
                }

                return true;
            }

            private ActivationState actstate = ActivationState.NoActivated;
            //----------------------------------------------------------------------
            internal bool IsActivated()
            {
                return (actstate != ActivationState.Disabled && actstate != ActivationState.NoActivated);
            }

            //----------------------------------------------------------------------
            internal bool PingNeeded(bool force)
            {
              
                if (force)
                {
                    //se non è attivato prima attivo e poi forzo il ping così è a posto.
                    if (!IsActivated())//se attivato faccio solo ping
                        PingInternal(666);//altrimenti procedo con attivazione//todo utenti connessi ?

                    PingInternal(69);
                }

                return !IsActivated() || waiting;
            }

            //----------------------------------------------------------------------
            internal bool IsActivatedInternal(string application, string functionality)
            {
                //ogni maxIsActivated eseguo un check delle info di attivazione
                isActivatedCount++;
                if (isActivatedCount > maxIsActivated)
                    isActivatedCount = 0;
                //se l'attivazione non è buona ritorno sempre false
                if (!IsActivated())
                    return false;

                //se non ho la lista di moduli attivati ritorno false
                if (activatedList == null)
                    return false;

                StringCollection sc = activatedList[application] as StringCollection;
                if (sc == null)
                    return false;
                return (sc.Contains(functionality.ToLower(CultureInfo.InvariantCulture)));
            }

            //----------------------------------------------------------------------
            internal StringCollection GetActivatedListInternal()
            {
                StringCollection list = new StringCollection();

                //se l'attivazione non è buona ritorno sempre false
                if (!IsActivated())
                    return list;

                //se non ho la lista di moduli attivati ritorno false
                if (activatedList == null)
                    return list;

                string mask = "{0}.{1}";
                foreach (DictionaryEntry e in activatedList)
                {
                    StringCollection sc = e.Value as StringCollection;
                    if (sc == null)
                        continue;
                    string application = e.Key as string;
                    if (application != String.Empty)
                        foreach (string s in sc)
                            list.Add(String.Format(mask, application, s));
                    else
                        foreach (string s in sc)
                            list.Add(s);
                }
                return list;
            }

            #endregion

            #region fatelweb
            //---------------------------------------------------------------------------
            internal bool FEUsed(string authenticationToken)
            {
                if (
                    authenticationToken == null ||
                    authenticationToken.Trim().Length == 0 ||
                    !IsValidToken(authenticationToken)
                    )
                    return false;
                ////già il metodo isvalid verifica che slot sia diverso da null
                //string userName = authenticationSlots.GetSlot(authenticationToken).LoginName;
                feused = true;
                return true;
            }

            //--------------------------------------------------------------------------
            //      FATELWEB
            //--------------------------------------------------------------------------
            //TODO GESTIRE ERRORI
            //la parte gestionale quando genera xml per fatel mi chiama il metodo set con cui mi imposta tutti i parametri necessari alla chiamata che viene fatta  dopo il ping ( per randomizzarla)
            //e nel conetempo verifica se l'utente è attivo ed imposta il booleano che viene spedito col ping
            //tutto questo per evitare nel modo più indolore possibile che ci copino il codice della fatturazione elettronica che è nella parte gestionale.
            bool channelFree = true;//sempre true a meno che non abbia ottenuto false da ws zucchetti che mi dice che il codice non va bene, se eccezioni  imposto a true.
            bool feused = false;//bool che mi dice se qualche client ha sato fatelweb e che verrà poi messo nelle info del ping, impostato a true ogni volta che viene fatta una set
            string fetoken = null;//token di connessione al ws zucchetti
            FatelWeb.fatelwV1 fwservice;
            FatelWeb.fatelwV1Return result;
            string response;
            string return_err;
            ChannelCode channelCode;//oggetto che contiene le info necessarie a connettersi al ws zucchetti che mi vengono inviate dai client

            //---------------------------------------------------------------------------
            //---------------------------------------------------------------------------
            private class ChannelCode
            {

                //---------------------------------------------------------------------------
                public ChannelCode(string[] code)
                {
                    if (code == null)
                        return;
                    if (code.Length != 6)
                        return;
                    URL = code[0];
                    Username = code[1];
                    Password = code[2];
                    Company = code[3];
                    IdPaese = code[4];
                    IdCodice = code[5];

                }
                public string URL = null;
                public string Username = null;
                public string Password = null;
                public string Company = null;
                public string IdPaese = null;
                public string IdCodice = null;


                //---------------------------------------------------------------------------
                internal bool IsValid()
                {
                    //se anche solo un parametro non è impostato per me l 'oggetto non è utilizzabile
                    //Magari perchè nessuno l ha impostato, magari perchè nessuno in questa installazione usa fatelweb
                    return (
                !String.IsNullOrWhiteSpace(URL) &&
                !String.IsNullOrWhiteSpace(Username) &&
                !String.IsNullOrWhiteSpace(Password) &&
                !String.IsNullOrWhiteSpace(Company) &&
                !String.IsNullOrWhiteSpace(IdPaese) &&
                !String.IsNullOrWhiteSpace(IdCodice)
                );
                }
            }



            //---------------------------------------------------------------------------
            private void WrapChannelCode(string[] values)
            {
                if (values == null)
                {
                    channelCode = null;
                    return;
                }
                channelCode = new ChannelCode(values);
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.AllowCookies = true;

                EndpointAddress remoteAdd = new EndpointAddress(channelCode.URL);/* Url = "http://fatelwtest.aulla.zucchetti.it/fatelw10/services/fatelwV1"*/
                fwservice = new FatelWeb.fatelwV1Client(binding, remoteAdd);

            }

            //----------------------------------------------------------------------
            internal bool SetChannelFree(string authenticationToken, string[] channelCode)
            {
                feused = true;//prima cosa da farsi, se arrivo qui qualcuno ha usato la fatturazione elettronica

                try
                {
                    if (authenticationToken == null ||
                         authenticationToken.Trim().Length == 0 ||
                        !IsValidToken(authenticationToken))

                    {
                        diagnostic.Set
                           (
                           DiagnosticType.LogInfo | DiagnosticType.Warning,
                           "SetChannelFree setting error: Invalid Token."
                           );

                        return false;//token invalido
                    }

                    if (channelCode != null)//predispongo il canale di comunicazione per le chiamate successive
                        WrapChannelCode(channelCode);
                }
                catch (Exception e)
                {
                    diagnostic.Set
       (
       DiagnosticType.LogInfo | DiagnosticType.Warning,
       "SetChannelFree setting error: " + e.Message
       );
                }

                return channelFree;
            }

            //chiamato ogni tanto verifica che l utente abbia  l accesso alle funzionalità lato zucchetti.
            //----------------------------------------------------------------------
            private void CallFE()
            {
                try
                {
                    if (channelCode == null || !channelCode.IsValid()) return;
                    if (fwservice == null) return;

                    //chiamata ws zucchetti
                    if (Connect(channelCode.Username, channelCode.Password, channelCode.Company))
                        channelFree = GetCedente(channelCode.IdPaese, channelCode.IdCodice);
                    else
                    {
                        diagnostic.Set
                           (
                           DiagnosticType.LogInfo | DiagnosticType.Warning,
                           "SetChannelFree Calling error: Connection failed."
                           );
                        channelFree = false;
                    }
                }
                catch (Exception e)
                {
                    channelFree = false;
                    diagnostic.Set
                           (
                           DiagnosticType.LogInfo | DiagnosticType.Warning,
                           "SetChannelFree Calling error: " + e.Message
                           );
                }
                finally { Disconnect(); }
            }


            //----------------------------------------------------------------------
            internal bool GetChannelFree()
            {
                return channelFree;
            }

            /// <summary>
            /// Metodo per la connessione al WebService
            /// </summary>
            /// <param name="Username"></param>
            /// <param name="Password"></param>
            /// <param name="Company"></param>
            /// <returns>true se successo, false altrimenti. Viene anche copiato il messaggio di errore</returns>
            //----------------------------------------------------------------------
            private bool Connect(string Username, string password, string company)
            //private bool Connect(string Username = "ws@mago-a", string password = "fvjf84hf73", string company = "001")
            //private bool Connect(string Username = "ws@mago-m", string password = "jshewh6f36", string company = "001")
            {
                fetoken = null;
                try
                {
                    result = fwservice.connect(Username, password, company);
                    if (result.code > 0)
                    {
                        return_err = result.message;
                        diagnostic.Set
                           (
                           DiagnosticType.LogInfo | DiagnosticType.Warning,
                           "ChannelFree - Connection failed: code 02" + return_err
                           );
                    }
                    else
                        fetoken = result.token;
                }
                catch (TimeoutException)
                {

                    diagnostic.Set
                           (
                           DiagnosticType.LogInfo | DiagnosticType.Warning,
                           "ChannelFree TIMEOUT ERROR - Connecting error "
                           );

                    return_err = "ChannelFree TIMEOUT ERROR";// Microarea.ERP.EI_ITXMLManager.Properties.Resources.ConnectionTimeOut;
                    return false;
                }
                catch (Exception e)
                {
                    diagnostic.Set
                          (
                          DiagnosticType.LogInfo | DiagnosticType.Warning,
                          "ChannelFree Connecting error - " + e.Message
                          );
                    return_err = e.Message;
                    return false;
                }
                return result.code == 0;
            }

            /// <summary>
            /// Metodo per la disconnessione dal WebService
            /// </summary>
            /// <returns>true se successo, false altrimenti. Viene anche copiato il messaggio di errore</returns>
            //----------------------------------------------------------------------
            private bool Disconnect()
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(fetoken))
                        return true;

                    result = fwservice.disconnect(fetoken);
                    if (result.code > 0)
                    {
                        return_err = result.message;

                        diagnostic.Set
                           (
                           DiagnosticType.LogInfo | DiagnosticType.Warning,
                           "ChannelFree - Connection failed: code 03" + return_err
                           );
                    }
                }
                catch (Exception e)
                {
                    diagnostic.Set
                         (
                         DiagnosticType.LogInfo | DiagnosticType.Warning,
                         "ChannelFree Disconnecting error - " + e.Message
                         );
                    return_err = e.Message;
                    return false;
                }
                return result.code == 0;
            }

            //----------------------------------------------------------------------
            private bool GetCedente(string IdPaese, string IdCodice)
            //private bool GetCedente(string IdPaese = "IT", string IdCodice = "03472020101")
            {
                try
                {
                    result = fwservice.getCedente(fetoken, IdPaese, IdCodice);
                    if (result.code > 0)
                    {
                        return_err = result.message;
                        diagnostic.Set
                           (
                           DiagnosticType.LogInfo | DiagnosticType.Warning,
                           "ChannelFree - Connection failed: code 01" + return_err
                           );
                    }
                    else
                        response = result.response;
                }
                catch (TimeoutException)
                {
                    diagnostic.Set
                            (
                            DiagnosticType.LogInfo | DiagnosticType.Warning,
                            "ChannelFree TIMEOUT ERROR - Receiving error "
                            );
                    return_err = "ChannelFree TIMEOUT ERROR";//Microarea.ERP.EI_ITXMLManager.Properties.Resources.ConnectionTimeOut;
                    return false;
                }
                catch (Exception e)
                {
                    diagnostic.Set
                         (
                         DiagnosticType.LogInfo | DiagnosticType.Warning,
                         "ChannelFree Receiving error - " + e.Message
                         );
                    return_err = e.Message;
                    return false;
                }


                return ParseCedente(response);
            }

            //--------------------------------------------------------------------------
            private bool ParseCedente(string response)
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(response))
                    {
                        diagnostic.Set
                       (
                       DiagnosticType.LogInfo | DiagnosticType.Warning,
                       "ChannelFree Parse error - Response Empty"
                       );
                        return false;
                    }

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response);
                    XmlElement attivo = doc.SelectSingleNode("cedenti/cedente/attivo") as XmlElement;
                    if (attivo == null)
                    {
                        {
                            diagnostic.Set
                              (
                              DiagnosticType.LogInfo | DiagnosticType.Warning,
                              "ChannelFree Parse error - Empty Element."
                              );
                        }
                        return false;
                    }
                    bool s = String.Compare(attivo.InnerText, "s", StringComparison.InvariantCultureIgnoreCase) == 0;
                    if (!s)
                    {
                        diagnostic.Set
                          (
                          DiagnosticType.LogInfo | DiagnosticType.Warning,
                          "ChannelFree Parse error - Element not valid: " + attivo.InnerText
                          );
                    }
                    return s;
                }
                catch (Exception e)
                {
                    diagnostic.Set
                           (
                           DiagnosticType.LogInfo | DiagnosticType.Warning,
                           "ChannelFree Parsing error " + e.Message
                           );
                    Debug.WriteLine(e.ToString());
                }
                return false;


            }

            #endregion
            
            #region INFINITY
            //----------------------------------------------------------------------
            /// <summary>
            /// imposta sulla tabella companylogins il ssoid proveniente da infinity, sovrascrivendo un eventuale  ssoid già presente
            /// in realtà per infinity un ssoid corrisponde ad un utente, noi in modalità semplificata qui lo intendiamo come accoppiata utente company 
            /// da vedere se poi è usabile in multicompany o se bisognerà estrapolare la company dal cryptedtoken che ci arriva
            /// </summary>
            /// <param name="ssoid"></param>
            /// <param name="loginid"></param>
            /// <param name="companyid"></param>
            /// <returns></returns>
            internal int SetSSOID(SSOLoginDataBag bag, bool overwrite = true)
            {
                if (String.IsNullOrWhiteSpace(bag.SSOID))//todo se vuoto potrebbe essere che si sta scollegando i due utenti infinity-mago
                    return (int)LoginReturnCodes.InvalidSSOToken;

                if (!OpenSysDBConnection())
                    return (int)LoginReturnCodes.SysDBConnectionFailure;
                try
                {
                    /**/
                    if (!overwrite)//se l utente ha già un ssoid associato non sovrascrivo.
                    {
                        using (SqlCommand cmd = SysDBConnection.CreateCommand())
                        {
                            cmd.CommandText = @"SELECT SSOID FROM MSD_CompanyLogins WHERE LoginId = @loginID and companyId = @companyID";
                            cmd.Parameters.AddWithValue("@loginID", bag.loginid);
                            cmd.Parameters.AddWithValue("@companyID", bag.companyid);

                            using (SqlDataReader aReader = cmd.ExecuteReader())
                            {
                                if (aReader.HasRows)
                                {
                                   
                                    while (aReader.Read())
                                    {
                                        if (aReader.IsDBNull(0)) continue;
                                       
                                        string ssoidDB = aReader.GetString(0);
                                        if (ssoidDB.IsNullOrWhiteSpace()) continue;//vuoto vuol dire che non è associato ancora
                                        //se i due ssoid sono diversi torno erroroe non si può sovrascrivere
                                        if (String.Compare(bag.SSOID, ssoidDB, false) != 0)
                                            return (int)LoginReturnCodes.ImagoUserAlreadyAssociated;
                                    }
                                }
                                    
                            }
                        }
                    }
                    /**/

                    string query;
                    string mmenuid = "";

                    using (SqlCommand selSqlCommand = SysDBConnection.CreateCommand())
                    {
                        selSqlCommand.CommandText = "SELECT MMMENUID FROM MSD_CompanyLogins WHERE LoginId = @loginID and companyId = @companyID";
                        selSqlCommand.Parameters.AddWithValue("@loginID", bag.loginid);
                        selSqlCommand.Parameters.AddWithValue("@companyID", bag.companyid);

                        using (SqlDataReader aReader = selSqlCommand.ExecuteReader())
                        {
                            while (aReader.Read())
                            {
                                if (aReader.IsDBNull(0)) continue;
                                mmenuid = aReader.GetString(0);
                            }
                            if (mmenuid != "")
                                query = "UPDATE MSD_CompanyLogins SET ssoid= @ssoid, InfinityData = @infinityData, TB_Modified = @TB_Modified WHERE LoginId = @loginID and companyId = @companyID";
                            else
                            {
                                mmenuid = InfinityHelper.GetRandomMMMENUID();
                                query = "UPDATE MSD_CompanyLogins SET ssoid= @ssoid, InfinityData = @infinityData, MMMENUID=@mmenuid, TB_Modified = @TB_Modified WHERE LoginId = @loginID and companyId = @companyID";
                            }
                        }
                    }

                    SqlCommand aSqlCommand = new SqlCommand();

                    aSqlCommand.CommandText = query;
                    aSqlCommand.Connection = SysDBConnection;
                    aSqlCommand.Parameters.AddWithValue("@TB_Modified", DateTime.Now);
                    if (mmenuid != "")
                        aSqlCommand.Parameters.AddWithValue("@mmenuid", mmenuid);
                    aSqlCommand.Parameters.AddWithValue("@ssoid", bag.SSOID);
                    aSqlCommand.Parameters.AddWithValue("@loginID", bag.loginid);
                    aSqlCommand.Parameters.AddWithValue("@companyID", bag.companyid);
                    aSqlCommand.Parameters.AddWithValue("@infinityData", bag.token.GetInfoForDb());

                    aSqlCommand.ExecuteNonQuery();
                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "SetSSOID: " + err.Message);
                    return (int)LoginReturnCodes.SsoTokenError;
                }
                finally
                {

                }
                return (int)LoginReturnCodes.NoError;

            }

            //----------------------------------------------------------------------
            /// <summary>
            /// partendo dal token criptato in arrivo da infinity mi creo un pacchetto di informazioni che devo tenermi in memoria per poi poter ricreare un token cryptato da reinviare per le richieste da mago a infinity
            /// </summary>
            /// <param name="cryptedtoken">token criptato proveninete da infinity</param>
            /// <param name="ssoLoginDataBag">ritorna il pacchetto di informazioni collegate al token infinity se presente ssoid nella companylogins</param>
            /// <returns>ritorna  un codice di errore specifico  se il corrispondente ssoid non è trovato nella tabella companylogin, se invece è trovatoso di successo</returns>
            internal int GetDataViaSSOID(string cryptedtoken, out SSOLoginDataBag ssoLoginDataBag)
            {
                ssoLoginDataBag = new SSOLoginDataBag();

                if (cryptedtoken.IsNullOrWhiteSpace())
                    return (int)LoginReturnCodes.SsoTokenEmpty;

                ssoLoginDataBag.token = new TokenInfinity(cryptedtoken);

                if (!ssoLoginDataBag.token.IsValid()) return (int)LoginReturnCodes.InvalidSSOToken;

                if (!OpenSysDBConnection())
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetDataViaSSOID: OpenSysDBConnection failed");
                    TraceAction("", cryptedtoken, TraceActionType.LoginFailed, ProcessType.EasyLook, string.Empty, HttpContext.Current.Request.UserHostAddress);
                    return (int)LoginReturnCodes.SysDBConnectionFailure;
                }


                using (SqlCommand aSqlCommand = SysDBConnection.CreateCommand())
                {
                    aSqlCommand.CommandText =
    @"SELECT 
      MSD_Logins.Login,MSD_Logins.LoginID, MSD_Companies.Company, MSD_Companies.CompanyID, MSD_Logins.Password
FROM MSD_CompanyLogins INNER JOIN MSD_Logins
ON MSD_Logins.LoginId = MSD_CompanyLogins.LoginId
INNER JOIN MSD_Companies
ON MSD_Companies.CompanyId = MSD_CompanyLogins.CompanyId
WHERE
ssoid = @ssoid";
                    aSqlCommand.Parameters.AddWithValue("@ssoid", ssoLoginDataBag.SSOID);

                    using (SqlDataReader aReader = aSqlCommand.ExecuteReader())
                    {
                        if (!aReader.HasRows)
                        {
                            aReader.Close();
                            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Information, String.Format("infinityToken: infinityToken is not present: {0}", ssoLoginDataBag.SSOID));
                            //TraceAction("", ssoToken, TraceActionType.LoginFailed, ProcessType.EasyLook, string.Empty, HttpContext.Current.Request.UserHostAddress);
                            return (int)LoginReturnCodes.SSOIDNotAssociated;
                        }

                        int rows = 0;
                        int loginColIdx = aReader.GetOrdinal("Login");
                        int companyColIdx = aReader.GetOrdinal("Company");
                        int loginIDColIdx = aReader.GetOrdinal("LoginID");
                        int companyIDColIdx = aReader.GetOrdinal("CompanyID");
                        int passwordColIdx = aReader.GetOrdinal("Password");
                        while (aReader.Read())
                        {
                            rows++;
                            ssoLoginDataBag.loginName = aReader.GetString(loginColIdx);
                            ssoLoginDataBag.companyName = aReader.GetString(companyColIdx);
                            ssoLoginDataBag.loginid = aReader.GetInt32(loginIDColIdx);
                            ssoLoginDataBag.companyid = aReader.GetInt32(companyIDColIdx);
                            ssoLoginDataBag.password = Microarea.TaskBuilderNet.Core.Generic.Crypto.Decrypt(aReader.GetString(passwordColIdx));
                        }

                        if (rows > 1)
                        {
                            aReader.Close();
                            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, String.Format("ssoid is assigned to more <loginId, companyId>: {0}", ssoLoginDataBag.SSOID));
                            //TraceAction("", ssoToken, TraceActionType.LoginFailed, ProcessType.EasyLook, string.Empty, HttpContext.Current.Request.UserHostAddress);
                            return (int)LoginReturnCodes.MoreThanOneSSOToken;
                        }
                    }
                }
                return 0;
            }



            //----------------------------------------------------------------------
            internal string GetSSOData(int loginid, int companyid, out string dbinfo)
            {
                dbinfo = null;
                try
                {
                    if (!OpenSysDBConnection())
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetDataViaSSOID: OpenSysDBConnection failed");
                        // TraceAction("", cryptedtoken, TraceActionType.LoginFailed, ProcessType.EasyLook, string.Empty, HttpContext.Current.Request.UserHostAddress);
                        return null;
                    }

                    using (SqlCommand aSqlCommand = SysDBConnection.CreateCommand())
                    {
                        aSqlCommand.CommandText =
    @"SELECT ssoid, infinityData
FROM MSD_CompanyLogins 
WHERE   LoginId = @loginID and companyId = @companyID";
                        aSqlCommand.Parameters.AddWithValue("@loginID", loginid);
                        aSqlCommand.Parameters.AddWithValue("@companyID", companyid);

                        using (SqlDataReader aReader = aSqlCommand.ExecuteReader())
                        {
                            if (aReader.HasRows)
                            {
                                int ssoididx = aReader.GetOrdinal("ssoid");
                                int infinityDataidx = aReader.GetOrdinal("infinityData");
                                while (aReader.Read())
                                {
                                    dbinfo = aReader.GetString(infinityDataidx);
                                    return aReader.GetString(ssoididx);
                                }
                            }
                        }
                    }
                    return null;
                }
                catch (Exception exc)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetSSOData: " + exc.Message);

                    return null;
                }

            }


            //----------------------------------------------------------------------
            /// <summary>
            /// dato un authenticationtoken di login, se esisitente viene restituito il token cryptato in formato infinity
            /// </summary>
            /// <param name="authenticationToken">authentication token di login di Mago</param>
            /// <returns></returns>
            internal string GetIToken(string authenticationToken)
            {
                AuthenticationSlot slot = authenticationSlots.GetSlot(authenticationToken);
                if (slot == null)
                    return null;
                if (slot.ssologinDataBag == null)
                  
               
                {
                    SSOLoginDataBag bag = new SSOLoginDataBag();
                    bag.companyid = slot.CompanyID;
                    bag.loginid = slot.LoginID;
                    bag.loginName = slot.LoginName;
                    bag.companyName = slot.CompanyName;
                    string data = null;
                    string ssoid = GetSSOData(bag.loginid, bag.companyid, out data);
                    bag.token = new TokenInfinity();
                    if (!ssoid.IsNullOrWhiteSpace())
                        bag.token.FillTokenInfinity(data, ssoid);
                    slot.ssologinDataBag = bag;
                }
                return slot.ssologinDataBag.token.GetCryptedToken();


            }

            //-----------------------------------------------------------------------
            private string GetLoginMngSession()
            {
                string pwd = string.Empty;
                string loginSessionFile = BasePathFinder.BasePathFinderInstance.GetLoginMngSessionFile();
                if (File.Exists(loginSessionFile))
                {
                    try
                    {
                        StreamReader sr = new StreamReader(loginSessionFile);
                        pwd = sr.ReadToEnd();
                        sr.Close();
                    }
                    catch
                    {
                        pwd = string.Empty;
                    }
                }

                return pwd;
            }
            //----------------------------------------------------------------------
            /// <summary>
            /// Effettua la login a mago partendo da dati di login infinity
            /// </summary>
            /// <param name="ssologindb">pacchetto di informazioni infinity necessario per effettuare login a mago</param>
            /// <param name="authenticationToken">ritorna il token di autenticazione a mago.</param>
            /// <returns></returns>
            internal int LoginViaInfinityToken(SSOLoginDataBag ssologindb, out string authenticationToken)
            {
                authenticationToken = string.Empty;
                bool admin = false;
                int companyId = -1;
                string dbName = string.Empty;
                string dbServer = string.Empty;
                int providerId = -1;
                bool security = false;
                bool auditing = false;
                bool useKeyedUpdate = false;
                bool transactionUse = false;
                string preferredLanguage = string.Empty;
                string applicationLanguage = string.Empty;
                string providerName = string.Empty;
                string providerDescription = string.Empty;
                bool useConstParameter = false;
                bool stripTrailingSpaces = false;
                string providerCompanyConnectionString = string.Empty;
                string nonProviderCompanyConnectionString = string.Empty;
                string dbUser = string.Empty;
                string activationDB = string.Empty;

                bool winNTAuth = false;
                bool gdiLogin = false;
                bool concurrent = false; 
                bool webLogin = false;
                int loginId = 0;
                GetLoginId(ssologindb.loginName, out loginId, out winNTAuth, out webLogin, out gdiLogin, out concurrent);
                 if(winNTAuth)
                        ssologindb.password = GetLoginMngSession();

                

                int res = Login(
                     ref ssologindb.loginName,
                     ref ssologindb.companyName,
                     ssologindb.password,
                     ProcessType.MenuManager,
                     "",
                     true,//sso sempre sovrascrive la login !!! almeno evito errori di cal occupata
                     out admin,
                     out authenticationToken,
                     out companyId,
                     out dbName,
                     out dbServer,
                     out providerId,
                     out security,
                     out auditing,
                     out useKeyedUpdate,
                     out transactionUse,
                     out preferredLanguage,
                     out applicationLanguage,
                     out providerName,
                     out providerDescription,
                     out useConstParameter,
                     out stripTrailingSpaces,
                     out providerCompanyConnectionString,
                     out nonProviderCompanyConnectionString,
                     out dbUser,
                     out activationDB
                     );
                if (res == 0)
                {
                    AuthenticationSlot s = authenticationSlots.GetSlot(authenticationToken);
                    ssologindb.companyid = companyId;

                    //tokenInfinity.ParseAppReg: se la company con cui ti sei loginato non è la stessa che hai definito in fase di configurazione e che`e indicata nel token criptato ti impedisco di continuare ed effetto un logout.
                    if (IsActivatedInternal("Erp", "imago") && ssologindb.token.companyToLogin !=-1 && ssologindb.token.companyToLogin != ssologindb.companyid)
                    {
                        Logout(authenticationToken);
                        return (int)LoginReturnCodes.ImagoCompanyNotCorresponding;
                    }
                    int loginID = -1;
                    bool dummy = false; bool dummy2 = false; bool dummy3 = false;
                    GetLoginId(ssologindb.loginName, out loginID, out dummy, out dummy2, out dummy3);
                    ssologindb.loginid = loginID;
                    s.ssologinDataBag = ssologindb;
                    res = SetSSOID(ssologindb, false);
                    if (res != 0)
                        Logout(authenticationToken); 
                }
                return res;
            }

            //----------------------------------------------------------------------
            /// <summary>
            /// 
            /// </summary>
            /// <param name="cryptedtoken">token criptato infinity</param>
            /// <param name="authenticationToken">ritorna un authentication token di login mago</param>
            /// <param name="username">se presenti vuol dire che proviene da una login esplicita da infinity ( non era presente ssoid, utente non ancora mappato)</param>
            /// <param name="pwd">se presenti vuol dire che proviene da una login esplicita da infinity ( non era presente ssoid, utente non ancora mappato)</param>
            /// <param name="company">se presenti vuol dire che proviene da una login esplicita da infinity ( non era presente ssoid, utente non ancora mappato)</param>
            /// <returns></returns>
            internal int LoginViaInfinityToken(string cryptedtoken, out string authenticationToken, string username = null, string pwd = null, string company = null)
            {
                SSOLoginDataBag ssologindb = null;
                authenticationToken = null;
                int res = GetDataViaSSOID(cryptedtoken, out ssologindb);
                if ((string.IsNullOrWhiteSpace(username)  && res == (int)LoginReturnCodes.SSOIDNotAssociated )|| res == (int)LoginReturnCodes.InvalidSSOToken)//usernotassociated TODO
                    return res;
                if (!string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(ssologindb.loginName)) ssologindb.loginName = username;
                if (!string.IsNullOrWhiteSpace(company) && string.IsNullOrWhiteSpace(ssologindb.companyName)) ssologindb.companyName = company;
                if (!string.IsNullOrWhiteSpace(pwd) && string.IsNullOrWhiteSpace(ssologindb.password)) ssologindb.password = pwd;
               
                //se la password è null imposto stringa vuota se no dopo si incarta
                if (ssologindb.password == null)
                    ssologindb.password = string.Empty;

                return LoginViaInfinityToken(ssologindb, out authenticationToken);

            }

            //----------------------------------------------------------------------
            /// <summary>
            /// effettua il logoff partendo da un token criptato di infinity
            /// </summary>
            /// <param name="cryptedToken"></param>
            internal void SSOLogOff(string cryptedToken)
            {
                SSOLoginDataBag ssologdb = null;
                int res = GetDataViaSSOID(cryptedToken, out ssologdb);
                string autTok = authenticationSlots.GetSlot_InfinityMode_AutTok(ssologdb.loginid, ssologdb.companyid);
                Logout(autTok);
            }

            /// <summary>
            /// partendo dal token cryptato viene verificato se l utente con quel ssoid è già mappato nella companylogins
            /// </summary>
            /// <param name="cryptedtoken"></param>
            /// <returns></returns>
            //-----------------------------------------------------------------------
            internal bool ExistsSSOIDUser(string cryptedtoken)
            {
                if (cryptedtoken.IsNullOrWhiteSpace())
                    return false;
                SSOLoginDataBag bag = null;
                int res = GetDataViaSSOID(cryptedtoken, out bag);

                if (bag == null || !bag.Valid) return false;
                return res == (int)LoginReturnCodes.NoError;
            }
          
            //--------------------------------------------------------------------------
            //    END INFINITY Functionality ssologin 
            //--------------------------------------------------------------------------

            //--------------------------------------------------------------------------
            //     INFINITY
            //--------------------------------------------------------------------------
            //----------------------------------------------------------------------
            internal void LogOutViaInfinityToken(string ssoid)
            {
                SSOLoginDataBag ssologdb = null;
                int res = GetDataViaSSOID(ssoid, out ssologdb);
                string autTok = authenticationSlots.GetSlot_InfinityMode_AutTok(ssologdb.loginid, ssologdb.companyid);
                Logout(autTok);
            }

  #endregion 

            #region informazioni su utenti attualmente connessi
        //-----------------------------------------------------------------------
        internal int GetLoggedUsersNumber()
			{
				if (authenticationSlots == null)
					return 0;

				return authenticationSlots.GetLoggedUsersNumber();
			}

			//-----------------------------------------------------------------------
			internal int GetCompanyLoggedUsersNumber(int companyId)
			{
				if (authenticationSlots == null)
					return 0;

				return authenticationSlots.GetCompanyLoggedUsersNumber(companyId);
			}

            //-----------------------------------------------------------------------
            internal string GetLoggedUsers(string token)
            {
                if (authenticationSlots == null)
                    return string.Empty;

                bool writeTokens = (IsValidToken(token) || IsValidTokenForConsole(token) || SdRum(token));

                return authenticationSlots.ToXml(writeTokens);
            }

            //non sono scema, ho confuso il codice appositamente - per Vayra per avere  itoken anche  senza avere un token valido.
            //-----------------------------------------------------------------------
            private bool SdRum(string token)
            {
                if (token.IsNullOrEmpty()) return false;
                string authenticationToken = "{2E816F4A-7AB8-4352-BD0C-479984070570}";
                bool res = authenticationToken != null;
                res = token == System.Text.Encoding.UTF8.GetString(GetSdrumRes());
                return res;
            }

            //-----------------------------------------------------------------------
            private byte[] GetSdrumRes()
            { return new byte[] { 68, 117, 109, 109, 121, 84, 111, 107, 101, 110, 70, 114, 111, 109, 69, 120, 116, 101, 114, 110, 97, 108, 65, 112, 112, }; }



            //-----------------------------------------------------------------------
            internal bool IsUserLogged(int loginID)
			{
				if (authenticationSlots == null)
					return false;
				return authenticationSlots.IsUserLogged(loginID);
			}

            //---------------------------------------------------------------------------
            public bool ConfirmToken(string authenticationToken, string procType)
            {
                return ConfirmToken(authenticationSlots.GetSlot(authenticationToken), procType); 
            }
            //---------------------------------------------------------------------------
            public bool ConfirmToken(AuthenticationSlot authenticationSlot, string procType)
            {
                if (authenticationSlot == null) return false;
                return String.Compare(authenticationSlot.ProcessName, procType, StringComparison.InvariantCultureIgnoreCase) == 0;
            }

			//----------------------------------------------------------------------
			internal bool GetLoginInformation
				(
				string authenticationToken,
				out string userName,
				out int loginId,
				out string companyName,
				out int companyId,
				out bool admin,
				out string dbName,
				out string dbServer,
				out int providerId,
				out bool security,
				out bool auditing,
				out bool useKeyedUpdate,
				out bool transactionUse,
				out bool useUnicode,
				out string preferredLanguage,
				out string applicationLanguage,
				out string providerName,
				out string providerDescription,
				out bool useConstParameter,
				out bool stripTrailingSpaces,
				out string providerCompanyConnectionString,
				out string nonProviderCompanyConnectionString,
				out string dbUser,
				out string processName,
				out string userDescription,
				out string email,
				out bool easyBuilderDeveloper,
                out bool rowSecurity,
				out bool dataSynchro
				)
			{
				userName = string.Empty;
				loginId = -1;
				companyName = string.Empty;
				companyId = -1;
				admin = false;
				dbName = string.Empty;
				dbServer = string.Empty;
				providerId = -1;
				security = false;
				auditing = false;
				useKeyedUpdate = false;
				transactionUse = false;
				useUnicode = false;
				preferredLanguage = string.Empty;
				applicationLanguage = string.Empty;
				providerName = string.Empty;
				providerDescription = string.Empty;
				useConstParameter = false;
				stripTrailingSpaces = false;
				providerCompanyConnectionString = string.Empty;
				nonProviderCompanyConnectionString = string.Empty;
				dbUser = string.Empty;
				processName = string.Empty;
				userDescription = string.Empty;
				email = string.Empty;
				easyBuilderDeveloper = false;
                rowSecurity = false;
				dataSynchro = false;

				bool webLogin = false;
				if (!OpenSysDBConnection())
					return false;

				if (!GetAuthenticationInformations(authenticationToken, out loginId, out companyId, out webLogin))
					return false;

				processName = GetAuthenticatedProcessNameByToken(authenticationToken);

				userName = GetLoginName(loginId, out userDescription, out email);

				if (!GetUserLanguages(loginId, companyId, out preferredLanguage, out applicationLanguage))
					return false;

				admin = IsAdmin(loginId, companyId);
             
                easyBuilderDeveloper = IsEasyBuilderDeveloper(loginId, companyId);

				int port = 0;
				if (!GetCompanyInfo(companyId, out companyName, out dbName, out dbServer, out providerId, out port))
					return false;

                if (!GetOslStatus(companyId, out security, out auditing, out rowSecurity, out dataSynchro))
					return false;

				if (
					!GetConnectionParams
					(
					companyId,
					out useKeyedUpdate,
					out transactionUse,
					out useUnicode
					)
					)
					return false;

				if (!GetProviderInfo
					(
					providerId,
					out providerName,
					out providerDescription,
					out useConstParameter,
					out stripTrailingSpaces
					)
					)
					return false;

				providerCompanyConnectionString = GetCompanyConnectionString(loginId, companyId, true, dbName, dbServer, providerName, port);
				nonProviderCompanyConnectionString = GetCompanyConnectionString(loginId, companyId, false, dbName, dbServer, providerName, port);
				dbUser = GetDBUser(nonProviderCompanyConnectionString);
				
				return true;
			}

       

			//-----------------------------------------------------------------------
			internal bool GetAuthenticationInformations(string authenticationToken, out int loginId, out int companyId, out bool unNamed)
			{
				loginId = -1;
				companyId = -1;
				unNamed = false;

				if (authenticationSlots != null)
				{
					AuthenticationSlot slot = authenticationSlots.GetSlot(authenticationToken);
					if (slot != null)
					{
						loginId = slot.LoginID;
						companyId = slot.CompanyID;
						unNamed = slot.UnNamed;
						return true;
					}
				}

				return false;
			}

			//-------------------------------------------------------------------------
			internal bool GetAuthenticationNames(string authenticationToken, out string userName, out string companyName)
			{
				userName = string.Empty;
				companyName = string.Empty;

				if (authenticationSlots != null)
				{
					AuthenticationSlot slot = authenticationSlots.GetSlot(authenticationToken);
					if (slot != null)
					{
						userName = slot.LoginName;
						companyName = slot.CompanyName;

						return true;
					}
				}

				return false;
			}

			//-----------------------------------------------------------------------
			internal string GetAuthenticatedProcessNameByToken(string authenticationToken)
			{
				AuthenticationSlot slot = authenticationSlots.GetSlot(authenticationToken);
				if (slot == null)
					return string.Empty;

				return slot.ProcessName;
			}

            //DRAFT
            //Da 3.9.2 (marzo 2013) non è più possibile utilizzare MAgo in data applicazione successiva alla scadenza mlu se mlu è scaduto o disdettato 
            //di conseguenza facciamo il controllo  incambio data applicazione ma anche qui perchè il cambio data potrebbe essere fatto prima che loginamanager reperisca 
            //il dato inviato dal ping ( il ping può essere fatto fino ad un'ora entro l'avvio!)
            //NB: default(DateTime) = DateTime.MinValue
            //17/09/14 aggiungo un parametro che serve in 3.11.0 per la gestione della login 'segreta' del ws mobile
			//-----------------------------------------------------------------------
            internal bool IsValidToken(string authenticationToken, string mluexpired = null, string logintype = null)
			{
                //ad ogni verifica del token quel token viene rinfrescato.
                if (authenticationToken == null || authenticationToken == string.Empty)
					return false;

                if (authenticationSlots == null)
                {
                    diagnostic.Set
                        (
                        DiagnosticType.LogInfo | DiagnosticType.Error,
                        "IsValidToken:authenticationSlots is null"
                        );
                    return false;
                }
                string g = null;
                AuthenticationSlot s = authenticationSlots.GetSlot(authenticationToken, true);
                if (!string.IsNullOrWhiteSpace(logintype))
                {
                    if (!ConfirmToken(s, logintype)) 
                        return false;
                }
                
				return s != null && (mluexpired ==null ||  IsValidDate(mluexpired, out g));
			}


			#endregion

			#region info su company

			/// <summary>
			/// Restituisce il nome dell'azienda in base al nome
			/// </summary>
			/// <param name="companyId">Identificatore della compagnia</param>
			/// <param name="company">Nome dell'azienda</param>
			/// <returns></returns>
			//-----------------------------------------------------------------------
			private string GetCompanyName(int companyID)
			{
				if (!OpenSysDBConnection())
					return string.Empty;

				string companyName = string.Empty;

				string query = "SELECT Company FROM MSD_Companies WHERE CompanyId = @CompanyId";
				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;

				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Connection = SysDBConnection;
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyID);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (!aSqlDataReader.Read())
						return string.Empty;

					companyName = (string)aSqlDataReader[LoginManagerStrings.Company];
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyName: " + err.Message);
					return string.Empty;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}

				return companyName;
			}

			//-----------------------------------------------------------------------
			private int GetCompanyID(string company)
			{
				if (!OpenSysDBConnection())
					return -1;

				int companyId = -1;

				string query = "SELECT CompanyId FROM MSD_Companies WHERE Company = @Company";

				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;

				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Connection = SysDBConnection;
					aSqlCommand.Parameters.AddWithValue("@Company", company);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (!aSqlDataReader.Read())
						return companyId;

					return (int)aSqlDataReader[LoginManagerStrings.CompanyID];
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyID: " + err.Message);
					return -1;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

			//-----------------------------------------------------------------------
            internal bool GetCompanyInfo(int companyId, out string companyName, out string dbName, out string dbServer, out int providerId, out int port)
			{
				companyName = string.Empty;
				dbName = string.Empty;
				dbServer = string.Empty;
				providerId = -1;
				port = 0;

				if (!OpenSysDBConnection())
					return false;

				string query = "SELECT Company, CompanyDBServer, CompanyDBName, ProviderId, Port FROM MSD_Companies WHERE Disabled = @Disabled and CompanyId = @CompanyId";
				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;

				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Connection = SysDBConnection;
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (!aSqlDataReader.Read())
						return false;

					companyName = (string)aSqlDataReader[LoginManagerStrings.Company];
					dbName = (string)aSqlDataReader[LoginManagerStrings.CompanyDBName];
					dbServer = (string)aSqlDataReader[LoginManagerStrings.CompanyDBServer];
					providerId = (int)aSqlDataReader[LoginManagerStrings.ProviderID];
					port = (int)aSqlDataReader[LoginManagerStrings.Port];
					return true;
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyInfo: " + err.Message);
					return false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

            Dictionary<string, ClientData> ClientDataList = new Dictionary<string, ClientData>();
            //----------------------------------------------------------------------
            internal void SetClientData( ClientData cd)
            {
                if ( cd == null) return;

                string clientName = cd.Name;
                if (String.IsNullOrWhiteSpace(clientName)) return;

                if (!ClientDataList.ContainsKey(clientName.ToLower(CultureInfo.InvariantCulture)))
                    ClientDataList.Add(clientName.ToLower(CultureInfo.InvariantCulture), cd);
            }
            

            //imposta la partita iva della company comunicata da mago al momento della login, ricongiungendo l'info coi dati della companylogins
            //così da averli completi, considerando che nome e partita iva potrebbero non essere impostati
            //----------------------------------------------------------------------
            internal void SetCompanyInfo(string authToken, string aName, string aValue)
            {

                if (!IsValidToken(authToken))
                    return;
                AuthenticationSlot slot = authenticationSlots.GetSlot(authToken);

                //aggiungo al db la data di last login (cioè adesso) e nome pi dell'azienda come (e se) censita su Mago.net
                DateTime t = DateTime.Now;
                SetCompanyInfos(aName, aValue, slot.LoginID, slot.CompanyID, t);

            }

            //----------------------------------------------------------------------
            internal void SetCompanyInfos (string name, string code, int loginid, int companyid, DateTime t)
            {
                if (!OpenSysDBConnection())
                    return;

                string valueToInsert = Crypt(name, code);

                //update
                string query = "UPDATE MSD_CompanyLogins SET OtherData= @OtherData, TB_Modified = @TB_Modified WHERE LoginId = @loginID and companyId = @companyID";

                SqlCommand aSqlCommand = new SqlCommand();
                try
                {
                    aSqlCommand.CommandText = query;
                    aSqlCommand.Connection = SysDBConnection;
                    aSqlCommand.Parameters.AddWithValue("@TB_Modified", t);
                    aSqlCommand.Parameters.AddWithValue("@OtherData", valueToInsert);
                    aSqlCommand.Parameters.AddWithValue("@loginID", loginid);
                    aSqlCommand.Parameters.AddWithValue("@companyID", companyid);

                    aSqlCommand.ExecuteNonQuery();
                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "SetCompanyInfos: " + err.Message);
                 
                }
                finally
                {

                }
            }

            //----------------------------------------------------------------------
            private string Crypt(string name, string code)
            {
                return Microarea.TaskBuilderNet.Core.Generic.Crypto.Encrypt(code + ";" + name, "setcompanyinfo", "loginglobal");
            }
            //----------------------------------------------------------------------
            private void Decrypt(string todecrypt, out string name, out string code)
            {
                name = string.Empty;
                code = string.Empty;
                string res = Microarea.TaskBuilderNet.Core.Generic.Crypto.Decrypt(todecrypt, "setcompanyinfo", "loginglobal");
                string[] ress = res.Split(';');
                if (ress.Length >= 1)
                {
                    code = ress[0];
                    if (ress.Length == 2)
                        name = ress[1];
                }

            }


            //-----------------------------------------------------------------------
            internal IList<CompanyDataBag> GetCompanyDataBags()
            {
                IList<CompanyDataBag> cdb = new List<CompanyDataBag>();

                try
                {
                    if (!OpenSysDBConnection())
                        return cdb;

                    // la query barbatrucco che ho scritto con l'ausilio di Germano, permette di trovare per ogni company id la data di ultima login e poi tutti i dati relativi a questa riga .
                    string query = @"
                select MSD_Companies.Company, MSD_Logins.Login, (MSD_Companies.CompanyDBServer +'\'+  MSD_Companies.CompanyDBName) as Server, TB_Modified,OtherData 
from MSD_CompanyLogins, MSD_Companies, MSD_Logins
where str(MSD_CompanyLogins.CompanyId) + convert(nvarchar, MSD_CompanyLogins.TB_Modified,113)
in
	(
	select str(MSD_CompanyLogins.CompanyId) + convert(nvarchar, max(MSD_CompanyLogins.TB_Modified),113)
	from MSD_CompanyLogins
	group by CompanyId
	)
and MSD_CompanyLogins.CompanyId = MSD_Companies.CompanyId 
and MSD_CompanyLogins.LoginId = MSD_Logins.LoginId";

                    SqlCommand aSqlCommand = new SqlCommand();
                    SqlDataReader aSqlDataReader = null;
                    try
                    {

                        aSqlCommand.CommandText = query;
                        aSqlCommand.Connection = SysDBConnection;
                        aSqlDataReader = aSqlCommand.ExecuteReader();

                        while (aSqlDataReader.Read())
                        {
                            Debug.WriteLine((string)aSqlDataReader["Server"]);
                            string name = string.Empty;
                            string value = string.Empty;

                            if (aSqlDataReader["OtherData"] != DBNull.Value)
                                Decrypt((string)aSqlDataReader["OtherData"], out name, out value);
                            DateTime t = DateTime.MinValue;
                            if (aSqlDataReader["TB_Modified"] != DBNull.Value)
                                t = (DateTime)aSqlDataReader["TB_Modified"];

                            //todo verifica datetime, nulli o vali=ori estremi
                            CompanyDataBag c = new CompanyDataBag(name, value, t, (string)aSqlDataReader["Company"], (string)aSqlDataReader["Server"], (string)aSqlDataReader["Login"]);
                            
                            cdb.Add(c);
                        }

                    }
                    catch (Exception err)
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyDataBags1: " + err.Message);
                        return new List<CompanyDataBag>();
                    }
                    finally
                    {
                        if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
                            aSqlDataReader.Close();
                    }
                }
                catch (Exception exc)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyDataBags2: " + exc.Message);
                    return new List<CompanyDataBag>();
                }
                return cdb;
            }


			/// <summary>
			/// Restituisce informazioni relative alla connessione al database
			/// </summary>
			/// <param name="companyID">Identificatore della compagnia</param>
			/// <param name="useKeyedUpdate">Se utilizzare l'aggiornamento per chiave (default) o per posizione</param>
			/// <param name="transactionUse">se utilizzare (default) o meno le transazioni</param>
			/// <returns></returns>
			//-----------------------------------------------------------------------
			private bool GetConnectionParams(int companyID, out bool useKeyedUpdate, out bool transactionUse, out bool useUnicode)
			{
				useKeyedUpdate = true;
				transactionUse = true;
				useUnicode = false;

				if (!OpenSysDBConnection())
					return false;

				//Prendo lo LoginId dell'utente
				string query = "SELECT UseKeyedUpdate, UseTransaction, UseUnicode FROM MSD_Companies WHERE CompanyId = @CompanyID";

				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;

				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Connection = SysDBConnection;
					aSqlCommand.Parameters.AddWithValue("@CompanyID", companyID);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (!aSqlDataReader.Read())
						return false;

					useKeyedUpdate = (bool)aSqlDataReader[LoginManagerStrings.UseKeyedUpdate];
					transactionUse = (bool)aSqlDataReader[LoginManagerStrings.UseTransaction];
					useUnicode = (bool)aSqlDataReader[LoginManagerStrings.UseUnicode];
					return true;
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetConnectionParams: " + err.Message);
					return false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

			/// <summary>
			/// Restituisce informazioni sull'azienda
			/// </summary>
			/// <param name="company">azienda</param>
			/// <param name="security">indica se l'azienda utilizza la OSL Security</param>
			/// <param name="auditing">indica se l'azienda utilizza la OSL auditing</param>
			/// <returns>True se la chiamata ha avuto successo</returns>
			//-----------------------------------------------------------------------
			private bool GetOslStatus(int companyId, out bool security, out bool auditing, out bool rowSecurity, out bool dataSynchro)
			{
				security = false;
				auditing = false;
                rowSecurity = false;
				dataSynchro = false;

				if (!OpenSysDBConnection())
					return false;

				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;
				try
				{

					aSqlCommand.CommandText = "SELECT UseSecurity, UseAuditing, UseRowSecurity, UseDataSynchro FROM MSD_Companies WHERE CompanyId = @CompanyID";
					aSqlCommand.Connection = SysDBConnection;
					aSqlCommand.Parameters.AddWithValue("@CompanyID", companyId);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (aSqlDataReader.Read())
					{
						auditing = (bool)aSqlDataReader[LoginManagerStrings.UseAuditing];
						security = (bool)aSqlDataReader[LoginManagerStrings.UseSecurity];
                        rowSecurity = (bool)aSqlDataReader[LoginManagerStrings.UseRowSecurity];
						dataSynchro = (bool)aSqlDataReader[LoginManagerStrings.UseDataSynchro];
					}

					return true;
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetOSLStatus: " + err.Message);
					return false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

			/// <summary>
			/// restituisce l'elenco degli utenti configurati nel database di sistema per una 
			/// particolare azienda
			/// </summary>
			/// <param name="users">Lista degli utenti trovati</param>
			/// <returns>True se la funzione ha avuto successo</returns>
			//-----------------------------------------------------------------------
			internal StringCollection GetCompanyUsers(string companyName)
			{
				return GetCompanyUsers(companyName, false);
			}

			//-----------------------------------------------------------------------
			internal StringCollection GetCompanyUsers(string companyName, bool onlyNonNTUsers)
			{
				StringCollection users = new StringCollection();

				if (!OpenSysDBConnection())
					return users;

				int companyId = GetCompanyID(companyName);

				if (companyId == -1)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyUsers: " + string.Format(LoginManagerStrings.CompanyUnknown, companyName));
					return users;
				}

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;

				try
				{
					string query =
						"SELECT MSD_Logins.Login " +
						"FROM MSD_CompanyLogins, MSD_Logins " +
						"WHERE MSD_CompanyLogins.LoginId = MSD_Logins.LoginId AND " +
						"MSD_CompanyLogins.Disabled = @Disabled AND " +
						"MSD_Logins.Disabled = @Disabled AND " +
						"MSD_CompanyLogins.CompanyId = @CompanyId";

					if (onlyNonNTUsers)
						query += " AND MSD_Logins.WindowsAuthentication = 0";

					aSqlCommand = new SqlCommand(query, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
					aSqlDataReader = aSqlCommand.ExecuteReader();

					while (aSqlDataReader.Read())
						users.Add((string)aSqlDataReader[LoginManagerStrings.Login]);
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyUsers: " + err.Message);
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}

				return users;
			}

			/// <summary>
			/// restituisce l'elenco dei ruoli configurati nel database di sistema per una 
			/// particolare azienda
			/// </summary>
			/// <param name="roles">Lista dei ruoli trovati</param>
			/// <returns>True se la funzione ha avuto successo</returns>
			//-----------------------------------------------------------------------
			internal StringCollection GetCompanyRoles(string companyName)
			{
				StringCollection roles = new StringCollection();

				if (!OpenSysDBConnection())
					return roles;

				int companyId = GetCompanyID(companyName);

				if (companyId == -1)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsActivated: " + string.Format(LoginManagerStrings.CompanyUnknown, companyName));
					return roles;
				}

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;

				try
				{
					string query =
                        "SELECT Role  " +
						"FROM MSD_CompanyRoles " +
						"WHERE " +
						"MSD_CompanyRoles.Disabled = @Disabled AND " +
						"MSD_CompanyRoles.CompanyId = @CompanyId";

					aSqlCommand = new SqlCommand(query, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
					aSqlDataReader = aSqlCommand.ExecuteReader();

					while (aSqlDataReader.Read())
						roles.Add((string)aSqlDataReader["Role"]);
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyRoles: " + err.Message);
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}

				return roles;
			}


            //-----------------------------------------------------------------------
            /// <summary>
            /// ritorna true se  l'utente fa parte del ruolo degli utentei che possono usare EB  
            /// </summary>
            /// <param name="companyName"></param>
            /// <param name="userName"></param>
            /// <returns></returns>
            internal bool HasUserEBRoles(int loginID, int companyId)
            {
                if (!IsCompanySecured(companyId)) 
                    return false;

                if (!OpenSysDBConnection())
                    return false;

                SqlCommand aSqlCommand = null;
                SqlDataReader aSqlDataReader = null;
                try
                {
                    string query =
                            "SELECT COUNT(*) " +
                            "FROM MSD_CompanyRoles, MSD_CompanyRolesLogins " +
                            "WHERE MSD_CompanyRoles.RoleId = MSD_CompanyRolesLogins.RoleId AND " +
                            "MSD_CompanyRoles.Disabled = @Disabled AND  " +
                            "MSD_CompanyRoles.Role = @Role AND " +
                            "MSD_CompanyRoles.CompanyId = @CompanyId AND " +
                            "MSD_CompanyRolesLogins.CompanyId = @CompanyId AND  " +
                            "MSD_CompanyRolesLogins.LoginId = @LoginId";

                    aSqlCommand = new SqlCommand(query, SysDBConnection);
                    aSqlCommand.Parameters.AddWithValue("@Disabled", false);
                    aSqlCommand.Parameters.AddWithValue("@Role", NameSolverStrings.EasyStudioDeveloperRole);//TODO ILARIA GERMANO RUOLO EASYBUILDER
                    aSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
                    aSqlCommand.Parameters.AddWithValue("@LoginId", loginID);
                    int val = (int)aSqlCommand.ExecuteScalar();
                    return val != 0;

                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyRoles: " + err.Message);
                }
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}

                return false;
            }

			/// <summary>
			/// restituisce l'elenco dei ruoli associati ad un utente
			/// </summary>
			/// <param name="roles">Lista dei ruoli dell'utente</param>
			/// <returns>True se la funzione ha avuto successo</returns>
			//-----------------------------------------------------------------------
			internal StringCollection GetUserRoles(string companyName, string userName)
			{
				StringCollection roles = new StringCollection();

				if (!OpenSysDBConnection())
					return null;

				int companyId = GetCompanyID(companyName);

				if (companyId == -1)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsActivated: " + string.Format(LoginManagerStrings.CompanyUnknown, companyName));
					return roles;
				}

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;

				try
				{
					int loginID = -1;

					string query1 =
						"SELECT MSD_Logins.LoginId  " +
						"FROM MSD_CompanyLogins, MSD_Logins " +
						"WHERE MSD_CompanyLogins.LoginId = MSD_Logins.LoginId AND " +
						"MSD_CompanyLogins.Disabled = @Disabled AND " +
						"MSD_Logins.Disabled = @Disabled AND " +
						"MSD_Logins.Login = @User AND " +
						"MSD_CompanyLogins.CompanyId = @CompanyId";

					aSqlCommand = new SqlCommand(query1, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
					aSqlCommand.Parameters.AddWithValue("@User", userName);
					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (!aSqlDataReader.Read())
					{
						aSqlDataReader.Close();
						return roles;
					}

					loginID = (int)aSqlDataReader[LoginManagerStrings.LoginID];

					aSqlDataReader.Close();

					string query =
						"SELECT Role  " +
						"FROM MSD_CompanyRoles, MSD_CompanyRolesLogins " +
						"WHERE MSD_CompanyRoles.RoleId = MSD_CompanyRolesLogins.RoleId AND " +
						"MSD_CompanyRoles.Disabled = @Disabled AND  " +
						"MSD_CompanyRoles.CompanyId = @CompanyId AND " +
						"MSD_CompanyRolesLogins.CompanyId = @CompanyId AND  " +
						"MSD_CompanyRolesLogins.LoginId = @LoginId";

					aSqlCommand = new SqlCommand(query, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
					aSqlCommand.Parameters.AddWithValue("@LoginId", loginID);
					aSqlDataReader = aSqlCommand.ExecuteReader();

					while (aSqlDataReader.Read())
						roles.Add((string)aSqlDataReader["Role"]);
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyRoles: " + err.Message);
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}

				return roles;
			}


			//-----------------------------------------------------------------------
			internal StringCollection GetRoleUsers(string companyName, string roleName)
			{
				StringCollection users = new StringCollection();

				if (!OpenSysDBConnection())
					return null;

				int companyId = GetCompanyID(companyName);

				if (companyId == -1)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsActivated: " + string.Format(LoginManagerStrings.CompanyUnknown, companyName));
					return users;
				}

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;

				try
				{
					string query =
						"SELECT Login " +
						"FROM MSD_Logins " +
						"WHERE (LoginId in " +
						"(SELECT MSD_CompanyRolesLogins.LoginId " +
						"FROM MSD_CompanyRolesLogins INNER JOIN " +
						"MSD_CompanyRoles ON MSD_CompanyRoles.RoleId = MSD_CompanyRolesLogins.RoleId " +
						"WHERE (MSD_CompanyRoles.CompanyId = (SELECT CompanyId " +
						"FROM MSD_Companies WHERE Company ='" + companyName + "')) AND (MSD_CompanyRoles.Role = '" + roleName + "')))";

					aSqlCommand = new SqlCommand(query, SysDBConnection);
					aSqlDataReader = aSqlCommand.ExecuteReader();

					while (aSqlDataReader.Read())
						users.Add((string)aSqlDataReader["Login"]);
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyRoles: " + err.Message);
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}

				return users;
			}


			//-----------------------------------------------------------------------
			internal string GetDbOwner(int companyId)
			{
				if (!OpenSysDBConnection())
					return String.Empty;

				//Prendo lo LoginId dell'utente
				string query = "SELECT DBUser FROM MSD_CompanyLogins, MSD_Companies  " +
								"WHERE MSD_CompanyLogins.CompanyId = @CompanyId AND " +
								"CompanyDBOwner = LoginId AND MSD_CompanyLogins.CompanyId = MSD_Companies.CompanyId";
				string dbOwner = string.Empty;

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;
				try
				{
					aSqlCommand = new SqlCommand(query, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (!aSqlDataReader.Read())
					{
						aSqlDataReader.Close();

						return string.Empty;
					}

					return (string)aSqlDataReader[LoginManagerStrings.DBUser];
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetDatabaseOwner: " + err.Message);
					return string.Empty;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

			//-----------------------------------------------------------------------
			internal bool IsCompanySecured(int companyId)
			{
				if (!OpenSysDBConnection())
					return false;

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;

				try
				{
					string commandText = "SELECT UseSecurity FROM MSD_Companies WHERE CompanyId = @CompanyId";
					aSqlCommand = new SqlCommand(commandText, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (aSqlDataReader != null && aSqlDataReader.Read())
						return (bool)aSqlDataReader[LoginManagerStrings.UseSecurity];

					return false;
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsCompanyUnderSecurity: " + err.Message);
					return false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

			//----------------------------------------------------------------------
			internal bool GetCompanyLanguage(int companyID, out string cultureUI, out string culture)
			{
				cultureUI = culture = string.Empty;

				if (!OpenSysDBConnection())
					return false;

				CultureInfoReader cir = new CultureInfoReader(BasePathFinder.BasePathFinderInstance, SysDBConnection);
				return cir.GetCompanyLanguages(companyID, false, ref cultureUI, ref culture);
			}

			//---------------------------------------------------------------------
            internal int GetDBCultureLCID(int companyID)
			{
				if (!OpenSysDBConnection())
					return -1;

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;

				try
				{
					string commandText = "SELECT DatabaseCulture FROM MSD_Companies WHERE CompanyId = @CompanyId";
					aSqlCommand = new SqlCommand(commandText, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyID);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (aSqlDataReader != null && aSqlDataReader.Read())
						return (int)aSqlDataReader["DatabaseCulture"];

					return -1;
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetDBColumnsCollationName: " + err.Message);
					return -1;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

			#endregion //comp

			#region TbSender
			string tbSenderAuthenticationToken = "{4919D404-8B8D-4DC6-BDE0-45B81332A177}";
			string tbHermesAuthenticationToken = "{2695241D-D54A-423C-8A09-ECF83BD36EED}";
			//---------------------------------------------------------------------------
			internal List<TbSenderDatabaseInfo> GetCompanyDatabasesInfo(string authenticationToken)
			{

				if (String.IsNullOrEmpty(connectionString) ||
					(!IsValidTokenForConsole(authenticationToken) &&
					!IsValidToken(authenticationToken) &&
					authenticationToken != tbSenderAuthenticationToken &&
					authenticationToken != tbHermesAuthenticationToken))
					return null;

				return GetCompanyDatabasesInfo();
			} 
			#endregion

			# region info DMS (Easy Attachment)
			//---------------------------------------------------------------------------
			internal List<DmsDatabaseInfo> GetDMSDatabasesInfo(string authenticationToken)
			{
				string dmsAuthenticationToken = "{2E8164FA-7A8B-4352-B0DC-479984070507}";

				if (String.IsNullOrEmpty(connectionString) ||
					(!IsValidTokenForConsole(authenticationToken) &&
					!IsValidToken(authenticationToken) &&
					authenticationToken != dmsAuthenticationToken))
					return null;

				return GetDMSDatabasesInfo();
			}

			///<summary>
			/// GetDMSDatabasesInfo
			/// Metodo che ritorna una collezione di oggetti di tipo DmsDatabaseInfo
			/// Per ogni azienda che utilizza EasyAttachment sono estrapolate le seguenti informazioni:
			/// 1. stringa di connessione al database documentale associato con l'utente dbowner del database
			/// 2. id della company
			/// 3. lcid o cultura del database associato all'azienda
			///</summary>
			//-----------------------------------------------------------------------
			private List<DmsDatabaseInfo> GetDMSDatabasesInfo()
            {
                SqlCommand myCommand = new SqlCommand();
                SqlDataReader mySqlDataReader = null;
                List<DmsDatabaseInfo> dmsCompanies = new List<DmsDatabaseInfo>();
                try
                {
                    // apro la connessione al database di sistema
                    if (!OpenSysDBConnection())
					return dmsCompanies;

				string username = string.Empty;
				string password = string.Empty;
				string serverName = string.Empty;
				string databaseName = string.Empty;
				bool winAuthentication = false;

				string query = @"SELECT MSD_CompanyDBSlaves.ServerName, MSD_CompanyDBSlaves.DatabaseName, 
								MSD_SlaveLogins.SlaveDBUser, MSD_SlaveLogins.SlaveDBPassword, MSD_SlaveLogins.SlaveDBWindowsAuthentication,
								MSD_Companies.CompanyId, MSD_Companies.Company, MSD_Companies.DatabaseCulture
								FROM MSD_Companies 
								INNER JOIN MSD_CompanyDBSlaves ON MSD_Companies.CompanyId = MSD_CompanyDBSlaves.CompanyId 
								INNER JOIN MSD_SlaveLogins ON MSD_CompanyDBSlaves.SlaveId = MSD_SlaveLogins.SlaveId 
								AND MSD_CompanyDBSlaves.SlaveDBOwner = MSD_SlaveLogins.LoginId";

			

				try
				{
					myCommand.CommandText = query;
					myCommand.Connection = SysDBConnection;
					mySqlDataReader = myCommand.ExecuteReader();

					if (mySqlDataReader == null || !mySqlDataReader.HasRows)
						return dmsCompanies;

					// check se SOSConnector e' attivato
					bool isSosActivated = IsActivatedInternal(NameSolverStrings.Extensions, "SosConnectorFunctionality") && IsActivatedInternal(NameSolverStrings.Extensions, "DMSSOS");

					while (mySqlDataReader.Read())
					{
						username = (string)mySqlDataReader["SlaveDBUser"];
                        password = Microarea.TaskBuilderNet.Core.Generic.Crypto.Decrypt((string)mySqlDataReader["SlaveDBPassword"]);
                        serverName = (string)mySqlDataReader["ServerName"];
                        databaseName = (string)mySqlDataReader["DatabaseName"];
						winAuthentication = (bool)mySqlDataReader["SlaveDBWindowsAuthentication"];

						string dbConnectionString = (winAuthentication)
						? string.Format(NameSolverDatabaseStrings.SQLWinNtConnectionRedux, serverName, databaseName)
						: string.Format(NameSolverDatabaseStrings.SQLConnectionRedux, serverName, databaseName, username, password);

						DmsDatabaseInfo dmsInfo = new DmsDatabaseInfo(dbConnectionString, (int)mySqlDataReader["CompanyId"], (int)mySqlDataReader["DatabaseCulture"]);
						dmsInfo.IsSOSActivated = isSosActivated;
						dmsInfo.Server = serverName;
						dmsInfo.Database = databaseName;
                        dmsInfo.Company = (string)mySqlDataReader["Company"];

						dmsCompanies.Add(dmsInfo);
					}
					mySqlDataReader.Close();
					mySqlDataReader.Dispose();

					//per ogni DmsDatabaseInfo vado a comporre la stringa di connessione al database aziendale
					query = @" SELECT MSD_Companies.CompanyDBServer, MSD_Companies.CompanyDBName, MSD_CompanyLogins.DBUser, MSD_CompanyLogins.DBPassword, MSD_CompanyLogins.DBWindowsAuthentication,
                            MSD_Companies.CompanyId, MSD_Providers.Provider, MSD_Companies.Port FROM MSD_Companies, MSD_CompanyLogins, MSD_Providers
                            WHERE MSD_Companies.CompanyId = @CompanyId AND MSD_Companies.CompanyId = MSD_CompanyLogins.CompanyId AND MSD_Companies.CompanyDBOwner = MSD_CompanyLogins.LoginId
                            AND MSD_Providers.ProviderId = MSD_Companies.ProviderId";

					myCommand.CommandText = query;

					SqlParameter param = myCommand.Parameters.Add("@CompanyId", SqlDbType.Int);

					foreach (DmsDatabaseInfo dmsInfo in dmsCompanies)
					{
						param.Value = dmsInfo.CompanyId;
						mySqlDataReader = myCommand.ExecuteReader();

						if (mySqlDataReader == null || !mySqlDataReader.HasRows)
							continue;

						while (mySqlDataReader.Read())
						{
							serverName = (string)mySqlDataReader["CompanyDBServer"];
							databaseName = (string)mySqlDataReader["CompanyDBName"];
							username = (string)mySqlDataReader["DBUser"];
                            password = Microarea.TaskBuilderNet.Core.Generic.Crypto.Decrypt((string)mySqlDataReader["DBPassword"]);
                            winAuthentication = (bool)mySqlDataReader["DBWindowsAuthentication"];
                            string provider = (string)mySqlDataReader["Provider"];
							int port = (int)mySqlDataReader["Port"];

							dmsInfo.CompanyConnectionString = TBDatabaseType.GetConnectionString(serverName, username, password, databaseName, provider, winAuthentication, false, port);
							dmsInfo.CompanyDBMSType = TBDatabaseType.GetDBMSType(provider);
						}

						mySqlDataReader.Close();
						mySqlDataReader.Dispose();
					}
				}
				catch (SqlException e)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetDMSDatabasesInfo: " + e.Message +
						 string.Format(LoginManagerStrings.SysDBInfo, SysDBConnection.DataSource, SysDBConnection.Database));
					return dmsCompanies;
				}
				catch (Exception ex)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetDMSDatabasesInfo: " + ex.Message + 
						string.Format(LoginManagerStrings.SysDBInfo, SysDBConnection.DataSource, SysDBConnection.Database));
					return dmsCompanies;
				}
				finally
				{
					if (mySqlDataReader != null && !mySqlDataReader.IsClosed)
					{
						mySqlDataReader.Close();
						mySqlDataReader.Dispose();
					}
					myCommand.Dispose();
				}
                }
                catch (SqlException e)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetDMSDatabasesInfo: " + e.Message +
                         string.Format(LoginManagerStrings.SysDBInfo, SysDBConnection.DataSource, SysDBConnection.Database));
                    return dmsCompanies;
                }
                catch (Exception ex)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetDMSDatabasesInfo: " + ex.Message +
                        string.Format(LoginManagerStrings.SysDBInfo, SysDBConnection.DataSource, SysDBConnection.Database));
                    return dmsCompanies;
                }finally
				{
					if (mySqlDataReader != null && !mySqlDataReader.IsClosed)
					{
						mySqlDataReader.Close();
						mySqlDataReader.Dispose();
					}
					myCommand.Dispose();
				}
                return dmsCompanies;
			}

			/// <summary>
			/// ritorna una lista di TbSenderDatabaseInfo contenente le info di azienda (stringa connessione, nome db, ecc)
			/// delle sole aziende sql
			/// </summary>
			//---------------------------------------------------------------------------
			internal List<TbSenderDatabaseInfo> GetCompanyDatabasesInfo()
			{
				List<TbSenderDatabaseInfo> tbSenderCompanies = new List<TbSenderDatabaseInfo>();

				// apro la connessione al database di sistema
				if (!OpenSysDBConnection())
					return tbSenderCompanies;

				string username = string.Empty;
				string password = string.Empty;
				string serverName = string.Empty;
				string databaseName = string.Empty;
				bool winAuthentication = false;

				string query = @"SELECT  
								MSD_Companies.Company,
								MSD_Companies.CompanyDBServer,
								MSD_Companies.CompanyDBName,
								MSD_Companies.PreferredLanguage,
								MSD_Companies.ApplicationLanguage,
								MSD_CompanyLogins.DBUser, 
								MSD_CompanyLogins.DBPassword, 
								MSD_CompanyLogins.DBWindowsAuthentication,
								MSD_Companies.CompanyId
								FROM MSD_Companies 
								INNER JOIN MSD_CompanyLogins 
								ON MSD_Companies.CompanyId = MSD_CompanyLogins.CompanyId 
								INNER JOIN MSD_Providers
								on MSD_Companies.ProviderId = MSD_Providers.ProviderId
								WHERE MSD_CompanyLogins.LoginId = MSD_Companies.CompanyDBOwner
								and MSD_Providers.Provider = 'SQLOLEDB'";

				try
				{
					using (SqlCommand myCommand = new SqlCommand(query, SysDBConnection))
					using (SqlDataReader mySqlDataReader = myCommand.ExecuteReader())
					{
						if (mySqlDataReader == null || !mySqlDataReader.HasRows)
							return tbSenderCompanies;

						TbSenderDatabaseInfo senderInfo;

						while (mySqlDataReader.Read())
						{
							senderInfo = new TbSenderDatabaseInfo();

							senderInfo.Username = (string)mySqlDataReader["DBUser"];
                            senderInfo.Password = Microarea.TaskBuilderNet.Core.Generic.Crypto.Decrypt((string)mySqlDataReader["DBPassword"]);
                            senderInfo.Company = (string)mySqlDataReader["Company"];
                            senderInfo.ServerName = (string)mySqlDataReader["CompanyDBServer"];
							senderInfo.DatabaseName = (string)mySqlDataReader["CompanyDBName"];
							senderInfo.WinAuthentication = (bool)mySqlDataReader["DBWindowsAuthentication"];
							string dbConnectionString = (winAuthentication)
								? string.Format(NameSolverDatabaseStrings.SQLWinNtConnectionRedux, serverName, databaseName)
								: string.Format(NameSolverDatabaseStrings.SQLConnectionRedux, serverName, databaseName, username, password);
							senderInfo.CompanyId = (int)mySqlDataReader["CompanyId"];
							senderInfo.CompanyCultureUI = (string)mySqlDataReader[CultureInfoReader.PreferredLanguage] ?? string.Empty;
							senderInfo.CompanyCulture = (string)mySqlDataReader[CultureInfoReader.ApplicationLanguage] ?? string.Empty;

							senderInfo.ConnectionString = dbConnectionString;
								
							tbSenderCompanies.Add(senderInfo);
						}
					}
				}
				catch (SqlException e)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyDatabasesInfo: " + e.Message +
						 string.Format(LoginManagerStrings.SysDBInfo, SysDBConnection.DataSource, SysDBConnection.Database));
					return tbSenderCompanies;
				}
				catch (Exception ex)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyDatabasesInfo: " + ex.Message +
						string.Format(LoginManagerStrings.SysDBInfo, SysDBConnection.DataSource, SysDBConnection.Database));
					return tbSenderCompanies;
				}

				return tbSenderCompanies;
			}

			//---------------------------------------------------------------------------
			internal string GetDMSConnectionString(string authenticationToken)
			{
				if (String.IsNullOrEmpty(connectionString) ||
					(!IsValidTokenForConsole(authenticationToken) && !IsValidToken(authenticationToken)))
					return string.Empty;

				AuthenticationSlot slot = authenticationSlots.GetSlot(authenticationToken);
				if (slot == null)
					return string.Empty;

				return GetDMSConnectionString(slot.CompanyID, slot.LoginID);
			}

			///<summary>
			/// GetDMSConnectionString
			/// Metodo che ritorna la stringa di connessione al database documentale
			/// (dati un companyId e un loginId)
			///</summary>
			//-----------------------------------------------------------------------
			private string GetDMSConnectionString(int companyID, int loginID)
			{
				// apro la connessione al database di sistema
				if (!OpenSysDBConnection())
					return string.Empty;

				// controllo che l'azienda abbia uno slave associato
				if (!CompanyUseDBSlave(companyID))
					return string.Empty;

				string username = string.Empty;
				string password = string.Empty;
				string serverName = string.Empty;
				string databaseName = string.Empty;
				bool winAuthentication = false;

				string query = @"SELECT MSD_CompanyDBSlaves.ServerName, MSD_CompanyDBSlaves.DatabaseName, 
								MSD_SlaveLogins.SlaveDBUser, MSD_SlaveLogins.SlaveDBPassword, MSD_SlaveLogins.SlaveDBWindowsAuthentication
								FROM MSD_CompanyDBSlaves 
								INNER JOIN MSD_SlaveLogins ON 
								MSD_CompanyDBSlaves.SlaveId = MSD_SlaveLogins.SlaveId
								WHERE MSD_CompanyDBSlaves.CompanyId = @CompanyId AND MSD_SlaveLogins.LoginId = @LoginId";

				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;
				string dbConnectionString = string.Empty;

				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Connection = SysDBConnection;
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyID);
					aSqlCommand.Parameters.AddWithValue("@LoginId", loginID);
					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (aSqlDataReader == null || !aSqlDataReader.HasRows)
						return dbConnectionString;

					if (aSqlDataReader.Read())
					{
						username = (string)aSqlDataReader["SlaveDBUser"];
                        password = Microarea.TaskBuilderNet.Core.Generic.Crypto.Decrypt((string)aSqlDataReader["SlaveDBPassword"]);
                        serverName = (string)aSqlDataReader["ServerName"];
                        databaseName = (string)aSqlDataReader["DatabaseName"];
						winAuthentication = (bool)aSqlDataReader["SlaveDBWindowsAuthentication"];
					}
					else
						return dbConnectionString;

					dbConnectionString = (winAuthentication)
						? string.Format(NameSolverDatabaseStrings.SQLWinNtConnectionRedux, serverName, databaseName)
						: string.Format(NameSolverDatabaseStrings.SQLConnectionRedux, serverName, databaseName, username, password);
				}
				catch (SqlException e)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetDMSConnectionString: " + e.Message + 
						string.Format(LoginManagerStrings.SysDBInfo, SysDBConnection.DataSource, SysDBConnection.Database));
					return string.Empty;
				}
				catch (Exception ex)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetDMSConnectionString: " + ex.Message + 
						string.Format(LoginManagerStrings.SysDBInfo, SysDBConnection.DataSource, SysDBConnection.Database));
					return string.Empty;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
					{
						aSqlDataReader.Close();
						aSqlDataReader.Dispose();
					}
				}

				return dbConnectionString;
			}

			///<summary>
			/// CompanyUseDBSlave
			/// Metodo che ritorna un valore booleano per sapere se l'azienda ha agganciato
			/// uno slave database documentale (dato un companyId)
			///</summary>
			//---------------------------------------------------------------------------
			private bool CompanyUseDBSlave(int companyID)
			{
				bool exist = false;

				SqlDataReader aSqlDataReader = null;

				try
				{
					string query = "SELECT UseDBSlave FROM MSD_Companies WHERE CompanyId = @CompanyId";

					SqlCommand command = new SqlCommand(query, SysDBConnection);
					command.Parameters.AddWithValue("@CompanyId", companyID);
					aSqlDataReader = command.ExecuteReader();

					if (aSqlDataReader == null || !aSqlDataReader.HasRows)
						return exist;

					while (aSqlDataReader.Read())
						exist = (bool)aSqlDataReader["UseDBSlave"];
				}
				catch (SqlException e)
				{
					ExtendedInfo extendedInfo = new ExtendedInfo();
					extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
					extendedInfo.Add(DatabaseLayerStrings.ErrorCode, e.Number);
					extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyUseDBSlave");
					extendedInfo.Add(DatabaseLayerStrings.Library, "Microarea.WebServices.LoginManager");
					extendedInfo.Add(DatabaseLayerStrings.Source, e.Source);
					extendedInfo.Add(DatabaseLayerStrings.StackTrace, e.StackTrace);
					diagnostic.Set(DiagnosticType.Error, "Error reading UseDBSlave column from system database", extendedInfo);
					exist = false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
					{
						aSqlDataReader.Close();
						aSqlDataReader.Dispose();
					}
				}
				return exist;
			}
			#endregion

			#region info DataSynchro
			//---------------------------------------------------------------------------
			internal List<DataSynchroDatabaseInfo> GetDataSynchroDatabasesInfo(string authenticationToken)
			{
				string dsAuthenticationToken = "{2E8164FA-7A8B-4352-B0DC-479984070222}";

				if (String.IsNullOrEmpty(connectionString) ||
					(!IsValidTokenForConsole(authenticationToken) &&
					!IsValidToken(authenticationToken) &&
					authenticationToken != dsAuthenticationToken))
					return null;

				return GetDataSynchroDatabasesInfo();
			}

			///<summary>
			/// GetDataSynchroDatabasesInfo
			/// Metodo che ritorna una collezione di oggetti di tipo DataSynchroDatabaseInfo
			/// Per ogni azienda che ha il flag UseDataSynchro a true (ovvero utilizza il DataSynchronizer) 
			/// sono estrapolate le informazioni di login con il dbowner per istanziare un Mago ed altri
			/// dati potenzialmente utili
			///</summary>
			//-----------------------------------------------------------------------
			private List<DataSynchroDatabaseInfo> GetDataSynchroDatabasesInfo()
			{
				List<DataSynchroDatabaseInfo> dataSynchroCompanies = new List<DataSynchroDatabaseInfo>();

				// apro la connessione al database di sistema
				if (!OpenSysDBConnection())
					return dataSynchroCompanies;

                string query = @"SELECT MSD_CompanyLogins.CompanyId, COMPANIES.Company, COMPANIES.CompanyDBServer, COMPANIES.CompanyDBName,
                                MSD_CompanyLogins.LoginId, MSD_Logins.Login, MSD_Logins.Password, MSD_Logins.WindowsAuthentication,
                                MSD_CompanyLogins.DBUser, MSD_CompanyLogins.DBPassword, 
                                MSD_CompanyLogins.DBWindowsAuthentication, MSD_Providers.Provider, COMPANIES.Port
                                FROM MSD_CompanyLogins
                                INNER JOIN MSD_Companies AS COMPANIES ON COMPANIES.CompanyId = MSD_CompanyLogins.CompanyId 
                                INNER JOIN MSD_Logins ON MSD_CompanyLogins.LoginId = MSD_Logins.LoginId
                                INNER JOIN MSD_Providers ON COMPANIES.ProviderId = MSD_Providers.ProviderId
                                WHERE COMPANIES.UseDataSynchro = 1 and MSD_CompanyLogins.LoginId IN (SELECT TOP 1 LoginId from MSD_CompanyLogins WHERE Admin = 1 AND CompanyId = COMPANIES.CompanyId)
                                ORDER BY MSD_CompanyLogins.CompanyId, MSD_CompanyLogins.LoginId";

                SqlCommand myCommand = new SqlCommand();
				SqlDataReader mySqlDataReader = null;

				try
				{
					myCommand.CommandText = query;
					myCommand.Connection = SysDBConnection;
					mySqlDataReader = myCommand.ExecuteReader();

					if (mySqlDataReader == null || !mySqlDataReader.HasRows)
						return dataSynchroCompanies;

					while (mySqlDataReader.Read())
					{
						string serverName = (string)mySqlDataReader["CompanyDBServer"];
						string databaseName = (string)mySqlDataReader["CompanyDBName"];
						string username = (string)mySqlDataReader["DBUser"];
                        string password = Microarea.TaskBuilderNet.Core.Generic.Crypto.Decrypt((string)mySqlDataReader["DBPassword"]);
                        bool winAuthentication = (bool)mySqlDataReader["DBWindowsAuthentication"];
                        string provider = (string)mySqlDataReader["Provider"];
						int port = (int)mySqlDataReader["Port"];

						string dbConnectionString = TBDatabaseType.GetConnectionString(serverName, username, password, databaseName, provider, winAuthentication, false, port);

						DataSynchroDatabaseInfo dsInfo = new DataSynchroDatabaseInfo(dbConnectionString, (int)mySqlDataReader["CompanyId"]);
						dsInfo.Server = serverName;
						dsInfo.Database = databaseName;
						dsInfo.User = username;
						dsInfo.Password = password;
						dsInfo.WinAuthentication = winAuthentication;

						dsInfo.CompanyName = (string)mySqlDataReader["Company"];
						dsInfo.CompanyDBMSType = TBDatabaseType.GetDBMSType(provider);
						dsInfo.LoginId = (int)mySqlDataReader["LoginId"];
						dsInfo.LoginName = (string)mySqlDataReader["Login"];
                        dsInfo.LoginPassword = Microarea.TaskBuilderNet.Core.Generic.Crypto.Decrypt((string)mySqlDataReader["Password"]);
                        dsInfo.LoginWindowsAuthentication = (bool)mySqlDataReader["WindowsAuthentication"];

                        dataSynchroCompanies.Add(dsInfo);
					}
				}
				catch (SqlException e)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetDataSynchroDatabasesInfo: " + e.Message +
						 string.Format(LoginManagerStrings.SysDBInfo, SysDBConnection.DataSource, SysDBConnection.Database));
					return dataSynchroCompanies;
				}
				catch (Exception ex)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetDataSynchroDatabasesInfo: " + ex.Message +
						string.Format(LoginManagerStrings.SysDBInfo, SysDBConnection.DataSource, SysDBConnection.Database));
					return dataSynchroCompanies;
				}
				finally
				{
					if (mySqlDataReader != null && !mySqlDataReader.IsClosed)
					{
						mySqlDataReader.Close();
						mySqlDataReader.Dispose();
					}
					myCommand.Dispose();
				}

				return dataSynchroCompanies;
			}
			#endregion

			#region info su user

			//-----------------------------------------------------------------------
			internal string GetLoginName(int loginId, out string userDescription, out string email)
			{
				userDescription = string.Empty;
				email = string.Empty;

				if (!OpenSysDBConnection())
					return String.Empty;

				//Prendo lo LoginId dell'utente
				string query = "SELECT Login, Description, EMail FROM MSD_Logins WHERE Disabled = @Disabled AND LoginId= @LoginId";
				string userName = string.Empty;

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;
				try
				{
					aSqlCommand = new SqlCommand(query, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@LoginId", loginId);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (!aSqlDataReader.Read())
						return string.Empty;

					userName = (string)aSqlDataReader[LoginManagerStrings.Login];
					userDescription = (string)aSqlDataReader[LoginManagerStrings.Description];
					email = (string)aSqlDataReader[LoginManagerStrings.EMail];
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetLoginName: " + err.Message);
					return string.Empty;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}

				return userName;
			}

			//-----------------------------------------------------------------------
			internal bool GetLoginId(string userName, out int loginId, out bool winNTAuth, out bool webLogin, out bool gdiLogin)
			{
				bool concurrentLogin = false;
				return GetLoginId(userName, out loginId, out winNTAuth, out webLogin, out gdiLogin, out concurrentLogin);
			}


			/// <summary>
			/// restituisce la login ID dell'utente passato
			/// </summary>
			/// <param name="userName">Nome utente</param>
			/// <param name="loginId">Identificativo dell'utente</param>
			/// <param name="winNTAuth">Indica se l'utente è in sicurezza integrata</param>
			/// <returns></returns>
			//-----------------------------------------------------------------------
			internal bool GetLoginId(string userName, out int loginId, out bool winNTAuth, out bool webLogin, out bool gdiLogin, out bool concurrentLogin)
			{
				loginId = -1;
				winNTAuth = false;
				webLogin = false;
				gdiLogin = false;
				concurrentLogin = false;

				if (!OpenSysDBConnection())
					return false;

				//Prendo lo LoginId dell'utente
				string query = "SELECT LoginId, WindowsAuthentication, SmartClientAccess, WebAccess, ConcurrentAccess FROM MSD_Logins WHERE Disabled = @Disabled AND Login = @Login";
				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;
				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Connection = SysDBConnection;
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@Login", userName);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (!aSqlDataReader.Read())
						return false;

					loginId = (int)aSqlDataReader[LoginManagerStrings.LoginID];
					winNTAuth = (bool)aSqlDataReader[LoginManagerStrings.WindowsAuthentication];
					gdiLogin = (bool)aSqlDataReader[LoginManagerStrings.SmartClientAccess];
					////prima gli attributi erano indipendenti.
					//ora sono esclusivi, imposto i  flag con questo ordine 
					//in modo da amantenere una gerarchia in caso ci fossero attributi multipli a true
					if (!gdiLogin)
					{
						concurrentLogin = (bool)aSqlDataReader[LoginManagerStrings.ConcurrentLogin];
						if (!concurrentLogin)
							webLogin = (bool)aSqlDataReader[LoginManagerStrings.WebAccess];
					}

					return true;
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetLoginID: " + err.Message);
					return false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

            
			//-----------------------------------------------------------------------
			internal bool IsFloatingUser(string loginName, out bool floating)
			{
				floating = false;
				bool conc = false;
				bool named = false;

				if (!OpenSysDBConnection())
					return false;

				//Prendo lo LoginId dell'utente
				string query = "SELECT ConcurrentAccess, SmartClientAccess FROM MSD_Logins WHERE Disabled = @Disabled AND Login = @Login";
				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;
				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Connection = SysDBConnection;
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@Login", loginName);
					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (!aSqlDataReader.Read())
						return false;

					conc = (bool)aSqlDataReader[LoginManagerStrings.ConcurrentLogin];
					named = (bool)aSqlDataReader[LoginManagerStrings.SmartClientAccess];

					if (activationManager.GetEdition() == Edition.Enterprise)
						floating = named || conc;
					else
						floating = !named && conc;

					return true;
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsFloatingUser: " + err.Message);
					return false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

            //-----------------------------------------------------------------------
            internal bool IsWebUser(string loginName, out bool web)
			{
                web = false;
			
				if (!OpenSysDBConnection())
					return false;

				//Prendo lo LoginId dell'utente
				string query = "SELECT webAccess FROM MSD_Logins WHERE Disabled = @Disabled AND Login = @Login";
				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;
				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Connection = SysDBConnection;
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@Login", loginName);
					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (!aSqlDataReader.Read())
						return false;

					web = (bool)aSqlDataReader[LoginManagerStrings.WebAccess];
					
					return true;
				}
				catch (Exception err)
				{
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsWebUser: " + err.Message);
					return false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

			/// <summary>
			/// verifica che la password passata sia quella associata all'utente
			/// </summary>
			/// <param name="loginID">id utente</param>
			/// <param name="password">Password</param>
			/// <param name="winNTAuth">True se l'utente è in sicurezza integrata</param>
			/// <returns></returns>
			//-----------------------------------------------------------------------
			private bool IsValidUser(int loginID, string user, string password, bool winNTAuth)
			{
				//se siamo in sicurezza integrata verifica o la sessionguid presa dal file loginmngsession 
				//oppure la pwd (di dominio, utilizzata per easylook)
				if (winNTAuth && ((sessionGUID != password) && !ValidateDomainCredentials(user, password)))
					return false;

				if (InstallationData.ServerConnectionInfo.UseStrongPwd && !password.IsStrongPassword(InstallationData.ServerConnectionInfo.MinPasswordLength))
					return false;

				if (winNTAuth)
					return true;

				if (!OpenSysDBConnection())
					return false;

				//Prendo lo LoginId dell'utente
				string query = "SELECT Password FROM MSD_Logins " +
					"WHERE LoginId = @loginID AND WindowsAuthentication = @WindowsAuth AND Disabled = @Disabled";

				string dbPassword = string.Empty;

				SqlCommand aSqlCommand = new SqlCommand();
				aSqlCommand.Parameters.AddWithValue("@loginID", loginID);
				aSqlCommand.Parameters.AddWithValue("@WindowsAuth", winNTAuth);
				aSqlCommand.Parameters.AddWithValue("@Disabled", false);
				SqlDataReader reader = null;

				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Connection = SysDBConnection;

					reader = aSqlCommand.ExecuteReader();
					if (reader.Read())
						dbPassword = (string)reader["password"];
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsValidUser: " + err.Message);
					return false;
				}
				finally
				{
					if (reader != null && !reader.IsClosed)
						reader.Close();
				}

                string decryptedPwd = Microarea.TaskBuilderNet.Core.Generic.Crypto.Decrypt(dbPassword);
                return string.Compare(decryptedPwd, password) == 0;
            }

			//---------------------------------------------------------------------------
			private bool GetPasswordInfo
				(
				int loginId,
				out bool userCannotChangePassword,
				out bool userMustChangePassword,
				out DateTime expiredDatePassword,
				out bool passwordNeverExpired,
				out bool expiredDateCannotChange
				)
			{
				expiredDatePassword = DefaultMinDateTime;
				userMustChangePassword = false;
				userCannotChangePassword = false;
				passwordNeverExpired = true;
				expiredDateCannotChange = true;

				if (!OpenSysDBConnection())
					return false;

				//Dati sul db di sistema dell'utente relativi al cambio pwd.
				string query = "SELECT UserMustChangePassword, UserCannotChangePassword, " +
					"PasswordNeverExpired, ExpiredDateCannotChange, ExpiredDatePassword FROM MSD_Logins " +
					"WHERE Disabled = @Disabled AND WindowsAuthentication = @WinAuth AND LoginId = @LoginID";

				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;
				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Connection = SysDBConnection;
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@WinAuth", false);
					aSqlCommand.Parameters.AddWithValue("@LoginID", loginId);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (aSqlDataReader.Read())
					{
						userMustChangePassword = (bool)aSqlDataReader["UserMustChangePassword"];
						userCannotChangePassword = (bool)aSqlDataReader["UserCannotChangePassword"];

						expiredDatePassword = (DateTime)aSqlDataReader["ExpiredDatePassword"];
						passwordNeverExpired = (bool)aSqlDataReader["PasswordNeverExpired"];
						expiredDateCannotChange = (bool)aSqlDataReader["ExpiredDateCannotChange"];
					}

					return true;
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetPasswordInfo: " + err.Message);
					return false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

			/// <summary>
			/// Dice se un utente è amministatore
			/// </summary>
			/// <param name="loginId">Identificativo dell'utente</param>
			/// <param name="companyId">Identificativo della compagnia</param>
			/// <returns>true se l'utente è amministratore</returns>
			//-----------------------------------------------------------------------
			private bool IsAdmin(int loginId, int companyId)
			{
				if (!OpenSysDBConnection())
					return false;

				string query = "SELECT Admin FROM MSD_CompanyLogins " +
								"WHERE Disabled = @Disabled AND CompanyId = @CompanyId AND LoginId = @LoginID";

				bool isAdmin = false;

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;

				try
				{
					aSqlCommand = new SqlCommand(query, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
					aSqlCommand.Parameters.AddWithValue("@LoginID", loginId);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (aSqlDataReader.Read())
						isAdmin = (bool)aSqlDataReader[LoginManagerStrings.Admin];

					return isAdmin;
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsAdmin: " + err.Message);
					return false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}


            //-----------------------------------------------------------------------
            internal bool IsWinNT(int loginId)
            {
                if (!OpenSysDBConnection())
                    return false;

                string query = "SELECT WindowsAuthentication FROM MSD_Logins " +
                                "WHERE LoginId = @LoginID";

                bool isAdmin = false;

                SqlCommand aSqlCommand = null;
                SqlDataReader aSqlDataReader = null;

                try
                {
                    aSqlCommand = new SqlCommand(query, SysDBConnection);
                    aSqlCommand.Parameters.AddWithValue("@LoginID", loginId);

                    aSqlDataReader = aSqlCommand.ExecuteReader();

                    if (aSqlDataReader.Read())
                        isAdmin = (bool)aSqlDataReader[LoginManagerStrings.WindowsAuthentication];

                    return isAdmin;
                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "isWinNT: " + err.Message);
                    return false;
                }
                finally
                {
                    if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
                        aSqlDataReader.Close();
                }
            }

            /// <summary>
            /// Dice se un utente è un EasyBuilder Developer
            /// </summary>
            /// <param name="loginId">Identificativo dell'utente</param>
            /// <param name="companyId">Identificativo della company</param>
            /// <returns>true se l'utente è EasyBuilder Developer</returns>
            //-----------------------------------------------------------------------
            internal bool IsEasyBuilderDeveloper(int loginId, int companyId)
			{

				if (!OpenSysDBConnection())
					return false;

				string query = "SELECT EBDeveloper FROM MSD_CompanyLogins " +
								"WHERE Disabled = @Disabled AND CompanyId = @CompanyId AND LoginId = @LoginID";

				bool isEBDeveloper = false;

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;

				try
				{
					aSqlCommand = new SqlCommand(query, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
					aSqlCommand.Parameters.AddWithValue("@LoginID", loginId);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (aSqlDataReader.Read())
						isEBDeveloper = (bool)aSqlDataReader[LoginManagerStrings.EBDeveloper];

                  
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsEasyBuilderDeveloper: " + err.Message);
					return false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
                    
				}
                return isEBDeveloper || HasUserEBRoles(loginId, companyId);
			}

			/// <summary>
			/// Dice se un utente è un EasyBuilder Developer
			/// </summary>
			/// <param name="loginId">Identificativo dell'utente</param>
			/// <param name="companyId">Identificativo della company</param>
			/// <returns>true se l'utente è EasyBuilder Developer</returns>
			//-----------------------------------------------------------------------
			internal bool IsEasyBuilderDeveloper(string authenticationToken)
			{
				if (!OpenSysDBConnection())
					return false;

				string userName = string.Empty;
				int loginId = -1;
				string companyName = string.Empty;
				int companyId = -1;
				bool admin = false;
				string dbName = string.Empty;
				string dbServer = string.Empty;
				int providerId = -1;
				bool security = false;
				bool auditing = false;
				bool useKeyedUpdate = false;
				bool transactionUse = false;
				bool useUnicode = false;
				string preferredLanguage = string.Empty;
				string applicationLanguage = string.Empty;
				string providerName = string.Empty;
				string providerDescription = string.Empty;
				bool useConstParameter = false;
				bool stripTrailingSpaces = false;
				string providerCompanyConnectionString = string.Empty;
				string nonProviderCompanyConnectionString = string.Empty;
				string dbUser = string.Empty;
				string processName = string.Empty;
				string userDescription = string.Empty;
				string email = string.Empty;
				bool easyBuilderDeveloper = false;
                bool rowSecurity = false;
				bool dataSynchro = false;

				bool ok = GetLoginInformation(
					authenticationToken,
					out userName,
					out loginId,
					out companyName,
					out companyId,
					out admin,
					out dbName,
					out dbServer,
					out providerId,
					out security,
					out auditing,
					out useKeyedUpdate,
					out transactionUse,
					out useUnicode,
					out preferredLanguage,
					out applicationLanguage,
					out providerName,
					out providerDescription,
					out useConstParameter,
					out stripTrailingSpaces,
					out providerCompanyConnectionString,
					out nonProviderCompanyConnectionString,
					out dbUser,
					out processName,
					out userDescription,
					out email,
					out easyBuilderDeveloper,
                    out rowSecurity,
					out dataSynchro
				   );

				if (!ok)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "IsEasyBuilderDeveloper: Cannot get login information from token " + authenticationToken);
					return false;
				}

				return IsEasyBuilderDeveloper(loginId, companyId);
			}

			/// <summary>
			/// Dice se un utente è amministatore dell'Area Riservata su sito web
			/// </summary>
			/// <param name="loginId">Identificativo dell'utente</param>
			/// <returns>true se l'utente è amministratore dell'Area Riservata su sito web</returns>
			//-----------------------------------------------------------------------
			internal bool UserCanAccessWebSitePrivateArea(int loginId)
			{
				if (!OpenSysDBConnection())
					return false;

				string query = "SELECT PrivateAreaWebSiteAccess FROM MSD_Logins " +
								"WHERE LoginId = @LoginID";

				bool isAdmin = false;

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;

				try
				{
					aSqlCommand = new SqlCommand(query, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@LoginID", loginId);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (aSqlDataReader.Read())
						isAdmin = (bool)aSqlDataReader[LoginManagerStrings.PrivateAreaWebSiteAccess];

					return isAdmin;
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "PrivateAreaWebSiteAccess: " + err.Message);
					return false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

            //metodo che recupera dal db aziendale in uso le info relative agli utenti che devono essere inviate al back end
            //-----------------------------------------------------------------------
            internal List<LBag> GetLogins()
            {
                List<LBag> list = new List<LBag>();
                if (!OpenSysDBConnection())
                    return list;

                //Prendo lo LoginId dell'utente
                string query = "SELECT Login,PrivateAreaWebSiteAccess, Disabled  FROM MSD_Logins";
               
                string userName = string.Empty;
                SqlCommand aSqlCommand = null;
                SqlDataReader aSqlDataReader = null;
                try
                {
                    aSqlCommand = new SqlCommand(query, SysDBConnection);
                    aSqlDataReader = aSqlCommand.ExecuteReader();

                    if (!aSqlDataReader.HasRows)
                        return list;

                    while (aSqlDataReader.Read())
                        list.Add(new LBag( (string)aSqlDataReader[LoginManagerStrings.Login],  (bool)aSqlDataReader[LoginManagerStrings.Disabled],  (bool)aSqlDataReader[LoginManagerStrings.PrivateAreaWebSiteAccess]));
  
                }
                catch (Exception err)
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetLogins: " + err.Message);
                    return list;
                }
                finally
                {
                    if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
                        aSqlDataReader.Close();
                }

                return list;
            }


			#endregion

			#region info legate a utente + company


			//-----------------------------------------------------------------------
			internal StringCollection EnumAllUsers()
			{
				StringCollection users = new StringCollection();

				if (!OpenSysDBConnection())
					return users;

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;

				try
				{
					string query = "SELECT Login FROM MSD_Logins WHERE Disabled = @Disabled";
					aSqlCommand = new SqlCommand(query, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					while (aSqlDataReader.Read())
						users.Add((string)aSqlDataReader[LoginManagerStrings.Login]);
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "EnumAllUsers: " + err.Message);
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}

				return users;
			}

			//-----------------------------------------------------------------------
			internal StringCollection EnumAllCompanyUsers(int companyId, bool onlyNonNTUsers)
			{
				StringCollection users = new StringCollection();

				if (!OpenSysDBConnection())
					return users;

				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;

				try
				{
					string query = "SELECT MSD_Logins.Login " +
						"FROM  MSD_Logins INNER JOIN " +
						"MSD_CompanyLogins ON MSD_CompanyLogins.LoginId = MSD_Logins.LoginId " +
						"WHERE MSD_Logins.Disabled = @Disabled AND " +
						"CompanyId =" + companyId;

					if (onlyNonNTUsers)
						query += " AND MSD_Logins.WindowsAuthentication = 0";

					aSqlCommand = new SqlCommand(query, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					while (aSqlDataReader.Read())
						users.Add((string)aSqlDataReader[LoginManagerStrings.Login]);

				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "EnumAllUsers: " + err.Message);
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}

				return users;
			}

			/// <summary>
			/// con i dati di login crea la stringa di connessione al db aziendale
			/// </summary>
			/// <param name="authenticationToken">token di autenticazione</param>
			/// <param name="useProvider">Nome del database aziendale</param>
			/// <returns></returns>
			//-----------------------------------------------------------------------
			private string GetCompanyConnectionString
				(
				int loginId,
				int companyId,
				bool useProvider,
				string dbName,
				string dbServer,
				string providerName,
				int port
				)
			{
				if (!OpenSysDBConnection())
					return String.Empty;

				string dbConnectionString = string.Empty;
               
				//cerco user e pwd pwe il db aziendale nella CompanyLogins, se non lo trovo 
				//prendo quello della Company e in caso contrario gli stessi della login di osl
				string query = "SELECT DBUser, DBPassword, DBWindowsAuthentication FROM MSD_CompanyLogins " +
									"WHERE Disabled = @Disabled AND CompanyId = @CompanyId AND LoginId = @LoginId";
				string dbDefaultUser = string.Empty;
				string dbDefaultPassword = string.Empty;
				bool dbAuthenticationWindows = false;
				SqlCommand aSqlCommand = null;
				SqlDataReader aSqlDataReader = null;

				try
				{
					aSqlCommand = new SqlCommand(query, SysDBConnection);
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					aSqlCommand.Parameters.AddWithValue("@CompanyId", companyId);
					aSqlCommand.Parameters.AddWithValue("@LoginId", loginId);

					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (aSqlDataReader.Read())
					{
						dbDefaultUser = (string)aSqlDataReader[LoginManagerStrings.DBUser];
                        dbDefaultPassword = Microarea.TaskBuilderNet.Core.Generic.Crypto.Decrypt((string)aSqlDataReader[LoginManagerStrings.DBPassword]);
                        dbAuthenticationWindows = (bool)aSqlDataReader[LoginManagerStrings.DBWindowsAuthentication];
                    }

                    string connProvider = providerName;
					if (dbDefaultUser.Length == 0 && !dbAuthenticationWindows)
					{
						diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, string.Format(LoginManagerStrings.ErrNoUserConfigured, loginId.ToString()));
						return string.Empty;
					}

					DBMSType eDBMSType = TBDatabaseType.GetDBMSType(providerName);
					if (!dbAuthenticationWindows)
					{
						if (dbDefaultUser == null || dbDefaultUser.Length == 0)
						{
							diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, string.Format(LoginManagerStrings.ErrNoUserConfigured, loginId.ToString()));
							return string.Empty;
						}
						switch (eDBMSType)
						{
							case DBMSType.ORACLE:
								dbConnectionString = string.Format
									(
									NameSolverDatabaseStrings.OracleConnection,
									dbServer,
									dbDefaultUser,
									dbDefaultPassword
									);
								break;
							case DBMSType.SQLSERVER:
                               
								    dbConnectionString = string.Format
								    (
								    NameSolverDatabaseStrings.SQLConnection,
								    dbServer,
								    dbName,
								    dbDefaultUser,
								    dbDefaultPassword
								    );
								break;
						}
                        if (useProvider)
                        {
                            if (string.Compare(providerName, NameSolverDatabaseStrings.SQLODBCProvider) == 0)
                            {
                                return string.Format(NameSolverDatabaseStrings.DriverConnAttribute, connProvider) + string.Format
                                    (
                                    NameSolverDatabaseStrings.ODBCConnectionString,
                                    dbServer,
                                    dbName,
                                    dbDefaultUser,
                                    dbDefaultPassword
                                    ); 
                                
                            }
                            else
                                dbConnectionString = string.Format(NameSolverDatabaseStrings.ProviderConnAttribute, connProvider) + dbConnectionString;
                        }
					}
					else
						switch (eDBMSType)
						{
							case DBMSType.ORACLE:
								dbConnectionString = (useProvider)
												   ? string.Format(NameSolverDatabaseStrings.OracleWinNtConnectionWithProvider, providerName, dbServer)
												   : string.Format(NameSolverDatabaseStrings.OracleWinNtConnection, dbServer, dbDefaultUser, dbDefaultPassword);
								break;
							case DBMSType.SQLSERVER:
                             
                                    dbConnectionString = string.Format(NameSolverDatabaseStrings.SQLWinNtConnection, dbServer, dbName);

                                if (useProvider)
                                {
                                    if (string.Compare(providerName, NameSolverDatabaseStrings.SQLODBCProvider) == 0)
                                    {
                                        return string.Format(NameSolverDatabaseStrings.DriverConnAttribute, providerName) + string.Format(NameSolverDatabaseStrings.ODBCWinAuthConnectionString, dbServer, dbName);
                                       
                                    }
                                    else
                                        dbConnectionString = string.Format(NameSolverDatabaseStrings.ProviderConnAttribute, providerName) + dbConnectionString;
                                }
								break;
						}

					return dbConnectionString;
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyConnectionString: " + err.Message);
					return string.Empty;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

			/// <summary>
			/// Restituisce le lingue impostate per l'utente, se l'utente 
			/// non ha lingue configurate si prendone quelle dell'azienda
			/// </summary>
			/// <param name="userID">identificativo dell'utente</param>
			/// <param name="companyID">identificativo della compagnia</param>
			/// <param name="preferredLanguage">lingua dell'utente</param>
			/// <returns></returns>
			//-----------------------------------------------------------------------
			private bool GetUserLanguages(int loginID, int companyID, out string preferredLanguage, out string applicationLanguage)
			{
				preferredLanguage = string.Empty;
				applicationLanguage = string.Empty;

				if (!OpenSysDBConnection())
					return false;

				CultureInfoReader cir = new CultureInfoReader(BasePathFinder.BasePathFinderInstance, SysDBConnection);
				return cir.GetUserLanguages(loginID, companyID, true, out preferredLanguage, out applicationLanguage);
			}

			//-----------------------------------------------------------------------
			internal bool IsUserAssociatedToCompany(string userName, string companyName)
			{
				StringCollection users = GetCompanyUsers(companyName);

				foreach (string user in users)
				{
					if (string.Compare(user, userName, true, CultureInfo.InvariantCulture) == 0)
						return true;
				}

				return false;
			}

			#endregion

			#region info su company di un utente

			/// <summary>
			/// restituisce l'elenco delle aziende associate alla loginId
			/// se la loginId è -1 ritorna tutte le company
			/// </summary>
			/// <param name="userName">Nome utente</param>
			/// <param name="companies">Aziende associate all'utente</param>
			/// <returns>True se la funzione ha avuto successo</returns>
			//-----------------------------------------------------------------------
			private StringCollection EnumCompaniesById(int loginId)
			{
				if (!OpenSysDBConnection())
					return null;

				StringCollection companies = new StringCollection();

				ArrayList companyIds = GetCompanyIDs(loginId);

				if (companyIds.Count == 0)
					return companies;

				//prendo il nome delle aziende in base ai CompanyId
				//costrisco la query di select in
				string query = "SELECT Company FROM MSD_Companies WHERE Disabled = 0 AND CompanyId IN(";

				for (int i = 0; i < companyIds.Count; i++)
				{
					query += companyIds[i].ToString();

					if (i != companyIds.Count - 1)
						query += ", ";
				}

				query += ")";

				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;

				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Connection = SysDBConnection;

					aSqlDataReader = aSqlCommand.ExecuteReader();

					while (aSqlDataReader.Read())
						companies.Add((string)aSqlDataReader[LoginManagerStrings.Company]);
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "EnumCompaniesById: " + err.Message);
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}

				return companies;
			}

			/// <summary>
			/// restituisce l'elenco delle aziende associate allo userName
			/// </summary>
			/// <param name="userName">Nome utente</param>
			/// <returns>Aziende associate all'utente</returns>
			//-----------------------------------------------------------------------
			internal StringCollection EnumCompanies(string userName)
			{
				if (!OpenSysDBConnection())
					return new StringCollection();

				if (userName == string.Empty)
					return new StringCollection();

				int loginId = -1;
				bool wnta = false;
				bool webLogin = false;
				bool gdiLogin = false;

				if (!GetLoginId(userName, out loginId, out wnta, out webLogin, out gdiLogin))
					return new StringCollection();

				return EnumCompaniesById(loginId);
			}


			/// <summary>
			/// dice se l'utente è in sicurezza integrata oppure no
			/// </summary>
			/// <param name="userName">Nome utente</param>
			/// <returns>Aziende associate all'utente</returns>
			//-----------------------------------------------------------------------
			internal bool IsIntegratedSecurityUser(string userName)
			{
				if (!OpenSysDBConnection())
					return false;

				if (userName == string.Empty)
					return false;

				int loginId = -1;
				bool wnta = false;
				bool webLogin = false;
				bool gdiLogin = false;

				if (!GetLoginId(userName, out loginId, out wnta, out webLogin, out gdiLogin))
					return false;

				return wnta;
			}


			/// <summary>
			/// restituisce gli ID delle compagnie associate all'utente
			/// </summary>
			/// <param name="loginId">Identificatore dell'utente</param>
			/// <param name="companyIds">Gli identificatori delle compagnie associate all'utente</param>
			/// <returns></returns>
			//-----------------------------------------------------------------------
			private ArrayList GetCompanyIDs(int loginId)
			{
				ArrayList companyIds = new ArrayList();

				if (!OpenSysDBConnection())
					return companyIds;

				//prendo l'elenco degli ID delle aziende associate allo LoginId
				string query = "SELECT CompanyId FROM MSD_CompanyLogins WHERE Disabled = @Disabled";
				if (loginId != -1)
					query += " AND LoginId = @LoginId";

				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;
				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Parameters.AddWithValue("@Disabled", false);
					if (loginId != -1)
						aSqlCommand.Parameters.AddWithValue("@LoginId", loginId);
					aSqlCommand.Connection = SysDBConnection;

					aSqlDataReader = aSqlCommand.ExecuteReader();

					while (aSqlDataReader.Read())
						companyIds.Add((int)aSqlDataReader["CompanyId"]);
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetCompanyIDs: " + err.Message);
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}

				return companyIds;
			}

			#endregion

			#region info varie

			/// <summary>
			/// Restituisce informazioni relative ad un provider
			/// </summary>
			/// <param name="providerID">Identificatore del provider</param>
			/// <param name="useConstParameter/// "></param>
			/// <param name="stripTrailingSpaces/// "></param>
			/// <returns></returns>
			//-----------------------------------------------------------------------
            internal bool GetProviderInfo(int providerID, out string providerName, out string providerDescription, out bool useConstParameter, out bool stripTrailingSpaces)
			{
				useConstParameter = false;
				stripTrailingSpaces = false;
				providerName = string.Empty;
				providerDescription = string.Empty;

				if (!OpenSysDBConnection())
					return false;

				string query = "SELECT Provider, Description, UseConstParameter, " +
								"StripTrailingSpaces FROM MSD_Providers WHERE ProviderId = @providerID";

				SqlCommand aSqlCommand = new SqlCommand();
				SqlDataReader aSqlDataReader = null;
				try
				{
					aSqlCommand.CommandText = query;
					aSqlCommand.Connection = SysDBConnection;
					aSqlCommand.Parameters.AddWithValue("@providerID", providerID);
					aSqlDataReader = aSqlCommand.ExecuteReader();

					if (!aSqlDataReader.Read())
						return false;

					providerName = (string)aSqlDataReader[LoginManagerStrings.Provider];
					providerDescription = (string)aSqlDataReader[LoginManagerStrings.Description];
					useConstParameter = (bool)aSqlDataReader[LoginManagerStrings.UseConstParameter];
					stripTrailingSpaces = (bool)aSqlDataReader[LoginManagerStrings.StripTrailingSpaces];
					return true;
				}
				catch (Exception err)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetProviderInfo: " + err.Message);
					return false;
				}
				finally
				{
					if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
						aSqlDataReader.Close();
				}
			}

			/// <summary>
			/// dato il nome di un provider restituisce TRUE se è un provider per ORACLE false altrimenti
			/// questo serve per differenziare la stringa di connessione
			/// </summary>
			/// <param name="providerName"></param>
			/// <returns></returns>
			//-----------------------------------------------------------------------
			private bool IsOracleProvider(string providerName)
			{
				return
					(
					string.Compare(providerName, NameSolverDatabaseStrings.OraOLEDBProvider, true, CultureInfo.InvariantCulture) == 0 ||
					string.Compare(providerName, NameSolverDatabaseStrings.MSDAORAProvider, true, CultureInfo.InvariantCulture) == 0
					);

			}

			//-----------------------------------------------------------------------
			private string GetDBUser(string connectionString)
			{
				string lowerConnectionString = connectionString.ToLower(CultureInfo.InvariantCulture);
				int s = lowerConnectionString.IndexOf("user id=\'");

				if (s == -1)
					return string.Empty;

				s += 9;

				int e = lowerConnectionString.IndexOf("\'", s);

				if (e == -1)
					return string.Empty;

				return lowerConnectionString.Substring(s, e - s);
			/*
                MODIFICHE DA VERIFICARE!!!! todo !
				string lowerConnectionString = connectionString.ToLower(CultureInfo.InvariantCulture);
				int s = lowerConnectionString.IndexOf("user id=\'");
                bool odbcUid=false;
                if (s == -1)
                {
                    s = lowerConnectionString.IndexOf("uid=");
                    if (s == -1)
                        return string.Empty;
                    else odbcUid = true;
                }

				
				int e=0;
                if (odbcUid)
                {
                    s += 4;
                    e = lowerConnectionString.IndexOf("; pwd");
                }
                else
                {
                    s += 9;
                    e = lowerConnectionString.IndexOf("/'", s);
                }

				if (e == -1 && !odbcUid)
					return string.Empty;

                
				return lowerConnectionString.Substring(s, e - s);*/
			}

			#endregion

			#region Funzioni per il trace di operazioni
			//----------------------------------------------------------------------
			internal void TraceAction
				(
				string company,
				string login,
				TraceActionType type,
				string processName,
				string winUser,
				string location
				)
			{
				if (!OpenSysDBConnection())
					return;

				string query = "INSERT INTO MSD_TRACE ([Company], [Login], [Data], [Type], [ProcessName], [WinUser], [Location]) " +
				"VALUES (@CompanyT, @LoginT, @DataT, @TypeT, @ProcessNameT, @WinUserT, @LocationT)";

				SqlCommand sqlCommand = new SqlCommand();

				try
				{
					sqlCommand.CommandText = query;
					sqlCommand.Connection = SysDBConnection;
					sqlCommand.Parameters.AddWithValue("@CompanyT", company);
					sqlCommand.Parameters.AddWithValue("@LoginT", login);
					sqlCommand.Parameters.AddWithValue("@DataT", DateTime.Now);
					sqlCommand.Parameters.Add("@TypeT", SqlDbType.SmallInt, 2).Value = Convert.ToUInt16(type);
					sqlCommand.Parameters.AddWithValue("@ProcessNameT", processName);
					sqlCommand.Parameters.AddWithValue("@WinUserT", winUser);
					sqlCommand.Parameters.AddWithValue("@LocationT", location);

					sqlCommand.ExecuteNonQuery();
				}
				catch (Exception exc)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "TraceAction: " + exc.Message);
					sqlCommand.Dispose();
					return;
				}

				sqlCommand.Dispose();
			}

			//----------------------------------------------------------------------
			internal bool HasUserAlreadyChangedPasswordToday(string user)
			{
				if (!OpenSysDBConnection())
					return false;
                //verifico che la password non sia già cambiata oggi, senza considerare però i cambi effettuati da console, che sono da considerarsi operazioni di amministrazione
                //altrimenti se cambio la password ad un utente impostandogli il flag di cambio obbligatorio poi lui non riuscirà a cambiarla lo stesso giorno.
                //vedi anomalia 20629 
				string query = "SELECT COUNT(*) FROM MSD_Trace " +
							"WHERE day(Data) = @NDay AND month(Data) = @NMonth " +
                            "AND year(Data) = @NYear AND Type = @TypeT AND Login = @UserT AND ProcessName != @MicroareaConsole";

				SqlCommand sqlCommand = new SqlCommand();
				int recFound = 0;
				try
				{
					sqlCommand.CommandText = query;
					sqlCommand.Connection = SysDBConnection;
					sqlCommand.Parameters.AddWithValue("@NDay", DateTime.Now.Day);
					sqlCommand.Parameters.AddWithValue("@NMonth", DateTime.Now.Month);
					sqlCommand.Parameters.AddWithValue("@NYear", DateTime.Now.Year);
                    sqlCommand.Parameters.AddWithValue("@MicroareaConsole", ProcessType.MicroareaConsole);
                    
					sqlCommand.Parameters.Add("@TypeT", SqlDbType.SmallInt, 2).Value = Convert.ToUInt16(TraceActionType.ChangePassword);
					sqlCommand.Parameters.AddWithValue("@UserT", user);
					recFound = (int)sqlCommand.ExecuteScalar();
				}
				catch (Exception exc)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "HasUserAlreadyChangedPasswordToday: " + exc.Message);
					sqlCommand.Dispose();
					return false;
				}

				sqlCommand.Dispose();

				return recFound > 0;
			}

			#endregion

			#region pubblicazione dati di UserInfo.config
			//---------------------------------------------------------------------------
			internal string GetEdition()
			{
				if (!IsActivated())
					return string.Empty;
				return activationManager.GetEdition().ToString();
			}

            //---------------------------------------------------------------------------
            /// <summary>
            /// Ritorna l'edition comprendo anche PRO lite  ( che altrimetni è una pro)
            /// </summary>
            /// <returns></returns>
            internal string GetEditionType()
            {
                /*
        Undefined,
		Standard,
		Professional,
		Enterprise, 
        ALL
                 * */
                string ed = GetEdition();
                if (ed.StartsWith("pro", StringComparison.InvariantCultureIgnoreCase))
                {
                    DBNetworkType dbnt = GetDBNetworkType();
                    if (dbnt == DBNetworkType.Small)
                        return "Professional Lite";
                }
                return ed;
            }

            //-----------------------------------------------------------------------
            internal DBNetworkType GetDBNetworkType()
			{
				if (!IsActivated())
					return DBNetworkType.Undefined;
				return this.activationManager.GetDBNetworkType();
			}

			//---------------------------------------------------------------------------
			internal string GetCountry()
			{
				if (!IsActivated())
					return string.Empty;
				string country = activationManager.GetCountry();
				if (country == null || country.Length == 0)
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "GetCountry returns null.");
				return country;
			}

			//---------------------------------------------------------------------------
			internal byte[] GetConfigurationStream()
			{
                if (activationManager == null)
                    return null;
				MemoryStream fs = new MemoryStream();

				BinaryFormatter f = new BinaryFormatter();

                try
                {
                    f.Serialize(fs, activationManager);
                }
                catch
                { 
                    return null; 
                }
				return fs.ToArray();
			}

			//---------------------------------------------------------------------------
			internal string GetUserInfoString()
			{
				string ret = string.Empty;
				if (activationManager.User != null)
					activationManager.User.GetXmlString(out ret);

				return ret;
			}

			//---------------------------------------------------------------------------
			internal string GetUserInfoID()
			{
				return activationManager.GetStandardUserId();
			}

			#endregion

			#region funzioni che vengono chiamate dalla console in risposta agli eventi di cancellazione utente / company
			//---------------------------------------------------------------------------
			internal void DeleteAssociation(int loginId, int companyId)
			{
				authenticationSlots.RemoveAssociation(loginId, companyId);
			}

			//---------------------------------------------------------------------------
			internal void DeleteUser(int loginId)
			{
				authenticationSlots.RemoveUser(loginId);
			}

			//---------------------------------------------------------------------------
			internal void DeleteCompany(int companyId)
			{
				authenticationSlots.RemoveCompany(companyId);
			}

			//---------------------------------------------------------------------------
			internal void ReloadUserArticleBindings()
			{
				authenticationSlots.Init(this.activationManager, this.SysDBConnection, this.diagnostic);
			}

            #endregion

            #region funzioni per il branding

            //---------------------------------------------------------------------------
            internal string GetBrandedApplicationTitle(string applicationName)
            {
                string appMenuTitle = InstallationData.BrandLoader.GetApplicationBrandMenuTitle(applicationName);
                return appMenuTitle.IsNullOrEmpty() ? applicationName : appMenuTitle;
            }

            //---------------------------------------------------------------------------
            internal string GetMasterSolutionBrandedName()
            {
                return GetBrandedKey(GetMasterSolution());
            }

            //---------------------------------------------------------------------------
            internal string GetMasterSolution()
            {
                return activationManager.GetMasterSolutionName();
               
            }

			//---------------------------------------------------------------------------
			internal string GetMasterProductBrandedName()
			{
				string masterProduct = activationManager.GetMasterProductID();
				return GetBrandedKey(masterProduct);
			}

			//---------------------------------------------------------------------------
			internal string GetBrandedKey(string source)
			{

                return InstallationData.BrandLoader.GetBrandedStringBySourceString(source);
			}

			//---------------------------------------------------------------------------
			internal string GetBrandedProducerName()
			{

                return InstallationData.BrandLoader.GetCompanyName();
			}

			//---------------------------------------------------------------------------
			internal string GetBrandedProductTitle()
			{
                BrandInfo bInfo = InstallationData.BrandLoader.GetMainBrandInfo();
				if (bInfo == null)
					return string.Empty;

				return bInfo.ProductTitle;
			}

			#endregion

			#region funzioni di security

			//---------------------------------------------------------------------
			private bool ExistGrant(string aNameSpace, int type, int userId, int companyId, GrantType grantType)
			{
				if (SysDBConnection == null || SysDBConnection.State != ConnectionState.Open)
					return false;

				bool isSecurityLicensed = IsActivatedInternal("MicroareaConsole", "SecurityAdmin") &&
					IsCompanySecured(companyId);

				if (!isSecurityLicensed)
					return true;

				//Controllo se ho i grant per utilizare l'oggetto
				Security security = new Security	//superclasse di MenuSecurityFilter
					(
					SysDBConnection,
					companyId,
					userId,
					isSecurityLicensed,
					false
					);

				int grants = 0;
				int isProtected = 0;
				bool existGrants = true;
				bool ok = security.GetObjectUserGrant(aNameSpace, type, out isProtected, out grants);
				if (ok && isProtected == 1)
					existGrants = security.ExistGrant(grants, grantType);

				security.Dispose();

				return existGrants;
			}

			//---------------------------------------------------------------------------
			private int GetSecurityTypeByNSType(NameSpaceObjectType nsType)
			{
				switch (nsType)
				{
					case NameSpaceObjectType.Report:
						return 4;
					case NameSpaceObjectType.Function:
						return 3;
					case NameSpaceObjectType.Document:
						return 5;
					default:
						return 0;
				}
			}

			//---------------------------------------------------------------------------
			internal bool CanUseNamespace(string nameSpace, string authenticationToken, GrantType grantType)
			{
				if (!OpenSysDBConnection())
					return false;

				int loginId = -1;
				int companyId = -1;
				bool webLogin = false;
				bool security = false;
				bool auditing = false;
                bool rowSecurity = false;
				bool dataSynchro = false;

				if (!GetAuthenticationInformations(authenticationToken, out loginId, out companyId, out webLogin))
					return false;

				NameSpace ns = new NameSpace(nameSpace);
				if (!ns.IsValid())
					return false;

				string smallNS = ns.GetNameSpaceWithoutType();
				if (smallNS == null || smallNS == string.Empty)
					return false;

				//Check if the namespace can be executed according the security light
				//The SL check must be done before the standard security one
				if (securityLightEnabled)
					return !SecurityLightManager.IsAccessToObjectNameSpaceDenied(ns, companyId, loginId, this.SysDBConnection, (grantType == GrantType.SilentMode));

				if (!GetOslStatus(companyId, out security, out auditing, out rowSecurity, out dataSynchro))
					return false;

				if (!security)
					return true;

				if (IsAdmin(loginId, companyId))
					return true;

				int type = GetSecurityTypeByNSType(ns.NameSpaceType.Type);
				if (type == 0)
					return false;

				return ExistGrant(smallNS, type, loginId, companyId, grantType);
			}

			//---------------------------------------------------------------------------
			internal bool IsSecurityLightAccessAllowed(string nameSpace, string authenticationToken, bool unattended)
			{
				if (!securityLightEnabled)
					return true;

				int loginId = -1;
				int companyId = -1;
				bool webLogin = false;

				if (!GetAuthenticationInformations(authenticationToken, out loginId, out companyId, out webLogin))
					return false;

				NameSpace ns = new NameSpace(nameSpace);
				if (!ns.IsValid())
					return false;

				return !SecurityLightManager.IsAccessToObjectNameSpaceDenied
					(
					ns,
					companyId,
					loginId,
					this.SysDBConnection,
					unattended
					);

			}

			#endregion

			#region metodi di controllo attivazione


			//---------------------------------------------------------------------------
			internal ModuleNameInfo[] GetArticlesWithNamedCal()
			{
				if (activationManager == null || activationManager.Products == null)
					return null;
				ProductInfo[] pis = activationManager.Products;
				ArrayList list = new ArrayList();
				foreach (ProductInfo p in pis)
				{
					if (p == null) continue;
					foreach (ArticleInfo art in p.Articles)
					{
						if (art == null || !art.HasNamedCal() || !art.Licensed)
							continue;
                        int calN = art.NamedCalNumber;
                        if (art.CalType == CalTypeEnum.AutoTbs)
                            calN = art.ConcurrentCalNumber;
                        ModuleNameInfo m = new ModuleNameInfo(art.Name, art.LocalizedName, calN);
						list.Add(m);
					}
				}
				return (ModuleNameInfo[])list.ToArray(typeof(ModuleNameInfo));
			}

            //
            //Questo metodo serve attualmente solo alla COnsole per mostrare  i moduli floating con cal che possono essere associati agli utenti (dalla 393 di settembre 2013)
            //Siccome io non posso associare il modulo full, perchè poi la funzionalità usata appartiene effettivamente al modulo manufacturing, che però è un modulo back senza cal proprie,
            //faccio in modo di usare lo stesso metodo che uso nella iscalavailable, per tirare fuori i back exclusive non attivati (quelli attivati verrano aggiunti poi al loro turno)
            //Quindi io creo un oggetto che ha il nome visualizzato del full, per intenderci, ma dentro il db scrivo manufacturing
            //POtrebbe essere un problema nel momento in cui ci saranno più moduli di questo tipo dentro uno stesso modulo
            //---------------------------------------------------------------------------
            internal ModuleNameInfo[] GetArticlesWithFloatingCal()
            {
                if (activationManager == null || activationManager.Products == null)
                    return null;
                ProductInfo[] pis = activationManager.Products;
                ArrayList list = new ArrayList();
                foreach (ProductInfo p in pis)
                {
                    if (p == null) continue;
                    foreach (ArticleInfo art in p.Articles)
                    {
                        if (art == null || !art.HasFloatingCal() || !art.Licensed)
                            continue;
                        bool found = false;
                        foreach (IncludedSMInfo ismi in art.IncludedSM)
                        {
                            if (ismi.includedSMMode == IncludedSMInfo.IncludedSMModeEnum.Exclusive)
                            {
                                ArticleInfo foundArt = activationManager.FindModule(ismi.Name, p.Articles);
                                if (foundArt == null)
                                    continue;
                                if (!foundArt.Licensed)
                                {
                                    ModuleNameInfo m = new ModuleNameInfo(foundArt.Name, foundArt.LocalizedName + "("+art.LocalizedName+")", art.ConcurrentCalNumber);
                                    list.Add(m);  
                                    found = true;
                                }
                            }
                        }
                        if (!found)//se non ho trovato moduli inclusi exlusive non attivati aggiungol'attuale.
                        {
                            ModuleNameInfo m = new ModuleNameInfo(art.Name, art.LocalizedName, art.ConcurrentCalNumber);
                            list.Add(m);
                        }
                    }
                }
                return (ModuleNameInfo[])list.ToArray(typeof(ModuleNameInfo));
            }

              //---------------------------------------------------------------------------
            internal void RefreshFloatingMark()
            {
                authenticationSlots.RefreshFloatingMark();
            }

           
            //----------------------------------------------------------------------
            internal void SetMessagesRead(string authenticationToken, string messageID
                )
            {
                if (
                    authenticationToken == null ||
                    authenticationToken.Trim().Length == 0 ||
                    !IsValidToken(authenticationToken)
                    )
                    return;

				//già il metodo isvalid verifica che slot sia diverso da null
				string userName = authenticationSlots.GetSlot(authenticationToken).LoginName;
				try
				{
					messagesQueue.SetMessageRead(userName, messageID);
				}
				catch (ArgumentNullException)
				{ }
				catch (ArgumentException)
				{ }

			}

			//-----------------------------------------------------------------------
			private void GetFirstAdvertisement()
			{
				Thread.Sleep(120000);
				// Aspettiamo 2 minuti affinchè LgM sia completamente caricato.
				GetAdvertisement(null, null);
			}

          

			//-------------------------------------------------------------------------------------
			private Parameters GetParameters(int mode)
			{
				string userid = string.Empty;
                string useridact = string.Empty;
                if (activationManager.User != null &&
                    activationManager.User.UserIdInfos != null &&
                    activationManager.User.UserIdInfos.Length > 0 &&
                    activationManager.User.UserIdInfos[0] != null &&
                    activationManager.User.UserIdInfos[0].Value != null &&
                    activationManager.User.UserIdInfos[0].Value.Trim().Length > 0
                    )
                {
                    userid = activationManager.User.UserIdInfos[0].Value.Trim();
                    useridact = activationManager.User.UserIdInfos[0].ActivationID;
                    if (!String.IsNullOrEmpty(useridact))
                        useridact = useridact.Trim();
                }
				string traceRoute = string.Empty;
				XmlNode wce = activationManager.GetInstallationWceNode();
				Parameters p = new Parameters(
					activationManager.ActivationVersion,
					BasePathFinder.BasePathFinderInstance.Installation,
					LocalMachine.GetMacAddresses(),
					traceRoute,
					Dns.GetHostName(),
					wce,
					activationManager.ActivationKey,
					mode,
					string.Empty,
					this.GetCountry(),
					0,//CounterManager.ActivationPeriod,
					0,//CounterManager.WarningPeriod, 
					"",//error
					userid,
                    useridact,
					new ContractDataBag(),//counterManager.CurrentCounter.ContractData
                    ((activationManager.GetSerialNumberType() == SerialNumberType.DevelopmentIU) || (activationManager.IsDevelopmentPlus())) ? SerialNumberType.Development : activationManager.GetSerialNumberType()
					
                    );
                p.UsedCompanies.AddRange(GetCompanyDataBags());
                p.LoggedClientData = GetLoggedClientData();
                p.FeUsed = feused;
                p.Customizations = GetCustomizations();
				try
				{
					if (
						activationManager != null &&
						activationManager.User != null &&
						activationManager.User.UsersListAgreement
						)
						p.LoginInfos.AddRange(GetLogins());
				}
				catch//Continuo l'esecuzione, alla peggio perdo la lista deglle login mago.
				{}

                return p;

			}

            //-----------------------------------------------------------------------
            private StringCollection GetCustomizations()
            {
                StringCollection list = new StringCollection();
                try
                {
                    if (BaseCustomizationContext.CustomizationContextInstance == null || BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications == null || BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications.Count == 0)
                    {
                        foreach (BaseApplicationInfo bai in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
                            if (bai.ApplicationType == ApplicationType.Customization)
                                foreach (BaseModuleInfo bmi in bai.Modules)
                                    list.Add(bai.Name + "." + bmi.Name);
                        return list;
                    }


                    foreach (IEasyBuilderApp i in BaseCustomizationContext.CustomizationContextInstance.EasyBuilderApplications)
                    {
                        foreach (ICustomListItem x in i.EasyBuilderAppFileListManager.CustomList)
                        {
                            //recupero solo le indicazioni di dll
                            if (x.FilePath.ToLowerInvariant().EndsWith(".dll"))
                            {
                                //taglio per risparmiare spazio
                                int num = x.FilePath.IndexOf("Applications/");
                                if (num == 0)
                                    list.Add(x.FilePath);
                                else
                                    list.Add(x.FilePath.Substring(num));
                            }
                        }
                    }
                }
                catch (Exception exc){ 
                        diagnostic.Set(
                              DiagnosticType.LogInfo | DiagnosticType.Information,
                              "Customizations loading error: " + exc.ToString()
                              );}
                return list;
            }

            //-----------------------------------------------------------------------
            private List<ClientData> GetLoggedClientData()
            {
                List<ClientData> cds = new List<ClientData>();
                foreach (object e in ClientDataList.Values)
                {
                    ClientData cd = e as ClientData;
                    if (cd != null)
                    cds.Add(cd);
                }
                return cds;
            }

			//-----------------------------------------------------------------------
			internal bool SaveLicensed(string xml, string name)
			{

				if (string.IsNullOrEmpty(xml))
					return false;
          
				XmlDocument xmldocL = new XmlDocument();
				try
				{
					xmldocL.LoadXml(xml);

                    XmlNodeList modlist = xmldocL.SelectNodes("//SalesModule");
                    if (modlist == null || modlist.Count == 0)
                    {
                        diagnostic.Set(
                              DiagnosticType.LogInfo | DiagnosticType.Information,
                              "No activated modules for " + name
                              );
                        return false;
                    }



                    string filename = String.Format("{0}.Licensed.config", name);
					string path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetLogManAppDataPath(), filename);
					xmldocL.Save(path);
					return true;
				}
				catch (Exception exc)
				{
					diagnostic.Set(
							DiagnosticType.LogInfo | DiagnosticType.Error,
							"Error saving Licensed" + exc.Message
							);
					return false;
				}
            }
            //-----------------------------------------------------------------------
            internal string ValidateIToken(string itoken, string authenticationToken)
            {
                if (itoken.IsNullOrWhiteSpace()) return null;
                if (authenticationToken.IsNullOrWhiteSpace()) return LoginManagerStrings.NoValidUserLogged;//"Nessun utente valido connesso.";

                AuthenticationSlot slot = authenticationSlots.GetSlot(authenticationToken);
                if (slot == null || !slot.Logged) return LoginManagerStrings.NoValidUserLoggedNow;// "Nessun utente valido connesso in questo momento.";
                SSOLoginDataBag bag = null;
                int result = GetDataViaSSOID(itoken, out bag);
                if (result == 0)
                {
                    if (slot.ssologinDataBag == null)
                    {//in realtà se sono in questa situazione  vuol dire che non ho una login corrente fatta da infinity
                        //verifico che il token criptato punti all utentecompany correntemente loginato su questo slot
                        if (slot.LoginID == bag.loginid && slot.CompanyID == bag.companyid)
                        {
                            bag.token = new TokenInfinity(itoken);
                            slot.ssologinDataBag = bag;
                            //proseguo così fa il check del time
                        }
                        else return LoginManagerStrings.InfinityUserNotLogged;//"È stata ricevuta una richiesta di funzionalità IMago, ma l'utente correntemente connesso non è lo stesso connesso con Infinity, se si desidera accedere a tale funzionalità  sarà necessario cambiare login o avviare una nuova login.";
                    }
                    if (slot.ssologinDataBag.SSOID == bag.SSOID)//se ssoid espresso dal token criptato è lo stesso che ho salvato nello slot allora ok ( non accetto token diversi
                    {
                        bool timeok = bag.CheckTime();//controllo temporale del token.
                        if (!timeok) return LoginManagerStrings.TokenExpired;//"Il token infinity è scaduto e non più valido." ;
                        return null;

                    }
                    else return LoginManagerStrings.InfinityUserNotLogged;//"È stata ricevuta una richiesta di funzionalità IMago, ma l'utente correntemente connesso non è lo stesso connesso con Infinity, se si desidera accedere a tale funzionalità  sarà necessario cambiare login o avviare una nuova login.";
                }
                return LoginManagerWrapperStrings.GetString(result);
            }


            //-----------------------------------------------------------------------
            internal bool SaveUserInfo(string xml)
			{
				if (string.IsNullOrEmpty(xml))
					return false;
				XmlDocument xmldocL = new XmlDocument();
				try
				{
					xmldocL.LoadXml(xml);
                    string path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetLogManAppDataPath(), "UserInfo.config");
					xmldocL.Save(path);
					return true;
				}
				catch (Exception exc)
				{
					diagnostic.Set(
							DiagnosticType.LogInfo | DiagnosticType.Error,
							"Error saving UserInfo" + exc.Message
							);
					return false;
				}
			}

			//-----------------------------------------------------------------------
			internal void DeleteLicensed(string name)
			{
				string filename = String.Format("{0}.Licensed.config", name);
				string path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetLogManAppDataPath(), filename);
				try
				{
					File.Delete(path);
				}
				catch { }
			}

            //-----------------------------------------------------------------------
            internal void DeleteUserInfo()
            {
                string filename ="Userinfo.config";
                string path = Path.Combine(BasePathFinder.BasePathFinderInstance.GetLogManAppDataPath(), filename);
                try
                {
                    File.Delete(path);
                }
                catch { }
            }

			//-----------------------------------------------------------------------
			internal string GetMainSerialNumber()
			{
				return activationManager.GetMainSerialNumber();
			}

            //-----------------------------------------------------------------------
            internal string GetMLUExpiryDate()
            {
                return contractData.PaymentRenewalDate.ToString();
            }

			//-----------------------------------------------------------------------
			private Parameters GetParameters()
			{
				return GetParameters(0);
			}

			//-----------------------------------------------------------------------
			private void GetAdvertisement(object sender, ElapsedEventArgs e)
			{
				try
				{
					Type t = GetParametersMngClass("ProtocolManager");
					if (t == null)
						return;//Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.PongNotReadDll));

					Parameters p = GetParameters();


					IList advertisements = null;
					try
					{

						object[] args = new object[] { p , ActivationManager.GetMasterProductID()};
						advertisements = t.InvokeMember(
							"GetMessages",
							BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod,
							null,
							null,
							args) as IList;
					}
					catch (UriFormatException ufe)
					{
						string msg = "777-Invalid uri";
						if (ufe.InnerException != null)
							msg = String.Concat(msg, " : ", ufe.InnerException.Message);

						diagnostic.Set(
							DiagnosticType.LogInfo | DiagnosticType.Warning,
							msg
							);
					}
					catch (ProxySettingsException pse)
					{
						string msg = "777-Error setting proxy information";
						if (pse.InnerException != null)
							msg = String.Concat(msg, " : ", pse.InnerException.Message);

						diagnostic.Set(
							DiagnosticType.LogInfo | DiagnosticType.Warning,
							msg
							);
					}
					catch (Exception exc)
					{
						string msg = "Error code 777";
						if (exc.InnerException != null)
							msg = String.Concat(msg, " : ", exc.InnerException.Message);

						diagnostic.Set(
							DiagnosticType.LogInfo | DiagnosticType.Warning,
							msg
							);
					}

					messagesQueue.PumpMessageToQueue(advertisements);
				}
				catch (Exception exc)
				{
					try
					{
						diagnostic.Set(
							DiagnosticType.LogInfo | DiagnosticType.Error,
							exc.ToString()
							);
					}
					catch
					{}
				}
			}

			//---------------------------------------------------------------------
			internal ProxySettings GetProxySettings()
			{
				ProxySettings proxySettings = null;
				try
				{
					string filePath = BasePathFinder.BasePathFinderInstance.GetProxiesFilePath();
					proxySettings = ProxySettings.GetServerProxySetting(filePath);
					return (proxySettings == null) ?
							new ProxySettings() :
							proxySettings;

				}
				catch (Exception exc)
				{
					diagnostic.Set(
						DiagnosticType.LogInfo | DiagnosticType.Error,
						"error getting proxy settings: " + exc.Message
						);
					return new ProxySettings();
				}
			}

			//---------------------------------------------------------------------
            internal void SetProxySettings(ProxySettings proxySettings)
			{
				if (proxySettings == null) return;
				try
				{
					string filePath = BasePathFinder.BasePathFinderInstance.GetProxiesFilePath();
					proxySettings.SetFilePath(filePath);
					proxySettings.Save();
				}
				catch (Exception exc)
				{
					diagnostic.Set(
						DiagnosticType.LogInfo | DiagnosticType.Error,
						"error setting proxy settings: " + exc.Message
						);
				}
			}

			//----------------------------------------------------------------------
            internal bool SendAccessMail()
			{
				Type t = GetParametersMngClass("ProtocolManager");
				if (t == null)
					return false;//Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.PongNotReadDll));

				Parameters p = GetParameters();

				bool ok = false;
				try
				{
					object[] args = new object[] { p  , ActivationManager.GetMasterProductID() };
					ok = (bool)t.InvokeMember(
						"SendAccessMail",
						BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod,
						null,
						null,
						args);
				}

				catch (UriFormatException ufe)
				{
					string msg = "1717-Invalid uri";
					if (ufe.InnerException != null)
						msg = String.Concat(msg, " : ", ufe.InnerException.Message);

					diagnostic.Set(
						DiagnosticType.LogInfo | DiagnosticType.Warning,
						msg
						);
				}
				catch (ProxySettingsException pse)
				{
					string msg = "1717-Error setting proxy information";
					if (pse.InnerException != null)
						msg = String.Concat(msg, " : ", pse.InnerException.Message);

					diagnostic.Set(
						DiagnosticType.LogInfo | DiagnosticType.Warning,
						msg
						);
				}
				catch (Exception exc)
				{
					string msg = "Error code 1717";
					if (exc.InnerException != null)
						msg = String.Concat(msg, " : ", exc.InnerException.Message);

					diagnostic.Set(
						DiagnosticType.LogInfo | DiagnosticType.Warning,
						msg
						);
				}
				return ok;
			}

			//-----------------------------------------------------------------------
			private void FirtPingTimer_Elapsed(object sender, ElapsedEventArgs e)
            {
				if (!TryLockResources())
					return;
				try
                {
                    PingInternal(69);
                    // CallFE();

                }
                catch (Exception exc)
				{
					diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "FPTimer: " + exc.Message);
				}
				finally
				{
					ReleaseResources();
				}
			}
            
			//-----------------------------------------------------------------------
            private void SetTimer()
            {
                Random ftpr = new Random();
                int ftprInt = 60000;

                // in data 22/02 imposto il timer, come da richiesta di EI, su 60 minuti...
                //su primo ping fallito ritento ogni 16/20 minuti???
                //dopo il primo ping ritenta fra i 5 e i 6 giorni, 
                //NEWS luglio 2013 dopo che renner è rimasta bloccata perchè han cambiato politiche del firewall e poi sono andati tutti in vacanza:
                //se fallisce un ping dopo il primo (quindi quello periodiche dei 6 giorni) gli do 3/4 giorni di lasco per riprovare coi successivi tentativi, invece di avere solo un'ora, così se sono al mare han tempo di sistemare
                //Inoltre sarebbe opportuno indicare (nella console?) una mail da inviare tramite smtp interno quando fallisce il ping, così da poter avvisare il sistemista sotto l'ombrellone.
                switch (pingTime)
                {
                    case PingTimeEnum.Start: ftprInt = ftpr.Next(GetMilliSeconds(5, 0, 0), GetMilliSeconds(7, 0, 0)); //imposto un valore random tra 5/7 minuti circa come intervallo iniziale
                        break;
                    case PingTimeEnum.Short: ftprInt = ftpr.Next(GetMilliSeconds(18, 0, 0), GetMilliSeconds(20, 0, 0));
                        break;
                    case PingTimeEnum.Temp: ftprInt = ftpr.Next(GetMilliSeconds(1, 1, 1), GetMilliSeconds(2, 2, 1));
                        break;
                    case PingTimeEnum.Temp1: ftpr.Next(GetMilliSeconds(18, 0, 0), GetMilliSeconds(20, 0, 0));//come short
                        break;
                    case PingTimeEnum.Long: ftprInt = ftpr.Next(GetMilliSeconds(2, 1, 5), GetMilliSeconds(5, 2, 6));
                        break;
                }
                /*DEBUG TEST*/
                //int ftprInt = ftpr.Next(60000, 60005);//1 minuto ca;
                firstPingTimer.Interval = ftprInt;
                 firstPingTimer.Start(); 
            }

            //---------------------------------------------------------------------------
            private int GetMilliSeconds(int minuti, int ore, int giorni)
			{
				return
					(minuti * 60000) + (ore * 3600000) + (giorni * 86400000);
			}

			//---------------------------------------------------------------------------
			internal void StoreMLUChoice(bool userChoseMluInChargeToMicroarea)
			{
				Type t = GetParametersMngClass("ProtocolManager");
				if (t == null)
					return;


				Parameters p = GetParameters();
				p.UserChoseMluInChargeToMicroarea = userChoseMluInChargeToMicroarea;
				try
				{
					object[] args = new object[] { p, this.SysDBConnection, ActivationManager.GetMasterProductID() };
					t.InvokeMember(
						"StoreMLUChoice",
						BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod,
						null,
						null,
						args);
				}
				catch
				{ }
			}


			//---------------------------------------------------------------------------
			internal string GetSMSCode()
			{
				if (pingViaSMSCode == 0)
				{
					List<string> l = new List<string> { Environment.MachineName};
					if (l == null || l.Count == 0)
						return "-1";
                    cm = new MluExpiryDateCodeManager();
					pingViaSMSCode = cm.GenerateCode();
				}
                return String.Format("{0} {1}", pingViaSMSCode, GetCryptedReleaseDate(pingViaSMSCode));
			}

            //dalla 3.5 dobbiamo inviare nel ping via sms anche la release, così da saper i cambi direleasse che necessitano di invio eula, 
            //per modifiche, e cmq anche per controllare  chi pinga col sms.
            //Per rendere meno difficoltosa la digitazione trasfomiamo la data in un codice di tre cifre alfanumeriche
            //---------------------------------------------------------------------------
            internal string GetCryptedReleaseDate(int code)
            {
                 try
                 {
                     //il codice è sempre 5 cifre, divido per 10000 per ottenere il primo numero e così via
                     // uso questi valori per differenziare il codice risultante che altrimenti sarebbe per tutti uguale
                     int q11 =code/10000;//1 numero
                     int q1 = q11 % 2;//differenzia se il primo numero è dispari
                     int q2 =(code/1000) - (q11*10);//2 numero
                     int q3 = (code / 100)-((q11 * 100)+(q2 * 10));//3 numero

                     //dalla data della build recupero giorno mese e anno, li trasformo in maniera tale che siano composti da una cifra ciascuno.
                     //i primi 21 sono tutti lettere in modo che nel codice il valore in mezzo sia sempre una lettera, questo per rendere più interperetabile il codice in caso lo scrivessero senza spazi (vedi regular expression del webService che interepreta questo codice.
                     //eliminati per evitare confusioni 1 0 I O
                     List<string> table = new List<string> { "Y",  "E", "H", "C", "F", "A", "D", "W", "B", "U", "L", "X", "N", "Q", "P", "R", "Z", "T", "K", "V", "G", "M", "S","3", "5", "7", "J", "9", "8", "6", "4", "2"};
                    
                     DateTime versionDate = BasePathFinder.BasePathFinderInstance.InstallationVer.BDate;
                     
                     string dayVal = table[versionDate.Day-q1];//se dispari sottraggo 1
                     string monthVal= table[versionDate.Month + q2];//mese + seconda cifra (quindi 12 + un numero da 0 a 9 = uyn numero da 12-> 21)
                     int yearIndex = versionDate.Year >= 2011 ? versionDate.Year - 2010 : 0;//anni inferiori a 2011 verranno tutti 2010//perchè 2011 vale 1 e poi gli sommo la terza cifra del codice (questo algoritmo, considerando che l'array è di 31 elementi, - 9 numero max del codice  = 23 anni di durata max)
                     string yearVal = table[yearIndex+q3];
                     return String.Concat(yearVal, monthVal, dayVal); //concateno in formato anno mese giorno, con mese in mezzo per sapere con certezza che è una lettera
                 }
                 catch { return String.Empty; }

            }

            //metodo per decriptare il codice che rappresenta la data di release, che è da inviare nel sms
            //---------------------------------------------------------------------------
            private DateTime GetReleaseDate(string cryptedValue, int code)
            {
                //valore nullo, vuoto o diverso da 3
                if(String.IsNullOrWhiteSpace(cryptedValue ) || cryptedValue.Length!= 3)
                    return DateTime.MaxValue;
                //uppo x ricerca case insensitive
                cryptedValue = cryptedValue.ToUpperInvariant();

                try
                {
                     //il codice è sempre 5 cifre, divido per 10000 per ottenere il primo numero e così via
                    int q11 = code / 10000;//1 numero
                    int q1 = q11 % 2;//differenzia se il primo numero è dispari
                    int q2 = (code / 1000) - (q11 * 10);//2 numero
                    int q3 = (code / 100) - ((q11 * 100) + (q2 * 10));//3 numero
                    //dalla data della build recupero giorno mese e anno, li trasformo in maniera tale che siano composti da una cifra ciascuno.
                    //i primi 21 sono tutti lettere in modo che nel codice il valore in mezzo sia sempre una lettera, questo per rendere più interperetabile 
                    //il codice in caso lo scrivessero senza spazi (vedi regular expression del webService che interepreta questo codice.
                    //eliminati per evitare confusioni 1 0 I O
                    List<string> table = new List<string> { "Y", "E", "H", "C", "F", "A", "D", "W", "B", "U", "L", "X", "N", "Q", "P", "R", "Z", "T", "K", "V", "G", "M", "S", "3", "5", "7", "J", "9", "8", "6", "4", "2" };

                    //attendo formato anno mese giorno, con mese in mezzo per sapere con certezza che è una lettera
                    
                    int day = table.IndexOf(cryptedValue[2].ToString());
                   int month = table.IndexOf(cryptedValue[1].ToString());
                   int year = table.IndexOf(cryptedValue[0].ToString()) + 2010;
                   if(day == -1 || month == -1 || year < 2010)
                        return DateTime.MaxValue;
                    //Algoritmo al contrario:
                    //al giorno se dispari aggiungo 1
                    //al mese sottraggo la seconda cifra
                    //all'anno, una volta aggiunto 2010 tolgo la terza cifra
                   return new DateTime(year-q3, month-q2, day+q1);
                }
                catch { return DateTime.MaxValue; }

            }

            //---------------------------------------------------------------------
            public void SaveNewProductHash(string val = null)
            {
                XmlDocument doc = activationManager.User.GetXmlDom();
                if (doc == null) return;
                XmlNode root = doc.SelectSingleNode("UserInfo");
                if (root == null) return;
                XmlNode n = root.SelectSingleNode(WceStrings.Element.ProductHashCode);
                if (n == null)
                {
                    n = doc.CreateNode(XmlNodeType.Element, WceStrings.Element.ProductHashCode, "");
                    root.AppendChild(n);
                }
                n.InnerText = (val == null) ? MluExpiryDateCodeManager.CryptMluExpiryDate(MluExpiredDate, mluCancelled): val;

                SaveUserInfo(doc.OuterXml);
            }

            // la validazione con sms deve essere sempre possibile (23/07/2010) 
			//---------------------------------------------------------------------------
			internal bool ValidateSMS(string code)
			{
				//TODO
				//se lo stato è non attivato non dovrebbe tornare true, ma errore...
				//se ricevuto un codice sms verifico he sia corretto devo abbounarglielo come ping eseguito
				if (cm == null) return false;
				bool smsOk = cm.CheckCode(code);
                //smsOk = true;
				///// eseguo il codice che farei se fosse un ping andato a buon fine
				if (smsOk)
                {
                    SaveNewProductHash(cm.CryptedMluExpiryDate);
                    
                    //aggiorno anche il valore in memoria che è quello che utilizzo.
                    mluExpiredDate = activationManager.User.GetMluDate(out mluCancelled);
					//successo, reimposto i valori per i prossimi ping
                    actstate = ActivationState.Activated;
					waiting = false;
					failedPingViaSMS = 0;
					failedPing = 0;
                    PingTime = PingTimeEnum.Long;
				}
				else
				{
					//Se il ping fallisce e sono ancora sotto i 4 tentativi 
					//allora aumenti il numero dei tentativi e mi metto in stato di wait.
					//se sono alla fine dei tentativi  mi disattivo 
					// in tutti i casi pompo il relativo messaggio che ha tempo di scadenza come il tempo del timer,
					// in modo da non mostrare i messaggi vecchi

					if (failedPingViaSMS < maxpingFailedViaSMS)//RETRY ping the next tick
					{
						waiting = true;
						failedPingViaSMS++;
                        PingTime = PingTimeEnum.Short;
					}
					else
					{
						actstate = ActivationState.NoActivated;
						//disconnetto tutti gli utenti forzosamente così piano piano si accorgono che qualcosa non funziona
						if (authenticationSlots != null)
							authenticationSlots.Init(activationManager, SysDBConnection, diagnostic);

						diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "Max code errors reached.");
					}
				}
                //comunque col ping via sms imposto la data di scadenza mlu a mindate, poi chi la usa vedrà come comportarsi
                contractData.PaymentRenewalDate = DateTimeFunctions.MinValue;
                contractData.ContractData = ContractData.None;

				return smsOk;
			}

            //---------------------------------------------------------------------------
            private List<string> GetMacAddresses()
            {
                try
                {
                    StringCollection sc = LocalMachine.GetMacAddresses();
                    if (sc == null || sc.Count == 0)
                        return null;
                    List<string> l = new List<string>();
                    foreach (string s in sc)
                        l.Add(s);
                    return l;
                }
                catch
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "Error 100 generating code.");
                    return null;
                }
            }

            /// <summary>
            /// Invia i dati dell'installazione corrente al server microarea.
            /// Se la chiamata ha avuto successo azzera i counter e mette in attivato
            /// Se il server risponde false si va in stato non attivato
            /// Se la chiamata non trova il server (internet giu) non fa niente
            /// I parametri sono criptati
            /// </summary>
            //---------------------------------------------------------------------------
            internal string PingInternal(int mode)
			{
                if (mode == 666 || mode == 1717) InitActivation();

				Parameters result = Pong(mode);

                contractData = result.ContractData;
              
				//Parameters result = PongNoReflection(mode);
				ReturnValuesReader reader = new ReturnValuesReader(result);
                if (mode == 1717)
                {
                    if (reader.IsAMessage)
                    {
                        RefreshStatus = true;
                        ImportManager.SaveWCE(result.Wce, BasePathFinder.BasePathFinderInstance.GetLogManAppDataPath(), diagnostic);
                        InstallationVer.UpdateCachedDateAndSave();
                    }
                    else
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, reader.ToString());

                    return reader.ToString();
                }

				if (reader.IsAMessage)
                {
                    RefreshStatus = true;
					//Se avevo mollato dei messaggi allora mollo quello di riuscito ping :
					if (waiting && failedPing > 0)
						GetAdvertisementForPing(GetPingSuccessMessage());
					//successo, reimposto i valori per i prossimi ping
					waiting = false;
					failedPing = 0;
					failedPingViaSMS = 0;
                    PingTime = PingTimeEnum.Long;

                    ImportManager.SaveWCE(result.Wce, BasePathFinder.BasePathFinderInstance.GetLogManAppDataPath(), diagnostic);
                    InstallationVer.UpdateCachedDateAndSave();
					if (mode == 666)
					{
						activationManager.ActivationKey = reader.GetKey();
						actstate = activationManager.GetState();
                        mluExpiredDate = activationManager.User.GetMluDate(out mluCancelled);
                        try
                        {
                            InstallationData.ServerConnectionInfo.MasterSolutionName = GetMasterSolution();
                            InstallationData.ServerConnectionInfo.UnParse(BasePathFinder.BasePathFinderInstance.ServerConnectionFile);
                        }
                        catch { diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "Error writing master product name  on serverconnectioninfo.config"); }
                    }
                }
                else if (reader.IsARealTimeRequest || (reader.Errors != null && reader.Errors.Length > 0))//block
                {
                    //Se il ping fallisce e sono ancora sotto i 4 tentativi 
                    //allora aumenti il numero dei tentativi e mi metto in stato di wait.
                    //se sono alla fine dei tentativi  mi disattivo 
                    // in tutti i casi pompo il relativo messaggio che ha tempo di scadenza come il tempo del timer,
                    // in modo da non mostrare i messaggi vecchi

					if (!reader.IsAForcedTimeRequest && (failedPing < maxpingFailed && mode == 69))//RETRY ping the next tick
					{
						failedPing++;
						//se il ping fallisce mi metto in stato di wait
						waiting = true;
						//imposto il timer in stato di retry (intervallo breve)
                        if (PingTime == PingTimeEnum.Long && failedPing == 1) //se è il primo tentativo provo subito, magari e`stato solo un disguido
                            PingTime = PingTimeEnum.Temp1;
                        else if (PingTime== PingTimeEnum.Long || PingTime== PingTimeEnum.Temp1) //se no imposto l'intervallo un po' più lungo per eliminare il rischio di quelli che cambiano le impostazioni del firewall e poi se ne vanno a fare il weekend in riviera.    
                            PingTime = PingTimeEnum.Temp;

						// messaggi
						GetAdvertisementForPing(GetPingFailedMessage(failedPing));
					}
					else
					{
						actstate = ActivationState.NoActivated;
						//disconnetto tutti gli utenti forzosamente così piano piano si accorgono che qualcosa non funziona
						if (authenticationSlots != null)
							authenticationSlots.Init(activationManager, SysDBConnection, diagnostic);
                        RefreshStatus = false;
                        firstPingTimer.Stop();//blocco il timer se no continua ad inviare richieste inutilmente
						diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, reader.ToString());
                       
					}
				}
				return reader.ToString();
			}


            //-------------------------------------------------------------------------------------
            /// <summary>
            /// in caso di ping fallito, o di ping riuscito dopo ping falliti, invio il testo che invio anche via balloon al destinatario di posta SMTP eventualemnte indicato nella console
            /// in questo modo se percaso non ci sono utenti connessi viene inviata la mail che magari qualcuno legge.
            /// </summary>
            /// <param name="msg"></param>
            private void SendPingMail(string msg)
            {
                IServerConnectionInfo serverConnectionInfo = InstallationData.ServerConnectionInfo;
                if (serverConnectionInfo == null ||
                    !serverConnectionInfo.SendPingMail ||
                    String.IsNullOrWhiteSpace(msg)
                    ) 
                    return;


                string recipient = serverConnectionInfo.PingMailRecipient;
                if (String.IsNullOrWhiteSpace(recipient))
                    return;

                //tolgo la sezione dello style perchè non viene interpretata correttamente nelle mail
                int start = msg.IndexOf("<style>");
                int end = msg.IndexOf("</style>");
                if(end>start)
                    msg = msg.Remove(start, (end - start - 1));

                string messageSubject = InstallationData.InstallationName + " " + LoginManagerStrings.ValidationStatus;
                string messageContent = msg;
                string aSMTPRelayServerName = serverConnectionInfo.SMTPRelayServerName;
                string userName = serverConnectionInfo.SMTPUserName;
                string password = serverConnectionInfo.SMTPPassword;
                string domain = serverConnectionInfo.SMTPDomain;
                string fromAddress = serverConnectionInfo.SMTPFromAddress; 
                bool useDefaultCredentials = serverConnectionInfo.SMTPUseDefaultCredentials;
                bool useSSL = serverConnectionInfo.SMTPUseSSL;
                int port = serverConnectionInfo.SMTPPort; // the port number on the SMTP host. The default value is 25

                MailMessage mailTask = new MailMessage(fromAddress, recipient);
                mailTask.Subject = messageSubject;
                mailTask.IsBodyHtml = true;
                mailTask.BodyEncoding = Encoding.UTF8;

                if (messageContent != null && messageContent.Length > 0)
                {
                    messageContent = messageContent.Replace("\r\n", "<br>");
                    messageContent = messageContent.Replace("\n", "<br>");
                    mailTask.Body = "<html><body><p>" + messageContent + "</p></body></html>";
                }
                else
                    mailTask.Body = String.Empty;

                mailTask.Priority = MailPriority.Normal;

                string smtpMailSmtpServer = "localhost";

                if (aSMTPRelayServerName != null && aSMTPRelayServerName.Trim().Length > 0)
                    smtpMailSmtpServer = aSMTPRelayServerName.Trim();

                try
                {
                    SmtpClient aSmtpClient = new SmtpClient(smtpMailSmtpServer);
                    aSmtpClient.UseDefaultCredentials = useDefaultCredentials;
                    aSmtpClient.EnableSsl = useSSL;
                    if (!useDefaultCredentials)
                    {
                        aSmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        //se il server è GMail, domain deve essere vuoto
                        //probabilmente domain va usato solo se il server di posta è in dominio...
                        NetworkCredential netCredential =
                            new NetworkCredential
                            (
                            userName,
                            password,
                            domain  // The domain parameter specifies the domain or realm to which the user name belongs. 
                            // Typically, this is the host computer name where the application runs or the user domain for the
                            // currently logged in user.
                            );
                        // Il metodo CredentialCache.Add(string host, int port, string authenticationType, NetworkCredential credential)
                        // aggiunge un'istanza NetworkCredential da utilizzare con SMTP alla cache delle credenziali e la associa ad un 
                        // computer host, a una porta e a un protocollo di autenticazione. Le credenziali aggiunte con questo metodo 
                        // sono valide solo per SMTP. Questo metodo non funziona per le richieste HTTP o FTP.
                        //
                        CredentialCache netCredentialCache = new CredentialCache();
                        netCredentialCache.Add(smtpMailSmtpServer, port, "NTLM", netCredential);
                        aSmtpClient.Credentials = netCredentialCache.GetCredential(smtpMailSmtpServer, port, "NTLM");
                        //aSmtpClient.Credentials = netCredential;
                    }
                    else
                    {
                        aSmtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                    }
                    aSmtpClient.Send(mailTask);

                }
                catch (Exception exc)
                {
                    Debug.Fail(exc.ToString());
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, String.Format(" - Error 4376 - Error sending SMTP e-mail: {0}", exc.ToString()));

                }
            }            

			//-------------------------------------------------------------------------------------
			private void GetAdvertisementForPing(string msg)
			{
				if (String.IsNullOrEmpty(msg))
					return;
				Advertisement advertisement = new Advertisement();
				advertisement.Body = new Advertisement.AdvertisementBody("", "", msg);
				advertisement.HideDisclaimer = true;
                advertisement.ExpireWithRestart = true;
				DateTime now = DateTime.Now;
				TimeSpan span = new TimeSpan(0, 0, 0, 0, (int)firstPingTimer.Interval);
				advertisement.ExpiryDate = now.Add(span);// come il timer per non accavallare tutti i messaggi
				advertisement.ID = Guid.NewGuid().ToString();
				advertisement.Severity = 1;
				advertisement.Type = MessageType.Contract;
				messagesQueue.PumpMessageToQueue(advertisement);
                SendPingMail(msg);
			}

            //-------------------------------------------------------------------------------------
            internal bool IsValidDate(string applicationDateString, out string maxDate)
            {
                maxDate = MluExpiredDate.ToString("s");//cultureinvariant
                DateTime applicationDate = DateTimeFunctions.MaxValue;
                if (!DateTime.TryParse(applicationDateString, out applicationDate))
                {
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, String.Format(" - Error 13313 - Error evaluating operation date: {0}", applicationDateString));
                    return false;
                }

                bool b = (applicationDate <= MluExpiredDate);
                if (!b)
                    diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, String.Format(" - Error 13356 - Operation date {0} not allowed, max date allowed is {1}.", applicationDate.ToShortDateString(), mluExpiredDate.ToShortDateString()));

                return b;
            }

            ///<summary>
            ///Per inviare messaggi (di tipo update) dentro il balloon, lunga scadenza, senza informativa su come disabilitarlo, possibilità di decidere se solo per utente corrente
            /// </summary>
            //-------------------------------------------------------------------------------------
            internal void SendBalloon(string authenticationToken, string bodyMessage, MessageType messageType = MessageType.Updates, List<string> recipients = null)
            {
                if (String.IsNullOrWhiteSpace(bodyMessage) ||
                    String.IsNullOrWhiteSpace(authenticationToken) ||
                    (
                    !IsValidToken(authenticationToken) &&
                    !IsValidTokenForConsole(authenticationToken) &&
                    authenticationToken != tbSenderAuthenticationToken)
                    )
                    return;

                Advertisement advertisement = new Advertisement();
                advertisement.Body = new Advertisement.AdvertisementBody("", "", bodyMessage);
                advertisement.ExpiryDate = DateTime.Now.AddYears(1);//scadenza lunga, poco importante per messaggi di mago
                advertisement.ID = Guid.NewGuid().ToString();
                if (recipients != null && recipients.Count > 0)
                    advertisement.Recipients.AddRange(recipients);

                advertisement.Type = messageType;//per ora mago è in grado di mandare solo messagggi di update all'interno di se
                // E' stato Luca
                //if (onlyMe != 0)
                //{
                //    advertisement.Recipients.Clear();
                //    advertisement.Recipients.Add(s.LoginName);//messaggio inviato solo all'utente loginato che ha invocato il metodo
                //}
                messagesQueue.PumpMessageToQueue(advertisement);
            }

            //-------------------------------------------------------------------------------------
            internal void AdvancedSendBalloon(
                string authenticationToken,
                string bodyMessage,
                DateTime expiryDate,
                MessageType messageType = MessageType.Updates,
                string[] recipients = null,
                MessageSensation sensation = MessageSensation.Information,
                bool historicize = true,
                bool immediate = false,
                int timer = 0, string tag = null)
            {
                if (String.IsNullOrWhiteSpace(bodyMessage) ||
                    String.IsNullOrWhiteSpace(authenticationToken) ||
                    (
                    !IsValidToken(authenticationToken) &&
                    !IsValidTokenForConsole(authenticationToken) &&
                    authenticationToken != tbSenderAuthenticationToken)
                    )
                    return;

                Advertisement advertisement = new Advertisement();
                advertisement.Body = new Advertisement.AdvertisementBody("", "", bodyMessage);
                advertisement.AutoClosingTime = timer;
                advertisement.Sensation = sensation;
                advertisement.Immediate = immediate;
                advertisement.Historicize = historicize;
                advertisement.ExpiryDate = expiryDate;
                advertisement.Type = messageType;
                advertisement.ID = Guid.NewGuid().ToString();
                advertisement.Tag = tag;
                advertisement.Recipients.Clear();
                if (recipients != null && recipients.Length > 0)
                {

                    foreach (string user in recipients)
                        advertisement.Recipients.Add(user);
                }

                messagesQueue.PumpMessageToQueue(advertisement);
            }

			//-------------------------------------------------------------------------------------
			private string GetBalloonTemplate()
			{
				string text = string.Empty;
				try
				{
					Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.WebServices.LoginManager.BalloonTemplate.htm");
					StreamReader sr = new StreamReader(stream);
					text = sr.ReadToEnd();
				}
				catch (Exception exc)
				{
					Debug.WriteLine(exc.Message);
					return null;
				}
				return text;
			}

			//-------------------------------------------------------------------------------------
			private string GetPingSuccessMessage()
			{
				SetCulture();

				string text = GetBalloonTemplate();
				if (string.IsNullOrEmpty(text))
					return null;
				string brandedProduct = GetMasterProductBrandedName();

                text = text.Replace("{@IMAGE}", @"data:image/png;base64," + ImageToBase64(BasePathFinder.BasePathFinderInstance.LoginManagerPath + @"\img\icon_riattivazione.png"));
				text = text.Replace("{@URL}",""/* BasePathFinder.BasePathFinderInstance.PingViaSMSPage*/);
				text = text.Replace("{@COLOR}", "verde");//non tradurre nome dello style
				text = text.Replace("{@string1}", HttpUtility.HtmlEncode(String.Format(LoginManagerStrings.ValidationOK, brandedProduct)));
				text = text.Replace("{@string2}", "");
				text = text.Replace("{@string3}", HttpUtility.HtmlEncode(String.Format(LoginManagerStrings.ValidationOKInfo, brandedProduct)));
				text = text.Replace("{@string4}", "");
				text = text.Replace("{@string5}", "");
				text = text.Replace("{@string6}", "");
				text = text.Replace("{@string7}", "");
				return text;
			}

			//-------------------------------------------------------------------------------------
			private string GetPingFailedMessage(int pingfailed)
			{
				SetCulture();

				string text = GetBalloonTemplate();
				if (string.IsNullOrEmpty(text))
					return null;

				string brandedProduct = GetMasterProductBrandedName();

				text = text.Replace("{@URL}", BasePathFinder.BasePathFinderInstance.PingViaSMSPage);
				text = text.Replace("{@COLOR}", "rosso");//non tradurre nome dello style
				text = text.Replace("{@string3}", HttpUtility.HtmlEncode(String.Format(LoginManagerStrings.PingFailed3, brandedProduct)));
				text = text.Replace("{@string4}", HttpUtility.HtmlEncode(LoginManagerStrings.PingFailed4));
				text = text.Replace("{@string5}", HttpUtility.HtmlEncode(LoginManagerStrings.PingFailed5));
				text = text.Replace("{@string7}", HttpUtility.HtmlEncode(LoginManagerStrings.PingFailed7));

				switch (pingfailed)
				{ 
					case 1:
                        text = text.Replace("{@IMAGE}", @"data:image/png;base64," + ImageToBase64(BasePathFinder.BasePathFinderInstance.LoginManagerPath + @"\img\icon_warning1.png"));
						text = text.Replace("{@string1}", HttpUtility.HtmlEncode(String.Format(LoginManagerStrings.PingFailed1First, brandedProduct)));
						text = text.Replace("{@string2}", HttpUtility.HtmlEncode(String.Format(LoginManagerStrings.PingFailed2, brandedProduct)));
						text = text.Replace("{@string6}", HttpUtility.HtmlEncode(LoginManagerStrings.PingFailed6));
						break;
					case 2:
						text = text.Replace("{@IMAGE}", @"data:image/png;base64,"+ImageToBase64(BasePathFinder.BasePathFinderInstance.LoginManagerPath + @"\img\icon_warning2.png"));
						text = text.Replace("{@string1}", HttpUtility.HtmlEncode(String.Format(LoginManagerStrings.PingFailed1Second, brandedProduct)));
						text = text.Replace("{@string2}", HttpUtility.HtmlEncode(String.Format(LoginManagerStrings.PingFailed2, brandedProduct)));
						text = text.Replace("{@string6}",HttpUtility.HtmlEncode( LoginManagerStrings.PingFailed6));
						break;

					case 3:
						text = text.Replace("{@IMAGE}", @"data:image/png;base64,"+ImageToBase64(BasePathFinder.BasePathFinderInstance.LoginManagerPath + @"\img\icon_warning3.png"));
						text = text.Replace("{@string1}", HttpUtility.HtmlEncode(String.Format(LoginManagerStrings.PingFailed1Last, brandedProduct)));
						text = text.Replace("{@string2}", HttpUtility.HtmlEncode(String.Format(LoginManagerStrings.PingFailed8, brandedProduct)));
						text = text.Replace("{@string6}", HttpUtility.HtmlEncode((LoginManagerStrings.PingFailed6 + LoginManagerStrings.PingFailed9)));
						break;

					default:
						return null;
					
				}
				return text;
            }

            ////---------------------------------------------------------------------------
            public string ImageToBase64(string pngpath)
            {
                ImageFormat format = ImageFormat.Png;

                Image image = Image.FromFile(pngpath);
                using (MemoryStream ms = new MemoryStream())
                {
                    // Convert Image to byte[]
                    image.Save(ms, format);
                    byte[] imageBytes = ms.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }

            ////---------------------------------------------------------------------------
			//internal string PingInternal(int mode)
			//{
			//    Parameters result = Pong(mode);
			//    //Parameters result = PongNoReflection(mode);
			//    //ImportManager.SaveWCE(result.Wce, pathFinder.GetCustomActivationPath());//non richiesto più, era necessario per disattivazione da remoto
			//    string message = string.Empty;
			//    ReturnValuesReader reader = new ReturnValuesReader(result);

			//    //verifico il valore di ritorno, se è buono resetto i counter
			//    if (reader.IsAMessage)
			//    {
			//        if (mode == 666)
			//        {

			//            counterManager.CurrentCounter.Demo = false;
			//            activationManager.IsDemo = false;

			//            counterManager.CurrentCounter.ActivationKey = reader.GetKey();
			//            if (counterManager.CheckActivationKey())
			//            {
			//                counterManager.CurrentCounter.Reset();
			//                CounterManager.ActivationPeriod = result.ActivationPeriod;
			//                CounterManager.WarningPeriod = result.WarningPeriod;
			//                counterManager.CurrentCounter.ContractData = result.ContractData;
			//            }
			//            else
			//            {
			//                counterManager.CurrentCounter.SetMaxValue();
			//                reader.AddError(ErrorMessage.ActivationKey_Not_Valid, string.Empty);
			//                diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, ErrorMessage.ActivationKey_Not_Valid);
			//            }
			//        }
			//        else
			//        {
			//            counterManager.CurrentCounter.Reset();
			//            CounterManager.ActivationPeriod = result.ActivationPeriod;
			//            CounterManager.WarningPeriod = result.WarningPeriod;
			//            counterManager.CurrentCounter.ContractData = result.ContractData;
			//        }
			//    }
			//    else
			//    {
			//        if (!counterManager.CurrentCounter.Demo && reader.IsARealTimeRequest)
			//        {
			//            counterManager.CurrentCounter.SetMaxValue();
			//            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, reader.ToString());
			//        }
			//    }

			//    counterManager.WriteAllCounters();

			//    //imposta lo stato in base ai contatori appena modificati
			//    counterManager.SetActivationState();

			//    return reader.ToString();
			//}

			//#region ping asincrona
			///// <summary>
			///// Se viene trovata una req senza guid (o il contrario) si eliminano
			///// </summary>
			///// <returns>Lo stato della comunicazione asincrona</returns>
			////----------------------------------------------------------------------
			//internal RequestStatus GetRequestStatusInternal()
			//{
			//    Type t = GetParametersMngClass("RequestGUIDMng");
			//    if (t == null)
			//        return RequestStatus.NoReqNoRes;

			//    try
			//    {
			//        object[] args = new object[]{pathFinder, SysDBConnection};
			//        return (RequestStatus) t.InvokeMember(
			//            "GetRequestStatus", 
			//            BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, 
			//            null, 
			//            null, 
			//            args);
			//    }
			//    catch
			//    {
			//        return RequestStatus.NoReqNoRes;
			//    }
			//}

			////----------------------------------------------------------------------
			//internal bool DeleteRequestInternal()
			//{
			//    Type t = GetParametersMngClass("RequestGUIDMng");
			//    if (t == null)
			//        return false;

			//    try
			//    {
			//        object[] args = new object[]{pathFinder, SysDBConnection};
			//        return (bool) t.InvokeMember(
			//            "DeleteRequest", 
			//            BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, 
			//            null, 
			//            null, 
			//            args);
			//    }
			//    catch
			//    {
			//        return false;
			//    }
			//}

			////----------------------------------------------------------------------
			//internal bool ResetAsyncStateInternal()
			//{
			//    Type t = GetParametersMngClass("RequestGUIDMng");
			//    if (t == null)
			//        return false;

			//    bool ok;
			//    try
			//    {
			//        object[] args = new object[]{pathFinder, SysDBConnection};
			//        ok = (bool) t.InvokeMember(
			//            "DeleteRequest", 
			//            BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, 
			//            null, 
			//            null, 
			//            args);
			//    }
			//    catch
			//    {
			//        return false;
			//    }
			//    try
			//    {
			//        object[] args = new object[]{pathFinder};
			//        ok = ok && (bool) t.InvokeMember(
			//            "DeleteResponse", 
			//            BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, 
			//            null, 
			//            null, 
			//            args);
			//    }
			//    catch
			//    {
			//        return false;
			//    }
			//    return ok;
			//}

			//#endregion

			//-------------------------------------------------------------------------------------
			private Type GetParametersMngClass(string ns)
			{
				string paramsDll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NameSolverStrings.Bin);
				paramsDll = Path.Combine(paramsDll, "Microarea.TaskBuilderNet.ParametersManager.dll");

				if (!File.Exists(paramsDll))
				{
					diagnostic.Set(DiagnosticType.Error, "Error : " + ErrorMessage.ParamsDllNotFound);
					return null;
				}

				byte[] iv = new byte[] { 122, 166, 235, 135, 94, 83, 106, 204 };
				byte[] key = new byte[] { 213, 202, 90, 143, 129, 231, 16, 247, 116, 250, 185, 160, 251, 161, 85, 77, 23, 26, 233, 104, 61, 144, 66, 32 };

				TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
				byte[] fc;
				try
				{
					FileStream fs = File.OpenRead(paramsDll);
					fc = new byte[fs.Length];
					fs.Read(fc, 0, (int)fs.Length);
					fs.Close();
				}
				catch
				{
					diagnostic.Set(DiagnosticType.Error, "Error : " + ErrorMessage.PongNotReadDll);
					return null;
				}

				byte[] b;
				try
				{
                    b = Microarea.TaskBuilderNet.Core.Generic.Crypto.DecryptToByteArray(fc, "TripleDES", key, iv);
                }
				catch
				{
					diagnostic.Set(DiagnosticType.Error, "Error : " + ErrorMessage.PongNotDecriptDll);
					return null;
				}

				try
				{
					Assembly a = Assembly.Load(b);
					return a.GetType("Microarea.TaskBuilderNet.ParametersManager." + ns);
				}
				catch
				{
					diagnostic.Set(DiagnosticType.Error, "Error : " + ErrorMessage.PongNotDecriptDll);
					return null;
				}
			}

			/// <summary>
			/// Decrypta la dll di protocollo per il server microarea
			/// Chiama il server microarea passando i dati della corrente installazione
			/// </summary>
			/// <returns>la chiave di attivazione o un codice di errore</returns>
			//-------------------------------------------------------------------------------------
			private Parameters Pong(int mode)
			{
				Type t = GetParametersMngClass("ProtocolManager");
				if (t == null)
					return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.PongNotReadDll));

				Parameters p = GetParameters(mode);

				try
				{
					object[] args = new object[] { p, this.SysDBConnection , ActivationManager.GetMasterProductID()};
					return (t.InvokeMember(
						"Ping",
						BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod,
						null,
						null,
						args)) as Parameters;
				}
				catch (Exception exc)
				{
					if (exc.InnerException != null)
					{
						Type type = exc.InnerException.GetType();
						if (type == typeof(ArgumentNullException))
						{
							diagnostic.Set(DiagnosticType.Error, "Error : " + ErrorMessage.ArgumentsNull + " " + exc.InnerException.Message);
							return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ArgumentsNull, exc.InnerException.Message));
						}

						else
						{
							diagnostic.Set(DiagnosticType.Error, "Error : " + ErrorMessage.ParamManagerException + " " + exc.InnerException.Message);
							return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.ArgumentsNull, exc.InnerException.Message));
						}

					}
					diagnostic.Set(DiagnosticType.Error, "Error : " + ErrorMessage.PongReflectionError + " " + exc.Message);
					return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.PongReflectionError));
				}
			}

			////-------------------------------------------------------------------------------------
			//private Parameters PongNoReflection(int mode)
			//{
			//    Type t = GetParametersMngClass("ProtocolManager");
			//    if (t == null)
			//        return Parameters.GetParametersForError(ReturnValuesWriter.GetErrorString(ErrorMessage.PongNotReadDll));


			//    string userid = string.Empty;
			//    if (activationManager.User != null &&
			//        activationManager.User.UserIdInfos != null &&
			//        activationManager.User.UserIdInfos.Length > 0 &&
			//        activationManager.User.UserIdInfos[0] != null &&
			//        activationManager.User.UserIdInfos[0].Value != null &&
			//        activationManager.User.UserIdInfos[0].Value.Trim().Length > 0
			//        )
			//        userid = activationManager.User.UserIdInfos[0].Value.Trim();

			//    string traceRoute = string.Empty;

			//    Parameters p = GetParameters();
			//    try
			//    {

			//        Parameters pp = Microarea.TaskBuilderNet.ParametersManager.ProtocolManager.Ping(p, this.SysDBConnection);
			//        return pp;
			//    }
			//    catch (Exception e)
			//    {
			//        diagnostic.Set(DiagnosticType.Error, "Error : " + ErrorMessage.PongReflectionError + " " + e.Message);
			//        return null;// ReturnValuesWriter.GetErrorString(ErrorMessage.PongReflectionError);
			//    }
			//}

			/// <summary>
			/// ritorna lo stato corrente di attivazione
			/// </summary>
			/// <param name="daysToExpiration">numero di giorni alla scadenza</param>
			/// <returns></returns>
			//---------------------------------------------------------------------------
			internal ActivationState GetActivationStateInternal(out int daysToExpiration)
			{
				daysToExpiration = 0;
				return actstate;
			}

			//---------------------------------------------------------------------------
			internal bool IsVirginActivation()
			{
				return (ActivationManager == null) ? true : ActivationManager.IsVirgin();
			}

			#endregion

			#region MessagesManagement

			//----------------------------------------------------------------------
            internal ArrayList GetOldMessages(string authenticationToken)
			{
				if (
					authenticationToken == null ||
					authenticationToken.Trim().Length == 0 ||
					!IsValidToken(authenticationToken)
					)
					return new ArrayList();

				//già il metodo isvalid verifica che slot sia diverso da null
				string user = authenticationSlots.GetSlot(authenticationToken).LoginName;
				return messagesQueue.GetOldMessages(user);
			}

			//----------------------------------------------------------------------
            internal IList GetMessagesQueue(string authenticationToken, bool immediate = false)
            {
               
                if (
                    authenticationToken == null ||
                    authenticationToken.Trim().Length == 0 ||
                    !IsValidToken(authenticationToken)
                    )
                    return new ArrayList();

                //già il metodo isvalid verifica che slot sia diverso da null
                string user = authenticationSlots.GetSlot(authenticationToken).LoginName;
                ArrayList list = messagesQueue.ConsumeMessageFromQueue(user);
                //Filtro per tipologia messaggio secondo quanto indicato nelle properties dell'utente.
                //LoadTestMessages(list); 
                //MessageType mt = GetBalloonMessagesBlockedStatus(authenticationSlots.GetSlot(authenticationToken).LoginID);
                IList newl = new ArrayList();

                foreach (Advertisement o in list)
                    //  if ((mt & o.Type) != o.Type)//per ogni messagggio verifico che non sia bloccato dall'utente.
                    if (o.Recipients == null || o.Recipients.Count == 0 || o.Recipients.ContainsNoCase(user))//per ogni messagggio verifico che il messaggio sia per tali destinatari, vuoto = per tutti
                    {
                        if ((immediate && o.Immediate) || !immediate)//se devo restituire solo gli immediati, verifico he il messaggio sia immediato altrimenti tutti
                            newl.Add(o);
                    }

                return newl;
            }

            //----------------------------------------------------------------------
            internal IList GetImmediateMessagesQueue(string authenticationToken)
            {
                return GetMessagesQueue(authenticationToken, true);
            }

            //----------------------------------------------------------------------
            internal void DeleteMessageFromQueue(string id)
            {
                messagesQueue.RemoveMessage(id);
            }

            //----------------------------------------------------------------------
            internal void PurgeMessageByTag(string tag, string user = null)
            {
                messagesQueue.PurgeMessageByTag(tag, user);
            }


           [Obsolete]//così non rischiamo di rilasciare il metodo di test
            //----------------------------------------------------------------------
            private void LoadTestMessages( IList list)
            {
                 //////test
                Advertisement a = new Advertisement("prova Advrtsm", "", "prova Advrtsm html", true, DateTime.MaxValue, MessageType.Advrtsm, 1, Guid.NewGuid().ToString());
                Advertisement b = new Advertisement("prova Contract", "", "prova Contract html", true, DateTime.MaxValue, MessageType.Contract, 1, Guid.NewGuid().ToString());
                Advertisement c = new Advertisement("prova Updates", "", "prova Updates html", true, DateTime.MaxValue, MessageType.Updates, 1, Guid.NewGuid().ToString());
                list.Add(a);
                list.Add(b);
                list.Add(c);
                Advertisement a2 = new Advertisement("prova Advrtsm2", "", "prova Advrtsm html2", true, DateTime.MaxValue, MessageType.Advrtsm, 2, Guid.NewGuid().ToString());
                Advertisement b2 = new Advertisement("prova Contract2", "", "prova Contract html2", true, DateTime.MaxValue, MessageType.Contract, 2, Guid.NewGuid().ToString());
                Advertisement c2 = new Advertisement("prova Updates2", "", "prova Updates html2", true, DateTime.MaxValue, MessageType.Updates, 2, Guid.NewGuid().ToString());
                list.Add(a2);
                list.Add(b2);
                list.Add(c2);
                //////
            }


            //non usato, è stato scritto per permettere da console di scegliere al singolo utente quali messaggi riceve, 
            //poi enrico ha voluto che la scelta si facesse da sito

            ////----------------------------------------------------------------------
            //internal void SetBalloonInfo(string authenticationToken, MessageType messageType, bool block)
            //{
            //    if (
            //         authenticationToken == null ||
            //         authenticationToken.Trim().Length == 0 ||
            //         !IsValidToken(authenticationToken)
            //         )
            //        return ;

            //    //già il metodo isvalid verifica che slot sia diverso da null
            //    int loginID = authenticationSlots.GetSlot(authenticationToken).LoginID;

            //    MessageType t = GetBalloonMessagesBlockedStatus(loginID);
            //    if (block)//se bloccato aggiungo il valore dell'enumerativo 
            //        t = t | messageType;
            //    else//se invece il check non c'è allora devo eliminare il valore dell'enumerativo bloccato, anche se è una situazione che non dovrebbe avvvenire.
            //        t = t & ~ messageType;
            //    AddMessageType(t, loginID);

            //}

            ////---------------------------------------------------------------------
            //private void AddMessageType(MessageType balloonBlockedType, int loginID)
            //{
            //  if (!OpenSysDBConnection())
            //        return;

            //  //aggiorno i dati di pwd
            //  string query = "UPDATE MSD_Logins SET BalloonBlockedType = @BalloonBlockedType " +
            //                  "WHERE LoginId = @LoginID";
            //    SqlCommand sqlCommand = new SqlCommand();

            //    try
            //    {
            //        sqlCommand.CommandText = query;
            //        sqlCommand.Connection = SysDBConnection;
            //        sqlCommand.Parameters.Add(new SqlParameter("@BalloonBlockedType", (int)balloonBlockedType));
            //        sqlCommand.Parameters.Add(new SqlParameter("@LoginID", loginID));
            //        sqlCommand.ExecuteNonQuery();

            //    }
            //    catch(Exception exc)
            //    {
            //        sqlCommand.Dispose();
            //        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "AddMessageType: " + exc.Message);
            //    }
            //}

            ////----------------------------------------------------------------------
            //private MessageType GetBalloonMessagesBlockedStatus(int loginID)
            //{
            //   if (!OpenSysDBConnection())
            //        return MessageType.None;

            //    int val = 0;
            //    string query = "SELECT BalloonBlockedType FROM MSD_Logins WHERE loginID = @loginID";

            //    SqlCommand aSqlCommand = new SqlCommand();
            //    SqlDataReader aSqlDataReader = null;

            //    try
            //    {
            //        aSqlCommand.CommandText = query;
            //        aSqlCommand.Connection = SysDBConnection;
            //        aSqlCommand.Parameters.AddWithValue("@loginID", loginID);
            //        aSqlDataReader = aSqlCommand.ExecuteReader();

            //        if (aSqlDataReader.Read())
            //            val = (int)aSqlDataReader["BalloonBlockedType"];

            //        return (MessageType)val; 
            //    }
            //    catch (Exception err)
            //    {
            //        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, "GetBalloonMessagesBlockedStatus: " + err.Message);
            //        return MessageType.None; 
            //    }
            //    finally
            //    {
            //        if (aSqlDataReader != null && !aSqlDataReader.IsClosed)
            //            aSqlDataReader.Close();
            //    }
			

            //}

			#endregion
		}
	}

}
