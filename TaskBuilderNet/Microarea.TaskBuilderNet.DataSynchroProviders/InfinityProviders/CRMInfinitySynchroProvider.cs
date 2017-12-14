using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Unparsers;
using Microarea.TaskBuilderNet.DataSynchroProviders.Properties;
using Microarea.TaskBuilderNet.DataSynchroUtilities;
using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation;
using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders
{
    public static class StringExtensions
    {
        public static string TrimAndAddDot(this string stringWithoutDot)
        {
            if (string.IsNullOrEmpty(stringWithoutDot))
                return string.Empty;

            return string.Concat(stringWithoutDot.Trim(), ".");
        }
    }

    ///<summary>
    /// Provider per la comunicazione con il servizio del CRMInfinity
    /// TODO: prevedere le query anche con sintassi per Oracle
    ///</summary>
    //================================================================================
    [Serializable]
    public class CRMInfinitySynchroProvider : BaseSynchroProvider, IValidationProxy
    {
        // dictionary per cachare le informazioni parsate nelle actions
        private Dictionary<string, CRMActionInfo> crmActionsInfoDict = new Dictionary<string, CRMActionInfo>();

        private InfinitySyncroRef infinitySyncro = null;
        private TranscodingManager transcodingMng = null;
        private SynchroResponseParser responseParser = null;
        private string authenticationToken = null;
        //---------------------------------------------------------------------
        protected override string ApplicationFolderName { get { return "CRMInfinity"; } }

        //---------------------------------------------------------------------
        internal override BaseSynchroProfileParser SynchroProfileParser { get { return new SynchroProfileParser(); } }

        //---------------------------------------------------------------------
        internal TranscodingManager TranscodingMng { get { if (transcodingMng == null) transcodingMng = new TranscodingManager(ProviderName, CompanyName, LogWriter); return transcodingMng; } }

        //---------------------------------------------------------------------
        internal InfinitySyncroRef SyncroRef
        {
            get
            {
                return infinitySyncro;
            }
        }

        //---------------------------------------------------------------------
        internal SynchroResponseParser SynchroRespParser { get { if (responseParser == null) responseParser = new SynchroResponseParser(); return responseParser; } }
        // Attivazione modulo iMago, se non e' attivo non si tiene conto del iafmodule
        private bool bIMagoActivated = true;

        //---------------------------------------------------------------------
        public CRMInfinitySynchroProvider(string authenticationToken, ConnectionStringManager connectionStringManager, ProviderConfiguration config)
            : base(connectionStringManager)
        {
            IsValidationEnabled = true;
            infinitySyncro = new InfinitySyncroRef(ProviderName, CompanyName, LogWriter, true);

            string message = string.Empty;

            bIMagoActivated = GetLoginManager().IsActivated("ERP", "IMAGO");

            // significa che non ho ancora impostato i dati del provider sulla DS_Providers
            if (
                    !string.IsNullOrWhiteSpace(config.Url) ||
                    !string.IsNullOrWhiteSpace(config.User) ||
                    !string.IsNullOrWhiteSpace(config.IAFModule)
               )
                SetProviderParameters(authenticationToken, config.Url, config.User, config.Password, config.SkipCrtValidation, config.Parameters, config.IAFModule, out message);
            this.authenticationToken = authenticationToken;
        }

        ///<summary>
        /// Notifica inviata dal client-doc di Mago
        ///</summary>
        //---------------------------------------------------------------------
        public override bool Notify(int logID, bool onlyForDMS, string iMagoConfigurations)
        {
            bool notifyError = false;

            LogWriter.WriteToLog(CompanyName, ProviderName, string.Format("Called Notify (LogID = {0})", logID), "CRMInfinitySynchroProvider.Notify", ProviderName);

            if (logID <= 0)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "Notify returned false. LogID <= 0", "CRMInfinitySynchroProvider.Notify", ProviderName);
                return false;
            }

            // dato il logID vado a leggere sul database le informazioni dalla tabella DS_ActionsLog e riempio l'apposita struttura
            ActionToSynch actionToSynch = new ActionToSynch(LoadInfoToSynch(logID));
            if (actionToSynch == null) // almeno l'action corrente da eseguire deve essere presente, altrimenti non procedo
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "Notify returned false. actionToSynch == null", "CRMInfinitySynchroProvider.Notify", ProviderName);
                return false;
            }

            // per l'azione corrente di cui ci e' arrivata notifica
            // vado a caricare le azioni da eseguire dal xmlString dei profili di sincronizzazione (potrebbero essere piu' d'una)
            // e genero i relativi xmlString xml con gli appositi unparser
            if (!FillActionsToImport(actionToSynch))
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "Notify returned false. FillActionsToImport(...) returned false", "CRMInfinitySynchroProvider.Notify", ProviderName);
                return false;
            }

            SetWaitStatus(logID);

            bool sent = true;

            if(actionToSynch.ActionsToExecute.Count > 0)
                if (OnlyDMS(actionToSynch.ActionsToExecute[0].Name))
                {
                    UpdateSynchronizationData(sent ? SynchroStatusType.Synchro : SynchroStatusType.Error, string.Empty, "OnlyDMS", logID);
                    return sent;
                }

            string messageCumm = string.Empty;
            string message = string.Empty;

            LogInfo logInfo = new LogInfo()
            {
                ProviderLogWriter = this.LogWriter,
                CompanyName = this.CompanyName,
                ProviderName = this.ProviderName
            };

            LogWriter.WriteToLog(CompanyName, ProviderName, "ActionsToExecute loop started.", "CRMInfinitySynchroProvider.Notify", ProviderName);
            foreach (var action in actionToSynch.ActionsToExecute)
            {
                int count = 0;

                if (
                        !onlyForDMS &&
                        InfinityProviders.Parsers.Utilities.GetCRMEnabledForAction
                                                                (
                                                                    action.Name,
                                                                    InfinityProviders.Parsers.Utilities.GetIMagoCoonfigurationModules(iMagoConfigurations, logInfo),
                                                                    SyncroRef.InfinityData.IAFModule,
                                                                    logInfo,
                                                                    bIMagoActivated
                                                                )
                    )
                {
                    SyncroRef.ExecuteSyncroWithoutConnect(action, out message);
                }

                foreach (bool succeeded in action.IsSucceeded)
                {
                    if (
                            succeeded ||
                            onlyForDMS ||
                            !InfinityProviders.Parsers.Utilities.GetCRMEnabledForAction
                                                                (
                                                                    action.Name,
                                                                    InfinityProviders.Parsers.Utilities.GetIMagoCoonfigurationModules(iMagoConfigurations, logInfo),
                                                                    SyncroRef.InfinityData.IAFModule,
                                                                    logInfo,
                                                                    bIMagoActivated
                                                                )
                        )
                    {
                        if (!string.IsNullOrWhiteSpace(action.TBGuid[count]))
                        {
                            if (actionToSynch.ActionType == SynchroActionType.Exclude)
                                InsertSynchronizationInfo(SynchroStatusType.Excluded, action.TBGuid[count], actionToSynch.DocNamespace, 0, actionToSynch.ActionType, SynchroDirectionType.Outbound, DateTime.Now, ifExistsDoNothing: true);
                            else
                                InsertSynchronizationInfo(SynchroStatusType.Synchro, action.TBGuid[count], actionToSynch.DocNamespace, 0, actionToSynch.ActionType, SynchroDirectionType.Outbound, DateTime.Now, ifExistsDoNothing: true);
                            // per le action che possono essere disabilitate inserisco le chiavi nella tabella DS_Transcoding
                            // in modo da poter applicare i filtri di esclusione
                            if (!string.IsNullOrEmpty(action.Keys[count]))
                            {
                                if (actionToSynch.ActionType == SynchroActionType.Exclude)
                                    TranscodingMng.DeleteRow(connectionStringManager.CompanyConnectionString, action.Name, action.Keys[count]);
                                else
                                    TranscodingMng.InsertRow(connectionStringManager.CompanyConnectionString, action.Name, action.Keys[count], action.Name, action.Keys[count], action.TBGuid[count]);
                            }
                        }
                        if(!notifyError)
                        {
                            LoginManager loginMng = GetLoginManager();
                            loginMng.Init(false, authenticationToken);
                            loginMng.PurgeMessageByTag(action.TBGuid[count]);
                           
                        }
                        
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(action.TBGuid[count]))
                            action.TBGuid[count] = GetTbGuidFromDS_ActionsLog(logID);

                        if (!string.IsNullOrWhiteSpace(action.TBGuid[count]))
                            InsertSynchronizationInfo(SynchroStatusType.Error, action.TBGuid[count], actionToSynch.DocNamespace, 0, actionToSynch.ActionType, SynchroDirectionType.Outbound, DateTime.Now, ifExistsDoNothing: true);

                        sent = false;
                        messageCumm += string.IsNullOrWhiteSpace(message) ? action.ErrorMessages[count] : message + " ";
                        if(!action.Name.Contains("DIS") && (NotifyDataSynch.Equals("true") || NotifyDataSynch.Equals("TRUE")) && !notifyError)
                        {
                            string linkOpenDocu = GetUrlTBMago(action.Name, action.TBGuid[count], actionToSynch.DocNamespace);

                            if(!String.IsNullOrEmpty(linkOpenDocu))
                            {
                                LoginManager loginMng = GetLoginManager();
                                loginMng.Init(false, authenticationToken);
                                loginMng.PurgeMessageByTag(action.TBGuid[count]);

                                DateTime ExpriredDate = new DateTime(2091, 12, 17);
                                string messageError = "";
                                if (!string.IsNullOrEmpty(linkOpenDocu))
                                    messageError = @"<b>DataSynchronizer</b><br>" + action.ErrorMessages[count] + "<br><p><a href=\"" + linkOpenDocu + "\">Open Document</a>";
                                else
                                    messageError = @"<b>DataSynchronizer</b><br>" + action.ErrorMessages[count] + "<br><p>";
                                loginMng.AdvancedSendBalloon(
                                    authenticationToken,
                                    messageError,
                                    ExpriredDate,
                                    MessageType.DataSynch,
                                    new[] { ConnectionStrings.loginManager.UserName },
                                    MessageSensation.Warning,
                                    true,
                                    true,
                                    0,
                                    action.TBGuid[count]);

                                //loginMng.LogOff();
                                notifyError = true;
                            }
                           
                        }
                        

                    }

                    count++;
                }
            }
            LogWriter.WriteToLog(CompanyName, ProviderName, "ActionsToExecute loop ended.", "CRMInfinitySynchroProvider.Notify", ProviderName);

            //da rivedere!
            //se cancello il record vado cmq a cancellare le righe di transcodifica
            if (actionToSynch.ActionType == SynchroActionType.Delete)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "SynchroActionType is Delete.", "CRMInfinitySynchroProvider.Notify", ProviderName);
                TranscodingMng.DeleteRowByTBGuid(connectionStringManager.CompanyConnectionString, actionToSynch.DocTBGuid);
                DeleteSynchronizationData(actionToSynch.DocTBGuid);
            }
            else
            {
                if (actionToSynch.ActionType == SynchroActionType.Exclude)
                {
                    LogWriter.WriteToLog(CompanyName, ProviderName, "SynchroActionType is Exclude.", "CRMInfinitySynchroProvider.Notify", ProviderName);
                    UpdateSynchronizationData(sent ? SynchroStatusType.Excluded : SynchroStatusType.Error, string.Empty, messageCumm, logID);
                }
                else
                    UpdateSynchronizationData(sent ? SynchroStatusType.Synchro : SynchroStatusType.Error, string.Empty, messageCumm, logID);
                
            }

            LogWriter.WriteToLog(CompanyName, ProviderName, "Notify Finished", "CRMInfinitySynchroProvider.Notify", ProviderName);
            return sent;
        }

        //---------------------------------------------------------------------
        private string GetUrlTBMago(string action, string tbguid, string nameSpace)
        {
            int i = 0;
            int countField = 0;
            string[] field = new string[100];
            string[] valueField = new string[100];
            string result = "";
            /* string masterTable = "";
             string queryValues = "";

             CRMActionInfo  actions = ParseAction(action);
             if (actions.Actions.Count > 0)
                 masterTable = ParseAction(action).Actions[0].MasterTable;
             else
                 return "";*/
            string SynchroMassiveProfile = Path.Combine(PathFinder.BasePathFinderInstance.GetStandardApplicationPath("ERP"), @"SynchroConnector\SynchroProviders\CRMInfinity\SynchroMassiveProfiles.xml");
            string fileMasterTable = "";
            XmlTextReader reader;
            string masterTable = "";
            string queryValues = "";

            if (File.Exists(SynchroMassiveProfile))
            {
                reader = new XmlTextReader(SynchroMassiveProfile);

                while (reader.Read())
                {
                    if (reader.Name == "Action" && reader.GetAttribute("name") == action)
                    {
                        fileMasterTable = Path.Combine(PathFinder.BasePathFinderInstance.GetStandardApplicationPath("ERP"), @"SynchroConnector\SynchroProviders\CRMInfinity\Actions\", reader.GetAttribute("file") + ".xml");
                        break;
                    }
                }
            }

            if (File.Exists(fileMasterTable))
            {
                reader = new XmlTextReader(fileMasterTable);

                while (reader.Read())
                {
                    if (reader.Name == "Action" && reader.GetAttribute("name") == action)
                    {
                        masterTable = reader.GetAttribute("mastertable");
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(masterTable))
                return "";

            queryValues = "select";

            using (SqlConnection con = new SqlConnection(connectionStringManager.CompanyConnectionString))
            {

                con.Open();

                using (SqlCommand command = new SqlCommand("SELECT Col.Column_Name from INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab, INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col WHERE Col.Constraint_Name = Tab.Constraint_Name AND Col.Table_Name = Tab.Table_Name AND Constraint_Type = 'PRIMARY KEY'AND Col.Table_Name = '" + masterTable + "'", con))

                using (SqlDataReader read = command.ExecuteReader())
                {
                    while (read.HasRows)
                    {
                        while (read.Read())
                        {
                            field[i] = read.GetString(0);
                            queryValues = queryValues + " " + read.GetString(0) + ",";
                            i++;
                        }
                        read.NextResult();
                    }
                }

                countField = i;
                i = 0;

                queryValues = queryValues.Substring(0, queryValues.Length - 1) + " from " + masterTable + " where tbguid='" + tbguid + "'";


                using (SqlCommand myCommand = new SqlCommand(queryValues, con))
                {
                    using (IDataReader myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {

                            for (; i < countField; i++)
                            {
                                valueField[i] = myReader[field[i]].ToString();
                            }
                        }
                    }
                }

                con.Close();
            }

            result = "TB://" + nameSpace + "?";

            for (int j = 0; j < countField; j++)
            {
                result = result + field[j] + ":" + valueField[j] + ";";
            }

            return result;
        }


        //---------------------------------------------------------------------
        private string GetTbGuidFromDS_ActionsLog(int logID)
        {
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                using (TBCommand aSqlCommand = new TBCommand(myConnection))
                {
                    // controllo se esiste nella DS_SynchronizationInfo una riga con quel TBGuid
                    string queryexistInfo = "SELECT DocTBGuid FROM [DS_ActionsLog] WHERE [LogId] = @logId";

                    aSqlCommand.CommandText = queryexistInfo;
                    aSqlCommand.Parameters.Add("@logId", logID);
                    object tbGuidObj = aSqlCommand.ExecuteScalar();
                    return tbGuidObj == null ? string.Empty : tbGuidObj.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.InsertSynchronizationInfo");
                return string.Empty;
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        ///<summary>
        /// Restituisce Gli XML con le relative action associate al NameSpace indicato
        ///</summary>
        //---------------------------------------------------------------------
        private List<ActionToExport> GetXML(string tbguid, string nameSpace)
        {
            Dictionary<string, string> actionsDictionary = new Dictionary<string, string>();

            BaseObjectToSynch infoToSynch = new BaseObjectToSynch();

            infoToSynch.DocTBGuid = tbguid;
            infoToSynch.DocNamespace = nameSpace;
            infoToSynch.ActionType = DSUtils.GetSyncroActionType(Convert.ToInt32(31588352));
            infoToSynch.SynchStatus = DSUtils.GetSynchroStatusType(Convert.ToInt32(31457280));
            infoToSynch.ActionData = "";

            if (string.IsNullOrWhiteSpace(infoToSynch.DocTBGuid))
                return null;

            ActionToSynch actionToSynch = new ActionToSynch(infoToSynch);

            if (actionToSynch == null) // almeno l'action corrente da eseguire deve essere presente, altrimenti non procedo
                return null;

            // per l'azione corrente di cui ci e' arrivata notifica
            // vado a caricare le azioni da eseguire dal xmlString dei profili di sincronizzazione (potrebbero essere piu' d'una)
            // e genero i relativi xmlString xml con gli appositi unparser
            if (!FillActionsToImport(actionToSynch))
                return null;

            return actionToSynch.ActionsToExecute;
        }

        // ritorna il nome dell Action a partire dal namespace del documento
        //---------------------------------------------------------------------
        public string GetActionNameFromNs(string NameSpace)
        {
            DocumentToSync doc = GetDocumentToSyncFromNs(NameSpace) as DocumentToSync;

            if (doc.Actions.Count == 0)
                return string.Empty;

            return doc.Actions[0].Name;
        }

        // ritorna l'array delle Action a partire dal namespace del documento
        //---------------------------------------------------------------------
        public string[] GetArrayActionNameFromNs(string NameSpace)
        {
            DocumentToSync doc = GetDocumentToSyncFromNs(NameSpace) as DocumentToSync;
            string[] arr = new string[10];
            string pathFileXSD = Path.Combine(PathFinder.BasePathFinderInstance.GetStandardApplicationPath("ERP"), @"SynchroConnector\SynchroProviders\CRMInfinity\Actions");
            XmlTextReader reader = null;

            if (doc.Actions.Count == 0)
                return null;

            for (int i = 0; i < doc.Actions.Count; i++)
            {
                // arr[i] = doc.Actions[i].Name;
                reader = new XmlTextReader(Path.Combine(pathFileXSD, doc.Actions[i].Name + ".xml"));

                int j = 0;

                while (reader.Read())
                {
                    if (reader.LocalName == "Action" && !String.IsNullOrEmpty(reader.GetAttribute("name")))
                    {
                        arr[j] = reader.GetAttribute("name");
                        j++;
                    }
                }
            }

            return arr;
        }

        ///<summary>
        ///	Per l'azione corrente di cui ci e' arrivata notifica
        /// vado a caricare le azioni da eseguire dal xmlString dei profili di sincronizzazione (potrebbero essere piu' d'una)
        /// eseguo se necessario le query sul database di ERP e genero i relativi xmlString xml con gli appositi unparser
        ///</summary>
        //---------------------------------------------------------------------
        private bool FillActionsToImport(ActionToSynch actionToSynch, bool bDelta = false)
        {
            // dato il namespace del documento carico le azioni da eseguire dal xmlString dei profili di sincronizzazione
            DocumentToSync doc = GetDocumentToSyncFromNs(actionToSynch.DocNamespace) as DocumentToSync;
            if (doc == null || doc.Actions.Count == 0)
                return false;

            // scorro le azioni previste per quel documento, richiamo il parse (solo se necessario)
            // al momento ce n'e' solo una che poi va "esplosa"
            foreach (DSAction act in doc.Actions)
            {
                CRMActionInfo crmInfo = GetActionByName(act.Name);
                if (crmInfo == null) // se non trovo la descrizione dell'azione mi fermo subito
                    break;

                List<ActionToExport> xmlList = null;

                switch (actionToSynch.ActionType)
                {
                    case SynchroActionType.Exclude:
                    case SynchroActionType.Insert:
                        xmlList = UnparseStringForInsertAction(crmInfo, actionToSynch.DocTBGuid, actionToSynch.ActionType, string.Empty, 0, 500, bDelta);
                        break;

                    case SynchroActionType.Update:
                        xmlList = UnparseStringForUpdateAction(crmInfo, actionToSynch.DocTBGuid, actionToSynch.ActionType, actionToSynch.ActionData, bDelta);
                        break;

                    case SynchroActionType.Delete:
                        xmlList = UnparseStringForDeleteAction(crmInfo, actionToSynch.ActionType, actionToSynch.ActionData);
                        break;
                    default:
                        xmlList = UnparseStringForInsertAction(crmInfo, actionToSynch.DocTBGuid, actionToSynch.ActionType, string.Empty, 0, 500, bDelta);
                        break;

                }

                if (xmlList != null && xmlList.Count > 0)
                    actionToSynch.ActionsToExecute.AddRange(xmlList);
            }

            return true;
        }

        ///<summary>
        /// Data la porzione di xml da inviare al CRM si cerca il nome del primo nodo
        /// in modo da estrapolare il nome della action
        ///</summary>
        //---------------------------------------------------------------------
        private string GetActionNameFromXMLData(string synchXmlData)
        {
            try
            {
                XDocument xDoc = XDocument.Load(new StringReader(synchXmlData));
                XElement xActionElem = (XElement)xDoc.FirstNode;
                return xActionElem.Name.LocalName;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "CRMInfinitySynchroProvider.GetActionNameFromXMLData");
                return string.Empty;
            }
        }

        ///<summary>
        /// Metodo che si occupa di comporre la stringa xml con la sintassi corretta per essere poi inviata al server Infinity
        /// In base al tipo di azione da eseguire, si deve tener conto del contenuto della colonna ActionData, dove sono
        /// presenti informazioni specifiche memorizzate dal CDNotification, e dalla sua interpretazione sara' possibile
        /// generare l'xml comprensivo delle varie operazioni da eseguire
        ///</summary>
        //--------------------------------------------------------------------------------
        private List<ActionToExport> UnparseStringForInsertAction(CRMActionInfo crmInfo, string docTBGuid, SynchroActionType syncActionType, string massiveFilters = "", int offset = 0, int recordsATime = 500, bool bDelta = false)
        {
            List<ActionToExport> xmlActionList = new List<ActionToExport>();

            foreach (CRMAction action in crmInfo.Actions)
            {
                // eseguo la query per l'azione padre e riempio le strutture
                List<DTValuesToImport> dtToImportList = QueryValuesToImport(action, docTBGuid, syncActionType == SynchroActionType.Massive, massiveFilters, offset, recordsATime, bDelta);

                if (dtToImportList == null || dtToImportList.Count == 0)
                    continue;

                QueryValuesToImportForAppend(syncActionType, action, dtToImportList, bDelta);

                // richiamo la classe che si occupa di scrivere la stringa xml formattata e sintatticamente corretta da dare poi in pasto al ws del CRM
                ActionToExport myActionToExport = InfinityUnparseHelper.UnparseStringForAction(SyncroRef.RegisteredApp, action, syncActionType, dtToImportList);
                xmlActionList.Add(myActionToExport);

                // faccio la Dispose di tutti i datatable in memoria perche' ho finito di creare le stringhe xml
                foreach (DTValuesToImport dt in dtToImportList)
                    dt.MasterDt.Dispose();
            }

            return xmlActionList;
        }

        ///<summary>
        /// Nel caso di azioni in Append vado ad eseguire le query (andando con il tbguid del padre)
        /// ed arricchisco il datatable del padre con una lista di datatable in append
        ///</summary>
        //--------------------------------------------------------------------------------
        private void QueryValuesToImportForAppend(SynchroActionType syncActionType, CRMAction action, List<DTValuesToImport> dtToImportList, bool bDelta = false)
        {
            // gestisco le azioni in Append e vado ad innestare le informazioni nella struttura in memoria del padre
            for (int i = 0; i < action.AppendActions.Count; i++)
            {
                CRMAction appendAction = action.AppendActions[i];

                List<DTValuesToImport> dtAppendToImportList = new List<DTValuesToImport>();

                if (dtToImportList.Count > 0)
                {
                    DTValuesToImport dtValues = dtToImportList[0];

                    string appendMasterTBGuid = string.Empty;
                    try
                    {
                        appendMasterTBGuid = dtValues.MasterDt.Rows[0]["TBGuid"].ToString();
                    }
                    catch
                    {
                        continue;
                    }

                    // per la massiva non passiamo i filtri e gli offset perche' comanda il master estratto dal padre
                    dtAppendToImportList = QueryValuesToImport(appendAction, appendMasterTBGuid, syncActionType == SynchroActionType.Massive, string.Empty, 0, 500, bDelta);
                }

                for (int y = 0; y < dtToImportList.Count; y++)
                {
                    // aggiungo il datatable anche se non ho estratto nulla!
                    if (dtToImportList.Count != dtAppendToImportList.Count)
                        dtToImportList[y].AppendDtList.AddRange(new List<DTValuesToImport>());
                    else // per gestire le personalizzazioni delle righe (fix anomalia 23005)
                        dtToImportList[y].AppendDtList.Add(dtAppendToImportList[y]);
                }
            }
        }

        ///<summary>
        /// Metodo che si occupa di comporre la stringa xml con la sintassi corretta per essere poi inviata al server Infinity
        /// In caso di Update del record devo eseguire la query sul db ed interpretare cio' che e' indicato nella colonna ActionData
        /// Poi devo integrare le info caricate dal database con quelle
        ///</summary>
        //--------------------------------------------------------------------------------
        private List<ActionToExport> UnparseStringForUpdateAction(CRMActionInfo crmInfo, string docTBGuid, SynchroActionType syncActionType, string actionData, bool bDelta = false)
        {
            List<ActionToExport> xmlActionList = new List<ActionToExport>();

            foreach (CRMAction action in crmInfo.Actions)
            {
                List<DTValuesToImport> dtToImportList = QueryValuesToImport(action, docTBGuid);
                if (dtToImportList == null || dtToImportList.Count == 0)
                    continue;

                QueryValuesToImportForAppend(syncActionType, action, dtToImportList);

                ActionDataInfo actDataInfo = ActionDataParser.ParseActionData(syncActionType, actionData);

                // richiamo la classe che si occupa di scrivere la stringa xml formattata e sintatticamente corretta da dare poi in pasto al ws del CRM
                // ogni unparser deve preoccuparsi di quali informazioni memorizzate nell'oggetto ActionDataInfo ha bisogno
                // (attualmente vengono ritornati tutti i dbtslavebuffered e per ognuno le righe aggiunte/modificate/cancellate)
                ActionToExport myList = InfinityUnparseHelper.UnparseStringForAction(SyncroRef.RegisteredApp, action, syncActionType, dtToImportList, actDataInfo);
                xmlActionList.Add(myList);

                // faccio la Dispose di tutti i datatable in memoria perche' ho finito di creare le stringhe xml
                foreach (DTValuesToImport dt in dtToImportList)
                    dt.MasterDt.Dispose();
            }

            return xmlActionList;
        }

        ///<summary>
        /// Method returns export actions that failed during the massive export operation
        ///</summary>
        //--------------------------------------------------------------------------------
        private List<ActionToExport> UnparseStringForRecovery(CRMActionInfo crmInfo, List<ActionToExport> tbGuids, string nameSpace, string massiveFilters = "")
        {
            List<ActionToExport> xmlActionList = new List<ActionToExport>();

            foreach (CRMAction action in crmInfo.Actions)
            {
                foreach (ActionToExport tbGuid in tbGuids)
                {
                    if (!tbGuid.Name.Equals(nameSpace))
                        continue;

                    //foreach tbguid estratto vado as richiamare Queryablevaluestoimport con tbguid specifico
                    List<DTValuesToImport> dtToImportList = QueryValuesToImport(action, tbGuid.TBGuid[0], false, massiveFilters); // cambiare la query
                    if (dtToImportList == null || dtToImportList.Count == 0)
                        continue;

                    // richiamo la classe che si occupa di scrivere la stringa xml formattata e sintatticamente corretta da dare poi in pasto al ws del CRM
                    ActionToExport myList = InfinityUnparseHelper.UnparseStringForAction(SyncroRef.RegisteredApp, action, SynchroActionType.Insert, dtToImportList);
                    xmlActionList.Add(myList);

                    // faccio la Dispose di tutti i datatable in memoria perche' ho finito di creare le stringhe xml
                    foreach (DTValuesToImport dt in dtToImportList)
                        dt.MasterDt.Dispose();
                }
            }

            return xmlActionList;
        }

        ///<summary>
        /// Metodo che si occupa di comporre la stringa xml con la sintassi corretta per essere poi inviata al server Infinity
        /// In caso di eliminazione del record NON devo eseguire la query sul db, ma interpretare cio' che e' indicato nella colonna ActionData
        /// Basta infatti eliminare solo il master passando i valori della PK (facendo attenzione che la PK di ERP potrebbe non coincidere con quella di Infinity)
        ///</summary>
        //--------------------------------------------------------------------------------
        private List<ActionToExport> UnparseStringForDeleteAction(CRMActionInfo crmInfo, SynchroActionType syncActionType, string actionData)
        {
            List<ActionToExport> xmlActionList = new List<ActionToExport>();

            ActionDataInfo actDataInfo = ActionDataParser.ParseActionData(syncActionType, actionData);
            if (actDataInfo == null || actDataInfo.KeysForDeleteActionList.Count == 0)
                return xmlActionList;

            foreach (CRMAction action in crmInfo.Actions)
            {
                if (action.SkipForDelete)
                    continue;

                // richiamo la classe che si occupa di scrivere la stringa xml formattata e sintatticamente corretta da dare poi in pasto al ws del CRM
                ActionToExport myList = InfinityUnparseHelper.UnparseStringForAction(SyncroRef.RegisteredApp, action, syncActionType, dtValuesList: null, adi: actDataInfo);
                xmlActionList.Add(myList);
            }

            xmlActionList.Reverse();

            return xmlActionList;
        }


        private string InferMasterTable(string rawFromClause)
        {
            return rawFromClause;
        }

        private string GetDeltaFromClause(string masterTable)
        {
            return $" LEFT OUTER JOIN DS_SynchronizationInfo ON {masterTable.TrimAndAddDot()}TBGuid = DS_SynchronizationInfo.DocTBGuid";
        }

        //--------------------------------------------------------------------------------
        private string GetFromClause(CRMAction crmAction, string masterTable, bool bDelta)
        {
            string rawFromClause = bDelta ? crmAction.IncrFrom : crmAction.From;

            if (bDelta && !rawFromClause.ToUpper().Contains("DS_SYNCHRONIZATIONINFO"))
            {
                if (!string.IsNullOrEmpty(masterTable))
                    return $"{rawFromClause} {GetDeltaFromClause(masterTable)}";
                else
                {
                    return $"{rawFromClause} {GetDeltaFromClause(InferMasterTable(rawFromClause))}";
                }
            }
            else
            {
                return rawFromClause;
            }
                
        }

        private string GetDeltaWhereClause(string masterTable, bool bDelta, bool bIncludeAND, bool bIncludeWhere)
        {
            string dSPart = string.Empty;
            if (bDelta)
            {
                dSPart = $"({masterTable.TrimAndAddDot()}TBModified > DS_SynchronizationInfo.TBModified OR DS_SynchronizationInfo.TBModified IS NULL)";
                if (bIncludeWhere)
                    dSPart = $" WHERE {dSPart}";
                if(bIncludeAND)
                    dSPart = $"{dSPart} AND ";
            }

            return dSPart;
        }

        ///<summary>
        /// Prima faccio la query per la master action e riempio il suo datatable
        /// Poi per ognuno eseguo le query delle eventuali subaction e riempio tutti i DataTable figli necessari
        /// Alla fine ho una lista di DTValuesToImport da dare in pasto all'unparser
        ///</summary>
        //--------------------------------------------------------------------------------
        private List<DTValuesToImport> QueryValuesToImport(CRMAction crmAction, string docTBGuid, bool isMassive = false, string massiveFilters = "", int offset = 0, int recordsATime = 500, bool bDelta = false)
        {
            if (string.IsNullOrWhiteSpace(crmAction.Select) || string.IsNullOrWhiteSpace(crmAction.From))
                return null;

            // compongo SELECT + FROM
            string query = "SELECT " + crmAction.Select + " FROM " + GetFromClause(crmAction, crmAction.MasterTable, bDelta);

            // se NON sono nella massiva aggiungo la WHERE con il TBGuid in testa
            if (!isMassive)
            {
                query += string.Format(" WHERE {0}TBGuid = '{1}'",
                                       crmAction.MasterTable.TrimAndAddDot(),
                                       docTBGuid);
                if (!string.IsNullOrWhiteSpace(crmAction.Where))
                    query += " AND " + crmAction.Where;
            }
            else
            {
                // se sono nella massiva
                if (!string.IsNullOrWhiteSpace(massiveFilters))
                {
                    // se ho dei filtri li aggiungo, e poi in coda metto la MassiveWhere oppure la Where se esistono
                    query += $" WHERE {GetDeltaWhereClause(crmAction.MasterTable, bDelta, true, false)}" + massiveFilters;

                    if (!string.IsNullOrWhiteSpace(crmAction.MassiveWhere))
                        query += " AND " + (bDelta ? crmAction.IncrWhere : crmAction.MassiveWhere);
                    else
                        if (!string.IsNullOrWhiteSpace(crmAction.Where))
                        query += " AND " + crmAction.Where;
                }
                else
                {
                    // se NON ho filtri metto la MassiveWhere oppure la Where se esistono
                    if (!string.IsNullOrWhiteSpace(crmAction.MassiveWhere))
                        query += $" WHERE {GetDeltaWhereClause(crmAction.MasterTable, bDelta, true, false)}" + (bDelta ? crmAction.IncrWhere : crmAction.MassiveWhere);
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(crmAction.Where))
                            query += $" WHERE {GetDeltaWhereClause(crmAction.MasterTable, bDelta, true, false)}" + crmAction.Where;
                        else
                            query += $" {GetDeltaWhereClause(crmAction.MasterTable, bDelta, false, true)}";
                    }
                }
            }

            // se massiva devo dividere per blocchi di record
            if (isMassive)
            {
                string orderbykeys = string.Empty;
                foreach (CRMField field in crmAction.Fields)
                {
                    if (field.Key)
                        orderbykeys += field.Target + ",";
                }

                if (!string.IsNullOrWhiteSpace(orderbykeys))
                {
                    orderbykeys = orderbykeys.Remove(orderbykeys.Length - 1); // rimuovo la virgola finale

                    query = string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY {0}) rn FROM ({1}) t) q WHERE rn BETWEEN {2} AND {2} + {3} - 1",
                                    orderbykeys, query, offset, recordsATime);
                }

                if (crmAction.BaseAction && !bDelta)
                    query = string.Format("SELECT * FROM ({0}) k LEFT JOIN DS_Transcoding ON TBGuid = DocTBGuid AND ERPTableName = '{1}' WHERE DocTBGuid IS NULL", query, crmAction.ActionName);
            }

            // istanzio un dataTable di appoggio con tutte le righe estratte dalla query sul master
            DataTable masterRecDt = null;
            List<DTValuesToImport> dtToImportList = new List<DTValuesToImport>(); // lista dei master+slave da importare

            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                // eseguo la query sul database di ERP e con un DataAdapter fillo i dati in un datatable
                using (TBDataAdapter sda = new TBDataAdapter(query, myConnection))
                {
                    masterRecDt = new DataTable(crmAction.ActionName);
                    sda.Fill(masterRecDt);
                }

                // se la query non estrae nulla nel master continuo
                if (masterRecDt == null || masterRecDt.Rows.Count == 0)
                    return dtToImportList;

                // per ogni riga che identifica un master (ovvero la testa di un documento)
                // vado a riempire le righe specifiche per ogni subaction definita
                foreach (DataRow masterRow in masterRecDt.Rows)
                {
                    // creo il DataTable di un master
                    DataTable dtMasterAction = masterRecDt.Clone();
                    dtMasterAction.ImportRow(masterRow);

                    DTValuesToImport dtToImport = new DTValuesToImport(dtMasterAction);

                    // gestione SubActions (devo eseguire le singole query e riempire i vari DataTable)
                    foreach (CRMAction subAction in crmAction.Subactions)
                    {
                        List<string> values = new List<string>();

                        string subquery = "SELECT " + subAction.Select + " FROM " + subAction.From;

                        if (!string.IsNullOrWhiteSpace(subAction.Where))
                            subquery += " WHERE " + subAction.Where;

                        // se sono stati specificati degli attributi nella where li vado a mettere in una lista
                        for (int i = 0; i < subAction.SubactionsParams.Count; i++)
                            values.Add(dtMasterAction.Rows[0][subAction.SubactionsParams[i]].ToString().Replace("'", "''"));

                        if (values.Count > 0)
                            subquery = string.Format(subquery, values.ToArray());

                        // eseguo la query sul database di ERP e con un SqlDataAdapter fillo i dati in un datatable
                        using (TBDataAdapter sda = new TBDataAdapter(subquery, myConnection))
                        {
                            DataTable dtSubAction = new DataTable(subAction.ActionName);
                            sda.Fill(dtSubAction);

                            //TODO: se dtSubAction non ha righe ha senso aggiungerlo cmq alla dtToImport.SlavesDtList???

                            // memorizzo il DataTable della tabella slave
                            dtToImport.SlavesDtList.Add(dtSubAction);
                        }
                    }

                    dtToImportList.Add(dtToImport); // aggiungo alla lista il master correntemente analizzato
                }
            }
            catch (TBException ex)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "CRMInfinitySynchroProvider.QueryValuesToImport", crmAction.ActionName);
                Debug.WriteLine(ex.Message);
                return null;
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "CRMInfinitySynchroProvider.QueryValuesToImport", crmAction.ActionName);
                Debug.WriteLine(e.Message);
                return null;
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return dtToImportList;
        }

        /// <summary>
        /// Method extracts all TBGuids with e specific namespace that failed during the massive export operation
        /// </summary>
        /// <returns>List<string></returns>
        //--------------------------------------------------------------------------------
        private List<ActionToExport> GetTBGuidsForRecovery()
        {
            List<ActionToExport> tbguids = new List<ActionToExport>();

            string ActionsLogQuery = string.Format(@"SELECT DISTINCT DocTBGuid, DocNamespace
                                                    FROM    [DS_SynchronizationInfo] Join [DS_ActionsQueue] on [DS_SynchronizationInfo].DocNamespace=[DS_ActionsQueue].ActionName
                                                        AND [DS_ActionsQueue].ProviderName=[DS_SynchronizationInfo].ProviderName
                                                        AND [DS_ActionsQueue].SynchDirection=[DS_SynchronizationInfo].SynchDirection
                                                        AND [DS_ActionsQueue].SynchStatus = [DS_SynchronizationInfo].SynchStatus
                                                    WHERE   [DS_ActionsQueue].SynchDirection={0}
                                                        AND [DS_ActionsQueue].SynchStatus={1}
                                                        AND [DS_ActionsQueue].ProviderName='{2}'", (int)SynchroDirectionType.Outbound, (int)SynchroStatusType.Error, ProviderName);

            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                using (TBCommand myCommand = new TBCommand(ActionsLogQuery, myConnection))
                {
                    using (IDataReader myReader = myCommand.ExecuteReader())
                    {
                        while (myReader.Read())
                        {
                            ActionToExport action = new ActionToExport(myReader["DocNamespace"].ToString());
                            action.TBGuid.Add(myReader["DocTBGuid"].ToString());
                            tbguids.Add(action);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "CRMInfinitySynchroProvider.GetTBGuidsForRecovery");
                return null;
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return tbguids;
        }

        ///<summary>
        /// Richiama il parser del xmlString corrispondente alla action passata come parametro
        /// (ha lo stesso nome + estensione xml)
        /// parametro onlymassive solo per la synch massiva, evita di fare il controllo di esistenza
        /// del file per le azioni solo per la massiva (che non hanno un doc. gestionale associato)
        ///</summary>
        //--------------------------------------------------------------------------------
        private CRMActionInfo ParseAction(string action, bool onlymassive = false)
        {
            ActionParser actionParser = new ActionParser();
            CRMActionInfo crmActionForERP = null;

            string actionFullPath = string.Empty;
            bool isValidAction = true;

            // prima cerco l'action per ERP e creo la struttura CRMInfo
            if (SynchroFilesActionsFolderDict.TryGetValue(DatabaseLayerConsts.ERPSignature, out actionFullPath))
            {
                actionFullPath = Path.Combine(actionFullPath, action + ".xml");

                // controllo che il file esista e anche se il file di action e' stato correttamente dichiarato nel file di profilo
                if (File.Exists(actionFullPath))
                {
                    if (!onlymassive)
                        isValidAction = IsValidActionForAddOnApp(action, DatabaseLayerConsts.ERPSignature);

                    if (isValidAction)
                    {
                        try
                        {
                            if (actionParser.ParseFile(actionFullPath))
                                crmActionForERP = actionParser.CRMActionsInfo;
                        }
                        catch (Exception ex)
                        {
                            LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "CRMInfinitySynchroProvider.ParseAction");
                        }
                    }
                }
                // se il file non esiste procedo cmq, perche' l'action potrebbe essere legata ad un documento del solo verticale
            }

            // scorro i file per le altre applicazioni, skippando l'applicazione ERP perche' l'ho gia' analizzata
            foreach (KeyValuePair<string, string> kvp in SynchroFilesActionsFolderDict)
            {
                isValidAction = true;

                if (string.Compare(kvp.Key, DatabaseLayerConsts.ERPSignature, StringComparison.InvariantCultureIgnoreCase) == 0)
                    continue;

                actionFullPath = Path.Combine(kvp.Value, action + ".xml");

                // controllo che il file esista e anche se il file di action e' stato correttamente dichiarato nel file di profilo
                if (File.Exists(actionFullPath))
                {
                    if (!onlymassive)
                        isValidAction = IsValidActionForAddOnApp(action, kvp.Key);

                    if (isValidAction)
                    {
                        try
                        {
                            // se crmActionForERP e' diversa da NULL significa che sono in append e quindi devo aggiungere le info all'azione esistente
                            actionParser.ParseFile(actionFullPath, crmActionForERP);
                        }
                        catch (Exception ex)
                        {
                            LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "CRMInfinitySynchroProvider.ParseAction");
                            continue;
                        }
                    }
                }
            }

            return (crmActionForERP != null) ? crmActionForERP : actionParser.CRMActionsInfo;
        }

        ///<summary>
        /// Dato il nome di un file di un'action e l'addonapp di appartenenza, va a controllare
        /// che il file sia stato dichiarato nel corrispondente SynchroProfiles.xml
        /// (per evitare il caricamento di file cadaveri nei folder delle Actions)
        ///</summary>
        //--------------------------------------------------------------------------------
        private bool IsValidActionForAddOnApp(string action, string addOnApp)
        {
            string synchProfilesPath = string.Empty;
            if (SynchroProfilesFileDict.TryGetValue(addOnApp, out synchProfilesPath))
            {
                SynchroProfileParser spp = new SynchroProfileParser();
                if (
                    spp.ParseFile(synchProfilesPath, addOnApp)
                    &&
                    spp.IsValidActionForAddOnApp(action)
                    )
                    return true;
            }
            return false;
        }

        ///<summary>
        /// ritorna la struttura CRMActionInfo per la action passata come parametro
        /// il parse del xmlString relativo viene fatto solo se necessario
        ///</summary>
        //--------------------------------------------------------------------------------
        private CRMActionInfo GetActionByName(string action)
        {
            CRMActionInfo crmInfo = null;

            if (!crmActionsInfoDict.TryGetValue(action, out crmInfo))
            {
                // parse del xmlString con le info dell'azione
                crmInfo = ParseAction(action);
                if (crmInfo != null)
                    crmActionsInfoDict.Add(action, crmInfo);
            }

            return crmInfo;
        }

        ///<summary>
        /// esegue il parse del xmlString per la action passata come parametro
        /// visto che nel xmlString potrebbero essere state descritte piu' azioni,
        /// ritorna solo l'oggetto dell'azione specifica passata come secondo parametro
        /// (utilizzata nel caso di sincronizzazione massiva)
        ///</summary>
        //--------------------------------------------------------------------------------
        private CRMActionInfo GetCRMActionByName(ActionToMassiveSync atms)
        {
            CRMActionInfo crmInfo = null;

            if (!crmActionsInfoDict.TryGetValue(atms.File, out crmInfo))
            {
                // parse del xmlString con le info dell'azione
                crmInfo = ParseAction(atms.File, atms.OnlyMassive);
                if (crmInfo != null)
                    crmActionsInfoDict.Add(atms.File, crmInfo);
            }

            if (crmInfo != null)
            {
                foreach (CRMAction crmAction in crmInfo.Actions)
                {
                    if (string.Compare(crmAction.ActionName, atms.ActionName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        CRMActionInfo crmI = new CRMActionInfo();
                        crmI.Actions.Add(crmAction);
                        return crmI;
                    }
                }
            }

            return null;
        }

        ///<summary>
        /// Nella sincronizzazone massiva, ad esempio, le operazioni si scrittura su db che verranno svolte sono potenzialmente molto numerose.
        /// L'intento è quello di aprire una TBConnection una sola volta e non che non venga aperta e chiusa per ogni singola operazione.
        ///</summary>
        //--------------------------------------------------------------------------------
        private bool TryManageOpenDBConnection(out bool isMyTBConnection, out bool isNewTBConnectionFromTranscMng)
        {
            try
            {
                TBConnection tbConnection = OpenDbConnection(out isMyTBConnection);
                TranscodingMng.SetDbConnection(tbConnection);
                TranscodingMng.OpenDbConnection(connectionStringManager.CompanyConnectionString, out isNewTBConnectionFromTranscMng);
                return true;
            }
            catch (Exception e)
            {
                LogWriter?.WriteToLog(CompanyName, ProviderName, $"Message: {e.Message}.{Environment.NewLine}InnerMessage:{e.InnerException?.Message}", "CRMInfinitySynchroProvider.TryManageOpenDBConnection", ProviderName);
                isMyTBConnection = false;
                isNewTBConnectionFromTranscMng = false;
                return false;
            }
        }

        ///<summary>
        /// Chiudo esplicitamente la connessione.
        /// Al di fuori di questo processo l'apertura e la chiusura della connesione continuano ad essere delegate ai singoli metodi.
        ///</summary>
        //--------------------------------------------------------------------------------
        private void ManageCloseDBConnection(bool isMyTBConnection, bool isNewTBConnectionFromTranscMng)
        {
            TranscodingMng.CloseDbConnection(isNewTBConnectionFromTranscMng);
            CloseDbConnection(isMyTBConnection);
        }

        ///<summary>
        /// Metodo richiamato per l'esportazione massiva da Mago a Infinity
        ///</summary>
        //--------------------------------------------------------------------------------
        internal override void ExecuteMassiveExport(List<MassiveExportData> medList, DateTime startSynchroDate, bool isRecovery = false, bool bDelta = false)
        {
            Abort = false;

            LogWriter.WriteToLog(CompanyName, ProviderName, "Started ExecuteMassiveExport " + (isRecovery ? "for Recovery" : ""), "CRMInfinitySynchroProvider.ExecuteMassiveExport", ProviderName);

            // carichiamo tutti le azioni ordinate dentro il xmlString SynchroMassiveProfiles.xml
            Dictionary<string, string> massiveProfileDictionary = BasePathFinder.BasePathFinderInstance.GetSynchroMassiveProfilesFilePath(ApplicationFolderName);

            string massiveProfileFile;
            if (massiveProfileDictionary == null)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "No SynchroMassiveProfiles.xml found!", "CRMInfinitySynchroProvider.ExecuteMassiveExport", ProviderName);
                LogWriter.WriteToLog(CompanyName, ProviderName, "Finished ExecuteMassiveExport", "CRMInfinitySynchroProvider.ExecuteMassiveExport", ProviderName);
                return;
            }

            // L'intento è di aprire una TBConnection qui (e chiuderla alla fine di questo metodo) e che non venga aperta e chiusa per ogni singola operazione.
            bool isMyTBConnection = false;
            bool isNewDbConnectionFromTranscMng = false;
            if (!TryManageOpenDBConnection(out isMyTBConnection, out isNewDbConnectionFromTranscMng))
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "ExecuteMassiveExport Failed.", "CRMInfinitySynchroProvider.ExecuteMassiveExport", ProviderName);
                return;
            }

            // prima vado a ad eseguire i profili per l'applicazione ERP
            if (massiveProfileDictionary.TryGetValue(DatabaseLayerConsts.ERPSignature, out massiveProfileFile))
            {
                if (string.IsNullOrWhiteSpace(massiveProfileFile))
                {
                    LogWriter.WriteToLog(CompanyName, ProviderName, "No SynchroMassiveProfiles.xml for ERP found! Unable to proceed!", "CRMInfinitySynchroProvider.ExecuteMassiveExport", ProviderName);
                    LogWriter.WriteToLog(CompanyName, ProviderName, "Finished ExecuteMassiveExport", "CRMInfinitySynchroProvider.ExecuteMassiveExport", ProviderName);

                    ManageCloseDBConnection(isMyTBConnection, isNewDbConnectionFromTranscMng);
                    return;
                }

                LogWriter.WriteToLog(CompanyName, ProviderName, "Executing Massive Export for ERP", "CRMInfinitySynchroProvider.ExecuteMassiveExport", ProviderName);
                ExecuteMassiveExportForApplication(massiveProfileFile, medList, startSynchroDate, isRecovery, bDelta);
            }

            // vado ad eseguire i profili definiti per le altre applicazioni
            foreach (KeyValuePair<string, string> kvp in massiveProfileDictionary)
            {
                if (string.Compare(kvp.Key, DatabaseLayerConsts.ERPSignature, StringComparison.InvariantCultureIgnoreCase) == 0)
                    continue;  // skippo ERP

                LogWriter.WriteToLog(CompanyName, ProviderName, string.Format("Executing Massive Export for {0}", kvp.Key), "CRMInfinitySynchroProvider.ExecuteMassiveExport", ProviderName);
                ExecuteMassiveExportForApplication(kvp.Value, medList, startSynchroDate, isRecovery, bDelta);
            }

            if (bDelta)
                UpdateSynchronizationDirtyData(startSynchroDate);

            ManageCloseDBConnection(isMyTBConnection, isNewDbConnectionFromTranscMng);

            LogWriter.WriteToLog(CompanyName, ProviderName, "Finished ExecuteMassiveExport", "CRMInfinitySynchroProvider.ExecuteMassiveExport", ProviderName);
        }


        /// <summary>
        /// Richiamato per ogni applicazione che dichiara un file per la massiva (SynchroMassiveProfiles.xml)
        ///</summary>
        //--------------------------------------------------------------------------------
        internal void ExecuteMassiveExportForApplication(string massiveProfileFile, List<MassiveExportData> medList, DateTime startSynchroDate, bool isRecovery = false, bool bDelta = false)
        {
            SynchroMassiveProfileParser smpp = new SynchroMassiveProfileParser();
            if (!smpp.ParseFile(massiveProfileFile, new LogInfo()
            {
                ProviderName = ProviderName,
                CompanyName = CompanyName,
                ProviderLogWriter = LogWriter
            }))
                return;

            // considero solo le action che hanno dei fitri
            // vado ad impostare lo stato NoSynchro a tutte le righe nella DS_SynchInfo con lo stato Synchro
            // che hanno il tbguid contenuto nella DS_Transcoding
            List<string> namespacesWithFilters = new List<string>();
            foreach (MassiveExportData med in medList)
            {
                if (string.IsNullOrWhiteSpace(med.Filters))
                    continue;
                namespacesWithFilters.Add(med.EntityName);
            }

            // TODO: la query non va bene perche' nella actionsWithFilters ho i namespace, mentre la query nella
            // DS_Transcoding vuole il nome della action
            if (namespacesWithFilters.Count > 0 && !isRecovery)
                UpdateStatusForExclude(namespacesWithFilters, SynchroStatusType.NoSynchro, SynchroStatusType.Synchro);

            //usata solo nella fase di recovery
            List<ActionToExport> tbGuids = null;

            // NELLA RECOVERY
            // compongo la lista di tbguid con namespace corrispondente dove lo stato e' Error
            // setto lo stato a toSynchro nella DS_ActionsLog e DS_SynchronizationInfo
            if (isRecovery)
            {
                tbGuids = GetTBGuidsForRecovery(); // conmpongo la lista di tbguid
                foreach (MassiveExportData med in medList)
                {
                    foreach (var tbGuid in tbGuids)
                    {
                        if (tbGuid.Name.Equals(med.EntityName))
                            InsertSynchronizationData(SynchroStatusType.ToSynchro, tbGuid.TBGuid[0], med, startSynchroDate, false);
                    }
                }
            }

            List<string> nameSpacesSynchronized = new List<string>();
            //Lista con xml da eseguire
            List<ActionToExport> xmlList = null;

            // per ogni azione andiamo a cercare il corrispondente namespace del documento nella medList
            // per estrarre i filtri (che sono letti dalla tabella DS_ActionsQueue)
            // poi andiamo a generare le stringhe xml da inviare
            foreach (ActionToMassiveSync atms in smpp.SynchroProfileInfo.Documents)
            {
                if (Abort)
                    break;

                if (IsInPause)
                {
                    while (IsInPause)
                    {
                        Thread.Sleep(5000);
                    }
                }

                int recordsATime = 500;
                bool splitRecords = false;
                bool firstInsert = false;// parametro serve per sapere se il namespace era inserito per la prima volta allora posso andare ad aggiornare lo stato a synchro. la prox volta aggiorno solo se e' errore.
                MassiveExportData myMed = GetMedFromNs(atms.Name, medList);

                string filters = (myMed != null) ? myMed.Filters : null;
                if (filters == null)
                    continue;

                CRMActionInfo crmActionInfo = GetCRMActionByName(atms);
                if (crmActionInfo == null)
                    continue;

                //LogWriter.WriteToLog(CompanyName, ProviderName, "Start elaboration for action " + atms.ActionName, "CRMInfinitySynchroProvider.ExecuteMassiveExport");
                myMed.OffSet = 1;

                LogInfo logInfo = new LogInfo()
                {
                    ProviderLogWriter = this.LogWriter,
                    CompanyName = this.CompanyName,
                    ProviderName = this.ProviderName
                };

                // ciclo per spezzare l'estrazione dei dati a blocchi di 100 record
                for (;;) // almeno una volta entra perche'
                {
                    if (IsInPause)
                    {
                        while (IsInPause)
                        {
                            Thread.Sleep(5000);
                        }
                    }
                    //LogWriter.WriteToLog(CompanyName, ProviderName, string.Format("Extracting records from {0} to {1}", myMed.OffSet.ToString(), (k * 500 + 1).ToString()), "CRMInfinitySynchroProvider.ExecuteMassiveExport");

                    xmlList = (isRecovery)
                        ? UnparseStringForRecovery(crmActionInfo, tbGuids, atms.Name, filters)
                        : UnparseStringForInsertAction(crmActionInfo, string.Empty, SynchroActionType.Massive, filters, myMed.OffSet, recordsATime, bDelta);

                    if (xmlList == null || xmlList.Count == 0)
                        break;

                    myMed.OffSet += recordsATime;

                    firstInsert = !nameSpacesSynchronized.Contains(atms.Name);
                    if (firstInsert)
                        nameSpacesSynchronized.Add(atms.Name);

                    //primo inserimento in monitor
                    if (!isRecovery && !splitRecords)
                    {
                        foreach (ActionToExport action in xmlList)
                            foreach (string tbGuid in action.TBGuid)
                            {
                                if (!string.IsNullOrWhiteSpace(tbGuid))
                                {
                                    bool bCRMEnabled = InfinityProviders.Parsers.Utilities.GetCRMEnabledForAction(
                                                                                                                        atms,
                                                                                                                        SyncroRef.InfinityData.IAFModule,
                                                                                                                        logInfo,
                                                                                                                        bIMagoActivated
                                                                                                                    );
                                    InsertSynchronizationData(
                                                                    atms.OnlyForDMS || !bCRMEnabled ? SynchroStatusType.Synchro : SynchroStatusType.ToSynchro,
                                                                    tbGuid,
                                                                    myMed,
                                                                    startSynchroDate,
                                                                    bDelta,
                                                                    atms.OnlyForDMS || !bCRMEnabled ? "Added entry for DMS" : string.Empty,
                                                                    ifExistsDoNothing: !firstInsert

                                                                ); // se ho gia sincronizzato il namespace allora vado a cumulare il risultato
                                }
                            }
                    }

                    if (atms.OnlyForDMS)
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, $"Skipping action {atms.ActionName} because is Only for DMS...", "CRMInfinitySynchroProvider.ExecuteMassiveExportForApplication");
                        continue;
                    }

                    if (!InfinityProviders.Parsers.Utilities.GetCRMEnabledForAction(atms, SyncroRef.InfinityData.IAFModule, logInfo, bIMagoActivated))
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, $"Skipping action {atms.ActionName} because is not Included in IAF Modulle {SyncroRef.InfinityData.IAFModule}...", "CRMInfinitySynchroProvider.ExecuteMassiveExportForApplication");
                        continue;
                    }

                    string message = string.Empty;
                    foreach (ActionToExport action in xmlList)
                    {
                        if (IsInPause)
                        {
                            while (IsInPause)
                            {
                                Thread.Sleep(5000);
                            }
                        }

                        int count = 0;

                        //LogWriter.WriteToLog(CompanyName, ProviderName, "Start ExecuteSyncro for action " + atms.ActionName, "CRMInfinitySynchroProvider.ExecuteMassiveExport");
                        bool res = SyncroRef.ExecuteSyncroWithoutConnect(action, out message);
                        //LogWriter.WriteToLog(CompanyName, ProviderName, "End ExecuteSyncro for action " + atms.ActionName, "CRMInfinitySynchroProvider.ExecuteMassiveExport");

                        // Se si verifica un errore di xml devo spezzare 500 record in 50 record alla volta (50*10)
                        // vado a proccessare 50 records alla volta finche non trovo il blocco con l'errore
                        //  Gli errore vanno recoverati (50 records)
                        if (!res && message.Contains("-27") && !isRecovery)
                        {
                            if (!splitRecords)
                            {
                                Debug.WriteLine("PASSED TO 50 RECORDS A TIME");
                                splitRecords = true;
                                myMed.OffSet -= recordsATime;
                                recordsATime = 50;
                                break;
                            }
                            else
                            {
                                Debug.WriteLine("PASSED TO 500 RECORDS A TIME");
                                splitRecords = false;
                                recordsATime = 500;
                            }
                        }

                        foreach (string tbGuid in action.TBGuid)
                        {
                            if (!string.IsNullOrWhiteSpace(tbGuid))
                            {
                                if (action.IsSucceeded[count])
                                {
                                    InsertSynchronizationData(SynchroStatusType.Synchro, tbGuid, myMed, startSynchroDate, bDelta, ifExistsDoNothing: !firstInsert);

                                    // per le action che possono essere disabilitate inserisco le chiavi nella tabella DS_Transcoding
                                    // in modo da poter applicare i filtri di esclusione
                                    if (!string.IsNullOrEmpty(action.Keys[count]))
                                        TranscodingMng.InsertRow(connectionStringManager.CompanyConnectionString, action.Name, action.Keys[count], action.Name, action.Keys[count], tbGuid);
                                }
                                else
                                {
                                    LogWriter.WriteToLog(CompanyName, ProviderName, myMed.EntityName, "CRMInfinitySynchroProvider.ExecuteMassiveExport" + " " + action.XmlToImport, string.IsNullOrWhiteSpace(message) ? action.ErrorMessages[count] : message);
                                    InsertSynchronizationData(SynchroStatusType.Error, tbGuid, myMed, startSynchroDate, bDelta, string.IsNullOrWhiteSpace(message) ? action.ErrorMessages[count] : message);
                                    atms.IsSynchronized = false;
                                }
                            }
                            count++;
                        }
                    }

                    // nel caso di recovery devo fare break, altrimenti esegue il ciclo for per Int32.MaxValue volte!
                    // per la procedura di recovery non spezzo per blocchi (TODO: decidere se e' accettabile)
                    if (isRecovery)
                        break;
                }
            }

            // per andare a cambiare lo stato nella DS_ActionsQueue e' necessario che tutte le occorrenze di un certo
            // namespace siano state sincronizzate con successo
            foreach (MassiveExportData med in medList)
            {
                if (Abort)
                    break;

                if (IsInPause)
                {
                    while (IsInPause)
                    {
                        Thread.Sleep(5000);
                    }
                }

                bool isSynchronized = true;

                foreach (ActionToMassiveSync atms in smpp.SynchroProfileInfo.Documents)
                {
                    if (atms.Name.Equals(med.EntityName) && !atms.IsSynchronized)
                        isSynchronized = false;
                }

                UpdateActionQueueStatus(isSynchronized ? SynchroStatusType.Synchro : SynchroStatusType.Error, med.LogId, string.Empty);
            }

            // Gestione exclude
            //List<ActionToExport> excludeXmlList = UnparseStringForExcludeAction(authenticationToken);
            //if (excludeXmlList.Count > 0)
            //{
            //    SyncroRef.ExecuteSyncroWithoutConnect(excludeXmlList);
            //    foreach (ActionToExport ati in excludeXmlList)
            //    {
            //        if (ati.IsSucceeded[0])
            //        {
            //            UpdateStatusForExclude(authenticationToken, ati.TBGuid[0]);
            //            string action = ati.Name.Contains("_DIS") ? ati.Name.Remove(ati.Name.IndexOf("_DIS"), "_DIS".Length) : ati.Name;
            //            TranscodingMng.DeleteRow(CompanyConnectionString, action, ati.Keys[0]);
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Chiamata asincrona della SynchronizeErrorsRecovery
        /// </summary>
        //--------------------------------------------------------------------------------
        internal override void SynchronizeErrorsRecoveryAsynch()
        {
            List<MassiveExportData> medList = GetFiltersForMassiveExport(true);
            if (medList == null || medList.Count == 0)
                return;

            ExecuteMassiveExport(medList, DateTime.Now, true);
        }

        ///<summary>
        /// Gestione esclusione record
        /// Estraggo tutte le righe in DS_SynchronizationInfo con lo stato NoSynchro che hanno
        /// un TBGuid corrispondente in DS_Transcoding
        ///</summary>
        //--------------------------------------------------------------------------------
        private List<ActionToExport> UnparseStringForExcludeAction()
        {
            List<ActionToExport> xmlList = new List<ActionToExport>();

            string query = @"SELECT [ERPTableName], [ERPKey], [DS_Transcoding].[DocTBGuid] AS TBGuid FROM [DS_Transcoding]
                            JOIN [DS_SynchronizationInfo] ON [DS_SynchronizationInfo].[DocTBGuid] = [DS_Transcoding].[DocTBGuid]
                            AND [DS_Transcoding].[ProviderName] = [DS_SynchronizationInfo].[ProviderName]
                            WHERE [DS_Transcoding].[ProviderName] = '{0}' AND [DS_SynchronizationInfo].[SynchStatus] = {1}";

            bool isMyTBConnection = false;

            try
            {
                TBConnection tbConnection = OpenDbConnection(out isMyTBConnection);

                using (TBCommand tbCommand = new TBCommand(tbConnection))
                {
                    tbCommand.CommandText = string.Format(query, ProviderName, (int)SynchroStatusType.NoSynchro);

                    using (IDataReader dataReader = tbCommand.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            string actionName_DIS = dataReader["ERPTableName"].ToString();

                            //if (!actionName_DIS.Equals("MAGO_SUPPLIERS") && !actionName_DIS.Equals("MAGO_CUSTOMERS"))
                             //   actionName_DIS += "_DIS";

                            string erpKey = dataReader["ERPKey"].ToString();
                            string xmlDisAction = GetDISAction(actionName_DIS, erpKey);
                            string tbGuid = dataReader["TBGuid"].ToString();

                            ActionToExport ati = new ActionToExport(actionName_DIS, xmlDisAction);
                            ati.TBGuid.Add(tbGuid);
                            ati.Keys.Add(erpKey);
                            xmlList.Add(ati);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "CRMInfinitySynchroProvider.UnparseStringForExcludeAction");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return xmlList;
        }

        //--------------------------------------------------------------------------------
        private string GetDISAction(string actionName, string keys)
        {
            return string.Format(@"<{0} applicationId=""{1}""><Add_{0} {2} FLACTIVE=""0"" /></{0}>", actionName, SyncroRef.RegisteredApp, keys);
        }

        ///<summary>
        /// Metodo richiamato per l'esportazione massiva da Mago
        /// Dato un namespace ritorna, se esiste, il corrispondente elemento MassiveExportData
        ///</summary>
        //--------------------------------------------------------------------------------
        private MassiveExportData GetMedFromNs(string nameSpace, List<MassiveExportData> medList)
        {
            foreach (MassiveExportData med in medList)
            {
                if (string.Compare(med.EntityName, nameSpace, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return med;
            }

            return null; // nel caso in cui non trovassi proprio il namespace (non dovrebbe mai accadere)
        }

        //---------------------------------------------------------------------
        private void UpdateStatusForExclude(string docTBGuid)
        {
            string updateQuery = @"UPDATE [DS_SynchronizationInfo] SET [SynchStatus] = {0}, [SynchDate] = GetDate()
                                  WHERE [DocTBGuid] = '{1}' AND [ProviderName] = '{2}';
                                  UPDATE [DS_ActionsLog] SET [SynchStatus] = {0}
                                  WHERE [DocTBGuid] = '{1}' AND [ProviderName] = '{2}' AND LogId=(SELECT MAX(LogId) FROM [DS_ActionsLog] WHERE [DocTBGuid] = '{1}' AND [ProviderName] = '{2}')";

            bool isMyTBConnection = false;

            try
            {
                TBConnection tbConnection = OpenDbConnection(out isMyTBConnection);

                using (TBCommand tbCommand = new TBCommand(tbConnection))
                {
                    tbCommand.CommandText = string.Format(updateQuery, (int)SynchroStatusType.Excluded, docTBGuid, ProviderName);
                    tbCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "CRMInfinitySynchroProvider.UpdateStatusForExclude");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        ///<summary>
        /// Utilizzato nel cambio dei filtri della massiva di Infinity
        /// Aggiorna lo stato nella tabella DS_SynchronizationInfo come NoSynchro per tutte le azioni
        /// per le quali e' stato specificato un filtro e che sono presenti nella tabella di transcodifica
        /// (La query non e' molto performante, ma x ora lasciamo cosi)
        ///</summary>
        //---------------------------------------------------------------------
        private void UpdateStatusForExclude(List<string> namespacesList, SynchroStatusType newSynchroStatus, SynchroStatusType oldSynchroStatus)
        {
            string updateQuery = @"UPDATE [DS_SynchronizationInfo] SET [SynchStatus] = {2}, [SynchDate] = GetDate()
                                    WHERE [DS_SynchronizationInfo].[DocTBGuid] IN
                                    (SELECT [DS_SynchronizationInfo].[DocTBGuid] FROM [DS_SynchronizationInfo]
                                    INNER JOIN [DS_Transcoding] ON [DS_Transcoding].[DocTBGuid] = [DS_SynchronizationInfo].[DocTBGuid] AND
                                    [DS_Transcoding].[ProviderName] = [DS_SynchronizationInfo].[ProviderName] AND [DS_SynchronizationInfo].[SynchStatus] = {3}
                                    JOIN
                                    (SELECT DISTINCT [DocTBGuid], [DocNamespace] FROM [DS_ActionsLog]) k ON k.[DocTBGuid] = [DS_SynchronizationInfo].[DocTBGuid]
                                    WHERE [DS_Transcoding].[ProviderName] = '{0}' AND ({1}))";

            string filterQuery = string.Empty;
            foreach (string ns in namespacesList)
            {
                if (!string.IsNullOrWhiteSpace(filterQuery))
                    filterQuery += " OR ";
                filterQuery += string.Format("k.[DocNamespace] = '{0}'", ns);
            }

            bool isMyTBConnection = false;

            try
            {
                TBConnection tbConnection = OpenDbConnection(out isMyTBConnection);

                using (TBCommand tbCommand = new TBCommand(tbConnection))
                {
                    tbCommand.CommandText = string.Format(updateQuery, ProviderName, filterQuery, (int)newSynchroStatus, (int)oldSynchroStatus);
                    tbCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "CRMInfinitySynchroProvider.UpdateStatusForExclude");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        //---------------------------------------------------------------------
        public override bool TestProviderParameters(string url, string username, string password, bool skipCrtValidation, string parameters, out string message)
        {
            message = string.Empty;
            return SyncroRef.TestProviderParameters(url, username, password, skipCrtValidation, parameters, out message);
        }

        //---------------------------------------------------------------------
        public override bool SetProviderParameters(string authenticationToken, string url, string username, string password, bool skipCrtValidation, string parameters, string iafmodule, out string message)
        {
            message = string.Empty;
            var iToken = GetLoginManager().GetIToken(authenticationToken);
            return SyncroRef.SetProviderParameters(iToken, url, username, password, skipCrtValidation, parameters, iafmodule, out message);
        }

        //--------------------------------------------------------------------------------
        public override void SynchronizeInbound(string authenticationToken, string loginName, string loginPassword, bool loginWindowsAuthentication, string companyName, string companyConnectionString)
        {
            //se la massiva e' stata lanciata scarto l'operazione e riprovo tra n minuti (predefiniti on web.config)
            if (SharedLock.Instance.IsMassiveSynchronizing)
                return;

            TbServices tbServices = new TbServices();

            // 1) richiamare l'ExecuteSynchro con le azioni di OUT
            // MAGO_CUSTOMERS_OUT, MAGO_BRANCHES_OUT, MAGO_SALEORDERS_OUT
            // 2) parsare ogni risposta (che potrebbe contenere piu' record)
            // 3) generare l'xml da passare alla SetData
            // 4) fare la SetData (ricordarsi di inserire riga in ds_transcoding di MAGO_CUST!)
            // 5) analizzare la risposta e scrivere l'xml di commit o rollback e inviarlo al CRM

            LogWriter.WriteToLog(CompanyName, ProviderName, "started MAGO_CUSTOMERS_OUT", "CRMInfinitySynchroProvider.SynchronizeInbound");
            List<ActionToImport> newActionsList = new List<ActionToImport>();

            // Importazione nuovi clienti
            string customersFromCRM = GetXmlFromCRM("MAGO_CUSTOMERS_OUT");
            newActionsList = SynchroRespParser.GetCustomersString(customersFromCRM);

            ExecuteSetData(authenticationToken, tbServices, newActionsList);

            // inerimento synchronization info
            // inserimento in transcoding solo per MAGO_CUSTOMERS where DoRollback = false;
            foreach (var action in newActionsList)
            {
                if (action.DoRollback)
                    continue;

                string keys = string.Empty;
                foreach (var key in action.InfinityKeys)
                {
                    if (!key.Contains("APPREG"))
                        keys += key.Replace("=", "=\"") + "\" ";
                }

                TranscodingMng.InsertRow(companyConnectionString, "MAGO_CUSTOMERS", keys, "MAGO_CUSTOMERS", keys, action.TBGuid);
                InsertSynchronizationData(SynchroStatusType.Synchro, action, "Document.ERP.CustomersSuppliers.Documents.Customers", SynchroDirectionType.Inbound, SynchroActionType.Insert, companyConnectionString, DateTime.Now, false);
            }

            LogWriter.WriteToLog(CompanyName, ProviderName, "finished MAGO_CUSTOMERS_OUT", "CRMInfinitySynchroProvider.SynchronizeInbound");

            // Importazione nuove sedi (al momento sembra ritornare solo la primaria, peraltro gia' ritornata dalla MAGO_CUSTOMERS_OUT)
            //newActionsList.Clear();
            //string branchesFromCRM = GetXmlFromCRM("MAGO_BRANCHES_OUT");
            //            string branchesFromCRM=@"<?xml version=""1.0"" encoding=""iso-8859-1""?><ExecuteSyncroResult>
            //<Process id=""586701242"" AtomicLevel=""ENTITY"" GenericResult=""ok"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
            //<MAGO_BRANCHES_OUT Result=""(Select):0 Entities""/>
            //</Process>
            //<MAGO_BRANCHES_OUT applicationId=""MAGO"">
            //<Add_MAGO_BRANCHES_OUT DPTIPSOG_K=""FOR"" DPCODSOG_K=""0006"" DPCODCOM_K=""000000000000062"" DPTIPCON=""2"" DPTIPAGE=""AGE""
            //DPTIPVET=""VET"" DPCODSED_K=""0000000020"" Tipsed=""LEG"" Local=""GENOVA"" Cap=""16100"" Provin=""GE"" Address=""via 2""
            //Civnum="""" DPCODPAG="""" DPGIOFIS=""0"" DPVALFIS=""0"" DPCODCON="""" DPCODBAN="""" CCNumcor="""" DPZONCOM="""" DPZONSTA="""" DPCODMER=""""
            //            DPCODAGE="""" DPCLACHF="""" DPMODTRA="""" DPFLCODI=""0"" DPPREBOL=""0"" DPCODVE1="""" DPCODVE2="""" DPCODVE3="""" DPCODPOR=""""
            //            DPCODSPZ="""" DPTIPPAL="""" DPTCONTA="""" DPCOMMAR="""" DPCENCOS="""" DPCENPRO="""" Dessed=""sede 2"" Primary=""N"" Prephone=""""
            //            Phone="""" Mail=""""></Add_MAGO_BRANCHES_OUT></MAGO_BRANCHES_OUT></ExecuteSyncroResult>";

            //newActionsList = SynchroRespParser.GetBranchesString(branchesFromCRM);
            //ExecuteSetData(loginMng, tbServices, newActionsList);

            // Importazione nuovi ordini clienti

            bool execute_MAGO_SALEORDERS_OUT = !bIMagoActivated || (SyncroRef.InfinityData.IAFModule.Equals(Utilities.PROVIDER_IAF_CONFIGURATION_SALES) ||
                                                                    SyncroRef.InfinityData.IAFModule.Equals(Utilities.PROVIDER_IAF_CONFIGURATION_MARKETING) ||
                                                                    SyncroRef.InfinityData.IAFModule.Equals(Utilities.PROVIDER_IAF_CONFIGURATION_SALES_AND_MARKETING));


            if (execute_MAGO_SALEORDERS_OUT)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "started MAGO_SALEORDERSERP_OUT", "CRMInfinitySynchroProvider.SynchronizeInbound");
                newActionsList.Clear();
                string ordersFromCRM = GetXmlFromCRM("MAGO_SALEORDERSERP_OUT");
                newActionsList = SynchroRespParser.GetOrdersStringErp(ordersFromCRM, authenticationToken);
                ExecuteSetData(authenticationToken, tbServices, newActionsList);
                // TODO: valutare se inserire in DS_SyncInfo e quindi agganciare il CD anche al DE degli ordini (inibendo pero' le successive notify al CRM)

                foreach (var action in newActionsList)
                {
                    if (action.DoRollback)
                        continue;

                    string keys = string.Empty;
                    foreach (var key in action.InfinityKeys)
                    {
                        if (!key.Contains("APPREG"))
                            keys += key.Replace("=", "=\"") + "\" ";
                    }

                    InsertSynchronizationData(SynchroStatusType.Synchro, action, "Document.ERP.CustomersSuppliers.Documents.SaleOrders", SynchroDirectionType.Inbound, SynchroActionType.Insert, companyConnectionString, DateTime.Now, false);
                }

                LogWriter.WriteToLog(CompanyName, ProviderName, "finished MAGO_SALEORDERSERP_OUT", "CRMInfinitySynchroProvider.SynchronizeInbound");
            }
            else
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, $"MAGO_SALEORDERSERP skipped because of IAFMODULES configuration: {SyncroRef.InfinityData.IAFModule}", "CRMInfinitySynchroProvider.SynchronizeInbound");
            }



            if (execute_MAGO_SALEORDERS_OUT)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "started MAGO_SALEORDERS_OUT", "CRMInfinitySynchroProvider.SynchronizeInbound");
                newActionsList.Clear();
                string ordersFromCRM = GetXmlFromCRM("MAGO_SALEORDERS_OUT");
                newActionsList = SynchroRespParser.GetOrdersString(ordersFromCRM, authenticationToken);
                ExecuteSetData(authenticationToken, tbServices, newActionsList);
                // TODO: valutare se inserire in DS_SyncInfo e quindi agganciare il CD anche al DE degli ordini (inibendo pero' le successive notify al CRM)

                foreach (var action in newActionsList)
                {
                    if (action.DoRollback)
                        continue;

                    string keys = string.Empty;
                    foreach (var key in action.InfinityKeys)
                    {
                        if (!key.Contains("APPREG"))
                            keys += key.Replace("=", "=\"") + "\" ";
                    }

                    InsertSynchronizationData(SynchroStatusType.Synchro, action, "Document.ERP.CustomersSuppliers.Documents.SaleOrders", SynchroDirectionType.Inbound, SynchroActionType.Insert, companyConnectionString, DateTime.Now, false);
                }

                LogWriter.WriteToLog(CompanyName, ProviderName, "finished MAGO_SALEORDERS_OUT", "CRMInfinitySynchroProvider.SynchronizeInbound");
            }
            else
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, $"MAGO_SALEORDERS_OUT skipped because of IAFMODULES configuration: {SyncroRef.InfinityData.IAFModule}", "CRMInfinitySynchroProvider.SynchronizeInbound");
            }
        }

        //esegue setdata e fa rollback o commit
        //--------------------------------------------------------------------------------
        private void ExecuteSetData(string authenticationToken, TbServices tbServices, List<ActionToImport> newActionsList)
        {
            foreach (ActionToImport action in newActionsList)
            {
                if (!action.DoRollback && SetData(tbServices, authenticationToken, action))
                    Commit(action);

                if (action.DoRollback) // in questo modo se la Commit fallisce proviamo ad eseguire Rollback (giusto?)
                    Rollback(action);
            }
        }

        // compone xml di commit e esegue la commitAction
        //--------------------------------------------------------------------------------
        private void Commit(ActionToImport action)
        {
            string commitXml = SynchroRespParser.GetCommit(action, SyncroRef.RegisteredApp);

            if (!SyncroRef.Commit(commitXml, action).Equals("0"))
                LogWriter.WriteToLog(CompanyName, DatabaseLayerConsts.CRMInfinity, string.Format("Error during the Commit operation Action: {0} Keys: {1}", action.ActionName, string.Join(", ", action.InfinityKeys)), "CRMInfinitySynchroProvider.Commit");
        }

        //esegua la rollbackEntity
        //--------------------------------------------------------------------------------
        private void Rollback(ActionToImport action)
        {
            if (!SyncroRef.Rollback(action).Equals("0"))
                LogWriter.WriteToLog(CompanyName, DatabaseLayerConsts.CRMInfinity, string.Format("Error during the Rollback operation Action: {0} Keys: {1}", action.ActionName, string.Join(", ", action.InfinityKeys)), "CRMInfinitySynchroProvider.Rollback ");
        }

        //--------------------------------------------------------------------------------
        private string GetXmlFromCRM(string actionOUT)
        {
            string getOUTXml = @"<?xml version=""1.0""?>
                                 <{0} applicationId=""{1}"">
                                    <parameters>
                                        <pAppreg>{1}</pAppreg>
                                    </parameters>
                                 </{0} >";

            string getXml = string.Format(getOUTXml, actionOUT, SyncroRef.RegisteredApp);
            
            return SyncroRef.ExecuteSyncroWithoutConnect(actionOUT, getXml);
        }

        /// Restituisce XML Corrispondente all'action e all'ID passati
        ///</summary>
        //---------------------------------------------------------------------
        public string GetXML(string tbguid, string nameSpace, string action, string authenticationToken)
        {
            BaseObjectToSynch infoToSynch = new BaseObjectToSynch();

            infoToSynch.DocTBGuid = tbguid;
            infoToSynch.DocNamespace = nameSpace;
            infoToSynch.ActionType = DSUtils.GetSyncroActionType(Convert.ToInt32(31588352));
            infoToSynch.SynchStatus = DSUtils.GetSynchroStatusType(Convert.ToInt32(31457280));
            infoToSynch.ActionData = "";

            if (string.IsNullOrWhiteSpace(infoToSynch.DocTBGuid))
                return string.Empty;

            ActionToSynch actionToSynch = new ActionToSynch(infoToSynch);

            if (actionToSynch == null) // almeno l'action corrente da eseguire deve essere presente, altrimenti non procedo
                return string.Empty;

            // per l'azione corrente di cui ci e' arrivata notifica
            // vado a caricare le azioni da eseguire dal xmlString dei profili di sincronizzazione (potrebbero essere piu' d'una)
            // e genero i relativi xmlString xml con gli appositi unparser
            if (!FillActionsToImport(actionToSynch))
                return string.Empty;

            foreach (var xmlObj in actionToSynch.ActionsToExecute)
            {
                if (xmlObj.Name == action)
                    return xmlObj.XmlToImport;
            }

            return string.Empty;
        }

        //--------------------------------------------------------------------------------
        private bool SetData(TbServices tbServices, string authenticationToken, ActionToImport actionToImport)
        {
            try
            {
                int result = 0;

                string setDataResponse = string.Empty;
                // importo in Mago ogni xml generato il precedenza
                result = tbServices.SetData(authenticationToken, actionToImport.MagoXml, DateTime.Now, 0, false, out setDataResponse);

                //se TBServices non e' disponibile esco
                if (result == -1)
                {
                    Debug.WriteLine("Error in SetData: " + setDataResponse);
                    LogWriter.WriteToLog(CompanyName, ProviderName, "Impossible to create TBServices", "CRMInfinitySynchroProvider.SetData(CreateTB)", setDataResponse);
                    actionToImport.DoRollback = true;
                    return false;
                }

                if (result == 1)
                {
                    //InsertSynchronizationInfo(SynchroStatusType.Error, Guid.Empty.ToString(), dataInfo.Namespace, 0, SynchroActionType.Insert, SynchroDirectionType.Inbound, dsItem.companyConnectionString);
                    LogWriter.WriteToLog(CompanyName, ProviderName, string.Format("Error in SetData. Impossible to insert document DS_ActionsQueue.EntityName {0}): ", actionToImport.ActionName), "CRMInfinitySynchroProvider.SetData", setDataResponse);
                    actionToImport.DoRollback = true;
                    return false;
                }

                if (string.IsNullOrWhiteSpace(actionToImport.MagoKey))
                    actionToImport.MagoKey = SynchroRespParser.GetKeyFromMago(setDataResponse, actionToImport.ActionName);
                actionToImport.TBGuid = SynchroRespParser.GetTBGuidForMago(setDataResponse, actionToImport.ActionName);
                return true;

                //InsertSynchronizationInfo(SynchroStatusType.Synchro, dataInfo.TBGuid, dataInfo.Namespace, 0, SynchroActionType.Insert, SynchroDirectionType.Inbound, dsItem.companyConnectionString);
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "CRMInfinitySynchroProvider.SetData");
                return false;
            }
        }

        //---------------------------------------------------------------------
        public byte[] GetXsd(bool specificFileRequest, object[] parameters)
        {
            return infinitySyncro.GetXsd(specificFileRequest, parameters);
        }

        //---------------------------------------------------------------------
        public override bool CreateExternalServer(string extservername, string connstr, out string message)
        {
            message = string.Empty;
            return SyncroRef.CreateExternalServer(extservername, connstr, out message);
        }

        //---------------------------------------------------------------------
        public override bool CheckCompaniesToBeMapped(out string companylist, out string message)
        {
            message = string.Empty;
            return SyncroRef.CheckCompaniesToBeMapped(out companylist, out message);
        }

        //---------------------------------------------------------------------
        public override bool MapCompany(string appreg, int magocompany, string infinitycompany, string taxid, out string message)
        {
            message = string.Empty;
            return SyncroRef.MapCompany(appreg, magocompany, infinitycompany, taxid, out message);
        }

        //---------------------------------------------------------------------
        public override bool UploadActionPackage(string actionpath, out string message)
        {
            message = string.Empty;
            return SyncroRef.UploadActionPackage(actionpath, out message);
        }

        ///<summary>
        /// Metodo richiamato per l'esportazione massiva da Mago a Infinity
        ///</summary>
        //--------------------------------------------------------------------------------
        internal override void ExecuteMassiveValidation(bool bCheckFK, bool bCheckXSD, string filters, string serializedTree, int workerId)
        {
            LogWriter.WriteToLog(CompanyName, ProviderName, "Started ExecuteMassiveValidation ", "CRMInfinitySynchroProvider.ExecuteMassiveValidation", ProviderName);

            // carichiamo tutti le azioni ordinate dentro il xmlString SynchroMassiveProfiles.xml
            Dictionary<string, string> massiveProfileDictionary = BasePathFinder.BasePathFinderInstance.GetSynchroMassiveProfilesFilePath(ApplicationFolderName);

            string massiveProfileFile;
            if (massiveProfileDictionary == null)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "No SynchroMassiveProfiles.xml found!", "CRMInfinitySynchroProvider.ExecuteMassiveValidation", ProviderName);
                LogWriter.WriteToLog(CompanyName, ProviderName, "Finished ExecuteMassiveValidation", "CRMInfinitySynchroProvider.ExecuteMassiveValidation", ProviderName);
                return;
            }

            // L'intento è di aprire una TBConnection qui (e chiuderla alla fine di questo metodo) e che non venga aperta e chiusa per ogni singola operazione.
            bool isMyTBConnection = false;
            bool isNewDbConnectionFromTranscMng = false;
            if (!TryManageOpenDBConnection(out isMyTBConnection, out isNewDbConnectionFromTranscMng))
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "ExecuteMassiveValidation Failed.", "CRMInfinitySynchroProvider.ExecuteMassiveExport", ProviderName);
                return;
            }

            // prima vado a ad eseguire i profili per l'applicazione ERP
            if (massiveProfileDictionary.TryGetValue(DatabaseLayerConsts.ERPSignature, out massiveProfileFile))
            {
                if (string.IsNullOrWhiteSpace(massiveProfileFile))
                {
                    LogWriter.WriteToLog(CompanyName, ProviderName, "No SynchroMassiveProfiles.xml for ERP found! Unable to proceed!", "CRMInfinitySynchroProvider.ExecuteMassiveValidation", ProviderName);
                    LogWriter.WriteToLog(CompanyName, ProviderName, "Finished ExecuteMassiveValidation", "CRMInfinitySynchroProvider.ExecuteMassiveValidation", ProviderName);

                    ManageCloseDBConnection(isMyTBConnection, isNewDbConnectionFromTranscMng);
                    return;
                }

                LogWriter.WriteToLog(CompanyName, ProviderName, "Executing Massive Validation for ERP", "CRMInfinitySynchroProvider.ExecuteMassiveValidation", ProviderName);
                ExecuteMassiveValidationForApplication(massiveProfileFile, bCheckFK, bCheckXSD, filters, serializedTree, workerId);
            }

            // vado ad eseguire i profili definiti per le altre applicazioni
            foreach (KeyValuePair<string, string> kvp in massiveProfileDictionary)
            {
                if (string.Compare(kvp.Key, DatabaseLayerConsts.ERPSignature, StringComparison.InvariantCultureIgnoreCase) == 0)
                    continue;  // skippo ERP

                LogWriter.WriteToLog(CompanyName, ProviderName, string.Format("Executing Massive Validation for {0}", kvp.Key), "CRMInfinitySynchroProvider.ExecuteMassiveValidation", ProviderName);
                ExecuteMassiveValidationForApplication(kvp.Value, bCheckFK, bCheckXSD, filters, serializedTree, workerId, kvp.Key);
            }

            ManageCloseDBConnection(isMyTBConnection, isNewDbConnectionFromTranscMng);

            LogWriter.WriteToLog(CompanyName, ProviderName, "Finished ExecuteMassiveValidation", "CRMInfinitySynchroProvider.ExecuteMassiveValidation", ProviderName);
        }

        /// <summary>
        /// Richiamato per ogni applicazione che dichiara un file per la massiva (SynchroMassiveProfiles.xml)
        ///</summary>
        //--------------------------------------------------------------------------------
        internal void ExecuteMassiveValidationForApplication(string massiveProfileFile, bool bCheckFK, bool bCheckXSD, string filters, string serializedTree, int workerId, string applicationName = "ERP")
        {
            SynchroMassiveProfileParser smpp = new SynchroMassiveProfileParser();
            if (!smpp.ParseFile(massiveProfileFile, new LogInfo() { ProviderName = ProviderName, CompanyName = CompanyName, ProviderLogWriter = LogWriter }))
                return;

            Stopwatch stopWatch = new Stopwatch();
            string elapsedTime = string.Empty;
            TimeSpan timeElapsed;
            stopWatch.Start();

            //Lista con xml da eseguire
            List<ActionToExport> xmlList = null;
            int recordsATime = 0;
            int offSet = 0;
            DateTime startValidationDate = DateTime.Now;

            //Svuoto il contenuto delle tabelle DS_ValidationInfo e DS_ValidationFKToFix solo per il  applicazione ERP
            //sui verticali non devo cancellare altrimenti perdo msg di ERP
            if (applicationName == DatabaseLayerConsts.ERPSignature)
            { 
                DeleteInfoRecordsForMassiveValidation();
                DeleteFKToFixRecordsForMassiveValidation();
            }
            // Devo inserire tutti i TBGuid Esclusi dai filtri nella ValidationIndo con il flag UsedForFilter a TRUE
            List<ValidationFiltersInfo> filtersList = null;
            string filtersQuery = string.Empty;
            string arrangeQuery = string.Empty;

            List<string> skipList = new List<string>();
            Dictionary<string, ValidationFiltersInfoByNamespace> filterMap = new Dictionary<string, ValidationFiltersInfoByNamespace>();

            if (!filters.IsNullOrEmpty())
            {
                Logger.Instance.WriteToLog(CompanyName, ProviderName, "Start filter elaboration.", "ExecuteMassiveValidationForApplication", string.Empty);
                Factory.Instance.DeserializeValidationFilters(filters, ref filtersList);

                foreach (ValidationFiltersInfo elem in filtersList)
                {
                    filterMap.Add(elem.Namespace, new ValidationFiltersInfoByNamespace(elem.MasterTable, elem.SetType));

                    if (elem.Query.IsNullOrEmpty()) // significa che non va validato nessun esemento del namespace considerato,
                    {                               // quindi non inserisco righe nella DS_ValidationInfo con Exclude = TRUE con quel namespace
                        skipList.Add(elem.Namespace);
                        Logger.Instance.WriteToLog(CompanyName, ProviderName, $"FKCHECK: Namespace {elem.Namespace} has been excluded by fiters.", "ExecuteMassiveValidationForApplication", string.Empty);
                        continue;
                    }

                    Factory.Instance.ArrangeQuery(elem.Query, out arrangeQuery);
                    InsertUsedForFiltersRecord(arrangeQuery, elem.Namespace, startValidationDate, workerId);
                }

                Logger.Instance.WriteToLog(CompanyName, ProviderName, "End filter elaboration", "ExecuteMassiveValidationForApplication", string.Empty);
            }

            FlushValidationInfo();

            // Validazione FK
            if (bCheckFK)
            {
                Logger.Instance.WriteToLog(CompanyName, ProviderName, "Start Check Referential Integrity.", "ExecuteMassiveValidationForApplication", string.Empty);
                ExecuteValidationForeignKey(serializedTree, startValidationDate, workerId);
                Logger.Instance.WriteToLog(CompanyName, ProviderName, "End Check Referential Integrity.", "ExecuteMassiveValidationForApplication", string.Empty);
            }
            else
                Logger.Instance.WriteToLog(CompanyName, ProviderName, "Check Referential Integrity has not performed in order of your selection.", "ExecuteMassiveValidationForApplication", string.Empty);

            // Validazione XSD
            if (bCheckXSD)
            {
                Logger.Instance.WriteToLog(CompanyName, ProviderName, "Start Check XSD Integrity.", "ExecuteMassiveValidationForApplication", string.Empty);
                foreach (ActionToMassiveSync atms in smpp.SynchroProfileInfo.Documents)
                {
                    if (skipList != null && skipList.Count > 0 && skipList.Contains(atms.Name))
                    {
                        Logger.Instance.WriteToLog(CompanyName, ProviderName, $"XSDCHECK: Namespace {atms.Name} has been excluded by fiters.", "ExecuteMassiveValidationForApplication", string.Empty);
                        continue;
                    }
                    recordsATime = 500;
                    offSet = 1;

                    CRMActionInfo crmActionInfo = GetCRMActionByName(atms);
                    if (crmActionInfo == null)
                        continue;

                    filtersQuery = string.Empty;

                    if (filterMap.ContainsKey(atms.Name))
                    {
                        if (filterMap[atms.Name].SetType == "NOTIN")
                            filtersQuery = $"{filterMap[atms.Name].MasterTable}.TBGuid NOT IN (SELECT DocTBGuid FROM DS_ValidationInfo WHERE UsedForFilter = '1')";
                        else if (filterMap[atms.Name].SetType == "IN")
                            filtersQuery = $"{filterMap[atms.Name].MasterTable}.TBGuid IN (SELECT DocTBGuid FROM DS_ValidationInfo WHERE UsedForFilter = '1')";
                    }

                    // ciclo per spezzare l'estrazione dei dati a blocchi di 500 record
                    for (;;)
                    {
                        xmlList = UnparseStringForInsertAction(crmActionInfo, string.Empty, SynchroActionType.Massive, filtersQuery, offSet, recordsATime);
                        if (xmlList == null || xmlList.Count == 0)
                            break;

                        string msgValidation = string.Empty;
                        bool bValidation = ValidateXSD(xmlList, startValidationDate, atms.Name, workerId, out msgValidation);

                        offSet += recordsATime;
                    }
                }
                Logger.Instance.WriteToLog(CompanyName, ProviderName, "End Check XSD Integrity.", "ExecuteMassiveValidationForApplication", string.Empty);
            }
            else
                Logger.Instance.WriteToLog(CompanyName, ProviderName, "Check XSD Integrity has not performed in order of your selection.", "ExecuteMassiveValidationForApplication", string.Empty);

            stopWatch.Stop();
            timeElapsed = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", timeElapsed.Hours, timeElapsed.Minutes, timeElapsed.Seconds, timeElapsed.Milliseconds / 10);
            Logger.Instance.WriteToLog(CompanyName, ProviderName, "Massive Validation ends in " + elapsedTime, "ExecuteMassiveValidationForApplication", string.Empty);
        }

        /// <summary>
        /// Metodo che viene chiamato per la validazione puntuale.
        ///</summary>
        //--------------------------------------------------------------------------------
        internal override bool ExecuteValidateDocument(string nameSpace, string guidDoc, string serializedErrors, int workerId, bool includeXsd = true)
        {
            string message = string.Empty;
            DateTime startValidationDate = DateTime.Now;

            LogWriter.WriteToLog(CompanyName, ProviderName, "Started ExecuteValidateDocument ", "CRMInfinitySynchroProvider.ExecuteValidateDocument", ProviderName);

            DeleteRecordForValidationDocument(guidDoc);

            bool bFKValidationOK = serializedErrors.IsNullOrEmpty();
            if (!bFKValidationOK)
            {
                IAFError dataError = new IAFError(serializedErrors, true, false);
                InsertOrUpdateValidationInfo(guidDoc, string.Empty, nameSpace, startValidationDate, dataError, workerId, true);
            }

            FlushValidationInfo();

            bool bXSDValidationOk = true;
            if (includeXsd)
            {
                List<ActionToExport> xmlDoc = GetXML(guidDoc, nameSpace);
                if (xmlDoc?.Count >= 0)
                    bXSDValidationOk = ValidateXSD(xmlDoc, startValidationDate, nameSpace, workerId, out message);

                LogWriter.WriteToLog(CompanyName, ProviderName, "Endeded ExecuteValidateDocument ", "CRMInfinitySynchroProvider.ExecuteValidateDocument", ProviderName);
            }

            return bFKValidationOK && bXSDValidationOk;
        }

        /// <summary>
        /// Metodo che si occupa della validazione dei dati inseriti in un file xml. Confronta il file xml con il file xsd.
        /// Viene richiamato sia dalla validazione massiva che da quella puntuale.
        ///</summary>
        //--------------------------------------------------------------------------------
        internal bool ValidateXSD(List<ActionToExport> xmlList, DateTime startValidationDate, string docNamespace, int workerId, out string message)
        {
            TranslateResult ResultErrorTranslate = new TranslateResult();
            message = string.Empty;
            ValidatorResult validationResult;
            bool bOk = true;
            int nTBRow = 0;
            try
            {
                ValidateXMLInfo ValidateXMLInfo = new ValidateXMLInfo();
                IBusinessObjectInfo dataTypeBOInfo = null;

                if (!Factory.Instance.TryCreateBusinessObjectInfo("dinamicDataType.xsd", XsdResolverType.REMOTE_WITH_CACHE, this, out dataTypeBOInfo, out message))
                {
                    string.Concat("Error during creation of business object for dinamicDataType.xsd ", message);
                    return false;
                }

                foreach (var actionPair in xmlList)
                {
                    IBusinessObjectInfo bOInfo = null;
                    if (!Factory.Instance.TryCreateBusinessObjectInfo(actionPair.Name, XsdResolverType.REMOTE_WITH_CACHE, this, out bOInfo, out message))
                    {
                        message = string.Concat(string.Format("Error during creation of business object for {0}", actionPair.Name), message);
                        bOk = false;
                        continue;
                    }

                    ValidateXMLInfo.XMLString = actionPair.XmlToImport;

                    if (!ValidateXMLInfo.XMLString.IsNullOrEmpty())
                    {
                        IXSDResolver xsDResolver = Factory.Instance.CreateXSDResolver(bOInfo);
                        IXMLValidator xsDValidator = Factory.Instance.CreateXMLValidator();
                        IBusinessObjectXSDInfo dataTypeXsdInfo = xsDResolver.Resolve(dataTypeBOInfo);

                        if (dataTypeXsdInfo == null)
                        {
                            Logger.Instance.WriteToLog(CompanyName, ProviderName, "XSD not found for Business Object " + dataTypeBOInfo.Name, "CRMInfinitySynchroProvider.ValidateXSD", "Exception in Datasynchronizer Web Service");
                            continue; // TODO: gestire segnalazione errore a video.
                        }

                        IBusinessObjectXSDInfo xsdInfo = xsDResolver.Resolve(bOInfo);
                        if (!xsdInfo.Found)
                        {
                            Logger.Instance.WriteToLog(CompanyName, ProviderName, "XSD not found for Business Object " + actionPair.Name, "CRMInfinitySynchroProvider.ValidateXSD", "Exception in Datasynchronizer Web Service");
                            if (actionPair.Name.Equals("UploadDocuments", StringComparison.InvariantCultureIgnoreCase))
                                continue;
                            else
                                continue; // TODO: gestire segnalazione errore a video.
                        }

                        ValidateXMLInfo.XSDInfo.Add(dataTypeXsdInfo);
                        ValidateXMLInfo.XSDInfo.Add(xsdInfo);
                        validationResult = xsDValidator.Validate(ValidateXMLInfo, actionPair.Name);
                        ValidateXMLInfo.XSDInfo.Remove(dataTypeXsdInfo);
                        ValidateXMLInfo.XSDInfo.Remove(xsdInfo);

                        foreach (var errorElement in validationResult.IAFErrorList)
                        {
                            bOk = false;
                            nTBRow = errorElement.NoRowInXML - 1;
                            if (nTBRow >= 0 && nTBRow < actionPair.TBGuid.Count)
                            {
                                string tbGuid = actionPair.TBGuid[nTBRow];
                                InsertOrUpdateValidationInfo(tbGuid, actionPair.Name, docNamespace, startValidationDate, errorElement, workerId);
                            }
                            else
                            {
                                message = "Error during xsd validation: TBGuid index is negative or greater than the size of TBGuid List " + nTBRow.ToString();
                                Logger.Instance.WriteToLog(CompanyName, ProviderName, message, "CRMInfinitySynchroProvider.ValidateXSD", string.Empty);
                            }
                        }
                    }
                }

                FlushValidationInfo();
            }
            catch (Exception e)
            {
                Logger.Instance.WriteToLog(CompanyName, ProviderName, e.Message, "CRMInfinitySynchroProvider.ValidateXSD", "Exception in Datasynchronizer Web Service");
                message = "Error: " + e.Message;
                Debug.WriteLine(message.ToString());
                return false;
            }

            return bOk;
        }

        //--------------------------------------------------------------------------------
        internal bool ValidateForeignKeyXmlbyXSD(string sXml, out string message)
        {
            TranslateResult ResultErrorTranslate = new TranslateResult();
            bool bValidationOk = false;
            message = string.Empty;

            try
            {
                ValidateXMLInfo ValidateXMLInfo = new ValidateXMLInfo();
                IBusinessObjectInfo dataTypeBOInfo = null;

                if (!Factory.Instance.TryCreateBusinessObjectInfo("RICheckerTree.xsd", XsdResolverType.LOCAL, this, out dataTypeBOInfo, out message))
                    return bValidationOk;

                ValidateXMLInfo.XMLString = sXml;

                if (!ValidateXMLInfo.XMLString.IsNullOrEmpty())
                {
                    IXMLValidator xsDValidator = Factory.Instance.CreateXMLValidator();
                    IXSDResolver xsDResolver = Factory.Instance.CreateXSDResolver(dataTypeBOInfo);
                    IBusinessObjectXSDInfo dataTypeXsdInfo = xsDResolver.Resolve(dataTypeBOInfo);
                    ValidateXMLInfo.XSDInfo.Add(dataTypeXsdInfo);
                    bValidationOk = xsDValidator.Validate(ValidateXMLInfo, string.Empty).IsOK;
                    ValidateXMLInfo.XSDInfo.Remove(dataTypeXsdInfo);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.WriteToLog(CompanyName, ProviderName, e.Message, "CRMInfinitySynchroProvider.ValidateXSD", "Exception in Datasynchronizer Web Service");
                message = "Error: " + e.Message;
                Debug.WriteLine(message.ToString());
                bValidationOk = false;
            }

            return bValidationOk;
        }

        //--------------------------------------------------------------------------------
        internal bool ExecuteValidationForeignKey(string serializedTree, DateTime startValidationDate, int workerId)
        {
            string msg = string.Empty;

            bool bXsdOk = ValidateForeignKeyXmlbyXSD(serializedTree, out msg);

            if (!bXsdOk)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "Validation Foreign Key failed (file xml not consistent!)", "CRMInfinitySyncroProvider.ExecuteValidationForeignKey");
                return false;
            }

            IRICheckNode rootNode = null;
            Factory.Instance.CreateRoot(serializedTree, out rootNode, out msg);

            ExtractMassiveQueryForForeignKeyValidation(rootNode, startValidationDate, workerId);

            UpdateValidationFKtoFixWithRelatedErrors();

            return true;
        }

        //--------------------------------------------------------------------------------
        public void ExtractMassiveQueryForForeignKeyValidation(IRICheckNode node, DateTime startValidationDate, int workerId, bool bParallel = false)
        {
            if (!bParallel)
            {
                foreach (var elem in node.Sons)
                    ExtractMassiveQueryForForeignKeyValidation(elem, startValidationDate, workerId, false);
            }
            else
            {
                Parallel.ForEach(node.Sons, elem => { ExtractMassiveQueryForForeignKeyValidation(elem, startValidationDate, workerId, false); });
            }

            string arrangedQuery = string.Empty;

            int nCheckers = node.RICheckerInfoList.Count;

            // Siamo noi a creare l'xml quindi anche leggerlo "posizionale" non crea problemi
            for (int i = 0; i < nCheckers; i++)
            {
                Factory.Instance.ArrangeQuery(node.RICheckerInfoList.ElementAt(i).MassiveQuery, out arrangedQuery);
                ExecuteMassiveQueryForForeignKeyValidation(node.Name, node.Sons.ElementAt(i).Name, node.Father.Name, arrangedQuery, startValidationDate, workerId);
            }

            FlushValidationInfo();
        }

        //--------------------------------------------------------------------------------
        private DataTable GetResultTableThreadSafe(string queryToExecute, TBConnection connection)
        {
            lock (connection)
            {
                using (TBDataAdapter sda = new TBDataAdapter(queryToExecute, connection))
                {
                    DataTable resultTable = new DataTable();
                    sda.Fill(resultTable);
                    return resultTable;
                }
            }
        }

        //--------------------------------------------------------------------------------
        public void ExecuteMassiveQueryForForeignKeyValidation(string nodeName, string nodeSonName, string nodeFatherName, string queryToExecute, DateTime startValidationDate, int workerId)
        {
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                string tbGuid = null;
                object codeNotFound = null;

                DataTable resultTable = GetResultTableThreadSafe(queryToExecute, myConnection);

                if (resultTable.Rows.Count == 0) // NON ci sono errori di FK
                    return;

                DataColumn colCodeNoteFound = resultTable.Columns[1];
                DataColumn colMasterData = resultTable.Columns[2];

                string newErrorMessage = string.Empty;
                string providerName = nodeFatherName;
                string docNamespace = nodeName;

                IAFError newErrorElement = null;

                foreach (DataRow row in resultTable.Rows)
                {
                    tbGuid = row.ItemArray.GetValue(0).ToString();

                    codeNotFound = row.ItemArray.GetValue(1);

                    // Per oerenza con quanto accade nella validazione puntuale FK il msg di errore deve avere questo formato
                    // cwsprintf(_TB("%s.%s code %s not registered in Master Data."), sTableName, sColumnName, sCodeNotFound);
                    // definito in CheckerList::FormatFKMessageError (...\ERP\SynchroConnector\SynchroConnectorValidation.h)
                    // per come è fatta la stringa della query, il nome della colonna è già qualificato.
                    newErrorMessage = $"{colCodeNoteFound} {codeNotFound} code not registered in Master Data.";

                    newErrorElement = new IAFError(newErrorMessage, true, false);

                    InsertOrUpdateValidationInfo(tbGuid, string.Empty, docNamespace, startValidationDate, newErrorElement, workerId);

                    InsertValidationFKtoFix(nodeSonName, colMasterData.ToString(), codeNotFound.ToString(), startValidationDate, workerId, whereProviderName);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "CRMInfinitySyncroProvider.ExecuteMassiveQueryForForeignKeyValidation");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        //---------------------------------------------------------------------
        public override bool SetConvergenceCriteria(string xmlCriteria, out string message)
        {
            message = string.Empty;
            return SyncroRef.SetConvergenceCriteria(xmlCriteria, out message);
        }

        //---------------------------------------------------------------------
        public override bool GetConvergenceCriteria(string actionName, out string xmlCriteria, out string message)
        {
            TranslateResult ResultErrorTranslate = new TranslateResult();
            message = string.Empty;
            return SyncroRef.GetConvergenceCriteria(actionName, out xmlCriteria, out message);
        }

        //---------------------------------------------------------------------
        public override bool SetGadgetPerm(out string message)
        {
            message = string.Empty;
            return SyncroRef.SetGadgetPerm(out message);
        }

        //---------------------------------------------------------------------
        public override bool CheckVersion(string magoVersion, out string message)
        {
            message = string.Empty;
            return SyncroRef.CheckVersion(magoVersion, out message);
        }

        public bool OnlyDMS(string action)
        {
            string pathFileError = Path.Combine(PathFinder.BasePathFinderInstance.GetStandardApplicationPath("ERP"), @"SynchroConnector\SynchroProviders\CRMInfinity\SynchroProfiles.xml");

            if (!File.Exists(pathFileError))
                return false;

            XmlDocument xmlOnlyDMS = new XmlDocument();
            xmlOnlyDMS.Load(pathFileError);

            XmlNodeList xnList = xmlOnlyDMS.SelectNodes("/SynchroProfiles/Documents/Document");

            foreach (XmlNode xn in xnList)
            {
                if (xn["Action"].GetAttribute("name") == action)
                    if (xn["Action"].GetAttribute("onlyForDMS").Contains("true"))
                        return true;
                    else
                        return false;
            }
            return false;
        }

    
    }
}
