using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.DataSynchroUtilities;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.WebServices.DataSynchronizer.DataLayer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;

namespace Microarea.WebServices.DataSynchronizer
{
    /// <summary>
    /// DSProvider (contiene i dati di connessione al provider)
    /// </summary>
    //================================================================================
    public class DSProvider
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool Disabled { get; set; }
        public string Parameters { get; set; }
        public string IAFModules { get; set; }
        public bool SkipCrtValidation { get; set; }
    }

    public class LoginData
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Company { get; set; }
        public bool WindowsAuthentication { get; set; }

        public LoginData(string username, string password, bool windowsAuthentication, string company)
        {
            Username = username;
            Password = password;
            WindowsAuthentication = windowsAuthentication;
            Company = company;
        }
    }

    /// <summary>
    /// DataSynchronizerApplication (espone un metodo statico che ritorna un'istanza di DSEngine)
    /// </summary>
    //================================================================================
    public class DataSynchronizerApplication
    {
        private static DSEngine dsEngineInstance = null;
        private static Object _lock = "Lock";

        //---------------------------------------------------------------------
        public static DSEngine DSEngine
        {
            get
            {
                lock (_lock)
                {
                    if (dsEngineInstance == null)
                        dsEngineInstance = new DSEngine();
                    return dsEngineInstance;
                }
            }
        }

        //---------------------------------------------------------------------
        public static void ReleaseAllResources()
        {
            lock (_lock)
            {
                if (dsEngineInstance == null)
                    dsEngineInstance.Dispose();
            }
        }
    }

    //================================================================================
    public class DSEngine : IDisposable
    {
        private static EventLog eventLog = new EventLog("MA Server", ".", "DataSynchronizer");

        private static Dictionary<string, Timer> threadSyncInboundTimers = new Dictionary<string, Timer>(); // timer per la sincronizzazione degli ordini da Pat verso Mago
        private static int synchronizeInboundTickMinutes = 60; // di default 60 minuti

        private List<DataSynchroDatabaseInfo> globalDataSynchroDbInfoList = new List<DataSynchroDatabaseInfo>();

        public bool HasChangedThread { get; set; }

        public DSEngine()
        {
            HasChangedThread = false;
        }

        ///<summary>
        /// Imposto la culture al thread in modo da visualizzare i messaggi nella lingua
        /// scelta dall'utente (altrimenti viene usata quella del sistema operativo)
        ///</summary>
        //---------------------------------------------------------------------------
        internal void SetCulture()
        {
            try
            {
                if (!String.IsNullOrEmpty(InstallationData.ServerConnectionInfo.ApplicationLanguage))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(InstallationData.ServerConnectionInfo.ApplicationLanguage);

                if (!String.IsNullOrEmpty(InstallationData.ServerConnectionInfo.PreferredLanguage))
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(InstallationData.ServerConnectionInfo.PreferredLanguage);
            }
            catch
            {
            }
        }

        ///<summary>
        /// Richiamata dal web method Init
        /// Si occupa di istanziare un timer, che ogni tot ore richiama la funzione di sincronizzazione
        /// dei documenti da Pat a Mago
        ///</summary>
        //--------------------------------------------------------------------------------
        internal void InitTimer(string username, string password, bool windowsAuthentication, string company)
        {
            SetCulture();

            lock (typeof(DSEngine))
            {
                ReadAppSettings();
                
                // la funzione viene richiamata ogni tot ore (letti dal web.config)
                // (vedere se poi potrebbe andare a coincidere con altre sincronizzazioni piu' veloci)
                if (!threadSyncInboundTimers.ContainsKey(company))
                {
                    threadSyncInboundTimers[company] = new Timer
                    (
                        new TimerCallback(SynchronizeInboundCallBack),
                        new LoginData(username, password, windowsAuthentication, company),
                        new TimeSpan(hours: 0, minutes: 3, seconds: 0), // la prima volta parte dopo 3 minuti
                        new TimeSpan(hours: synchronizeInboundTickMinutes / 60, minutes: synchronizeInboundTickMinutes % 60, seconds: 0)
                    );

                    WriteMessageInEventLog(DSStrings.DSSynchroInit, EventLogEntryType.Information, true);
                }
            }
        }

        internal bool Reboot(string rebootToken)
        {
            lock (typeof(DSEngine))
            {
                if (rebootToken == "{2E8164FA-7A8B-4352-B0DC-479984070222}")
                {
                    foreach (var timer in threadSyncInboundTimers)
                    {
                        timer.Value.Change(new TimeSpan(hours: 0, minutes: 0, seconds: 1),
                                           new TimeSpan(hours: synchronizeInboundTickMinutes / 60, minutes: synchronizeInboundTickMinutes % 60, seconds: 0));
                    }
                    return true;
                }

                return false;
            }
        }

        ///<summary>
        /// Leggo i parametri definiti nel web.config
        ///</summary>
        //--------------------------------------------------------------------------------
        private static void ReadAppSettings()
        {
            //*************
            // legge il tempo in minuti dal parametro SynchronizeInboundTickMinutes nel web.config
            //*************
            int tickMinimum = 5;
            int tickDefault = 60; // il default ora e' 60

            synchronizeInboundTickMinutes = tickDefault;

            string tickMinutesAppSettings = ConfigurationManager.AppSettings["SynchronizeInboundTickMinutes"];

            if (!string.IsNullOrWhiteSpace(tickMinutesAppSettings))
            {
                try
                {
                    if (!Int32.TryParse(tickMinutesAppSettings, NumberStyles.Integer, CultureInfo.InvariantCulture, out synchronizeInboundTickMinutes))
                        synchronizeInboundTickMinutes = tickDefault;
                }
                catch
                {
                    synchronizeInboundTickMinutes = tickDefault;
                }
            }

            if (synchronizeInboundTickMinutes < tickMinimum)
                synchronizeInboundTickMinutes = tickMinimum;
        }

        ///<summary>
        /// Metodo richiamato dalla CallBack del Timer
        /// Viene eseguito ogni tot ore
        ///</summary>
        //-----------------------------------------------------------------------
        public void SynchronizeInboundCallBack(object o)
        {
            Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager loginMng = null;
            LoginData loginData = (LoginData)o;

            lock (this)
            {
                if (IsSyncInboundRunningForThisCompany(loginData.Company))
                    return;
                else
                    SetSyncInboundRunningForThisCompany(loginData.Company, Thread.CurrentThread.ManagedThreadId, false);
            }

            try
            {
                loginMng = GetLoginManager();
                if (loginMng.Login(loginData.Username, loginData.Password, loginData.WindowsAuthentication, loginData.Company, "DataSynchronizer", false) != 0)
                    return;

                ConnectionStringManager connectionStringManager = new ConnectionStringManager(loginMng.AuthenticationToken);
                List<ProviderConfiguration> providerList = DBManager.GetAllProviderConfiguration(connectionStringManager.CompanyConnectionString, Logger.Instance);

                foreach (ProviderConfiguration elem in providerList)
                {
                    IBaseSynchroProvider provider = DSFactory.Instance.GetProviderInbound(loginMng.AuthenticationToken, elem, connectionStringManager);
                    if (provider != null)
                    {
                        provider.SynchronizeInbound(loginMng.AuthenticationToken, loginData.Username, loginData.Password, loginData.WindowsAuthentication, loginData.Company, connectionStringManager.CompanyConnectionString);
                        CleanActionsLog(loginMng.AuthenticationToken, provider.ProviderName);
                    }
                }

                loginMng.LogOff();
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during SynchronizeInboundCallBack", ex);
                loginMng?.LogOff();
                return;
            }
            finally
            {
                lock (this)
                    SetSyncInboundRunningForThisCompany(loginData.Company, Thread.CurrentThread.ManagedThreadId, true);
            }
        }

        //-----------------------------------------------------------------------
        private Dictionary<string, int> _syncInboundCompanyRunning = new Dictionary<string, int>();

        //-----------------------------------------------------------------------
        private void SetSyncInboundRunningForThisCompany(string company, int threadId, bool bFree)
        {
            if (bFree)
            {
                if (threadId != _syncInboundCompanyRunning[company])
                    HasChangedThread = true;
                _syncInboundCompanyRunning.Remove(company);
            }
            else
                _syncInboundCompanyRunning.Add(company, threadId);
        }

        //-----------------------------------------------------------------------
        private bool IsSyncInboundRunningForThisCompany(string company)
        {
            return _syncInboundCompanyRunning.ContainsKey(company);
        }

        ///<summary>
        /// Notifica dal clientdoc, in fase di inserimento/update/cancellazione dal DE di Mago
        ///</summary>
        //-----------------------------------------------------------------------
        internal void Notify(string authenticationToken, int logID, string providerName, string onlyForDMS, string iMagoConfigurations)
        {
            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                if (!provider.DoNotify(logID, onlyForDMS, iMagoConfigurations))
                {
                    Logger.Instance.WriteToLog("Error during Notify", new Exception());
                    WriteMessageInEventLog(String.Format(DSStrings.MsgNotifyEndedWithError, provider.ProviderName, logID.ToString()), EventLogEntryType.Error, true);
                }
                else
                {
                    WriteMessageInEventLog(String.Format(DSStrings.MsgNotifyEndedWithSuccess, provider.ProviderName, logID.ToString()), EventLogEntryType.Information, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during Notify", ex);
            }
        }

        ///<summary>
        /// Istanzio il LM per poter poi richiamare il web method che mi serve
        ///</summary>
        //--------------------------------------------------------------------------------
        private Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager GetLoginManager()
        {
            try
            {
                Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager loginManager = new Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager();
                return loginManager;
            }
            catch (Exception e)
            {
                WriteMessageInEventLog(string.Format(DSStrings.MethodExceptionWithDetail, DSStrings.LMInit, e.ToString()), EventLogEntryType.Error, true);
                return null;
            }
        }

        ///<summary>
        /// Dato il companyId ed il providername vado a controllare se la batch
        /// massiva e' in esecuzione o meno
        ///</summary>
        //-----------------------------------------------------------------------
        internal bool IsMassiveSynchronizing(int companyId)
        {
            return SharedLock.Instance.IsMassiveSynchronizing;
        }

        ///<summary>
        /// Dato il companyId vado a controllare se la batch
        /// massiva di validazione e' in esecuzione o meno
        ///</summary>
        //-----------------------------------------------------------------------
        internal bool IsMassiveValidating()
        {
            return SharedLock.Instance.IsMassiveValidating;
        }

        ///<summary>
        /// Per gestire le chiamate outbound, ovvero la sincronizzazione dei dati da Mago verso le applicazioni esterne del CRM
        /// Si tratta di una sincronizzazione massiva, alla query standard e' necessario accodare eventuali filtri
        /// impostati nell'apposita batch in Mago
        ///</summary>
        //-----------------------------------------------------------------------
        internal void SynchronizeOutbound(string authenticationToken, string providerName, string startSynchroDate = "", bool bDelta = false)
        {
            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                provider.SynchronizeOutbound(startSynchroDate, bDelta);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during SynchronizeOutbound", ex);
            }
        }

        internal bool PauseResume(string authenticationToken, string providerName, bool bPause)
        {
            ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

            ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

            IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

            if (provider == null)
                throw new Exception("Provider is null");


            provider.IsInPause = bPause;
            return provider.IsInPause;
        }

        internal bool Abort(string authenticationToken, string providerName)
        {
            ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

            ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

            IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

            if (provider == null)
                throw new Exception("Provider is null");


            provider.Abort = true;
            return provider.Abort;
        }

        ///<summary>
        /// Per gestire le chiamate outbound, ovvero la sincronizzazione dei dati da Mago verso le applicazioni esterne del CRM
        /// Si tratta di una sincronizzazione massiva, alla query standard e' necessario accodare eventuali filtri
        /// impostati nell'apposita batch in Mago
        ///</summary>
        //-----------------------------------------------------------------------
        internal void SynchronizeErrorsRecovery(string authenticationToken, string providerName)
        {
            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                provider.SynchronizeErrorsRecovery();
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during SynchronizeErrorsRecovery", ex);
            }
        }

        //-----------------------------------------------------------------------
        public string GetActionsForDocument(string providerName, string docNamespace)
        {
            IBaseSynchroProvider baseSynchProvider = null;
            string message = string.Empty;

            try
            {
                // istanzio al volo un unparser del tipo giusto e carico i namespace
                // dei documenti da sincronizzare (non serve discriminare per companyId)
                baseSynchProvider = CreateWrapper(providerName, out message);
                Debug.WriteLine(string.Format("DSEngine.GetActionsForDocument(): {0}", message));

                if (baseSynchProvider == null)
                    return "000";

                var type = baseSynchProvider.GetType();
                FieldInfo v = type.GetField("DocumentToSyncList");
                if (v == null)
                    return "111";

                if (baseSynchProvider.GetType().Name.Equals("InfiniteCRMSynchroProvider", StringComparison.InvariantCultureIgnoreCase))
                {
                    List<IDocumentToSync> patDocList = v.GetValue(baseSynchProvider) as List<IDocumentToSync>;
                    foreach (IDocumentToSync doc in patDocList)
                    {
                        if (string.Compare(doc.Name, docNamespace, StringComparison.InvariantCultureIgnoreCase) == 0)
                            return doc.ActionsAttribute;
                    }
                }

                // Per Infinity le operazioni sui documenti sono tutte abilitate
                if (baseSynchProvider.GetType().Name.Equals("CRMInfinitySynchroProvider", StringComparison.InvariantCultureIgnoreCase))
                    return "111";
            }
            catch (Exception)
            {
            }
            finally
            {
                baseSynchProvider = null;
            }

            return "000";
        }

        //--------------------------------------------------------------------------------
        public void WriteErrorLog(Exception ex)
        {
            WriteErrorLog(ex.ToString());
        }

        ///<summary>
        /// Metodo thread safe per scrivere il messaggio nell'eventlog
        ///</summary>
        //--------------------------------------------------------------------------------
        public void WriteErrorLog(string error)
        {
            //todo verificare se thread safe
            lock (eventLog)
            {
                WriteMessageInEventLog(string.Format(DSStrings.MethodErrorWithDetail, "DataSynchronizer", error));
            }
        }

        ///<summary>
        /// Metodo generico per scrivere il messaggio nell'eventlog
        ///</summary>
        //----------------------------------------------------------------
        internal void WriteMessageInEventLog(string message, EventLogEntryType entryType = EventLogEntryType.Error, bool alwaysWrite = false)
        {
            string logEntry = string.Format("{0}: {1}", InstallationData.InstallationName, message);
            try
            {
                eventLog.WriteEntry(logEntry, entryType);
            }
            catch (Exception)
            {
                Debug.WriteLine(string.Format("{0} - {1}", DateTime.Now.ToString(), logEntry));
            }
        }

        ///<summary>
        /// Data una stringa di connessione ad un'azienda ritornatami da LM
        /// istanzio al volo una connection e vado a leggere i dati dei providers
        /// nella tabella DS_Providers
        ///</summary>
        //----------------------------------------------------------------
        internal List<DSProvider> GetProvidersForCompany(string companyConnString)
        {
            List<DSProvider> providers = new List<DSProvider>();

            if (string.IsNullOrWhiteSpace(companyConnString)) // non dovrebbe mai succedere!
                return providers;

            IDataReader myDataReader = null;
            try
            {
                using (TBConnection myConnection = new TBConnection(companyConnString, DBMSType.SQLSERVER))
                {
                    myConnection.Open();

                    TBDatabaseSchema dbSchema = new TBDatabaseSchema(myConnection);
                    if (!dbSchema.ExistTable("DS_Providers")) // check esistenza tabella DS_Providers
                        return providers;

                    using (TBCommand myCommand = new TBCommand(myConnection))
                    {
                        myCommand.CommandText = "SELECT [Name], [ProviderUrl], [ProviderUser], [ProviderPassword], [Disabled], [ProviderParameters], [IAFModules], [SkipCrtValidation] FROM [DS_Providers]"; // TODO sintassi Oracle
                        myDataReader = myCommand.ExecuteReader();
                        if (myDataReader == null)
                            return providers;

                        while (myDataReader.Read())
                        {
                            DSProvider dsProvider = new DSProvider();
                            dsProvider.Name = myDataReader["Name"].ToString();
                            dsProvider.Url = myDataReader["ProviderUrl"].ToString();
                            dsProvider.Login = myDataReader["ProviderUser"].ToString();
                            dsProvider.Password = myDataReader["ProviderPassword"].ToString();
                            dsProvider.Disabled = myDataReader["Disabled"].ToString().Equals("1");
                            dsProvider.Parameters = myDataReader["ProviderParameters"].ToString();
                            dsProvider.IAFModules = myDataReader["IAFModules"].ToString();
                            dsProvider.SkipCrtValidation = myDataReader["SkipCrtValidation"].ToString().Equals("1");
                            providers.Add(dsProvider);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteMessageInEventLog(string.Format(DSStrings.MethodExceptionWithDetail, "GetProvidersForCompany()", ex.Message), EventLogEntryType.Error, true);
                Debug.WriteLine(string.Format(DSStrings.MethodExceptionWithDetail, "GetProvidersForCompany()", ex.Message));
            }
            finally
            {
                if (myDataReader != null && !myDataReader.IsClosed)
                {
                    myDataReader.Close();
                    myDataReader.Dispose();
                }
            }

            return providers;
        }

        ///<summary>
        /// Dato il nome del provider via reflection istanzio la sua classe specifica
        ///</summary>
        //--------------------------------------------------------------------------------
        internal IBaseSynchroProvider CreateWrapper(string providerName, out string message, string companyName = "")
        {
            // invece di fare uno switch sul nome dell'azione, via reflection vado a cercare la classe relativa la provider
            IBaseSynchroProvider wrapper = null;
            message = string.Empty;

            try
            {
                //AssembliesLoader.LoadFromFullAssemblyName
                Dictionary<string, string> dictAppFiles = BasePathFinder.BasePathFinderInstance.GetSynchroProfilesFilePath(providerName);
                foreach (var key in dictAppFiles)
                {
                    string nameSpace = string.Empty;
                    string assemblyName = string.Empty;

                    if (!File.Exists(key.Value))
                        continue;

                    try
                    {
                        XDocument xDoc = XDocument.Load(key.Value);
                        nameSpace = xDoc.Element("SynchroProfiles").Element("Provider").Element("Namespace").Value;
                        assemblyName = xDoc.Element("SynchroProfiles").Element("Provider").Element("AssemblyName").Value;
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.WriteToLog(companyName, providerName, e.GetType().ToString() + " " + e.Message, "DSEngine.CreateWrapper");
                        message = e.Message;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(nameSpace) || string.IsNullOrWhiteSpace(assemblyName))
                        continue;

                    string configuration = string.Empty;
#if DEBUG
                    configuration = Path.Combine("TBApps", "Debug");
#else
                    if (Directory.Exists(Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppsPath(),"Publish")))
                        configuration="Publish";
                    else if (Directory.Exists(Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppsPath(),"TBApps")))
                        configuration=Path.Combine("TBApps","Release");
                    else
                        Logger.Instance.WriteToLog(companyName, providerName, "Unable to find DataSynchroProviders.dll", "DSEngine.CreateWrapper");

#endif
                    Logger.Instance.WriteToLog(companyName, providerName, "Load DataSynchroProviders.dll from " + configuration, "DSEngine.CreateWrapper");

                    Assembly assembly = Assembly.LoadFrom(Path.Combine(BasePathFinder.BasePathFinderInstance.GetAppsPath(), configuration, assemblyName.Replace(".dll", null) + ".dll"));
                    string type = nameSpace + "." + providerName + "SynchroProvider";
                    wrapper = (IBaseSynchroProvider)assembly.CreateInstance(type);
                    wrapper.LogWriter = Logger.Instance;
                    wrapper.CompanyName = companyName; // assegno il companyName al provider
                    break;
                }
            }
            catch (TypeLoadException e)
            {
                Logger.Instance.WriteToLog(companyName, providerName, e.GetType().ToString() + " " + e.Message, "DSEngine.CreateWrapper");
                WriteMessageInEventLog(e.GetType().ToString() + " " + e.Message);
                System.Diagnostics.Debug.Fail(e.Message);
                message += "\n" + e.Message;
                return null;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog(companyName, providerName, ex.GetType().ToString() + " " + ex.Message, "DSEngine.CreateWrapper");
                WriteMessageInEventLog(ex.GetType().ToString() + " " + ex.Message);
                message += "\n" + ex.Message;
                return null;
            }

            return wrapper;
        }

        ///<summary>
        /// Richiamato da ERP per testare i provider per company
        /// viene richiamato nell'OnOkTransaction della form dei providers (ma solo se disabled = false)
        ///</summary>
        //---------------------------------------------------------------------------
        internal bool TestProviderParameters(string authenticationToken, string providerName, string url, string username, string password, bool skipCrtValidation, string parameters, out string message, bool disabled)
        {
            message = string.Empty;
            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance, disabled);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                return provider.TestProviderParameters(url, username, password, skipCrtValidation, parameters, out message);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during TestProviderParameters", ex);
                return false;
            }
        }

        ///<summary>
        /// Richiamato da ERP per inizializzare i provider per company
        /// viene richiamato in fase di salvataggio dalla form dei providers
        ///</summary>
        //---------------------------------------------------------------------------
        internal bool SetProviderParameters(string authenticationToken, string providerName, string url, string username, string password, bool skipCrtValidation, string IAFModule, string parameters, out string message)
        {
            message = string.Empty;

            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                return provider.SetProviderParameters(authenticationToken, url, username, password, skipCrtValidation, parameters, IAFModule, out message);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during SetDataSynchProviderInfo", ex);
                return false;
            }
        }

        ///<summary>
        /// Richiamata dal web method Init
        /// Esegue una pulizia dei record non necessari nella DS_ActionsLog
        ///</summary>
        //--------------------------------------------------------------------------------
        internal void CleanActionsLog(string authenticationToken, string providerName)
        {
            ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

            ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

            IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

            if (provider == null)
                throw new Exception("Provider is null");

            // per ogni azienda sottoposta a sincronizzazione richiamo la clean

            if (!provider.IsProviderValid)
                return;

            using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {
                myConnection.Open();

                try
                {
                    using (TBCommand aSqlCommand = new TBCommand(myConnection))
                    {
                        // se il provider e' DMS cambiano i tipi di azioni da eliminare
                        if (string.Compare(provider.ProviderName, "DMSInfinity", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            aSqlCommand.CommandText = string.Format(@"DELETE FROM [DS_ActionsLog] FROM [DS_ActionsLog] p LEFT JOIN (SELECT [LogId] FROM
                                                        (SELECT MAX(LogId) AS [LogId], [DocTBGuid] FROM [DS_ActionsLog]
                                                        WHERE [ActionType] = {0} AND [ProviderName] = '{1}' GROUP BY [DocTBGuid]
                                                        UNION
                                                        SELECT MAX(LogId) AS [LogId], [DocTBGuid] FROM [DS_ActionsLog]
                                                        WHERE [ActionType] = {2} AND [ProviderName] = '{1}' GROUP BY [DocTBGuid])k )l ON p.LogId = l.LogId WHERE l.LogId IS NULL AND p.ProviderName = '{1}'",
                                                        (int)SynchroActionType.NewAttachment,
                                                        provider.ProviderName,
                                                        (int)SynchroActionType.DeleteAttachment);
                        }
                        else
                            aSqlCommand.CommandText = string.Format(@"DELETE FROM [DS_ActionsLog] FROM [DS_ActionsLog] p LEFT JOIN (SELECT [LogId] FROM
                                                        (SELECT MAX(LogId) AS [LogId], [DocTBGuid] FROM [DS_ActionsLog]
                                                        WHERE ([ActionType] = {0} OR [ActionType] = {1}) AND [ProviderName] = '{2}' GROUP BY [DocTBGuid]
                                                        UNION
                                                        SELECT MAX(LogId) AS [LogId], [DocTBGuid] FROM [DS_ActionsLog]
                                                        WHERE [ActionType] = {3} AND [ProviderName] = '{2}' GROUP BY [DocTBGuid])k )l ON p.LogId = l.LogId WHERE l.LogId IS NULL AND p.ProviderName = '{2}'",
                                                        (int)SynchroActionType.Insert,
                                                        (int)SynchroActionType.Massive,
                                                        provider.ProviderName,
                                                        (int)SynchroActionType.Update);
                        aSqlCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    Logger.Instance.WriteToLog(provider.CompanyName, provider.ProviderName, ex.Message, "DSEngine.CleanActionsLog");
                }
            }
        }

        ///<summary>
        /// Metodo per la creazione dei server esterni
        ///</summary>
        //---------------------------------------------------------------------------
        internal bool CreateExternalServer(string authenticationToken, string providerName, string extservername, string connstr, out string message)
        {
            message = string.Empty;
            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                return provider.CreateExternalServer(extservername, connstr, out message);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during CreateExternalServer", ex);
                return false;
            }
        }

        ///<summary>
        /// Metodo per la ricerca delle azienda di Infinity non ancora mappate.
        ///</summary>
        //---------------------------------------------------------------------------
        internal bool CheckCompaniesToBeMapped(string authenticationToken, string providerName, out string companylist, out string message)
        {
            message = string.Empty;
            companylist = string.Empty;

            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                return provider.CheckCompaniesToBeMapped(out companylist, out message);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during CheckCompaniesToBeMapped", ex);
                return false;
            }
        }

        ///<summary>
        /// Metodo per l'associazione dell'azienda di  Mago
        ///</summary>
        //---------------------------------------------------------------------------
        internal bool MapCompany(string authenticationToken, string providerName, string appreg, int magocompany, string infinitycompany, string taxid, out string message)
        {
            message = string.Empty;

            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                return provider.MapCompany(appreg, magocompany, infinitycompany, taxid, out message);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during MapCompany", ex);
                return false;
            }
        }

        ///<summary>
        /// Metodo per l'upload dei package di azioni
        //---------------------------------------------------------------------------
        internal bool UploadActionPackage(string authenticationToken, string providerName, string actionpath, out string message)
        {
            message = string.Empty;

            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                return provider.UploadActionPackage(actionpath, out message);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during UploadActionPackage", ex);
                return false;
            }
        }

        ///<summary>
        /// Validate Outbound
        ///</summary>
        //--------------------------------------------------------------------------------
        internal bool ValidateOutbound(string authenticationToken, string providerName, bool bCheckFK, bool bCheckXSD, string filters, string serializedTree, int workerId, out string message)
        {
            message = string.Empty;

            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                provider.ValidateOutbound(bCheckFK, bCheckXSD, filters, serializedTree, workerId);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during ValidateOutbound", ex);
                return false;
            }
        }

        ///<summary>
        /// Metodo per la validizione dell'XML, relativo al documento, tramite XSD
        ///</summary>
        //--------------------------------------------------------------------------------
        internal bool ValidateDocument(string authenticationToken, string providerName, string nameSpace, string guidDoc, string serializedErrors, int workerId, out string message, bool includeXsd = true)
        {
            message = string.Empty;

            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                return provider.ValidateDocument(nameSpace, guidDoc, serializedErrors, workerId, out message, includeXsd);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during ValidateDocument", ex);
                return false;
            }
        }

        ///<summary>
        /// Metodo per settare i criteri di convergenza
        //---------------------------------------------------------------------------
        internal bool SetConvergenceCriteria(string authenticationToken, string providerName, string xmlCriteria, out string message)
        {
            message = string.Empty;
            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                return provider.SetConvergenceCriteria(xmlCriteria, out message);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during SetConvergenceCriteria", ex);
                return false;
            }
        }

        ///<summary>
        /// Metodo per settare i criteri di convergenza
        //---------------------------------------------------------------------------
        internal bool GetConvergenceCriteria(string authenticationToken, string providerName, string actionName, out string xmlCriteria, out string message)
        {
            message = string.Empty;
            xmlCriteria = string.Empty;

            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                return provider.GetConvergenceCriteria(actionName, out xmlCriteria, out message);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during GetConvergenceCriteria", ex);
                return false;
            }
        }

        ///<summary>
        /// Metodo per settare i criteri di convergenza
        //---------------------------------------------------------------------------
        internal bool SetGadgetPerm(string authenticationToken, string providerName, out string message)
        {
            message = string.Empty;
            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                return provider.SetGadgetPerm(out message);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during GetConvergenceCriteria", ex);
                return false;
            }
        }

        /// <summary>
        /// Cancella i log del SynchroConnector creati da più di 7 giorni
        /// </summary>
        //----------------------------------------------
        internal bool PurgeSynchroConnectorLog(int companyId)
        {
            string message = string.Empty;
            bool result = false;

            DataSynchroDatabaseInfo dsdi = globalDataSynchroDbInfoList.FirstOrDefault(x => x.CompanyId == companyId);
            if (dsdi != null)
            {
                try
                {
                    LogCleaner logCleaner = new LogCleaner();
                    logCleaner.PurgeSynchroConnectorLog(dsdi.CompanyName, out message);
                    result = true;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    Debug.WriteLine(ex.ToString());
                    result = false;
                }
                finally
                {
                    WriteMessageInEventLog(message,EventLogEntryType.Information);
                }
            }
            return result;
        }

        ///<summary>
        /// Metodo per la ricerca delle azienda di Infinity non ancora mappate.
        ///</summary>
        //---------------------------------------------------------------------------
        internal bool CheckVersion(string authenticationToken, string providerName, string magoVersion, out string message)
        {
            message = string.Empty;
            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);

                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance, true);

                IBaseSynchroProvider provider = DSFactory.Instance.GetProvider(authenticationToken, providerConfiguration, connectionStringManager);

                if (provider == null)
                    throw new Exception("Provider is null");

                return provider.CheckVersion(magoVersion, out message);
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during CheckVersion", ex);
                return false;
            }
        }

        //---------------------------------------------------------------------------
        internal bool IsProviderEnabled(string authenticationToken, string providerName)
        {
            try
            {
                ConnectionStringManager connectionStringManager = new ConnectionStringManager(authenticationToken);
                ProviderConfiguration providerConfiguration = DBManager.GetProviderConfiguration(connectionStringManager.CompanyConnectionString, providerName, Logger.Instance, true);
                return providerConfiguration != null;
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteToLog("Error during IsProviderEnabled", ex);
                return false;
            }
        }

        //----------------------------------------------
        public void Dispose()
        {
            try
            {
                if (threadSyncInboundTimers != null)
                {
                    foreach (var timer in threadSyncInboundTimers)
                        timer.Value.Dispose();

                    threadSyncInboundTimers.Clear();
                }
            }
            catch { }
        }

    }
}
