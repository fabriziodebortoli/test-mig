using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders;
using Microarea.TaskBuilderNet.DataSynchroUtilities;
using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation;
using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.DataSynchroProviders
{
    //================================================================================
    public abstract class BaseSynchroProvider : IBaseSynchroProvider
    {
        internal RecoveryManager recoveryMng = null;

        private List<string> docNamespaceToSynchronize = new List<string>();
        internal string whereProviderName = " AND [ProviderName] = @providerName";

        public List<BaseDocumentToSync> documentToSyncList = new List<BaseDocumentToSync>();
        internal List<Task> pool = new List<Task>(); // task pool per DMS

        //---------------------------------------------------------------------
        private TBConnection _dbConnection = null;

        protected ConnectionStringManager connectionStringManager;

        public string NotifyDataSynch
        {
            get;
            set;
        }

        public BaseSynchroProvider(ConnectionStringManager _connectionStringManager)
        {
            connectionStringManager = _connectionStringManager;
            CompanyName = _connectionStringManager.CompanyConnectionString.GetCompany();

            LogWriter = Logger.Instance;
        }

        public BaseSynchroProvider()
        {
        }

        // Metodo che ritorna la TBConnection: se gli è stata passata dall'esterno, la restituisce, altrimenti la crea.
        // Il parametro in out indica se la TBConnection è stata creata in questo metodo
        //---------------------------------------------------------------------
        /// <exception cref="Microarea.TaskBuilderNet.Data.DatabaseLayer.TBException" />
        private TBConnection CreateDbConnection(out bool isMyTBConnection)
        {
            try
            {
                isMyTBConnection = false;

                if (_dbConnection == null)
                {
                    _dbConnection = new TBConnection(connectionStringManager.CompanyConnectionString + "MultipleActiveResultSets=True", DBMSType.SQLSERVER);
                    isMyTBConnection = true;
                }

                if (string.IsNullOrEmpty(connectionStringManager.CompanyConnectionString))
                    _dbConnection.ConnectionString = connectionStringManager.CompanyConnectionString + "MultipleActiveResultSets=True";

                return _dbConnection;
            }
            catch (TBException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new TBException("Sorry... No message for this exception. See inner exception", e);
            }
        }

        /// <summary>
        /// Controlla se è presente una connessione (isMyTBConnection = false), altrimenti la crea (isMyTBConnection = true)
        /// Apre la connessione, se non lo è ancora, e la ritorna.
        /// <summary>
        //---------------------------------------------------------------------
        /// <exception cref="Microarea.TaskBuilderNet.Data.DatabaseLayer.TBException" />
        public TBConnection OpenDbConnection(out bool isMyTBConnection)
        {
            lock (this)
            {
                try
                {
                    TBConnection tbConnection = CreateDbConnection(out isMyTBConnection);

                    if (tbConnection.State != ConnectionState.Open)
                        tbConnection.Open();
                    return tbConnection;
                }
                catch (TBException e)
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// La connessione viene chiusa all'interno di questo metodo solo se è stata creata e aperta nel rispettivo metodo di Open (isMyTBConnection = true)
        /// Se la creazione e la open sono state fatte in un metodo chiamante (isMyTBConnection = false), sarà li che dovrà essere chiusa la connessione.
        /// <summary>
        //---------------------------------------------------------------------
        /// <exception cref="Microarea.TaskBuilderNet.Data.DatabaseLayer.TBException" />
        public void CloseDbConnection(bool isMyTBConnection)
        {
            lock (this)
            {
                try
                {
                    if (!isMyTBConnection)
                        return;

                    if (_dbConnection != null)
                    {
                        if (_dbConnection.State != ConnectionState.Closed)
                            _dbConnection.Close();

                        _dbConnection.Dispose();
                        _dbConnection = null;
                    }
                }
                catch (Exception e)
                {
                    throw new TBException("Sorry... No message for this exception. See inner exception", e);
                }
            }
        }

        //---------------------------------------------------------------------
        protected abstract string ApplicationFolderName { get; }

        //---------------------------------------------------------------------
        internal abstract BaseSynchroProfileParser SynchroProfileParser { get; }

        //--------------------------------------------------------------------------------
        public abstract bool Notify(int logId, bool onlyForDMS, string iMagoConfigurations);

        //--------------------------------------------------------------------------------
        internal abstract void ExecuteMassiveExport(List<MassiveExportData> medList, DateTime startSynchroDate, bool isRecovery = false, bool bDelta = false);

        //--------------------------------------------------------------------------------
        internal abstract void ExecuteMassiveValidation(bool bCheckFK, bool bCheckXSD, string filters, string serializedTree, int workerId);

        //--------------------------------------------------------------------------------
        internal abstract bool ExecuteValidateDocument(string nameSpace, string guidDoc, string serializedErrors, int workerId, bool includeXsd = true);

        //--------------------------------------------------------------------------------
        internal abstract void SynchronizeErrorsRecoveryAsynch();

        //---------------------------------------------------------------------
        public IList<BaseDocumentToSync> DocumentToSyncList
        {
            get
            {
                if (documentToSyncList == null || documentToSyncList.Count == 0)
                    FillDocumentsToSynchronizeList();
                return documentToSyncList;
            }
        }

        ///<summary>
        /// Istanzio il LM per poter poi richiamare il web method che mi serve
        ///</summary>
        //--------------------------------------------------------------------------------
        protected LoginManager GetLoginManager()
        {
            try
            {
                LoginManager loginManager = new LoginManager();
                return loginManager;
            }
            catch (Exception)
            {
                //WriteMessageInEventLog(string.Format(EASyncStrings.MethodExceptionWithDetail, EASyncStrings.LMInit, e.ToString()), EventLogEntryType.Error, true);
                return null;
            }
        }

        ///<summary>
        /// Dato il logID vado a leggere sul database le informazioni dalla tabella DS_ActionsLog e riempio l'apposita struttura
        /// NON estraggo le eventuali righe con errori di quel tbguid da eseguire prima della notifica corrente, perche' in Pat:
        /// 1. se vado in UPDATE di un record che non esiste vado in INSERT
        /// 2. dovendo portare sempre tutti i campi a Pat, non e' necessario eseguire le operazioni intermedie eventualmente fallite
        ///</summary>
        //---------------------------------------------------------------------
        internal BaseObjectToSynch LoadInfoToSynch(int logID)
        {
            BaseObjectToSynch infoToSynch = new BaseObjectToSynch();
            infoToSynch.LogID = logID;

            // fare una query sulla tabella DS_ActionsLog per LogId ed estrarre le info del namespace e dell'operazione da eseguire
            // TODO: prevedere anche la sintassi Oracle!!

            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                string query = string.Format
                    (
                        "SELECT [DocNamespace], [DocTBGuid], [ActionType], [ActionData], [SynchStatus] FROM [DS_ActionsLog] WHERE [LogId] = {0} AND [ProviderName]= '{1}'",
                        logID.ToString(), ProviderName
                    );

                using (TBCommand myCommand = new TBCommand(query, myConnection))
                {
                    using (IDataReader myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            infoToSynch.DocTBGuid = myReader["DocTBGuid"].ToString();
                            infoToSynch.DocNamespace = myReader["DocNamespace"].ToString();
                            infoToSynch.ActionType = DSUtils.GetSyncroActionType(Convert.ToInt32(myReader["ActionType"]));
                            infoToSynch.SynchStatus = DSUtils.GetSynchroStatusType(Convert.ToInt32(myReader["SynchStatus"]));
                            infoToSynch.ActionData = myReader["ActionData"].ToString();
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(infoToSynch.DocTBGuid))
                    return null;
            }
            catch (TBException ex)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.LoadInfoToSynch", "LogId = " + logID.ToString());
                return null;
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return infoToSynch;
        }

        private bool GetOnlyForDMS(string onlyForDMS)
        {
            try
            {
                if (string.IsNullOrEmpty(onlyForDMS) || string.IsNullOrWhiteSpace(onlyForDMS))
                    return false;

                if (onlyForDMS.Trim().Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                   onlyForDMS.Trim().Equals("1", StringComparison.InvariantCultureIgnoreCase))
                    return true;
                else if (onlyForDMS.Trim().Equals("false", StringComparison.InvariantCultureIgnoreCase) ||
                        onlyForDMS.Trim().Equals("0", StringComparison.InvariantCultureIgnoreCase))
                    return false;
                else
                {
                    LogWriter.WriteToLog(CompanyName, ProviderName, "Argument not handled", $"BaseSynchroProvider.GetOnlyForDMS(onlyForDMS:{onlyForDMS})");
                    return false;
                }
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, $"BaseSynchroProvider.GetOnlyForDMS(onlyForDMS:{onlyForDMS})");
                return false;
            }
        }

        /// <summary>
        /// Metodo chiamato da Mago dopo che ho salvato un documento oggetto di sincronizzazione
        /// Viene istanziato un task che chiama la Notify interna in modalità asincrona
        /// </summary>
        //---------------------------------------------------------------------
        public override bool DoNotify(int logID, string onlyForDMS, string iMagoConfigurations)
        {
            if (logID <= 0)
                return false;

            // Create a task and supply a user delegate by using a lambda expression.
            Task taskNotify = Task.Factory.StartNew(async () =>
            {
                while (SharedLock.Instance.IsLocked)
                    await Task.Delay(5000);

                lock (SharedLock.Instance.Obj)
                {
                    SharedLock.Instance.IsLocked = true;
                    SharedLock.Instance.ProviderName = ProviderName;
                    try
                    {
                        Notify(logID, GetOnlyForDMS(onlyForDMS), iMagoConfigurations);
                    }
                    catch (Exception e)
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, $"BaseSynchroProvider.Notify(logId = {logID},onlyForDms = {GetOnlyForDMS(onlyForDMS)}, iMagoConfigurations = {iMagoConfigurations})");
                    }
                    finally
                    {
                        SharedLock.Instance.IsLocked = false;
                    }
                }
            }, TaskCreationOptions.PreferFairness);

            return true;
        }

        private bool TryGetDateTimeFromMagoString(string magoDate, out DateTime result)
        {
            result = DateTime.MinValue;
            try
            {
                result = DateTime.ParseExact(magoDate.Replace("ts", "").Replace("{", "").Replace("}", "").Replace("'", "").Trim(), "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);
            }
            catch (Exception e)
            {
                string logMagoDate = string.IsNullOrEmpty(magoDate) ? "" : magoDate;
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, $"BaseSynchroProvider.TryGetDateTimeFromMagoString({logMagoDate})");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Metodo chiamato nella fase di esportazione massiva
        /// Viene istanziato un task che chiama la SynchronizeOutbound interna in modalità asincrona
        /// </summary>
        //--------------------------------------------------------------------------------
        public override void SynchronizeOutbound(string startSynchroDate = "", bool bDelta = false)
        {
            // Create a task and supply a user delegate by using a lambda expression.
            Task taskSynchOutbound = null;
            taskSynchOutbound = Task.Factory.StartNew(() =>
            {
                SharedLock.Instance.PushSynchroRequest();
                while (SharedLock.Instance.IsLocked)
                {
                    taskSynchOutbound.Wait(5000);
                }

                lock (SharedLock.Instance.Obj)
                {
                    SharedLock.Instance.IsLocked = true;
                    SharedLock.Instance.IsMassiveSynchronizing = true;
                    SharedLock.Instance.ProviderName = ProviderName;
                    DateTime startSynchDate;
                    if(!string.IsNullOrEmpty(startSynchroDate) && TryGetDateTimeFromMagoString(startSynchroDate, out startSynchDate))
                        SynchronizeOutboundAsynch(startSynchDate, bDelta);
                    else
                        SynchronizeOutboundAsynch(DateTime.Now, bDelta);


                    SharedLock.Instance.PopSynchroRequest();

                    SharedLock.Instance.IsLocked = false;

                    if (SharedLock.Instance.GetSynchroRequestCount() == 0)
                        SharedLock.Instance.IsMassiveSynchronizing = false;
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Chiamata asincrona della SynchronizeOutbound
        /// </summary>
        //--------------------------------------------------------------------------------
        private void SynchronizeOutboundAsynch(DateTime startSynchroDate, bool bDelta = false)
        {
            //if (!IsProviderValid)
            //{
            //    LogWriter.WriteToLog(CompanyName, ProviderName, "LogonFailed", "BaseSynchroProvider.SynchronizeOutbound", string.Format(Resources.ProviderNotValid, ProviderName));
            //    return;
            //}
            //else
            //    LogWriter.WriteToLog(CompanyName, ProviderName, "Called SynchronizeOutboundAsynch", "BaseSynchroProvider.SynchronizeOutboundAsynch", ProviderName);

            LogWriter.WriteToLog(CompanyName, ProviderName, string.Empty, "Starting SynchronizeOutbound...");

            // leggo le righe salvate sulla tabella DS_ActionsQueue e gli eventuali filtri
            List<MassiveExportData> medList = GetFiltersForMassiveExport();
            if (medList == null || medList.Count == 0)
                return;

            // se ho trovato delle righe da analizzare nella tabella di coda procedo con la massiva
            ExecuteMassiveExport(medList, startSynchroDate, false, bDelta);

            //CleanActionsQueue();

            LogWriter.WriteToLog(CompanyName, ProviderName, string.Empty, "Ending SynchronizeOutbound...");
        }

        /// <summary>
        /// Metodo chiamato nella fase di ri-esecuzione delle righe con errore
        /// </summary>
        //--------------------------------------------------------------------------------
        public override void SynchronizeErrorsRecovery()
        {
            // Create a task and supply a user delegate by using a lambda expression.
            Task taskSynchErrRecovery = null;
            taskSynchErrRecovery = Task.Factory.StartNew(() =>
            {
                SharedLock.Instance.PushSynchroRequest();
                while (SharedLock.Instance.IsLocked)
                {
                    taskSynchErrRecovery.Wait(5000);
                }

                lock (SharedLock.Instance.Obj)
                {
                    SharedLock.Instance.IsLocked = true;
                    SharedLock.Instance.IsMassiveSynchronizing = true;
                    SharedLock.Instance.ProviderName = ProviderName;

                    SynchronizeErrorsRecoveryAsynch();

                    SharedLock.Instance.PopSynchroRequest();

                    SharedLock.Instance.IsLocked = false;

                    if (SharedLock.Instance.GetSynchroRequestCount() == 0)
                        SharedLock.Instance.IsMassiveSynchronizing = false;
                }
            });
        }

        /// <summary>
        /// Legge dalla tabella DS_ActionsQueue i filtri da eseguire
        /// </summary>
        //--------------------------------------------------------------------------------
        internal List<MassiveExportData> GetFiltersForMassiveExport(bool isRecovery = false)
        {
            List<MassiveExportData> medList = new List<MassiveExportData>();
            SynchroStatusType status = isRecovery ? SynchroStatusType.Error : SynchroStatusType.ToSynchro;

            string query = string.Format(@"SELECT [LogId], [ActionName], [SynchFilters], [TBCreatedID], [TBModifiedID] FROM [DS_ActionsQueue]
                                        WHERE [SynchStatus] = {0} AND [SynchDirection] = {1} AND [ProviderName] = '{2}' ORDER BY [LogId]",
                                        (int)status, (int)SynchroDirectionType.Outbound, this.ProviderName);

            bool isMyTBConnection = false;

            try
            {
                TBConnection connection = OpenDbConnection(out isMyTBConnection);

                using (TBCommand command = new TBCommand(query, connection))
                {
                    IDataReader dr = command.ExecuteReader();
                    if (dr != null && connection.DataReaderHasRows(dr))
                    {
                        while (dr.Read())
                        {
                            MassiveExportData med = new MassiveExportData();
                            med.Filters = dr["SynchFilters"].ToString();
                            med.EntityName = dr["ActionName"].ToString();
                            med.LogId = Convert.ToInt32(dr["LogId"]);
                            med.TBCreatedID = Convert.ToInt32(dr["TBCreatedID"]);
                            med.TBModifiedID = Convert.ToInt32(dr["TBModifiedID"]);
                            medList.Add(med);
                        }

                        dr.Close();
                        dr.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: gestion errore
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.GetFiltersForMassiveExport");
                return null;
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return medList;
        }

        //---------------------------------------------------------------------
        protected virtual void FillDocumentsToSynchronizeList()
        {
            BaseSynchroProfileParser spp = SynchroProfileParser;
            if (spp == null)
                return;

            foreach (KeyValuePair<string, string> kvp in SynchroProfilesFileDict)
                spp.ParseFile(kvp.Value, kvp.Key);

            documentToSyncList.AddRange(spp.SynchroProfileInfo.Documents);
        }

        //---------------------------------------------------------------------
        protected virtual void FillDocNamespaceToSynchronizeList()
        {
            foreach (BaseDocumentToSync d in DocumentToSyncList)
                if (!docNamespaceToSynchronize.Contains(d.Name, StringComparer.InvariantCultureIgnoreCase))
                    docNamespaceToSynchronize.Add(d.Name);
        }

        ///<summary>
        /// Ritorna un Dictionary con tutte le applicazioni che hanno esposto il modulo SynchroConnector ed
        /// il file SynchroProfiles.xml per quel specifico provider
        ///</summary>
        //---------------------------------------------------------------------
        protected Dictionary<string, string> SynchroProfilesFileDict
        {
            get { return BasePathFinder.BasePathFinderInstance.GetSynchroProfilesFilePath(ApplicationFolderName); }
        }

        ///<summary>
        /// Ritorna un Dictionary con tutte le applicazioni che hanno esposto il modulo SynchroConnector ed
        /// un subfolder Actions per quel specifico provider
        ///</summary>
        //---------------------------------------------------------------------
        protected Dictionary<string, string> SynchroFilesActionsFolderDict
        {
            get { return BasePathFinder.BasePathFinderInstance.GetSynchroFilesActionFolderPath(ApplicationFolderName); }
        }

        ///<summary>
        /// Dato un namespace ritorna l'oggetto DocumentToSync corrispondente
        ///</summary>
        //---------------------------------------------------------------------
        protected BaseDocumentToSync GetDocumentToSyncFromNs(string docNamespace)
        {
            foreach (BaseDocumentToSync dts in DocumentToSyncList)
                if (string.Compare(dts.Name, docNamespace, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return dts;

            return null;
        }

        //---------------------------------------------------------------------
        public virtual void SetNoSyncroStatus(int logID)
        {
            SetStatus(logID, SynchroStatusType.NoSynchro);
        }

        //---------------------------------------------------------------------
        public virtual void SetWaitStatus(int logID)
        {
            SetStatus(logID, SynchroStatusType.Wait);
        }

        //---------------------------------------------------------------------
        public virtual void SetExcludedStatus(int logID)
        {
            SetStatus(logID, SynchroStatusType.Excluded);
        }

        //---------------------------------------------------------------------
        internal virtual void SetStatus(int logID, SynchroStatusType status)
        {
            UpdateSynchronizationData(status, null, null, logID);
        }

        /// <summary>
        /// Questo metodo viene richiamato al termine di ogni INSERT/UPDATE di un'entita' (su singolo documento)
        /// Va ad aggiornare la riga nella tabella DS_ActionsLog con il nuovo stato di sincronizzazione e poi
        /// va ad inserire o aggiornare la riga globale nella DS_SynchronizationInfo
        ///</summary>
        //---------------------------------------------------------------------
        internal virtual void UpdateSynchronizationData(SynchroStatusType SynchStatus, string importXml, string synchMessage, int logID)
        {
            string synchMessageSub = "";

            if (!String.IsNullOrEmpty(synchMessage))
            {
                if(synchMessage.Length>=1024)
                    synchMessageSub = synchMessage.Substring(0, 1024);
                else
                    synchMessageSub = synchMessage;
            }
                
            // update della DS_ActionsLog con dati relativi alla sincronizzazione appena avvenuta.
            string query1 = "UPDATE [DS_ActionsLog] SET [SynchStatus] = @synchStatus WHERE [LogId] = @logid" + whereProviderName;
            string query2 = @"UPDATE [DS_ActionsLog] SET [SynchMessage] = @synchMessageSub, [SynchStatus] = @synchStatus, [SynchXMLData] = @synchXMLData, [TBModified] = @TBModified,
                            [TBModifiedID] = [TBCreatedID] WHERE [LogId] = @logid" + whereProviderName;

            bool pre = (string.IsNullOrWhiteSpace(importXml) && string.IsNullOrWhiteSpace(synchMessageSub));
            string query = pre ? query1 : query2;

            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                try
                {
                    using (TBCommand aSqlCommand = new TBCommand(myConnection))
                    {
                        aSqlCommand.CommandText = query;
                        if (!pre)
                        {
                            aSqlCommand.Parameters.Add("@synchMessageSub", string.IsNullOrWhiteSpace(synchMessageSub) ? String.Empty : synchMessageSub);
                            aSqlCommand.Parameters.Add("@synchXmlData", string.IsNullOrWhiteSpace(importXml) ? String.Empty : importXml);
                        }
                        aSqlCommand.Parameters.Add("@TBModified", DateTime.Now);
                        aSqlCommand.Parameters.Add("@synchStatus", SynchStatus);
                        aSqlCommand.Parameters.Add("@logid", logID);
                        aSqlCommand.Parameters.Add("@providerName", ProviderName);
                        aSqlCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.UpdateSynchronizationData");
                }

                string docTBGuid = null;
                string docNamespace = null;
                int workerID = 0;
                SynchroActionType lastAction = SynchroActionType.Insert;
                SynchroDirectionType synchDirection = SynchroDirectionType.Inbound;

                // rileggo i valori della riga appena aggiornata
                string queryexistAction = @"SELECT [DocTBGuid], [DocNamespace], [TBModified], [SynchDirection], [ActionType], [TBCreatedID]
                                            FROM [DS_ActionsLog] WHERE [LogId] = @logID" + whereProviderName;

                try
                {
                    using (TBCommand aSqlCommand = new TBCommand(myConnection))
                    {
                        aSqlCommand.CommandText = queryexistAction;
                        aSqlCommand.Parameters.Add("@logID", logID);
                        aSqlCommand.Parameters.Add("@providerName", ProviderName);
                        using (IDataReader myReader = aSqlCommand.ExecuteReader())
                        {
                            while (myReader.Read())
                            {
                                docTBGuid = myReader["DocTBGuid"].ToString();
                                docNamespace = myReader["DocNamespace"].ToString();
                                lastAction = DSUtils.GetSyncroActionType((int)myReader["ActionType"]);
                                synchDirection = DSUtils.GetSynchroDirectionType((int)myReader["SynchDirection"]);
                                workerID = (int)myReader["TBCreatedID"];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.UpdateSynchronizationData");
                }

                InsertSynchronizationInfo(SynchStatus, docTBGuid, docNamespace, workerID, lastAction, synchDirection, DateTime.Now);
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        protected void UpdateSynchronizationDirtyData(DateTime startSyncDate)
        {
            bool isMyTBConnection = false;
            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);
                string updateQuery = "UPDATE [DS_SynchronizationInfo] SET [HasDirtyTbModified] = '0', [TBModified] = @TBModified WHERE [HasDirtyTbModified] = '1'";
                using (TBCommand aSqlCommand = new TBCommand(myConnection))
                {
                    aSqlCommand.CommandText = updateQuery;
                    aSqlCommand.Parameters.Add("@TBModified", startSyncDate);
                    int updated = aSqlCommand.ExecuteNonQuery();

                    LogWriter.WriteToLog(CompanyName, ProviderName, "Updated " + updated + " record of DS_SynchronizationInfo for Incremental Synchronization", "BaseSynchroProvider.UpdateSynchronizationDirtyData");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.UpdateSynchronizationDirtyData");
            }
        }

        ///<summary>
        /// Inserisce o aggiorna una riga nella DS_SynchronizationInfo
        ///</summary>
       //---------------------------------------------------------------------
        internal void InsertSynchronizationInfo
            (
            SynchroStatusType synchStatus,
            string docTBGuid,
            string docNamespace,
            int workerID,
            SynchroActionType lastAction,
            SynchroDirectionType synchDirection,
            DateTime startSynchroDate,
            string auxConnectionString = "",
            bool ifExistsDoNothing = false,
            bool bDelta = false
            )
        {
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                // controllo se esiste nella DS_SynchronizationInfo una riga con quel TBGuid
                string queryexistInfo = "SELECT COUNT(*) FROM [DS_SynchronizationInfo] WHERE [DocTBGuid] = @docTBGuid" + whereProviderName;
                string queryIfExistsDoNothing = string.Empty;

                if (ifExistsDoNothing)
                    queryIfExistsDoNothing = queryexistInfo + " AND ([SynchStatus]=" + (int)SynchroStatusType.Error + " OR [SynchStatus]=" + (int)SynchroStatusType.Synchro + ")";

                bool exist = false;
                bool ifDoNothing = false;

                try
                {
                    using (TBCommand aSqlCommand = new TBCommand(myConnection))
                    {
                        aSqlCommand.CommandText = queryexistInfo;
                        aSqlCommand.Parameters.Add("@docTBGuid", docTBGuid);
                        aSqlCommand.Parameters.Add("@providerName", ProviderName);
                        exist = (int)aSqlCommand.ExecuteScalar() > 0;

                        if (ifExistsDoNothing)
                        {
                            aSqlCommand.CommandText = queryIfExistsDoNothing;
                            ifDoNothing = (int)aSqlCommand.ExecuteScalar() > 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.InsertSynchronizationInfo");
                }

                //se esiste e non devo fare niente allora esco
                if (ifDoNothing && ifExistsDoNothing)
                    return;

                try // faccio una INSERT o un UPDATE a seconda dell'esistenza del record
                {
                    using (TBCommand aSqlCommand = new TBCommand(myConnection))
                    {
                        if (exist)
                        {
                            if (bDelta)
                            {
                                aSqlCommand.CommandText =
                                        @"UPDATE [DS_SynchronizationInfo] SET [SynchStatus] = @SynchStatus, [SynchDate] = @SynchDate, [SynchDirection] = @SynchDirection,
                                    [LastAction] = @LastAction, [TBModified] = @TBModified, [TBModifiedID] = @TBModifiedID, [WorkerID] =  @WorkerID, [StartSynchDate] = @StartSynchDate, [HasDirtyTbModified] = '1' WHERE [DocTBGuid] = @DocTBGuid" + whereProviderName;
                            }
                            else
                            {
                                aSqlCommand.CommandText =
                                        @"UPDATE [DS_SynchronizationInfo] SET [SynchStatus] = @SynchStatus, [SynchDate] = @SynchDate, [SynchDirection] = @SynchDirection,
                                    [LastAction] = @LastAction, [TBModified] = @TBModified, [TBModifiedID] = @TBModifiedID, [WorkerID] =  @WorkerID, [StartSynchDate] = @StartSynchDate WHERE [DocTBGuid] = @DocTBGuid" + whereProviderName;
                            }
                        }
                        else
                        {
                            if (bDelta)
                            {
                                aSqlCommand.CommandText = @"INSERT INTO [DS_SynchronizationInfo] ([DocTBGuid], [ProviderName], [DocNamespace], [SynchStatus], [SynchDate], [SynchDirection],
                                                    [LastAction], [TBCreated], [TBCreatedID], [TBModifiedID], [WorkerID], [StartSynchDate], [HasDirtyTbModified])
                                                    VALUES (@DocTBGuid, @providerName, @DocNamespace, @SynchStatus, @SynchDate, @SynchDirection, @LastAction, @TBCreated,
                                                    @TBCreatedID, @TBModifiedID, @WorkerID, @StartSynchDate, '1')";
                            }
                            else
                            {
                                aSqlCommand.CommandText = @"INSERT INTO [DS_SynchronizationInfo] ([DocTBGuid], [ProviderName], [DocNamespace], [SynchStatus], [SynchDate], [SynchDirection],
                                                    [LastAction], [TBCreated], [TBModified], [TBCreatedID], [TBModifiedID], [WorkerID], [StartSynchDate])
                                                    VALUES (@DocTBGuid, @providerName, @DocNamespace, @SynchStatus, @SynchDate, @SynchDirection, @LastAction, @TBCreated,
                                                    @TBModified, @TBCreatedID, @TBModifiedID, @WorkerID, @StartSynchDate)";
                            }
                            

                            aSqlCommand.Parameters.Add("@DocNamespace", docNamespace);
                            aSqlCommand.Parameters.Add("@TBCreated", DateTime.Now);
                            aSqlCommand.Parameters.Add("@TBCreatedID", workerID);
                        }

                        aSqlCommand.Parameters.Add("@providerName", ProviderName);
                        aSqlCommand.Parameters.Add("@DocTBGuid", docTBGuid);
                        aSqlCommand.Parameters.Add("@SynchStatus", (int)synchStatus);
                        aSqlCommand.Parameters.Add("@SynchDate", DateTime.Now);
                        aSqlCommand.Parameters.Add("@SynchDirection", (int)synchDirection);
                        aSqlCommand.Parameters.Add("@LastAction", (int)lastAction);
                        if(!bDelta)
                            aSqlCommand.Parameters.Add("@TBModified", DateTime.Now);
                        else
                            aSqlCommand.Parameters.Add("@TBModified", new DateTime(1799, 12, 31));
                        aSqlCommand.Parameters.Add("@TBModifiedID", workerID);
                        aSqlCommand.Parameters.Add("@WorkerID", workerID);
                        aSqlCommand.Parameters.Add("@StartSynchDate", startSynchroDate);
                        aSqlCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.InsertSynchronizationInfo");
                }
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        ///<summary>
        /// Metodo che si occupa di effettuare l'INSERT/UPDATE iniziale nella DS_ActionsLog e DS_SynchronizationInfo
        /// in fase di inserimento massivo da CRMInfinity
        /// A fronte di un'azione di sincronizzazione massiva per un tipo documento (namespace), e' necessario
        /// comporre la query e scrivere il testo xml da inviare all'esterno.
        /// Prima bisogna "esplodere" tutti i documenti suddividendoli per TBGuid ed andare ad inserire n-righe
        /// nella tabella DS_ActionsLog e DS_SynchronizationInfo
        ///</summary>
       //---------------------------------------------------------------------
        internal void InsertSynchronizationData(SynchroStatusType statusType, ActionToImport action, string docNameSpace, SynchroDirectionType direction, SynchroActionType actionType, string connectionString, DateTime startSynchroDate, bool bDelta)
        {
            bool isMyTBConnection = false;


            string synchMessageSub = "";

            if (!String.IsNullOrEmpty(action.Message))
            {
                if (action.Message.Length >= 1024)
                    synchMessageSub = action.Message.Substring(0, 1024);
                else
                    synchMessageSub = action.Message;
            }


            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                using (TBCommand myCommand = new TBCommand(myConnection))
                {
                    string actionsLogQuery = string.Format(@"INSERT INTO [DS_ActionsLog]
                                                            ([ProviderName], [DocNamespace], [DocTBGuid], [ActionType],  [SynchDirection], [SynchStatus], [SynchMessage], [TBCreatedID], [TBModifiedID],
                                                            [ActionData], [SynchXMLData], [TBCreated], [TBModified])
                                                            VALUES ('{0}', '{1}', '{2}', {3}, {4}, {5}, '{6}', {7}, {8},'','', getdate(), getdate());",
                                                            ProviderName, docNameSpace, string.IsNullOrWhiteSpace(action.TBGuid) ? Guid.Empty.ToString() : action.TBGuid, (int)actionType,
                                                            (int)direction, (int)statusType, synchMessageSub.Replace("'", "''"), 0, 0);

                    myCommand.CommandText = actionsLogQuery;
                    myCommand.ExecuteNonQuery();
                }

                // per ogni riga di DS_ActionsLog vado ad inserire/aggiornare la riga con lo stesso TBGuid nella DS_SynchronizationInfo
                InsertSynchronizationInfo(statusType, action.TBGuid, docNameSpace, 0, actionType, direction, startSynchroDate, auxConnectionString: connectionString, ifExistsDoNothing: false, bDelta: bDelta);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "CRMInfinitySynchroProvider.InsertSynchronizationData");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        ///<summary>
        /// Metodo che si occupa di effettuare l'INSERT/UPDATE iniziale nella DS_ActionsLog e DS_SynchronizationInfo
        /// in fase di inserimento massivo da CRMInfinity.
        /// Viene utilizzata anche per l'azione UpdateProvider scatenata nel DMSInfinity
        /// A fronte di un'azione di sincronizzazione massiva per un tipo documento (namespace), e' necessario
        /// comporre la query e scrivere il testo xml da inviare all'esterno.
        /// Prima bisogna "esplodere" tutti i documenti suddividendoli per TBGuid ed andare ad inserire n-righe
        /// nella tabella DS_ActionsLog e DS_SynchronizationInfo
        ///</summary>
        internal void InsertSynchronizationData
        (
            SynchroStatusType statusType,
            string docTBGuid,
            MassiveExportData med,
            DateTime startSynchroDate,
            bool bDelta,
            string message = "",
            bool ifExistsDoNothing = false,
            SynchroActionType synchActionType = SynchroActionType.Massive
        )
        {
            bool exist = false;
            bool ifDoNothing = false;

            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                // controllo se esiste nella DS_SynchronizationInfo una riga con quel TBGuid
                string queryexistInfo = "SELECT COUNT(*) FROM [DS_ActionsLog] WHERE [DocTBGuid] = @docTBGuid AND [DocNamespace] = @docNamespace" + whereProviderName;
                string queryIfExistsDoNothing = string.Empty;
                if (ifExistsDoNothing)
                    queryIfExistsDoNothing = queryexistInfo + " AND ([SynchStatus]=" + (int)SynchroStatusType.Error + " OR [SynchStatus]=" + (int)SynchroStatusType.Synchro + ")";

                try
                {
                    using (TBCommand aSqlCommand = new TBCommand(myConnection))
                    {
                        aSqlCommand.CommandText = queryexistInfo;
                        aSqlCommand.Parameters.Add("@docTBGuid", docTBGuid);
                        aSqlCommand.Parameters.Add("@providerName", ProviderName);
                        aSqlCommand.Parameters.Add("@docNamespace", med.EntityName);
                        exist = (int)aSqlCommand.ExecuteScalar() > 0;
                        if (ifExistsDoNothing)
                        {
                            aSqlCommand.CommandText = queryIfExistsDoNothing;
                            ifDoNothing = (int)aSqlCommand.ExecuteScalar() > 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.InsertSynchronizationInfo");
                }

                //se esiste e non devo fare niente allora esco
                if (ifDoNothing && ifExistsDoNothing)
                    return;

                try
                {
                    using (TBCommand myCommand = new TBCommand(myConnection))
                    {
                        string actionsLogQuery;

                        if (exist)
                        {
                            actionsLogQuery = string.Format(@"UPDATE [DS_ActionsLog] SET [SynchStatus] = {0} ,[SynchMessage] = '{1}', [TBModified]=getdate()
                                                            WHERE logid= (select MAX(LogId) from [DS_ActionsLog] where DocTBGuid='{2}') AND [ProviderName] = '{3}'",
                                                            (int)statusType, message.Replace("'", "''"), docTBGuid, ProviderName);

                        }
                        else
                        {
                            actionsLogQuery = string.Format(@"INSERT INTO [DS_ActionsLog]
                                                            ([ProviderName], [DocNamespace], [DocTBGuid], [ActionType],  [SynchDirection], [SynchStatus], [SynchMessage], [TBCreatedID], [TBModifiedID], [ActionData],
                                                            [SynchXMLData],[TBCreated],[TBModified])
                                                            VALUES ('{0}', '{1}', '{2}', {3}, {4}, {5}, '{6}', {7}, {8}, '', '', getdate(), getdate());",
                                                               ProviderName, med.EntityName, string.IsNullOrWhiteSpace(docTBGuid) ? Guid.Empty.ToString() : docTBGuid, (int)synchActionType,
                                                               (int)SynchroDirectionType.Outbound, (int)statusType, message.Replace("'", "''"), med.TBCreatedID, med.TBModifiedID);
                        }

                        myCommand.CommandText = actionsLogQuery;
                        myCommand.ExecuteNonQuery();
                    }

                    // per ogni riga di DS_ActionsLog vado ad inserire/aggiornare la riga con lo stesso TBGuid nella DS_SynchronizationInfo
                    InsertSynchronizationInfo(statusType, docTBGuid, med.EntityName, med.TBCreatedID, synchActionType, SynchroDirectionType.Outbound, startSynchroDate, ifExistsDoNothing: ifExistsDoNothing, bDelta: bDelta);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "CRMInfinitySynchroProvider.InsertSynchronizationData");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "CRMInfinitySynchroProvider.InsertSynchronizationData");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        ///<summary>
        /// Aggiorna lo stato della riga nella DS_ActionsQueue
        ///</summary>
        internal void UpdateActionQueueStatus(SynchroStatusType synchStatus, int logID, string synchXMLData)
        {
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                using (TBCommand aSqlCommand = new TBCommand(myConnection))
                {
                    aSqlCommand.CommandText = string.Format("UPDATE [DS_ActionsQueue] SET [SynchStatus] = {0}, [SynchXMLData] = '{1}' WHERE [LogId] = {2}",
                        (int)synchStatus, string.IsNullOrWhiteSpace(synchXMLData) ? string.Empty : synchXMLData, logID.ToString());

                    aSqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.UpdateActionQueueStatus");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        ///<summary>
        /// Metodo richiamato in fase di notifica di una DELETE
        /// Devo eliminare tutti i record nella DS_ActionsLog e DS_SynchronizationInfo per quel TBGuid e il providerName corrente
        ///</summary>
       //---------------------------------------------------------------------
        public void DeleteSynchronizationData(string tbGuid)
        {
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                using (TBCommand aSqlCommand = new TBCommand(myConnection))
                {
                    aSqlCommand.CommandText = string.Format("DELETE FROM [DS_ActionsLog] WHERE [DocTBGuid] = '{0}' AND [ProviderName] = '{1}'", tbGuid, ProviderName);
                    aSqlCommand.ExecuteNonQuery();
                    aSqlCommand.CommandText = string.Format("DELETE FROM [DS_SynchronizationInfo] WHERE [DocTBGuid] = '{0}' AND [ProviderName] = '{1}'", tbGuid, ProviderName);
                    aSqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.DeleteSynchronizationData");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        /// <summary>
        /// Metodo chiamato nella fase di validazione massiva
        /// Viene istanziato un task che chiama la ValidateOutbound interna in modalità asincrona
        /// </summary>
        //--------------------------------------------------------------------------------
        public override void ValidateOutbound(bool bCheckFK, bool bCheckXSD, string filters, string serializedTree, int workerId)
        {
            // Create a task and supply a user delegate by using a lambda expression.
            Task taskSynchOutbound = null;
            taskSynchOutbound = Task.Factory.StartNew(() =>
            {
                SharedLock.Instance.PushSynchroRequest();
                while (SharedLock.Instance.IsLocked)
                {
                    taskSynchOutbound.Wait(5000);
                }

                lock (SharedLock.Instance.Obj)
                {
                    SharedLock.Instance.IsLocked = true;
                    SharedLock.Instance.IsMassiveValidating = true;
                    SharedLock.Instance.ProviderName = ProviderName;

                    ValidateOutboundAsynch(bCheckFK, bCheckXSD, filters, serializedTree, workerId);

                    SharedLock.Instance.PopSynchroRequest();

                    SharedLock.Instance.IsLocked = false;

                    if (SharedLock.Instance.GetSynchroRequestCount() == 0)
                        SharedLock.Instance.IsMassiveValidating = false;
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Chiamata asincrona della ValidateOutbound
        /// </summary>
        //--------------------------------------------------------------------------------
        private void ValidateOutboundAsynch(bool bCheckFK, bool bCheckXSD, string filters, string serializedTree, int workerId)
        {
            //if (!IsProviderValid)
            //{
            //    LogWriter.WriteToLog(CompanyName, ProviderName, "LogonFailed", "BaseSynchroProvider.ValidateOutbound", string.Format(Resources.ProviderNotValid, ProviderName));
            //    return;
            //}
            //else
            //    LogWriter.WriteToLog(CompanyName, ProviderName, "Called ValidateOutboundAsynch", "BaseSynchroProvider.ValidateOutboundAsynch", ProviderName);

            LogWriter.WriteToLog(CompanyName, ProviderName, string.Empty, "Starting ValidateOutbound...");

            // se ho trovato delle righe da analizzare nella tabella di coda procedo con la massiva
            ExecuteMassiveValidation(bCheckFK, bCheckXSD, filters, serializedTree, workerId);

            LogWriter.WriteToLog(CompanyName, ProviderName, string.Empty, "Ending ValidateOutbound...");
        }

        /// <summary>
        /// Validazione puntuale xsd
        /// </summary>
        //--------------------------------------------------------------------------------
        public override bool ValidateDocument(string nameSpace, string guidDoc, string serializedErrors, int workerId, out string message, bool includeXsd = true)
        {
            message = string.Empty;

            return ExecuteValidateDocument(nameSpace, guidDoc, serializedErrors, workerId, includeXsd);
        }

        ///<summary>
        /// Metodo per formattare correttamente gli errori di validazione xsd e FK
        ///</summary>
        //---------------------------------------------------------------------
        internal void InsertOrUpdateFormattedIAFMessageError(string docTBGuid, IAFError dataError)
        {
            bool isMyTBConnection = false;
            DataTable ValidationInfoTable = null;
            string previousMsgError = string.Empty;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);
                string query = $"SELECT MessageError FROM DS_ValidationInfo WHERE DocTBGuid = '{docTBGuid}'";

                using (TBDataAdapter sda = new TBDataAdapter(query, myConnection))
                {
                    ValidationInfoTable = new DataTable();
                    sda.Fill(ValidationInfoTable);
                }

                if (ValidationInfoTable.Rows.Count != 0)
                {
                    foreach (DataRow rowVal in ValidationInfoTable.Rows) // avrò al massimo un record perchè ho filtrato per Guid
                        previousMsgError = rowVal.ItemArray.GetValue(0).ToString();
                }

                Factory.Instance.FormatValidationError(previousMsgError, dataError);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "BaseSynchroProvider.InsertOrUpdateIAFMessageError");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        //---------------------------------------------------------------------
        private int UpdateValidationInfo(TBConnection myConnection, string docTBGuid, string actionName, string docNamespace, DateTime validationDate, IAFError dataError, int workerID)
        {
            return Factory.Instance.GetValidationDBManager().UpdateValidationInfo(myConnection, docTBGuid, actionName, docNamespace, validationDate, dataError, workerID, ProviderName, whereProviderName);
        }

        //---------------------------------------------------------------------
        private int InsertValidationInfo(TBConnection myConnection, string docTBGuid, string actionName, string docNamespace, DateTime validationDate, IAFError dataError, int workerID, bool bUsedForFilter)
        {
            return Factory.Instance.GetValidationDBManager().InsertValidationInfo(myConnection, docTBGuid, actionName, docNamespace, validationDate, dataError, workerID, bUsedForFilter, ProviderName, whereProviderName);
        }

        ///<summary>
        /// Controlla se esite la riga nella DS_ValidationInfo
        ///</summary>
        //---------------------------------------------------------------------
        private bool ExistRecordInValidationInfo(string docTBGuid)
        {
            bool isMyTBConnection = false;
            bool bRecordExist = false;
            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                try
                {
                    return Factory.Instance.GetValidationDBManager().ExistRecordInValidationInfo(myConnection, docTBGuid, ProviderName, whereProviderName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.ExistRecordInValidationInfo");
                }
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return bRecordExist;
        }

        public void FlushValidationInfo()
        {
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);
                Factory.Instance.GetValidationDBManager().Flush(connectionStringManager.CompanyConnectionString);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "BaseSynchroProvider.FlushValidationInfo");
            }
        }

        ///<summary>
        /// Inserisce o aggiorna una riga nella DS_ValidationInfo
        ///</summary>
        //---------------------------------------------------------------------
        internal void InsertOrUpdateValidationInfo(string docTBGuid, string actionName, string docNamespace, DateTime validationDate, IAFError dataError, int workerID, bool bErrorAlreadFormatted = false)
        {
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                if (!bErrorAlreadFormatted)
                    InsertOrUpdateFormattedIAFMessageError(docTBGuid, dataError);

                bool bRecordExist = ExistRecordInValidationInfo(docTBGuid);

                try
                {
                    if (bRecordExist)
                    {
                        UpdateValidationInfo(myConnection, docTBGuid, actionName, docNamespace, validationDate, dataError, workerID);
                    }
                    else
                    {
                        InsertValidationInfo(myConnection, docTBGuid, actionName, docNamespace, validationDate, dataError, workerID, false);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.InsertOrUpdateValidationInfo");
                }
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        ///<summary>
        /// Metodo chiamato prima di eseguire la procedura di validazione massiva. Cancella tutti i record della tabella DS_ValidationInfo con data antecedente
        /// alla data in cui si e' eseguita l'ultima validazione massiva.
        ///</summary>
        //---------------------------------------------------------------------
        internal int DeleteInfoRecordsForMassiveValidation()
        {
            bool isMyTBConnection = false;

            Logger.Instance.WriteToLog(CompanyName, ProviderName, "Start delete DS_ValidationInfo", "DeleteInfoRecordsForMassiveValidation", string.Empty);

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                try
                {
                    return Factory.Instance.GetValidationDBManager().DeleteValidationInfo(myConnection, ProviderName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    Logger.Instance.WriteToLog(CompanyName, ProviderName, "Error during delete DS_ValidationInfo" + ex.Message, "DeleteInfoRecordsForMassiveValidation", string.Empty);
                }
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
                Logger.Instance.WriteToLog(CompanyName, ProviderName, "End delete DS_ValidationInfo", "DeleteInfoRecordsForMassiveValidation", string.Empty);
            }
            return -1;
        }

        ///<summary>
        /// Metodo che viene chiamato al termine della validazione puntuale.
        /// Serve ad eliminare il record in DS_ValidationInfo se per quel TB_Guid la validazione non ha dato piu' nessun errore.
        ///</summary>
        //---------------------------------------------------------------------
        internal int DeleteRecordForValidationDocument(string docTBGuid)
        {
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);
                try
                {
                    return Factory.Instance.GetValidationDBManager().DeleteValidationInfo(myConnection, ProviderName, docTBGuid);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.DeleteRecordForValidationDocument");
                }
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
            return -1;
        }

        ///<summary>
        /// Metodo per inserire nella tabella DS_ValidationInfo i records relativi ai filtri di validazione
        /// (possono essere sia quelli esclusi dalla validazione che quelli che devono essere validati, a seconda di qual è l'insieme di cardinalità minore)
        /// poichè esclusi dai filtri
        ///</summary>
        //---------------------------------------------------------------------
        internal void InsertUsedForFiltersRecord(string filterQuery, string docnamespace, DateTime validationDate, int workerID)
        {
            bool isMyTBConnection = false;
            DataTable excludedTable = null;
            string docTBGuid = string.Empty;
            bool bRecordExist = false;
            string query = filterQuery;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                using (TBDataAdapter sda = new TBDataAdapter(query, myConnection))
                {
                    excludedTable = new DataTable();
                    sda.Fill(excludedTable);
                }

                if (excludedTable.Rows.Count != 0)
                {
                    foreach (DataRow rowVal in excludedTable.Rows)
                    {
                        docTBGuid = rowVal.ItemArray.GetValue(0).ToString();

                        try
                        {
                            bRecordExist = ExistRecordInValidationInfo(docTBGuid);

                            if (!bRecordExist)
                            {
                                IAFError iafError = new IAFError();
                                InsertValidationInfo(myConnection, docTBGuid, string.Empty, docnamespace, validationDate, iafError, workerID, true);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.InsertUsedForFiltersRecord");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "BaseSynchroProvider.InsertUsedForFiltersRecord");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        ///<summary>
        /// Aggiorna una riga nella DS_ValidationFKtoFix
        ///</summary>
        //---------------------------------------------------------------------
        internal void InsertValidationFKtoFix(string docNamespace, string qualifiedField, string value, DateTime validationDate, int workerID, string whereProviderName)
        {
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                Factory.Instance.GetValidationDBManager().InsertValidationFKtoFix(myConnection, ProviderName, docNamespace, qualifiedField, value, validationDate, workerID, whereProviderName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.InsertValidationFKtoFix");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        ///<summary>
        /// Aggiorna le righe della DS_ValidationFKtoFix con l'informazione RelatedErrors
        ///</summary>
        //---------------------------------------------------------------------
        internal void UpdateValidationFKtoFixWithRelatedErrors()
        {
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);
                Factory.Instance.GetValidationDBManager().UpdateValidationFKtoFixWithRelatedErrors(myConnection, ProviderName, whereProviderName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.UpdateValidationFKtoFixWithRelatedErrors");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        ///<summary>
        /// Controlla se esite la riga nella DS_ValidationInfo
        ///</summary>
        internal bool ExistRecordInValidationFKtoFix(string docNamespace, string qualifiedField, string value)
        {
            bool isMyTBConnection = false;
            bool bRecordExist = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);
                try
                {
                    bRecordExist = Factory.Instance.GetValidationDBManager().ExistRecordInValidationFKtoFix(myConnection, ProviderName, docNamespace, qualifiedField, value, whereProviderName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.ExistRecordInValidationFKtoFix");
                }
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return bRecordExist;
        }

        ///<summary>
        /// Metodo chiamato prima di eseguire la procedura di validazione massiva. Cancella tutti i record della tabella DS_ValidationFKToFix con data antecedente
        /// alla data in cui si e' eseguita l'ultima validazione massiva.
        ///</summary>
        //---------------------------------------------------------------------
        internal void DeleteFKToFixRecordsForMassiveValidation()
        {
            // Pulisco la mappa che contiene l'associazione errore di FK - numero errori correlati
            Factory.Instance.GetFKToFixErrors().Free();

            Logger.Instance.WriteToLog(CompanyName, ProviderName, "Start delete DS_ValidationFKToFix", "DeleteFKToFixRecordsForMassiveValidation", string.Empty);
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                try
                {
                    Factory.Instance.GetValidationDBManager().DeleteFKToFixRecordsForMassiveValidation(myConnection, ProviderName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    Logger.Instance.WriteToLog(CompanyName, ProviderName, "Error during delete DS_ValidationFKToFix" + ex.Message, "DeleteFKToFixRecordsForMassiveValidation", string.Empty);
                }
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
                Logger.Instance.WriteToLog(CompanyName, ProviderName, "End delete DS_ValidationFKToFix", "DeleteFKToFixRecordsForMassiveValidation", string.Empty);
            }
        }
    }
}
