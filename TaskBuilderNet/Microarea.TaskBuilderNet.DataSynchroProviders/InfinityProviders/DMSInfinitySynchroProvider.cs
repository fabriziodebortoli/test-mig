using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Unparsers;
using Microarea.TaskBuilderNet.DataSynchroProviders.Properties;
using Microarea.TaskBuilderNet.DataSynchroUtilities;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders
{
    ///<summary>
    /// Provider per InfinityDMS e EasyAttachment
    ///</summary>
    //================================================================================
    public class DMSInfinitySynchroProvider : BaseSynchroProvider
    {
        // dictionary per cachare le informazioni parsate nelle actions
        private Dictionary<string, CRMActionInfo> crmActionsInfoDict = new Dictionary<string, CRMActionInfo>();

        private InfinitySyncroRef infinitySyncro = null;
        private SynchroResponseParser responseParser = null;
        private string authenticationToken = null;
        //---------------------------------------------------------------------
        private TBConnection _dmsDbConnection = null;

        //---------------------------------------------------------------------
        protected override string ApplicationFolderName { get { return "DMSInfinity"; } }

        //---------------------------------------------------------------------
        internal override BaseSynchroProfileParser SynchroProfileParser { get { return new SynchroProfileParser(); } }

        //---------------------------------------------------------------------
        internal InfinitySyncroRef SyncroRef { get { return infinitySyncro; } }

        //---------------------------------------------------------------------
        internal SynchroResponseParser SynchroRespParser { get { if (responseParser == null) responseParser = new SynchroResponseParser(); return responseParser; } }

        //---------------------------------------------------------------------
        public DMSInfinitySynchroProvider()
        {
        }

        //---------------------------------------------------------------------
        public DMSInfinitySynchroProvider(string authenticationToken, ConnectionStringManager connectionStringManager, ProviderConfiguration config)
            : base(connectionStringManager)
        {
            infinitySyncro = new InfinitySyncroRef(ProviderName, CompanyName, LogWriter, true);
            string message = string.Empty;
            SetProviderParameters(authenticationToken, config.Url, config.User, config.Password, config.SkipCrtValidation, config.Parameters, config.IAFModule, out message);
            this.authenticationToken = authenticationToken;
        }

        //---------------------------------------------------------------------
        public override bool TestProviderParameters(string url, string username, string password, bool skipCrtValidation, string parameters, out string message)
        {
            message = string.Empty;

            try
            {
                XElement xelem = XDocument.Parse(parameters).Element("Parameters");

                if (xelem != null)
                {
                    // attenzione che e' case-sensitive
                    // facciamo un controllo preventivo sui parametri opzionali, in modo che tutti siano presenti
                    if (
                        string.IsNullOrWhiteSpace(xelem.Element("MagoUrl").Value) ||
                        string.IsNullOrWhiteSpace(xelem.Element("ApplicationUser").Value) ||
                        string.IsNullOrWhiteSpace(xelem.Element("CompanyName").Value))
                    {
                        throw new Exception();
                    }
                }
            }
            catch (Exception)
            {
                message = Resources.ParametersMissing;
                LogWriter.WriteToLog(CompanyName, DatabaseLayerConsts.DMSInfinity, Resources.ParametersMissing, "DMSInfinitySynchroProvider.TestProviderParameters", parameters);
                return false;
            }

            return SyncroRef.TestProviderParameters(url, username, password, skipCrtValidation, parameters, out message);
        }

        //---------------------------------------------------------------------
        public override bool SetProviderParameters(string authenticationToken, string url, string username, string password, bool skipCrtValidation, string parameters, string iafmodule, out string message)
        {
            message = string.Empty;
            var iToken = GetLoginManager().GetIToken(authenticationToken);

            if (!SyncroRef.SetProviderParameters(iToken, url, username, password, skipCrtValidation, parameters, iafmodule, out message))
                return false;

            // TO DO RIVEDERE
            // se la set dei parametri e' andata a buon fine procedo a richiamare le azioni di Update del provider
            if (!UpdateProvider(out message))
                return false;

            return true;
        }

        ///<summary>
        /// Notifica di Update dal DE Providers
        /// Andiamo ad eseguire le azioni MAGO_EA_SERVER e MAGO_EA_ARCHIVE per creare il server esterno e l'archivio principale
        ///</summary>
        //---------------------------------------------------------------------
        private bool UpdateProvider(out string messageCumm)
        {
            messageCumm = string.Empty;

            ActionToSynch actionToSynch = new ActionToSynch(new BaseObjectToSynch());
            actionToSynch.ActionType = SynchroActionType.UpdateProvider;

            if (!FillActionsToImport(actionToSynch))
            {
                return false;
            }

            bool result = true;

            // vado ad eseguire tutte le stringhe xml verso il server Infinity
            foreach (ActionToExport action in actionToSynch.ActionsToExecute)
            {
                string message = string.Empty;
                SyncroRef.ExecuteSyncroWithoutConnect(action, out message);
                if (!action.IsSucceeded[0])
                    result = false;

                try
                {
                    MassiveExportData med = new MassiveExportData();
                    med.EntityName = "Extensions.TbDataSynchroClient.TbDataSynchroClient.Providers";

                    InsertSynchronizationData((action.IsSucceeded[0]) ? SynchroStatusType.Synchro : SynchroStatusType.Error, action.TBGuid[0], med, DateTime.Now, false, message, ifExistsDoNothing: true, synchActionType: SynchroActionType.UpdateProvider);

                    if (!action.IsSucceeded[0])
                        messageCumm += string.IsNullOrWhiteSpace(message) ? action.ErrorMessages[0] : message + " ";
                }
                catch
                {
                }
            }

            return result;
        }

        ///<summary>
        /// Notifica da EasyAttachment
        /// Quando viene creato il primo allegato per un tipo di documento gestionale
        /// la notifica e' doppia: prima l'UpdateCollection per creare il folder, gli attributi e la
        /// relativa classe documentale, poi la NewAttachment per creare l'allegato e valorizzare gli attributi
        ///</summary>
        //---------------------------------------------------------------------
        public override bool Notify(int logID, bool onlyForDMS, string iMagoConfigurations)
        {
            bool notifyError = false;
            LogWriter.WriteToLog(CompanyName, ProviderName, string.Format("Called NotifyDMS (LogID = {0})", logID), "DMSInfinitySynchroProvider.NotifyDMS", ProviderName);

            if (logID <= 0)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "Notify returned false. LogID <= 0", "DMSInfinitySynchroProvider.Notify", ProviderName);
                return false;
            }

            // dato il logID vado a leggere sul database le informazioni dalla tabella DS_ActionsLog e riempio l'apposita struttura
            ActionToSynch actionToSynch = new ActionToSynch(LoadInfoToSynch(logID));
            if (actionToSynch == null) // almeno l'action corrente da eseguire deve essere presente, altrimenti non procedo
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "Notify returned false. actionToSynch == null", "DMSInfinitySynchroProvider.Notify", ProviderName);
                return false;
            }

            // per l'azione corrente di cui ci e' arrivata notifica
            // vado a caricare le azioni da eseguire dal xmlString dei profili di sincronizzazione (potrebbero essere piu' d'una)
            // e genero i relativi xmlString xml con gli appositi unparser
            if (!FillActionsToImport(actionToSynch))
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "Notify returned false. FillActionsToImport(...) returned false", "DMSInfinitySynchroProvider.Notify", ProviderName);
                return false;
            }

            SetWaitStatus(logID);

            bool sent = true;

            string messageCumm = string.Empty;

            LogWriter.WriteToLog(CompanyName, ProviderName, "ActionsToExecute loop started.", "DMSInfinitySynchroProvider.Notify", ProviderName);
            foreach (ActionToExport action in actionToSynch.ActionsToExecute)
            {
                string message = string.Empty;
                int count = 0;

                SyncroRef.ExecuteSyncroWithoutConnect(action, out message);

                foreach (bool succeeded in action.IsSucceeded)
                {
                    try
                    {
                        if (succeeded)
                        {
                            if (!string.IsNullOrWhiteSpace(actionToSynch.DocTBGuid))
                            {
                                InsertSynchronizationInfo(SynchroStatusType.Synchro, actionToSynch.DocTBGuid, actionToSynch.DocNamespace, 0, actionToSynch.ActionType, SynchroDirectionType.Outbound, DateTime.Now, ifExistsDoNothing: true);

                                // solo per la action che si occupa di inserire i riferimenti all'allegato
                                // inserisco la riga nella transcodifica e nella tabella della sincro degli attachment
                                if (string.Compare(action.Name, "MAGO_EA_ATTACH", StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    if (!string.IsNullOrEmpty(action.Keys[count]))
                                    {
                                        // todo: decidere se e' necessario inserire nella tabella di transcodifica (utile per la cancellazione) oppure
                                        // se ci bastano le info scritte nell'actiondata
                                        //TranscodingMng.InsertRow(CompanyConnectionString, action.Name, action.Keys[count], action.Name, action.Keys[count], actionToSynch.DocTBGuid);

                                        if (actionToSynch.ActionType == SynchroActionType.DeleteAttachment)
                                            DeleteAttachmentSynchroInfo(actionToSynch.DocTBGuid, action.Keys[count]);
                                        else
                                            InsertAttachmentSynchroInfo(SynchroStatusType.Synchro, actionToSynch.DocTBGuid, action.Keys[count], actionToSynch.DocNamespace, 0, actionToSynch.ActionType);
                                    }
                                }
                                if (!notifyError)
                                {
                                    LoginManager loginMng = GetLoginManager();
                                    loginMng.Init(false, authenticationToken);
                                    loginMng.PurgeMessageByTag(action.TBGuid[count]);

                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(actionToSynch.DocTBGuid))
                            {
                                InsertSynchronizationInfo(SynchroStatusType.Error, actionToSynch.DocTBGuid, actionToSynch.DocNamespace, 0, actionToSynch.ActionType, SynchroDirectionType.Outbound, DateTime.Now, ifExistsDoNothing: true);

                                if (string.Compare(action.Name, "MAGO_EA_ATTACH", StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    if (!string.IsNullOrEmpty(action.Keys[count]))
                                        InsertAttachmentSynchroInfo(SynchroStatusType.Error, actionToSynch.DocTBGuid, action.Keys[count], actionToSynch.DocNamespace, 0, actionToSynch.ActionType);
                                }

                                sent = false;
                                messageCumm += string.IsNullOrWhiteSpace(message) ? action.ErrorMessages[count] : message + " ";
                                if (!action.Name.Contains("DIS") && (NotifyDataSynch.Equals("true") || NotifyDataSynch.Equals("TRUE")) && !notifyError)
                                {
                                    string linkOpenDocu = GetUrlTBMago(action.Name, action.TBGuid[count], actionToSynch.DocNamespace);

                                    if (!String.IsNullOrEmpty(linkOpenDocu))
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
                        }
                    }
                    catch
                    {
                    }

                    count++;
                }
            }
            LogWriter.WriteToLog(CompanyName, ProviderName, "ActionsToExecute loop ended.", "DMSInfinitySynchroProvider.Notify", ProviderName);

            //se cancello il record vado cmq a cancellare le righe di transcodifica
            if (actionToSynch.ActionType == SynchroActionType.DeleteAttachment)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "SynchroActionType is DeleteAttachment.", "DMSInfinitySynchroProvider.Notify", ProviderName);
                //TranscodingMng.DeleteRowByTBGuid(CompanyConnectionString, actionToSynch.DocTBGuid);
                DeleteSynchronizationData(actionToSynch.DocTBGuid);
            }
            else
                UpdateSynchronizationData(sent ? SynchroStatusType.Synchro : SynchroStatusType.Error, string.Empty, messageCumm, logID);

            LogWriter.WriteToLog(CompanyName, ProviderName, "NotifyDMS Finished", "DMSInfinitySynchroProvider.NotifyDMS", ProviderName);
            return sent;
        }


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
            string SynchroMassiveProfile = Path.Combine(PathFinder.BasePathFinderInstance.GetStandardApplicationPath("ERP"), @"SynchroConnector\SynchroProviders\DMSInfinity\SynchroMassiveProfiles.xml");
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
        private bool FillActionsToImport(ActionToSynch actionToSynch)
        {
            // dato il namespace del documento carico le azioni da eseguire dal xmlString dei profili di sincronizzazione
            DocumentToSync doc = GetDocumentToSyncFromNs(actionToSynch.ActionType.ToString()) as DocumentToSync;
            if (doc == null || doc.Actions.Count == 0)
                return false;

            // scorro le azioni previste per quel documento, richiamo il parse (solo se necessario)
            foreach (DSAction act in doc.Actions)
            {
                CRMActionInfo crmInfo = GetActionByName(act.Name);
                if (crmInfo == null) // se non trovo la descrizione dell'azione mi fermo subito
                    break;

                List<ActionToExport> xmlList = null;

                switch (actionToSynch.ActionType)
                {
                    case SynchroActionType.NewAttachment:
                    case SynchroActionType.NewCollection:
                    case SynchroActionType.UpdateCollection:
                    case SynchroActionType.UpdateProvider:
                        xmlList = UnparseStringForInsert(crmInfo, actionToSynch.ActionType, actionToSynch.ActionData);
                        break;

                    case SynchroActionType.DeleteAttachment:
                        xmlList = UnparseStringForDelete(crmInfo, actionToSynch.ActionType, actionToSynch.ActionData);
                        break;
                }

                if (xmlList != null && xmlList.Count > 0)
                    actionToSynch.ActionsToExecute.AddRange(xmlList);
            }

            return true;
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
        /// Richiama il parser del xmlString corrispondente alla action passata come parametro
        /// (ha lo stesso nome + estensione xml)
        /// parametro onlymassive solo per la synch massiva
        ///</summary>
        //--------------------------------------------------------------------------------
        private CRMActionInfo ParseAction(string action, bool onlymassive = false)
        {
            ActionParser actionParser = new ActionParser();
            CRMActionInfo crmActionForERP = null;

            string actionFullPath = string.Empty;

            // cerco l'action SOLO per ERP
            if (SynchroFilesActionsFolderDict.TryGetValue(DatabaseLayerConsts.ERPSignature, out actionFullPath))
            {
                actionFullPath = Path.Combine(actionFullPath, action + ".xml");

                // controllo che il file esista e anche se il file di action e' stato correttamente dichiarato nel file di profilo
                if (File.Exists(actionFullPath))
                {
                    try
                    {
                        if (actionParser.ParseFile(actionFullPath))
                            crmActionForERP = actionParser.CRMActionsInfo;
                    }
                    catch (Exception ex)
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "DMSInfinitySynchroProvider.ParseAction");
                    }
                }
            }

            return actionParser.CRMActionsInfo;
        }

        ///<summary>
        /// Metodo che si occupa di comporre la stringa xml con la sintassi corretta per essere poi inviata al server Infinity
        /// In caso di Update del record devo eseguire la query sul db ed interpretare cio' che e' indicato nella colonna ActionData
        /// Poi devo integrare le info caricate dal database con quelle
        ///</summary>
        //--------------------------------------------------------------------------------
        private List<ActionToExport> UnparseStringForInsert(CRMActionInfo crmInfo, SynchroActionType syncActionType, string actionData)
        {
            List<ActionToExport> xmlActionList = new List<ActionToExport>();

            string keyValue = string.Empty;
            ActionDataInfo actDataInfo = null;

            foreach (CRMAction action in crmInfo.Actions)
            {
                if (!string.IsNullOrWhiteSpace(actionData))
                {
                    // da mettere a posto il parse della chiave
                    actDataInfo = ActionDataParser.ParseActionData(syncActionType, actionData);

                    if (!string.IsNullOrWhiteSpace(actDataInfo.DmsKey))
                        keyValue = actDataInfo.DmsKey;
                }

                List<DTValuesToImport> dtToImportList = QueryValuesToImport(action, keyValue);
                if (dtToImportList == null || dtToImportList.Count == 0)
                    continue;

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
        /// Metodo che si occupa di comporre la stringa xml con la sintassi corretta per essere poi inviata al server Infinity
        /// In caso di eliminazione del record NON devo eseguire la query sul db, ma interpretare cio' che e' indicato nella colonna ActionData
        /// Basta infatti eliminare solo il master passando i valori della PK (facendo attenzione che la PK di ERP potrebbe non coincidere con quella di Infinity)
        ///</summary>
        //--------------------------------------------------------------------------------
        private List<ActionToExport> UnparseStringForDelete(CRMActionInfo crmInfo, SynchroActionType syncActionType, string actionData)
        {
            List<ActionToExport> xmlActionList = new List<ActionToExport>();
            ActionDataInfo actDataInfo = null;

            if (!string.IsNullOrWhiteSpace(actionData))
            {
                // da mettere a posto il parse della chiave
                actDataInfo = ActionDataParser.ParseActionData(syncActionType, actionData);
                if (actDataInfo == null || actDataInfo.KeysForDeleteActionList.Count == 0)
                    return xmlActionList;
            }

            foreach (CRMAction action in crmInfo.Actions)
            {
                // richiamo la classe che si occupa di scrivere la stringa xml formattata e sintatticamente corretta da dare poi in pasto al ws del CRM
                ActionToExport myList = InfinityUnparseHelper.UnparseStringForAction(SyncroRef.RegisteredApp, action, syncActionType, dtValuesList: null, adi: actDataInfo);
                xmlActionList.Add(myList);
            }

            xmlActionList.Reverse();

            return xmlActionList;
        }

        ///<summary>
        /// Prima faccio la query per la master action e riempio il suo datatable
        /// Poi per ognuno eseguo le query delle eventuali subaction e riempio tutti i DataTable figli necessari
        /// Alla fine ho una lista di DTValuesToImport da dare in pasto all'unparser
        ///</summary>
        //--------------------------------------------------------------------------------
        private List<DTValuesToImport> QueryValuesToImport(CRMAction crmAction, string keyValue, bool isMassive = false, string massiveFilters = "", int offset = 0, int recordsATime = 500)
        {
            if (string.IsNullOrWhiteSpace(crmAction.Select) || string.IsNullOrWhiteSpace(crmAction.From))
                return null;

            // compongo SELECT + FROM
            string query = "SELECT " + crmAction.Select + " FROM " + crmAction.From;

            // se NON sono nella massiva
            if (!isMassive)
            {
                if (!string.IsNullOrWhiteSpace(crmAction.Where))
                {
                    query += " WHERE " + crmAction.Where;
                    query = string.Format(query, keyValue);
                }
            }
            else
            {
                // se sono nella massiva
                if (!string.IsNullOrWhiteSpace(massiveFilters))
                {
                    // se ho dei filtri li aggiungo, e poi in coda metto la MassiveWhere oppure la Where se esistono
                    query += " WHERE " + massiveFilters;
                    if (!string.IsNullOrWhiteSpace(crmAction.MassiveWhere))
                        query += " AND " + crmAction.MassiveWhere;
                    else
                        if (!string.IsNullOrWhiteSpace(crmAction.Where))
                        query += " AND " + crmAction.Where;
                }
                else
                {
                    // se NON ho filtri metto la MassiveWhere oppure la Where se esistono
                    if (!string.IsNullOrWhiteSpace(crmAction.MassiveWhere))
                        query += " WHERE " + crmAction.MassiveWhere;
                    else
                        if (!string.IsNullOrWhiteSpace(crmAction.Where))
                        query += " WHERE " + crmAction.Where;
                }
            }

            // istanzio un dataTable di appoggio con tutte le righe estratte dalla query sul master
            DataTable masterRecDt = null;
            List<DTValuesToImport> dtToImportList = new List<DTValuesToImport>(); // lista dei master+slave da importare

            try
            {
                string currentConnectionString = string.Empty;
                // Da eliminare aggiungendo su ogni action una proprieta' che dica qual e' il database corretto
                bool bUseCompanyConnectionString = crmAction.ActionName == "MAGO_EA_SERVER" || crmAction.ActionName == "MAGO_EA_ARCHIVE";
                currentConnectionString = bUseCompanyConnectionString ? connectionStringManager.CompanyConnectionString : connectionStringManager.DMSConnectionString;

                //using (TBConnection myConnection = new TBConnection(currentConnectionString, DBMSType.SQLSERVER))
                //{
                //    myConnection.Open();

                bool isMyTBConnection = false;
                TBConnection myConnection = null;
                if (bUseCompanyConnectionString)
                    myConnection = OpenDbConnection(out isMyTBConnection);
                else
                    myConnection = OpenDMSDbConnection(out isMyTBConnection);

                // serve quando siamo nell'UpdateProvider per avere la tabella sloccata
                IDbTransaction dbTransaction = myConnection.BeginTransaction(IsolationLevel.ReadUncommitted);

                try
                {
                    using (TBDataAdapter sda = new TBDataAdapter(query, myConnection))
                    {
                        sda.SelectCommand.Transaction = dbTransaction;
                        masterRecDt = new DataTable(crmAction.ActionName);
                        sda.Fill(masterRecDt);
                    }
                    dbTransaction.Commit();
                }
                catch(Exception e)
                {
                    LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "DMSInfinitySynchroProvider.QueryValuesToImport");
                    dbTransaction.Rollback();
                }
                // eseguo la query sul database di EasyAttachment e con un DataAdapter fillo i dati in un datatable
                

                

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
                            values.Add(dtMasterAction.Rows[0][subAction.SubactionsParams[i]].ToString());

                        if (values.Count > 0)
                            subquery = string.Format(subquery, values.ToArray());

                        // eseguo la query sul database di ERP e con un SqlDataAdapter fillo i dati in un datatable
                        using (TBDataAdapter sda = new TBDataAdapter(subquery, myConnection))
                        {
                            DataTable dtSubAction = new DataTable(subAction.ActionName);
                            sda.Fill(dtSubAction);

                            // memorizzo il DataTable della tabella slave
                            dtToImport.SlavesDtList.Add(dtSubAction);
                        }
                    }

                    dtToImportList.Add(dtToImport); // aggiungo alla lista il master correntemente analizzato
                }
                //}

                if (bUseCompanyConnectionString)
                    CloseDbConnection(isMyTBConnection);
                else
                    CloseDMSDbConnection(isMyTBConnection);
            }
            catch (TBException ex)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "DMSInfinitySynchroProvider.QueryValuesToImport", crmAction.ActionName);
                Debug.WriteLine(ex.Message);
                return null;
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "DMSInfinitySynchroProvider.QueryValuesToImport", crmAction.ActionName);
                Debug.WriteLine(e.Message);
                return null;
            }

            return dtToImportList;
        }

        ///<summary>
        /// Inserisce o aggiorna una riga nella DS_AttachmentSynchroInfo
        ///</summary>
        //---------------------------------------------------------------------
        internal void InsertAttachmentSynchroInfo(SynchroStatusType synchStatus, string docTBGuid, string attachmentId, string docNamespace, int workerID, SynchroActionType lastAction, bool ifExistsDoNothing = false)
        {
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                bool exist = false;
                bool ifDoNothing = false;

                using (TBCommand aSqlCommand = new TBCommand(myConnection))
                {
                    // controllo se esiste nella DS_AttachmentSynchroInfo una riga con quel TBGuid
                    string queryexistInfo = "SELECT COUNT(*) FROM [DS_AttachmentSynchroInfo] WHERE [DocTBGuid] = @docTBGuid AND [AttachmentID] = @attId" + whereProviderName;

                    string queryIfExistsDoNothing = string.Empty;
                    if (ifExistsDoNothing)
                        queryIfExistsDoNothing = queryexistInfo + " AND ([SynchStatus]=" + (int)SynchroStatusType.Error + " OR [SynchStatus]=" + (int)SynchroStatusType.Synchro + ")";

                    aSqlCommand.CommandText = queryexistInfo;
                    aSqlCommand.Parameters.Add("@docTBGuid", docTBGuid);
                    aSqlCommand.Parameters.Add("@attId", Int32.Parse(attachmentId));
                    aSqlCommand.Parameters.Add("@providerName", ProviderName);
                    exist = (int)aSqlCommand.ExecuteScalar() > 0;

                    if (ifExistsDoNothing)
                    {
                        aSqlCommand.CommandText = queryIfExistsDoNothing;
                        ifDoNothing = (int)aSqlCommand.ExecuteScalar() > 0;
                    }
                }

                //se esiste e non devo fare niente allora esco
                if (ifDoNothing && ifExistsDoNothing)
                    return;

                using (TBCommand aSqlCommand = new TBCommand(myConnection)) // faccio una INSERT o un UPDATE a seconda dell'esistenza del record
                {
                    if (exist)
                        aSqlCommand.CommandText =
                                    @"UPDATE [DS_AttachmentSynchroInfo] SET [SynchStatus] = @SynchStatus, [SynchDate] = @SynchDate, [SynchDirection] = @SynchDirection,
                                    [LastAction] = @LastAction, [TBModified] = @TBModified, [TBModifiedID] = @TBModifiedID, [WorkerID] =  @WorkerID
                                    WHERE [DocTBGuid] = @DocTBGuid AND [AttachmentID] = @AttachId" + whereProviderName;
                    else
                    {
                        aSqlCommand.CommandText = @"INSERT INTO [DS_AttachmentSynchroInfo] ([ProviderName], [DocTBGuid],  [AttachmentID], [DocNamespace], [SynchStatus], [SynchDate], [SynchDirection],
                                                    [LastAction], [TBCreated], [TBModified], [TBCreatedID], [TBModifiedID], [WorkerID])
                                                    VALUES (@providerName, @DocTBGuid, @AttachId, @DocNamespace, @SynchStatus, @SynchDate, @SynchDirection, @LastAction, @TBCreated,
                                                    @TBModified, @TBCreatedID, @TBModifiedID, @WorkerID)";

                        aSqlCommand.Parameters.Add("@DocNamespace", docNamespace);
                        aSqlCommand.Parameters.Add("@TBCreated", DateTime.Now);
                        aSqlCommand.Parameters.Add("@TBCreatedID", workerID);
                    }

                    aSqlCommand.Parameters.Add("@providerName", ProviderName);
                    aSqlCommand.Parameters.Add("@DocTBGuid", docTBGuid);
                    aSqlCommand.Parameters.Add("@AttachId", Int32.Parse(attachmentId));
                    aSqlCommand.Parameters.Add("@SynchStatus", (int)synchStatus);
                    aSqlCommand.Parameters.Add("@SynchDate", DateTime.Now);
                    aSqlCommand.Parameters.Add("@SynchDirection", (int)SynchroDirectionType.Outbound);
                    aSqlCommand.Parameters.Add("@LastAction", (int)lastAction);
                    aSqlCommand.Parameters.Add("@TBModified", DateTime.Now);
                    aSqlCommand.Parameters.Add("@TBModifiedID", workerID);
                    aSqlCommand.Parameters.Add("@WorkerID", workerID);
                    aSqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "DMSInfinitySynchroProvider.InsertAttachmentSynchroInfo");
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }
        }

        ///<summary>
        /// Metodo richiamato in fase di notifica di una DELETE di un allegato
        /// Devo il record nella DS_AttachmentSynchroInfo per quel TBGuid e quell'attachmentId ed il providerName corrente
        ///</summary>
        //---------------------------------------------------------------------
        private void DeleteAttachmentSynchroInfo(string tbGuid, string attachmentId)
        {
            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(out isMyTBConnection);

                using (TBCommand aSqlCommand = new TBCommand(myConnection))
                {
                    aSqlCommand.CommandText = string.Format("DELETE FROM [DS_AttachmentSynchroInfo] WHERE [DocTBGuid] = '{0}' AND [AttachmentID] = {1} AND [ProviderName] = '{2}'",
                                                            tbGuid, attachmentId, ProviderName);
                    aSqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "DMSInfinitySynchroProvider.DeleteAttachmentSynchroInfo");
            }
        }

        ///<summary>
        /// Procedura massiva
        ///</summary>
        //---------------------------------------------------------------------
        internal override void ExecuteMassiveExport(List<MassiveExportData> medList, DateTime startSynchroDate, bool isRecovery = false, bool bDelta = false)
        { 
            if (IsInPause)
            {
                while (IsInPause)
                {
                    Thread.Sleep(5000);
                }
            }

            if (!Abort)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "Started ExecuteMassiveExport " + (isRecovery ? "for Recovery" : ""), "DMSInfinitySynchroProvider.ExecuteMassiveExport", ProviderName);

                // carichiamo tutti le azioni ordinate dentro il xmlString SynchroMassiveProfiles.xml
                Dictionary<string, string> dict = BasePathFinder.BasePathFinderInstance.GetSynchroMassiveProfilesFilePath(ApplicationFolderName);

                string massiveProfileFile;
                if (dict == null || !dict.TryGetValue(DatabaseLayerConsts.ERPSignature, out massiveProfileFile))
                    return;

                // parso il file dei profili di sincronizzazione massiva
                SynchroMassiveProfileParser smpp = new SynchroMassiveProfileParser();
                if (!smpp.ParseFile(massiveProfileFile, new LogInfo() { ProviderName = ProviderName, CompanyName = CompanyName, ProviderLogWriter = LogWriter }))
                    return;

                string updateMessage;
                if (!UpdateProvider(out updateMessage))
                {
                    LogWriter.WriteToLog(CompanyName, ProviderName, updateMessage, "DMSInfinitySynchroProvider.UpdateProvider");
                    return;
                }

                // prima di elaborare i singoli namespace dei documenti devo creare il folder parent
                // che conterra' tutti i folder/classi documentali corrispondenti alle collection di EasyAttachment
                List<ActionToExport> list = GetXmlForMAGO_EA_FOLDER_PARENT();
                foreach (ActionToExport action in list)
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

                    string msg;
                    if (!SyncroRef.ExecuteSyncroWithoutConnect(action, out msg))
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, msg, "DMSInfinitySynchroProvider.ExecuteMAGO_EA_FOLDER");
                        break;
                    }
                }

                // PER OGNI NAMESPACE ESTRATTO VADO AD ESEGUIRE LE AZIONI SPECIFICATE NEL FILE SYNCHROMASSIVE
                // 1. per ogni med (riga della actionsqueue) leggo il ns
                // 2. con il ns vado a cercare il corrispondente riga di mapping nel massivesynchroprofiles
                // 3. devo creare il folder della collection a cui appartiene in namespace con l'action MAGO_EA_FOLDER con questa query:
                // select CollectionID AS VFCODICEID_K, DMS_Collection.Name AS VFNAME,
                // (SELECT CollectionID FROM DMS_Collection WHERE DMS_Collection.Name = 'Repository') AS VFPARENT,
                // DMS_Collection.Name AS VFDESCRI, 'MAGOSERVER' AS VFEDSARCHIVE
                // from DMS_Collection
                // join DMS_Collector on DMS_Collector.CollectorID = DMS_Collection.CollectorID
                // and DMS_Collection.TemplateName = 'Standard' and DMS_Collection.Name = '{5o token}' and DMS_Collector.Name = '{3o token}'
                // 4. mi tengo da parte il CollectionID e vado ad eseguire le azioni MAGO_EA_ATTRIBUTI, MAGO_EA_CLASSEDOC
                // 5. faccio la query in ds_synchinfo + mastertable e prendiamo tbguid + valore pk
                // 6. faccio query su EA per trovare gli AttachmentId da sincronizzare (spezzare la query in 500 righe alla volta?)
                // SELECT AttachmentID FROM (SELECT [ErpDocumentID], [DocNamespace], [PrimaryKeyValue], [DescriptionValue] FROM [DMS_ErpDocument]
                // WHERE DocNamespace = 'Document.ERP.Sales.Documents.Invoice'
                // and (PrimaryKeyValue like '%:70;' or PrimaryKeyValue like '%:169;' or PrimaryKeyValue like '%:181;')) k join
                // (select AttachmentID, ErpDocumentID from DMS_Attachment) l on k.ErpDocumentID = l.ErpDocumentID
                // 7. vado a richiamare la MAGO_EA_ATTACH

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

                    NamespaceMap doc = (NamespaceMap)smpp.NamespaceMapInfo.Documents.Find(d => d.Name.Equals(med.EntityName));
                    if (doc == null)
                        continue;

                    bool result = true;
                    string message = string.Empty;
                    string collectionId = string.Empty;

                    // creo il folder relativo alla collection
                    List<ActionToExport> xmlList = GetXmlForMAGO_EA_FOLDER(doc, out collectionId);
                    foreach (ActionToExport action in xmlList)
                    {
                        if (!(result = SyncroRef.ExecuteSyncroWithoutConnect(action, out message)))
                        {
                            LogWriter.WriteToLog(CompanyName, ProviderName, message, "DMSInfinitySynchroProvider.ExecuteMAGO_EA_FOLDER");
                            break;
                        }
                    }
                    // se non sono riuscita a creare il folder oppure il collectionId e' empty non procedo e passo al namespace successivo
                    if (!result || string.IsNullOrWhiteSpace(collectionId))
                        continue;

                    // creo gli attributi (anche se ho qualche errore procedo cmq)
                    xmlList = GetXmlForMAGO_EA_ATTRIBUTI_CLASSEDOC("MAGO_EA_ATTRIBUTI", collectionId);
                    foreach (ActionToExport action in xmlList)
                    {
                        if (!SyncroRef.ExecuteSyncroWithoutConnect(action, out message))
                            LogWriter.WriteToLog(CompanyName, ProviderName, message, "DMSInfinitySynchroProvider.ExecuteMAGO_EA_ATTRIBUTI");
                    }

                    // creo la classe documentale e il legame con gli attributi (anche se ho qualche errore procedo cmq)
                    xmlList = GetXmlForMAGO_EA_ATTRIBUTI_CLASSEDOC("MAGO_EA_CLASSEDOC", collectionId);
                    foreach (ActionToExport action in xmlList)
                    {
                        if (!SyncroRef.ExecuteSyncroWithoutConnect(action, out message))
                            LogWriter.WriteToLog(CompanyName, ProviderName, message, "DMSInfinitySynchroProvider.ExecuteMAGO_EA_CLASSEDOC");
                    }

                    // vado a comporre le stringhe per la creazione degli allegati
                    DataTable dtPkValues;
                    xmlList = GetXmlForMAGO_EA_ATTACH(doc, startSynchroDate, bDelta, out dtPkValues);

                    // per visualizzare nel monitor le righe che andremo ad aggiornare
                    // inserisco una riga in DS_ActionsLog e in DS_SynchroInfo e le metto d'ufficio con lo stato ToSynchro
                    foreach (ActionToExport action in xmlList)
                        foreach (string tbGuid in action.TBGuid)
                        {
                            if (!string.IsNullOrWhiteSpace(tbGuid))
                                InsertSynchronizationData(SynchroStatusType.ToSynchro, tbGuid, med, startSynchroDate, bDelta, ifExistsDoNothing: false);
                        }

                    bool success = true;

                    // vado ad eseguire ogni stringa xml per creare gli allegati
                    foreach (ActionToExport action in xmlList)
                    {
                        message = string.Empty;

                        SyncroRef.ExecuteSyncroWithoutConnect(action, out message);

                        if (!string.IsNullOrWhiteSpace(action.TBGuid[0]))
                        {
                            if (action.IsSucceeded[0])
                            {
                                InsertSynchronizationData(SynchroStatusType.Synchro, action.TBGuid[0], med, startSynchroDate, bDelta, ifExistsDoNothing: true);

                                if (!string.IsNullOrEmpty(action.Keys[0]))
                                {
                                    // todo: decidere se e' necessario inserire nella tabella di transcodifica (utile per la cancellazione) oppure
                                    // se ci bastano le info scritte nell'actiondata
                                    //TranscodingMng.InsertRow(CompanyConnectionString, action.Name, action.Keys[count], action.Name, action.Keys[count], actionToSynch.DocTBGuid);
                                    InsertAttachmentSynchroInfo(SynchroStatusType.Synchro, action.TBGuid[0], action.Keys[0], doc.Name, 0, SynchroActionType.Massive);
                                }
                            }
                            else
                            {
                                LogWriter.WriteToLog(CompanyName, ProviderName, med.EntityName, "DMSInfinitySynchroProvider.ExecuteMassiveExport", string.IsNullOrWhiteSpace(message) ? action.ErrorMessages[0] : message);
                                InsertSynchronizationData(SynchroStatusType.Error, action.TBGuid[0], med, startSynchroDate, bDelta, string.IsNullOrWhiteSpace(message) ? action.ErrorMessages[0] : message);

                                if (!string.IsNullOrEmpty(action.Keys[0]))
                                    InsertAttachmentSynchroInfo(SynchroStatusType.Error, action.TBGuid[0], action.Keys[0], doc.Name, 0, SynchroActionType.Massive);

                                success = false;
                            }
                        }
                    }

                    // vado ad aggiornare la riga nella tabella DS_ActionsQueue
                    UpdateActionQueueStatus(success ? SynchroStatusType.Synchro : SynchroStatusType.Error, med.LogId, string.Empty);
                }

                if (bDelta)
                    UpdateSynchronizationDirtyData(startSynchroDate);

                LogWriter.WriteToLog(CompanyName, ProviderName, "Finished ExecuteMassiveExport", "DMSInfinitySynchroProvider.ExecuteMassiveExport", ProviderName);
            }

            
        }

        ///<summary>
        /// Query cablata per creare il folder parent MAGO_ROOT
        ///</summary>
        //---------------------------------------------------------------------
        private List<ActionToExport> GetXmlForMAGO_EA_FOLDER_PARENT()
        {
            List<ActionToExport> myList = new List<ActionToExport>();

            CRMActionInfo crmActionInfo = new CRMActionInfo(GetActionByName("MAGO_EA_FOLDER"));
            if (crmActionInfo == null || crmActionInfo.Actions.Count == 0)
                return myList;

            CRMAction magoEAFolder = crmActionInfo.Actions[0];

            magoEAFolder.Select = "CollectionID AS VFCODICEID_K, 'MAGO_EA_ROOT' AS VFNAME, -1 AS VFPARENT, 'MAGO_EA_ROOT' AS VFDESCRI, 'MAGOARC' AS VFEDSARCHIVE";
            magoEAFolder.From = "DMS_Collection";
            magoEAFolder.Where = "DMS_Collection.Name = 'Repository'";

            myList = UnparseStringForInsert(crmActionInfo, SynchroActionType.Insert, string.Empty);

            return myList;
        }

        ///<summary>
        /// Query cablata per creare il folder specifico per il namespace del documento passato come parametro
        ///</summary>
        //---------------------------------------------------------------------
        private List<ActionToExport> GetXmlForMAGO_EA_FOLDER(NamespaceMap doc, out string collectionId)
        {
            collectionId = string.Empty;
            List<ActionToExport> myList = new List<ActionToExport>();

            CRMActionInfo crmActionInfo = new CRMActionInfo(GetActionByName("MAGO_EA_FOLDER"));
            if (crmActionInfo == null || crmActionInfo.Actions.Count == 0)
                return myList;

            NameSpace ns = new NameSpace(doc.Name);

            CRMAction magoEAFolder = crmActionInfo.Actions[0];

            magoEAFolder.Select = @"CollectionID AS VFCODICEID_K, CASE DMS_Collection.Name 
			                        WHEN 'Invoice' 
				                        THEN  
					                        CASE DMS_Collector.Name 
						                         WHEN 'Purchases' THEN 'Purch'+ DMS_Collection.Name 
						                         WHEN 'Sales' THEN 'Sales'+ DMS_Collection.Name 
					                        END
			                        ELSE DMS_Collection.Name END AS VFNAME, (SELECT CollectionID FROM DMS_Collection WHERE DMS_Collection.Name = 'Repository')
                                    AS VFPARENT, 
                                    CASE DMS_Collection.Name 
			                        WHEN 'Invoice' 
				                        THEN  
					                        CASE DMS_Collector.Name 
						                         WHEN 'Purchases' THEN 'Purch'+ DMS_Collection.Name 
						                         WHEN 'Sales' THEN 'Sales'+ DMS_Collection.Name 
					                        END
			                        ELSE DMS_Collection.Name  
			                        END AS VFDESCRI, 'MAGOARC' AS VFEDSARCHIVE";

            magoEAFolder.From = string.Format(@"DMS_Collection JOIN DMS_Collector ON DMS_Collector.CollectorID = DMS_Collection.CollectorID AND
                                                DMS_Collection.TemplateName = 'Standard' AND DMS_Collection.Name = '{0}' AND DMS_Collector.Name = '{1}'", ns.Document, ns.Module);

            magoEAFolder.Where = string.Empty;

            myList = UnparseStringForInsert(crmActionInfo, SynchroActionType.Insert, string.Empty);

            try
            {
                if (myList.Count > 0)
                {
                    XDocument xDoc = XDocument.Parse(myList[0].XmlToImport);
                    if (xDoc != null)
                        collectionId = xDoc.Element("MAGO_EA_FOLDER").Element("Add_MAGO_EA_FOLDER").Attribute("VFCODICEID_K").Value;
                }
            }
            catch (Exception)
            {
            }

            return myList;
        }

        ///<summary>
        /// Aggiungo alla WHERE clause il collectionId da utilizzare per l'estrazione dei dati
        ///</summary>
        //---------------------------------------------------------------------
        private List<ActionToExport> GetXmlForMAGO_EA_ATTRIBUTI_CLASSEDOC(string action, string collectionId)
        {
            List<ActionToExport> myList = new List<ActionToExport>();

            CRMActionInfo crmActionInfo = new CRMActionInfo(GetActionByName(action));
            if (crmActionInfo == null || crmActionInfo.Actions.Count == 0)
                return myList;

            CRMAction magoEA = crmActionInfo.Actions[0];

            magoEA.Where = string.Format(magoEA.Where, collectionId);

            return UnparseStringForInsert(crmActionInfo, SynchroActionType.Insert, string.Empty);
        }

        ///<summary>
        /// 1. faccio la query in ds_synchinfo + mastertable e prendiamo tbguid + valore pk
        /// 2. faccio query su EA per trovare gli AttachmentId da sincronizzare (spezzare la query in 500 righe alla volta?)
        ///  SELECT AttachmentID FROM (SELECT [ErpDocumentID], [DocNamespace], [PrimaryKeyValue], [DescriptionValue] FROM [DMS_ErpDocument]
        ///  WHERE DocNamespace = 'Document.ERP.Sales.Documents.Invoice' AND PrimaryKeyValue LIKE '%:70;') k
        ///  JOIN (SELECT AttachmentID, ErpDocumentID FROM DMS_Attachment) l ON k.ErpDocumentID = l.ErpDocumentID
        ///</summary>
        //---------------------------------------------------------------------
        private List<ActionToExport> GetXmlForMAGO_EA_ATTACH(NamespaceMap doc, DateTime startSyncDate, bool bDelta, out DataTable dtPkValues)
        {
            List<ActionToExport> myList = new List<ActionToExport>();
            string query = string.Empty;
            if (bDelta)
            {
                query = string.Format
                            (
                            @"SELECT DocTBGuid, '{0}:' + CAST({0} AS varchar(max)) + ';' AS PKKEY, '' AS AttachmentId, DocNamespace
                            FROM DS_SynchronizationInfo
                            JOIN {1} ON {1}.TBGuid = DS_SynchronizationInfo.DocTBGuid
                            WHERE ProviderName = 'CRMInfinity' AND DocNamespace = '{2}' AND SynchStatus = {3} AND (StartSynchDate = '{4}' OR HasDirtyTbModified = '1')",
                                doc.PKName, doc.MasterTable, doc.Name, (int)SynchroStatusType.Synchro, startSyncDate.ToString("yyyy-MM-dd HH:mm:ss.fff")
                            );
            }
            else
            {
                query = string.Format
                            (
                            @"SELECT DocTBGuid, '{0}:' + CAST({0} AS varchar(max)) + ';' AS PKKEY, '' AS AttachmentId, DocNamespace
                            FROM DS_SynchronizationInfo
                            JOIN {1} ON {1}.TBGuid = DS_SynchronizationInfo.DocTBGuid
                            WHERE ProviderName = 'CRMInfinity' AND DocNamespace = '{2}' AND SynchStatus = {3}",
                                doc.PKName, doc.MasterTable, doc.Name, (int)SynchroStatusType.Synchro
                            );
            }
                

            bool isMyTBConnection = false;
            TBConnection tbConnection = OpenDbConnection(out isMyTBConnection);
            //using (TBConnection tbConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            //{
            //tbConnection.Open();

            // eseguo la query sul database di ERP e con un DataAdapter fillo i dati in un datatable
            using (TBDataAdapter sda = new TBDataAdapter(query, tbConnection))
            {
                dtPkValues = new DataTable(doc.MasterTable);
                sda.Fill(dtPkValues);
            }
            //}
            CloseDbConnection(isMyTBConnection);

            if (dtPkValues == null || dtPkValues.Rows.Count == 0)
                return myList;

            //using (TBConnection tbConnection = new TBConnection(connectionStringManager.DMSConnectionString, DBMSType.SQLSERVER))
            //{
            //tbConnection.Open();

            isMyTBConnection = false;
            tbConnection = OpenDMSDbConnection(out isMyTBConnection);

            foreach (DataRow dr in dtPkValues.Rows)
            {
                // estraggo tutti gli AttachmentId per il documento gestionale
                query = string.Format(@"SELECT AttachmentID FROM
                (SELECT [ErpDocumentID], [DocNamespace], [PrimaryKeyValue], [DescriptionValue] FROM [DMS_ErpDocument] WHERE DocNamespace = '{0}' AND PrimaryKeyValue LIKE '%{1}%') k
                JOIN (SELECT AttachmentID, ErpDocumentID FROM DMS_Attachment) l ON k.ErpDocumentID = l.ErpDocumentID", doc.Name, dr["PKKEY"].ToString());

                
                using (TBCommand tbCommand = new TBCommand(query, tbConnection))
                {
                    try
                    {
                        using (IDataReader tbReader = tbCommand.ExecuteReader())
                        {
                            string attList = string.Empty;

                            // concateno gli n-attachmentId per ogni tbguid (con separatore = ;)
                            while (tbReader.Read())
                                attList += tbReader["AttachmentId"].ToString() + ";";

                            dr["AttachmentId"] = attList;
                        }
                    }
                    catch (Exception e)
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "DMSInfinitySynchroProvider.GetXmlForMAGO_EA_ATTACH", ProviderName);
                    }
                    
                }
            }

            CloseDMSDbConnection(isMyTBConnection);
            //}

            CRMActionInfo crmActionInfo = new CRMActionInfo(GetActionByName("MAGO_EA_ATTACH"));
            if (crmActionInfo == null || crmActionInfo.Actions.Count == 0)
                return myList;

            CRMAction magoEAAttach = crmActionInfo.Actions[0];

            // mi tengo da parte la clausola di WHERE specificata nel file dell'azione
            // altrimenti sovrascrivo il placeholder la prima volta e non riesco piu' ad utilizzarlo in seguito!
            string originalWhereClause = magoEAAttach.Where;

            foreach (DataRow dr in dtPkValues.Rows)
            {
                string attachments = dr["AttachmentId"].ToString();
                if (string.IsNullOrWhiteSpace(attachments))
                    continue;

                string[] attArray = attachments.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < attArray.Length; i++)
                {
                    magoEAAttach.Where = originalWhereClause; // serve per mantenere il placeholder
                    magoEAAttach.Where = string.Format(magoEAAttach.Where, attArray[i]);

                    List<ActionToExport> ate = new List<ActionToExport>();

                    ate = UnparseStringForInsert(crmActionInfo, SynchroActionType.Insert, string.Empty);
                    if (ate.Count == 0)
                        continue;
                    // vado sull'elemento 0 perche' sto estraendo per un AttachmentId alla volta
                    ate[0].TBGuid[0] = dr["DocTBGuid"].ToString();
                    ate[0].Keys[0] = attArray[i];

                    myList.AddRange(ate);
                }
            }

            return myList;
        }

        ///<summary>
        /// Per il DMS non e' prevista
        ///</summary>
        //---------------------------------------------------------------------
        internal override void SynchronizeErrorsRecoveryAsynch()
        {
        }

        ///<summary>
        /// Per il DMS non e' prevista
        ///</summary>
        //---------------------------------------------------------------------
        public override void SynchronizeInbound(string authenticationToken, string loginName, string loginPassword, bool loginWindowsAuthentication, string companyName, string companyConnectionString)
        {
        }

        //---------------------------------------------------------------------
        public override bool CreateExternalServer(string extservername, string connstr, out string message)
        {
            message = "Not implemented";
            return false;
        }

        //---------------------------------------------------------------------
        public override bool CheckCompaniesToBeMapped(out string companylist, out string message)
        {
            message = "Not implemented";
            companylist = string.Empty;
            return false;
        }

        //---------------------------------------------------------------------
        public override bool MapCompany(string appreg, int magocompany, string infinitycompany, string taxid, out string message)
        {
            message = "Not implemented";
            return false;
        }

        //---------------------------------------------------------------------
        public override bool UploadActionPackage(string actionpath, out string message)
        {
            message = "Not implemented";
            return false;
        }

        //--------------------------------------------------------------------------------
        internal override void ExecuteMassiveValidation(bool bCheckFK, bool bCheckXSD, string filters, string serializedTree, int workerId)
        {
        }

        //--------------------------------------------------------------------------------
        internal override bool ExecuteValidateDocument(string nameSpace, string guidDoc, string serializedErrors, int workerId, bool includeXsd = true)
        {
            return true;
        }

        //---------------------------------------------------------------------
        public override bool SetConvergenceCriteria(string xmlCriteria, out string message)
        {
            message = "Not implemented";
            return false;
        }

        //---------------------------------------------------------------------
        public override bool GetConvergenceCriteria(string actionName, out string xmlCriteria, out string message)
        {
            message = "Not implemented";
            xmlCriteria = "";
            return false;
        }

        //---------------------------------------------------------------------
        public override bool SetGadgetPerm(out string message)
        {
            message = "Not implemented";
            return false;
        }

        //---------------------------------------------------------------------
        public override bool CheckVersion(string magoVersion, out string message)
        {
            message = string.Empty;
            return SyncroRef.CheckVersion(magoVersion, out message);
        }

        // Metodo che ritorna la TBConnection: se gli è stata passata dall'esterno, la restituisce, altrimenti la crea.
        // Il parametro in out indica se la TBConnection è stata creata in questo metodo
        //---------------------------------------------------------------------
        /// <exception cref="Microarea.TaskBuilderNet.Data.DatabaseLayer.TBException" />
        private TBConnection CreateDMSDbConnection(out bool isMyTBConnection)
        {
            try
            {
                isMyTBConnection = false;

                if (_dmsDbConnection == null)
                {
                    _dmsDbConnection = new TBConnection(connectionStringManager.DMSConnectionString + "MultipleActiveResultSets=True", DBMSType.SQLSERVER);
                    isMyTBConnection = true;
                }

                if (string.IsNullOrEmpty(connectionStringManager.DMSConnectionString))
                    _dmsDbConnection.ConnectionString = connectionStringManager.DMSConnectionString + "MultipleActiveResultSets=True";

                return _dmsDbConnection;
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
        public TBConnection OpenDMSDbConnection(out bool isMyTBConnection)
        {
            lock (this)
            {
                try
                {
                    TBConnection tbConnection = CreateDMSDbConnection(out isMyTBConnection);

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
        public void CloseDMSDbConnection(bool isMyTBConnection)
        {
            lock (this)
            {
                try
                {
                    if (!isMyTBConnection)
                        return;

                    if (_dmsDbConnection != null)
                    {
                        if (_dmsDbConnection.State != ConnectionState.Closed)
                            _dmsDbConnection.Close();

                        _dmsDbConnection.Dispose();
                        _dmsDbConnection = null;
                    }
                }
                catch (Exception e)
                {
                    throw new TBException("Sorry... No message for this exception. See inner exception", e);
                }
            }
        }
    }
}
