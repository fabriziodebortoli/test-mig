using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Parsers;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Unparsers;
using Microarea.TaskBuilderNet.DataSynchroProviders.Properties;
using Microarea.TaskBuilderNet.DataSynchroUtilities;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM
{
    ///<summary>
    /// Provider per la comunicazione con il servizio di Pat
    /// TODO: prevedere le query anche con sintassi per Oracle
    ///</summary>
    //=========================================================================
    public class InfiniteCRMSynchroProvider : BaseSynchroProvider
    {
        private InfiniteCRMSyncroRef patSyncroReference = null;
        private TranscodingManager transcodingMng = null;

        // dictionary per cachare le informazioni parsate nelle entita'
        private Dictionary<string, CRMEntityInfo> crmEntityInfoDict = new Dictionary<string, CRMEntityInfo>();

        private List<DTPatValuesToImport> dtToImportList = new List<DTPatValuesToImport>(); // lista dei datatable da importare

        public List<string> PKValues = new List<string>();

        //---------------------------------------------------------------------
        protected override string ApplicationFolderName { get { return "InfiniteCRM"; } }

        //---------------------------------------------------------------------
        internal InfiniteCRMSyncroRef PatSyncroRef { get { if (patSyncroReference == null) patSyncroReference = new InfiniteCRMSyncroRef(CompanyName, LogWriter); return patSyncroReference; } }

        //---------------------------------------------------------------------
        internal TranscodingManager TranscodingMng { get { if (transcodingMng == null) transcodingMng = new TranscodingManager(ProviderName, CompanyName, LogWriter); return transcodingMng; } }

        //---------------------------------------------------------------------
        internal override BaseSynchroProfileParser SynchroProfileParser { get { return new PatSynchroProfileParser(); } }

        //---------------------------------------------------------------------
        public InfiniteCRMSynchroProvider()
        { }

        ///<summary>
        /// Richiama il metodo Execute del InfiniteCRM
        /// Ritorna false se si verifica una qualunque eccezione
        /// Se torna true cmq non e' detto che l'invio sia andato a buon fine
        /// (e' necessario parsare la response e controllare il msg ritornato)
        ///</summary>
        //---------------------------------------------------------------------
        private bool RunExecute(string xml, out string patResponse)
        {
            patResponse = string.Empty;

            // nel caso di server irraggiungibile o cmq problemi di comunicazione
            // riproviamo ad eseguire la chiamata 3 volte ogni 5 secondi
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    patResponse = PatSyncroRef.Execute(xml);
                    IsProviderValid = true;
                    break;
                }
                catch (Exception e)
                {
                    //probabile timeout
                    patResponse = e.Message;
                    LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "InfiniteCRMSynchroProvider.RunExecute", string.Format(Resources.AttemptNr, i + 1));
                    IsProviderValid = false;
                    System.Threading.Thread.Sleep(5000);
                }
            }

            try
            {
                if (string.IsNullOrWhiteSpace(patResponse))
                    return false;

                string responseCode = string.Empty;

                PatSynchroResponseParser.ParseResponse(patResponse, out responseCode);

                if (string.Compare(responseCode, "LogonFailed", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    LogWriter.WriteToLog(CompanyName, ProviderName, Resources.WrongUserPw, "InfiniteCRMSynchroProvider.RunExecute");
                    IsProviderValid = false;
                    return false;
                }
            }
            catch (Exception e)
            {
                // una qualunque altra exception
                patResponse = e.Message;
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "InfiniteCRMSynchroProvider.RunExecute");
                return false;
            }

            return true;
        }

        ///<summary>
        /// Metodo che si occupa di comporre la stringa xml con la sintassi corretta per essere poi inviata a Pat
        /// In base al tipo di azione da eseguire, si deve tener conto del contenuto della colonna ActionData, dove sono
        /// presenti informazioni specifiche memorizzate dal CDNotification, e dalla sua interpretazione sara' possibile
        /// generare l'xml comprensivo delle varie operazioni da eseguire
        ///</summary>
        //--------------------------------------------------------------------------------
        private string UnparseStringForInsertEntity(CRMEntityInfo crmInfo, string pkValue, bool isSubEntity = false)
        {
            QueryValuesToImport(crmInfo, pkValue, isSubEntity);
            if (dtToImportList == null || dtToImportList.Count == 0)
                return string.Empty;

            // gestione dei fields dichiarati FK
            if (crmInfo.Entity.FKFields.Count > 0)
                FindEntityID(crmInfo.Entity.FKFields);

            try
            {
                // richiamo la classe che si occupa di scrivere la stringa xml formattata e sintatticamente corretta da dare poi in pasto al ws del CRM
                return PatUnparseHelper.UnparseStringForEntity(crmInfo.Entity, SynchroActionType.Insert, dtToImportList);
            }
            catch (DSException e)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, e.Source, e.ExtendedInfo);
                return string.Empty;
            }
        }

        ///<summary>
        /// Metodo che si occupa di comporre la stringa xml con la sintassi corretta per essere poi inviata al server Pat
        /// In caso di Update del record devo eseguire la query sul db e NON considero cio' che e' indicato nella colonna ActionData
        /// visto che al momento non gestiamo le righe
        ///</summary>
        //--------------------------------------------------------------------------------
        private string UnparseStringForUpdateEntity(CRMEntityInfo crmInfo, string pkValue, out bool convertedInInsert, MassiveExportData med = null)
        {
            convertedInInsert = false;

            QueryValuesToImport(crmInfo, pkValue, false, med);
            if (dtToImportList == null || dtToImportList.Count == 0)
                return string.Empty;

            // gestione dei fields dichiarati FK
            if (crmInfo.Entity.FKFields.Count > 0)
                FindEntityID(crmInfo.Entity.FKFields);

            string response = string.Empty;
            List<string> erpKeyValuesToDelete = new List<string>();
            string id = string.Empty;
            string getXml = string.Empty;
            string erpKey = string.Empty;

            foreach (DTPatValuesToImport dt in dtToImportList)
            {
                if (dt.MasterDt.Rows.Count == 0)
                    continue;

                // transcodifica se necessario
                if (crmInfo.Entity.IsPrimary)
                {
                    try
                    {
                        erpKey = dt.MasterDt.Rows[0][crmInfo.Entity.TranscodingExternalField].ToString();
                    }
                    catch (Exception e)
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "InfiniteCRMSynchroProvider.UnparseStringForUpdateEntity", string.Format(Resources.ColumnNotExist, crmInfo.Entity.TranscodingExternalField, crmInfo.Entity.Name));
                        continue;
                    }

                    id = TranscodingMng.GetEntityID(connectionStringManager.CompanyConnectionString, crmInfo.Entity.TranscodingTable, erpKey);
                    if (id.Equals("-1"))
                    {
                        if (med == null && crmInfo.Entity.Name.Equals("Order"))
                            return string.Empty;
                        // nel caso in cui non esiste l'EntityId nella tabella di transcodifica provo a richiamare direttamente la INSERT
                        convertedInInsert = true;
                        continue;
                    }
                    erpKeyValuesToDelete.Add(erpKey);
                }
                else
                {
                    try
                    {
                        id = dt.MasterDt.Rows[0][InfiniteCRMConsts.ID].ToString();
                        if (id.Equals("-1"))
                            continue;
                    }
                    catch (Exception e)
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "InfiniteCRMSynchroProvider.UnparseStringForUpdateEntity", string.Format(Resources.ColumnNotExist, InfiniteCRMConsts.ID, crmInfo.Entity.Name));
                        continue;
                    }
                }

                dt.PatID = id;
                // compongo la stringa di Get per farmi tornare i dati del record con il suo id
                getXml += PatUnparseHelper.BuildGetXmlForEntityById(crmInfo.Entity.Name, id);
            }

            // nel caso in cui la stringa di get sia vuota vado direttamente all'Unparse
            if (!string.IsNullOrWhiteSpace(getXml))
            {
                getXml = "<" + InfiniteCRMConsts.Operations + ">" + getXml + "</" + InfiniteCRMConsts.Operations + ">";

                // eseguo la Get
                if (!RunExecute(getXml, out response))
                    return string.Empty;

                PatSynchroResponseInfo psri = PatSynchroResponseParser.GetResponseInfo(response);
                for (int i = 0; i < psri.Results.Count; i++)
                {
                    Result getResult = psri.GetResultByPosition(i);
                    if (getResult != null && string.Compare(getResult.Code, "ObjectNotFound", StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        // se si tratta di entita' primaria presente nella transcodifica ma assente in Pat allora elimino il record e riprovo ad inserirlo
                        if (crmInfo.Entity.IsPrimary)
                        {
                            if (
                                TranscodingMng.DeleteRow(connectionStringManager.CompanyConnectionString, crmInfo.Entity.TranscodingTable, erpKeyValuesToDelete[i]) &&
                                (crmInfo.Entity.SubEntities.Count > 0 ?
                                TranscodingMng.DeleteRowBySubEntityName(connectionStringManager.CompanyConnectionString, erpKeyValuesToDelete[i], crmInfo.Entity.SubEntities[0]) : true)
                               )
                                convertedInInsert = true;
                        }
                    }
                }
            }

            // richiamo la classe che si occupa di scrivere la stringa xml formattata e sintatticamente corretta da dare poi in pasto al ws del CRM
            // ogni unparser deve preoccuparsi di quali informazioni memorizzate nell'oggetto ActionDataInfo ha bisogno
            // (attualmente vengono ritornati tutti i dbtslavebuffered e per ognuno le righe aggiunte/modificate/cancellate)
            try
            {
                return PatUnparseHelper.UnparseStringForEntity(crmInfo.Entity, SynchroActionType.Update, dtToImportList, null, response);
            }
            catch (DSException e)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, e.Source, e.ExtendedInfo);
                return string.Empty;
            }
        }

        ///<summary>
        /// Metodo che si occupa di comporre la stringa xml con la sintassi corretta per essere poi inviata
        /// In caso di eliminazione del record NON devo eseguire la query sul db, ma interpretare cio' che e' indicato nella colonna ActionData
        ///</summary>
        //--------------------------------------------------------------------------------
        private string UnparseStringForDeleteAction(CRMEntityInfo crmInfo, SynchroActionType syncActionType, string actionData)
        {
            try
            {
                ActionDataInfo actDataInfo = ActionDataParser.ParseActionData(syncActionType, actionData);
                if (actDataInfo == null || actDataInfo.KeysForDeleteActionList.Count == 0)
                    return string.Empty;

                string patId = string.Empty;
                string erpKey = string.Empty;

                // se l'entita' e' primaria devo andare a cercare nella Transconding la chiave di Pat
                if (crmInfo.Entity.IsPrimary)
                {
                    if (actDataInfo.KeysForDeleteActionList[0].Values.TryGetValue(crmInfo.Entity.TranscodingField, out erpKey))
                        patId = TranscodingMng.GetEntityID(connectionStringManager.CompanyConnectionString, crmInfo.Entity.TranscodingTable, erpKey);

                    if (patId.Equals("-1"))
                        return string.Empty;
                }
                else
                {
                    System.Collections.IEnumerator enumerator = actDataInfo.KeysForDeleteActionList[0].Values.Keys.GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        object firstKey = enumerator.Current;
                        if (firstKey == null || !actDataInfo.KeysForDeleteActionList[0].Values.TryGetValue((string)firstKey, out erpKey))
                            return string.Empty;
                    }
                }

                crmInfo.Entity.PKValues.Add(erpKey);

                if (string.IsNullOrWhiteSpace(crmInfo.Entity.DeleteTarget) || string.IsNullOrWhiteSpace(crmInfo.Entity.DeleteValue))
                    return string.Empty;

                // compongo la stringa di Get per farmi tornare i dati del record con il suo id
                string getXml = PatUnparseHelper.BuildGetXmlForEntityById(crmInfo.Entity.Name, patId);

                if (string.IsNullOrWhiteSpace(getXml))
                    return string.Empty;

                getXml = "<" + InfiniteCRMConsts.Operations + ">" + getXml + "</" + InfiniteCRMConsts.Operations + ">";

                string getResponse;
                // eseguo la Get verso Pat
                if (!RunExecute(getXml, out getResponse))
                    return string.Empty;

                // se non trovo l'oggetto in pat non procedo
                PatSynchroResponseInfo psri = PatSynchroResponseParser.GetResponseInfo(getResponse);
                for (int i = 0; i < psri.Results.Count; i++)
                {
                    Result getResult = psri.GetResultByPosition(i);
                    if (getResult != null && string.Compare(getResult.Code, "ObjectNotFound", StringComparison.InvariantCultureIgnoreCase) == 0)
                        return string.Empty;
                }

                // richiamo la classe che si occupa di scrivere la stringa xml formattata e sintatticamente corretta da dare poi in pasto al ws del CRM
                return PatUnparseHelper.UnparseStringForEntity(crmInfo.Entity, syncActionType, dtValuesList: null, adi: actDataInfo, getResponseXml: getResponse);
            }
            catch (DSException e)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, e.Source, e.ExtendedInfo);
                return string.Empty;
            }
        }

        ///<summary>
        /// Metodo che si occupa di comporre la stringa xml con la sintassi corretta
        /// In caso di esclusione delle entita' NON devo procedere alla cancellazione, devo andare
        /// a fare la Get degli elementi da disabilitare e imposto, ove possibile, lo stato Disable
        /// Nel caso in cui nella tabella di transcodifica ho dei valori che non esistono piu' in Pat
        /// vado ad eliminare la riga corrispondente
        ///</summary>
        //--------------------------------------------------------------------------------
        private string UnparseStringForExcludeAction(CRMEntityInfo crmInfo, string docNamespace, string docTBGuid /* se vuoto si tratta di massiva */, int offset = 0)
        {
            // se l'entita' non e' predisposta a essere esclusa (quindi disabilitata) non procedo
            if (string.IsNullOrWhiteSpace(crmInfo.Entity.DeleteTarget) || string.IsNullOrWhiteSpace(crmInfo.Entity.DeleteValue))
                return string.Empty;

            try
            {
                // vado a leggere nella tabella di transcodifica tutte le coppie EntityName + EntityId che devo andare a escludere in pat (le mettero' disattive)
                DataTable dtEntityValues = TranscodingMng.GetEntityValuesToExclude(connectionStringManager.CompanyConnectionString, docTBGuid, docNamespace, offset);

                string getXml = string.Empty;

                foreach (DataRow dr in dtEntityValues.Rows)
                {
                    string entityName = dr["EntityName"].ToString();
                    string patId = dr["EntityID"].ToString();

                    // compongo la stringa di Get per farmi tornare i dati del record con il suo id
                    getXml += PatUnparseHelper.BuildGetXmlForEntityById(entityName, patId);
                }

                if (string.IsNullOrWhiteSpace(getXml))
                    return string.Empty;

                getXml = "<" + InfiniteCRMConsts.Operations + ">" + getXml + "</" + InfiniteCRMConsts.Operations + ">";

                string getResponse;
                // eseguo la Get verso Pat
                if (!RunExecute(getXml, out getResponse))
                    return string.Empty;

                // richiamo la classe che si occupa di scrivere la stringa xml formattata e sintatticamente corretta da dare poi in pasto al ws del CRM
                return PatUnparseHelper.UnparseStringForEntity(crmInfo.Entity, SynchroActionType.Exclude, dtValuesList: null, adi: null, getResponseXml: getResponse);
            }
            catch (DSException e)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, e.Source, e.ExtendedInfo);
                return string.Empty;
            }
        }

        ///<summary>
        /// Metodo che si occupa di comporre la query da eseguire per estrarre i dati da mandare a Pat
        /// Viene utilizzato sia per la massiva (ed eventuali filtri), che per le query della notify
        ///</summary>
        //--------------------------------------------------------------------------------
        private string GetQuery(CRMEntityInfo crmInfo, string pkValue, string filters = null, bool isSubEntity = false)
        {
            // compongo la query con SELECT + FROM + WHERE (la where non e' detto che ci sia sempre)
            string query = crmInfo.Entity.GetSQLFormattedQuery();

            if (isSubEntity)
                query = string.Format(query, pkValue);
            else
            {
                // NOTIFY SINGOLA (devo filtrare per TBGuid!)
                if (filters == null)
                {
                    string whereTBGuid = string.Format("{0}.TBGuid = '{1}'", crmInfo.Entity.TranscodingTable, pkValue);
                    // ora devo aggiungere in coda il filtro sul TBGuid
                    if (string.IsNullOrWhiteSpace(crmInfo.Entity.Where))
                        query += " WHERE " + whereTBGuid; // se non esiste una where aggiungo solo il TBGuid
                    else
                        query += " AND " + whereTBGuid; // altrimenti aggiungo il TBGuid in AND
                }
                else
                {
                    // MASSIVA: se ci sono filtri nella massiva li aggiungo e devo controllare l'eventuale MassiveWhere (per gestione Disabled)
                    if (string.IsNullOrWhiteSpace(crmInfo.Entity.MassiveWhereClause) && string.IsNullOrWhiteSpace(filters))
                        return query;
                    else
                    {
                        // aggiungo la WHERE oppure l'AND
                        if (string.IsNullOrWhiteSpace(crmInfo.Entity.Where))
                            query += " WHERE ";
                        else
                            query += " AND ";

                        // se ho degli altri parametri li aggiungo in coda con la concatenazione
                        if (!string.IsNullOrWhiteSpace(crmInfo.Entity.MassiveWhereClause))
                            query += crmInfo.Entity.MassiveWhereClause + " AND ";

                        if (!string.IsNullOrWhiteSpace(filters))
                            query += filters;

                        if (query.EndsWith("AND "))
                            query = query.Substring(0, query.Length - 4); // tolgo l'ultimo AND (se presente)
                    }
                }
            }

            return query;
        }

        ///<summary>
        /// Metodo che si occupa di eseguire la query sul database di Mago e riempire la lista
        /// dei datatable
        ///</summary>
        //--------------------------------------------------------------------------------
        private DataTable ExecuteQuery(string entityName, string query)
        {
            // istanzio un dataTable di appoggio con tutte le righe estratte dalla query sul master
            DataTable masterRecDt = null;

            try
            {
                using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
                {
                    myConnection.Open();

                    // eseguo la query sul database di ERP e con un DataAdapter fillo i dati in un datatable
                    using (TBDataAdapter sda = new TBDataAdapter(query, myConnection))
                    {
                        masterRecDt = new DataTable(entityName);
                        sda.Fill(masterRecDt);
                    }
                }
            }
            catch (TBException ex)
            {
                Debug.WriteLine(ex.Message);
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, string.Format("InfiniteCRMSynchroProvider.ExecuteQuery (Entity '{0}')", entityName), query);
                return null;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, string.Format("InfiniteCRMSynchroProvider.ExecuteQuery (Entity '{0}')", entityName), query);
                return null;
            }

            return masterRecDt;
        }

        //--------------------------------------------------------------------------------
        private void FillDtImportList(DataTable masterRecDt, CRMEntityInfo crmInfo, bool isSubEntity = false)
        {
            // per ogni riga che identifica un master (ovvero la testa di un documento)
            foreach (DataRow masterRow in masterRecDt.Rows)
            {
                // creo il DataTable di un master
                DataTable dtMasterAction = masterRecDt.Clone();
                dtMasterAction.ImportRow(masterRow);

                DTPatValuesToImport dtToImport = new DTPatValuesToImport(dtMasterAction);

                // mi tengo da parte il il valore della PK di Mago, se si tratta di un'entita' primaria
                if (!string.IsNullOrWhiteSpace(crmInfo.Entity.TranscodingExternalField))
                {
                    try
                    {
                        PKValues.Add(masterRow[crmInfo.Entity.TranscodingExternalField].ToString());
                        crmInfo.Entity.PKValues.Add(masterRow[crmInfo.Entity.TranscodingExternalField].ToString());
                    }
                    catch (Exception e)
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "InfiniteCRMSynchroProvider.FillDtImportList", string.Format(Resources.ColumnNotExist, crmInfo.Entity.TranscodingExternalField, crmInfo.Entity.Name));
                        return;
                    }
                }

                if (!isSubEntity) // mi tengo da parte anche il TBGuid (ma solo per i master!)
                {
                    try
                    {
                        crmInfo.Entity.TBGuids.Add(masterRow["TBGuid"].ToString());
                    }
                    catch (Exception e)
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "InfiniteCRMSynchroProvider.FillDtImportList", string.Format(Resources.ColumnNotExist, "TBGuid", crmInfo.Entity.Name));
                        return;
                    }
                }

                dtToImportList.Add(dtToImport); // aggiungo alla lista il master correntemente analizzato
            }
        }

        ///<summary>
        /// Prima faccio la query per la master action e riempio il suo datatable
        /// Poi per ognuno eseguo le query delle eventuali subaction e riempio tutti i DataTable figli necessari
        /// Alla fine ho una lista di DTValuesToImport da dare in pasto all'unparser
        ///</summary>
        //--------------------------------------------------------------------------------
        private void QueryValuesToImport(CRMEntityInfo crmInfo, string pkValue, bool isSubEntity, MassiveExportData med = null)
        {
            string query = GetQuery(crmInfo, pkValue, (med == null) ? null : med.Filters, isSubEntity);

            // se sono in massiva e sono un'entita' primaria spezzo la query a blocchi di 200 righe
            if (med != null && crmInfo.Entity.IsPrimary)
                query = string.Format("SELECT * FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY {0}) rn FROM ({1}) t) q WHERE rn BETWEEN {2} AND {2} + 199;",
                            crmInfo.Entity.TranscodingExternalField, query, med.OffSet);

            DataTable dt = ExecuteQuery(crmInfo.Entity.Name, query);

            // faccio sempre la Clear prima in modo da togliere i valori vecchi estratti dalle query precedenti
            // e cmq prima di fare la Fill, altrimenti abbiamo risultati travisati
            dtToImportList.Clear();
            PKValues.Clear();
            crmInfo.Entity.TBGuids.Clear();
            crmInfo.Entity.PKValues.Clear();

            if (dt != null && dt.Rows.Count > 0)
                FillDtImportList(dt, crmInfo, isSubEntity);
        }

        ///<summary>
        /// Richiama il parser del file corrispondente all'entity passata come parametro
        /// (ha lo stesso nome + estensione xml)
        ///</summary>
        //--------------------------------------------------------------------------------
        private CRMEntityInfo ParseEntity(string entity)
        {
            EntityParser entityParser = new EntityParser();
            CRMEntityInfo crmEntityForERP = null;

            string entityFullPath = string.Empty;

            // prima cerco l'entita' per ERP e creo la struttura CRMInfo
            if (SynchroFilesActionsFolderDict.TryGetValue(DatabaseLayerConsts.ERPSignature, out entityFullPath))
            {
                entityFullPath = Path.Combine(entityFullPath, entity + ".xml");
                if (File.Exists(entityFullPath))
                {
                    try
                    {
                        if (entityParser.ParseFile(entityFullPath))
                            crmEntityForERP = entityParser.CrmEntityInfo;
                    }
                    catch (Exception ex)
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "InfiniteCRMSynchroProvider.ParseEntity");
                    }
                }
                // se il file non esiste procedo cmq, perche' l'entita' potrebbe essere legata ad un documento del solo verticale
            }

            // scorro i file per le altre applicazioni, skippando l'applicazione ERP perche' l'ho gia' analizzata
            foreach (KeyValuePair<string, string> kvp in SynchroFilesActionsFolderDict)
            {
                if (string.Compare(kvp.Key, DatabaseLayerConsts.ERPSignature, StringComparison.InvariantCultureIgnoreCase) == 0)
                    continue;

                entityFullPath = Path.Combine(kvp.Value, entity + ".xml");
                if (!File.Exists(entityFullPath)) // se non trovo il file continuo sugli eventuali altri
                    continue;

                try
                {
                    // se crmEntityForERP e' diversa da NULL significa che sono in append e quindi devo aggiungere le info alle esistenti
                    if (crmEntityForERP != null)
                        entityParser.ParseFile(entityFullPath, crmEntityForERP);
                    else
                        entityParser.ParseFile(entityFullPath);
                }
                catch (Exception ex)
                {
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "InfiniteCRMSynchroProvider.ParseEntity");
                    continue;
                }
            }

            return (crmEntityForERP != null) ? crmEntityForERP : entityParser.CrmEntityInfo;
        }

        ///<summary>
        /// ritorna la struttura CRMEntityInfo per l'entita' passata come parametro
        /// il parse del file relativo viene fatto solo se necessario
        ///</summary>
        //--------------------------------------------------------------------------------
        private CRMEntityInfo GetEntityByName(string entity)
        {
            CRMEntityInfo crmInfo = null;

            if (!crmEntityInfoDict.TryGetValue(entity, out crmInfo))
            {
                // parse del file con le info dell'azione
                crmInfo = ParseEntity(entity);
                if (crmInfo != null)
                    crmEntityInfoDict.Add(entity, crmInfo);
            }

            return crmInfo;
        }

        ///<summary>
        /// Cerco l'ID da passare a Pat, relativamente alle colonne che si riferiscono ad entita' primarie
        /// Scorro la lista dei field di tipo FK, carico in memoria il relativo file in memoria, leggo
        /// il nome della tabella di transcodifica e identifico il valore nel relativo DataTable con il nome
        /// A questo punto faccio una query nella tabella di transcodifica per trovare l'EntityID memorizzato
        /// in Pat e sostituisco il valore nel DataTable (che verra' poi utilizzato per scrivere la stringa xml
        /// da inviare a Pat)
        ///</summary>
        //--------------------------------------------------------------------------------
        private void FindEntityID(List<CRMField> fkFields)
        {
            foreach (CRMField field in fkFields)
            {
                CRMEntityInfo entityInfo = GetEntityByName(field.Entity);
                if (entityInfo == null)
                    continue;

                foreach (DTPatValuesToImport dtToImport in dtToImportList)
                {
                    foreach (DataRow dr in dtToImport.MasterDt.Rows)
                    {
                        string erpKey = string.Empty;
                        try
                        {
                            erpKey = dr[field.Target].ToString();
                        }
                        catch (Exception e)
                        {
                            LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "InfiniteCRMSynchroProvider.FindEntityID", string.Format(Resources.ColumnNotExist, field.Target, entityInfo.Entity.Name));
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(erpKey))
                        {
                            dr[field.Target] = "-1"; // la FK nell'entita' di partenza non e' empty in Mago forzo il valore -1 per Pat (soprattutto per la massiva)
                            continue;
                        }

                        string entityId = TranscodingMng.GetEntityID(connectionStringManager.CompanyConnectionString, entityInfo.Entity.TranscodingTable, erpKey);
                        dr[field.Target] = entityId;
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        public override bool Notify(int logID, bool onlyForDMS, string iMagoConfigurations)
        {
            if (!IsProviderValid)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "LogonFailed", "InfiniteCRMSynchroProvider.Notify", string.Format(Resources.ProviderNotValid, ProviderName));
                return false;
            }

            if (logID <= 0) return false;

            // dato il logID vado a leggere sul database le informazioni dalla tabella DS_ActionsLog e riempio l'apposita struttura
            EntityToSynch entityToSynch = new EntityToSynch(LoadInfoToSynch(logID));
            if (entityToSynch == null) // almeno l'entita' corrente da eseguire deve essere presente, altrimenti non procedo
                return false;

            // dato il namespace del documento carico le entita' da eseguire dal file dei profili di sincronizzazione
            PatDocumentToSync doc = GetDocumentToSyncFromNs(entityToSynch.DocNamespace) as PatDocumentToSync;
            if (doc == null || doc.Entities.Count == 0)
                return false;

            // metto lo stato a wait sulla riga
            SetWaitStatus(logID);

            bool sent = false;

            // scorro le entita' previste per quel documento, richiamo il parse
            foreach (DSEntity dsEntity in doc.Entities)
            {
                switch (entityToSynch.ActionType)
                {
                    case SynchroActionType.Insert:
                        sent = InsertEntity(dsEntity, entityToSynch);
                        break;

                    case SynchroActionType.Update:
                        sent = UpdateEntity(dsEntity, entityToSynch);
                        break;

                    case SynchroActionType.Delete:
                        sent = DeleteEntity(dsEntity, entityToSynch);
                        break;

                    case SynchroActionType.Exclude:
                        sent = ExcludeEntity(dsEntity, entityToSynch);
                        break;
                }
            }

            // vado ad aggiornare la riga in DS_ActionsLog e nella DS_SynchronizationInfo
            if (sent)
            {
                if (entityToSynch.ActionType == SynchroActionType.Delete)
                    DeleteSynchronizationData(entityToSynch.DocTBGuid); // se la cancellazione in Pat e' andata a buon fine vado a cancellare tutta la storia nel nostro db
                else
                    UpdateSynchronizationData((entityToSynch.ActionType == SynchroActionType.Exclude) ? SynchroStatusType.Excluded : SynchroStatusType.Synchro, string.Empty, string.Empty, entityToSynch.LogID);
            }
            else
                if (entityToSynch.ActionType != SynchroActionType.Exclude)
                UpdateSynchronizationData(SynchroStatusType.Error, entityToSynch.SynchXMLData, entityToSynch.ResultString, entityToSynch.LogID);

            return sent;
        }

        ///<summary>
        /// In questa procedura la connectionString alla company e' locale, ovvero
        /// quella passata direttamente nella classe DataSynchroDatabaseInfo
        /// (per evitare che venga cambiata sotto banco alla Notify ed alla SynchronizeInbound)
        ///</summary>
        //---------------------------------------------------------------------
        public override void SynchronizeInbound(string authenticationToken, string loginName, string loginPassword, bool loginWindowsAuthentication, string companyName, string companyConnectionString)
        {
            //se la massiva e' stata lanciata scarto l'operazione e riprovo tra n minuti (predefiniti on web.config)
            if (SharedLock.Instance.IsMassiveSynchronizing)
                return;

            Task task = Task.Factory.StartNew(() =>
                {
                    if (!IsProviderValid)
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, "LogonFailed", "InfiniteCRMSynchroProvider.SynchronizeInbound", string.Format(Resources.ProviderNotValid, ProviderName));
                        return;
                    }

                    LogWriter.WriteToLog(CompanyName, ProviderName, string.Empty, "Starting SynchronizeInbound...");

                    // viene chiamato il provider specifico che deve occuparsi di:
                    // 1. chiamare il WS dell'applicazione esterna e farsi ritornare i dati richiesti (esegue la Get - decidere eventuali filtri)
                    // (per Pat dobbiamo tenerci da parte l'elenco degli ID della testa e delle righe)
                    // 2. trasformare l'xml ritornato e generare un altro xml con una sintassi compatibile per MagicLink
                    // 3. l'xml trasformato deve essere suddiviso per singolo documento
                    // 4. viene fatta la LoginCompact a TbServices + CreateTB
                    // 5. ogni documento viene inserito in mago tramite una SetData (potrebbe esserci una lista di oggetti)
                    // 6. analizzare la risposta della SetData e, solo per Pat, andare a scrivere nella tabella di transcodifica i dati necessari

                    // viene rimpallato ai singoli provider la richiesta di xml all'esterno

                    //**********************************************************//
                    //     lettura dalla ds_queue ed esecuzione di pending script  //
                    //**********************************************************//
                    /*---------RECOVERY----------------*/
                    recoveryMng = new InfiniteCRMRecoveryManager(this, loginName, loginPassword, loginWindowsAuthentication, companyName, connectionStringManager.CompanyConnectionString);
                    recoveryMng.StartRecovery();
                    /*---------RECOVERY----------------*/

                    List<SetDataInfo> setDataInfoList = GetInboundProspectsXml(companyConnectionString);

                    // attenzione che questa lista contiene tutti i prospect che devono essere inseriti come clienti
                    // e in coda tutti gli ordini da importare (legati o meno ai prospect).

                    if (setDataInfoList != null)
                    {
                        int res = SetData(loginName, loginPassword, loginWindowsAuthentication, companyName, companyConnectionString, setDataInfoList);

                        if (res == 1) /*---------RECOVERY----------------*/
                            recoveryMng.ChangeActionsQueueStatus(SynchroStatusType.Error);

                        if (res == 0) /*---------RECOVERY----------------*/
                            recoveryMng.ChangeActionsQueueStatus(SynchroStatusType.Synchro);

                        FinalizeSetData(setDataInfoList, companyConnectionString);
                    }

                    setDataInfoList = GetInboundOrdersXml(companyConnectionString);

                    if (setDataInfoList != null)
                    {
                        int res = SetData(loginName, loginPassword, loginWindowsAuthentication, companyName, companyConnectionString, setDataInfoList);
                        if (res == 1)
                            /*---------RECOVERY----------------*/
                            recoveryMng.ChangeActionsQueueStatus(SynchroStatusType.Error);
                        if (res == 0)
                            /*---------RECOVERY----------------*/
                            recoveryMng.ChangeActionsQueueStatus(SynchroStatusType.Synchro);

                        FinalizeSetData(setDataInfoList, companyConnectionString);
                    }

                    LogWriter.WriteToLog(CompanyName, ProviderName, string.Empty, "Ending SynchronizeInbound...");
                }, TaskCreationOptions.PreferFairness);
        }

        ///<summary>
        /// Metodo richiamato in fase di INSERT della sola entita' master
        /// (non si usa per la massiva)
        ///</summary>
        //---------------------------------------------------------------------
        private bool InsertEntity(DSEntity dsEntity, EntityToSynch ets)
        {
            CRMEntityInfo masterEntityCrmInfo = GetEntityByName(dsEntity.Name);
            if (masterEntityCrmInfo == null) // se non trovo la descrizione dell'entita' mi fermo subito
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, string.Empty, "InfiniteCRMSynchroProvider.InsertEntity", string.Format(Resources.MissingEntity, dsEntity.Name));
                return false;
            }

            // richiamo l'unparse dell'entita' master
            string xml = UnparseStringForInsertEntity(masterEntityCrmInfo, ets.DocTBGuid);
            if (string.IsNullOrWhiteSpace(xml))
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, string.Empty, "InfiniteCRMSynchroProvider.InsertEntity", Resources.XmlStringEmpty);
                return false;
            }

            string response = string.Empty;
            // eseguo l'xml del master
            bool execResult = RunExecute(xml, out response);

            bool sent = false;

            // se il master e' andato a buon fine
            if (execResult)
            {
                // parso la risposta ritornata
                PatSynchroResponseInfo prsi = PatSynchroResponseParser.GetResponseInfo(response);
                Result result = prsi.GetResultByPosition(0); // leggo il result alla posizione 0 perche' eseguo un master alla volta!

                if (prsi.GetSynchroStatusByPosition(0) == SynchroStatusType.Synchro)
                {
                    sent = true;

                    // se si tratta dell'inserimento di un'entita' primaria allora vado ad inserire la riga nella DS_Transcoding
                    if (masterEntityCrmInfo.Entity.IsPrimary)
                    {
                        for (int y = 0; y < PKValues.Count; y++)
                        {
                            string pk = PKValues[y];
                            result = prsi.GetResultByPosition(y);
                            TranscodingMng.InsertRow(connectionStringManager.CompanyConnectionString, masterEntityCrmInfo.Entity.TranscodingTable, pk, masterEntityCrmInfo.Entity.Name, result.Id, ets.DocTBGuid);
                        }
                    }
                }
                else // se e' fallito l'invio del master mi tengo da parte l'xml per l'eventuale recovery
                {
                    ets.SynchXMLData = (masterEntityCrmInfo.Entity.SubEntities.Count > 0) ? ("[master]" + xml) : xml;
                    ets.ResultString = (result != null) ? result.ExtendedErrorDescription : response;
                }
            }
            else
            {
                ets.SynchXMLData = (masterEntityCrmInfo.Entity.SubEntities.Count > 0) ? ("[master]" + xml) : xml;
                ets.ResultString = response;
            }

            // GESTIONE SUBENTITIES (solo se l'inserimento del master e' andato a buon fine)
            if (masterEntityCrmInfo.Entity.SubEntities.Count > 0 && sent)
            {
                foreach (string subE in masterEntityCrmInfo.Entity.SubEntities)
                {
                    // per ogni subentity carico le info
                    CRMEntityInfo subEntitycrmInfo = GetEntityByName(subE);
                    foreach (string pk in masterEntityCrmInfo.Entity.PKValues)
                        sent = sent && InsertSubEntity(subEntitycrmInfo, pk, ref ets.SynchXMLData, ref ets.ResultString);
                }
            }

            return sent;
        }

        ///<summary>
        /// Metodo richiamato in fase di INSERT di una SubEntity a fronte di un INSERT di un'entita' master
        ///</summary>
        //---------------------------------------------------------------------
        private bool InsertSubEntity(CRMEntityInfo subEntitycrmInfo, string masterPKValue, ref string synchXMLDataToDB, ref string resultString)
        {
            string xml = UnparseStringForInsertEntity(subEntitycrmInfo, masterPKValue, true);
            if (string.IsNullOrWhiteSpace(xml))
                return true;

            string response = string.Empty;

            // eseguo l'xml dello slave
            bool sent = RunExecute(xml, out response);

            if (sent)
            {
                // parso la risposta ritornata
                PatSynchroResponseInfo prsi = PatSynchroResponseParser.GetResponseInfo(response);

                // trattandosi di subentity potrei avere piu' righe, quindi devo ciclare sui risultati ed analizzarli per posizione
                for (int i = 0; i < prsi.Results.Count; i++)
                {
                    Result subEResult = prsi.Results[i];
                    if (subEResult.GetResultSynchroStatus() == SynchroStatusType.Synchro)
                    {
                        // se si tratta dell'inserimento di un'entita' primaria allora vado ad inserire la riga nella DS_Transcoding
                        if (subEntitycrmInfo.Entity.IsPrimary)
                        {
                            string pk = PKValues[i];
                            TranscodingMng.InsertRow
                                (
                                connectionStringManager.CompanyConnectionString,
                                subEntitycrmInfo.Entity.TranscodingTable,
                                string.Concat(masterPKValue, "_", pk), // concateno la PK del master con la PK di slave
                                subEntitycrmInfo.Entity.Name,
                                subEResult.Id
                                );
                            // N.B. il Concat tra la PK del master (CustSupp) con la PK di slave server solo per non avere conflitti di PK nella tabella DS_Transcoding
                        }
                    }
                    else
                    {
                        synchXMLDataToDB += GetOperationNodeFragmentByIdx(xml, i);
                        resultString += (subEResult != null) ? subEResult.ExtendedErrorDescription : string.Empty;
                    }
                }
            }
            else
                synchXMLDataToDB += "@@@" + xml;

            return sent;
        }

        ///<summary>
        /// Metodo richiamato in fase di UPDATE di un'entita' master
        ///</summary>
        //---------------------------------------------------------------------
        private bool UpdateEntity(DSEntity dsEntity, EntityToSynch ets)
        {
            CRMEntityInfo masterEntityCrmInfo = GetEntityByName(dsEntity.Name);
            if (masterEntityCrmInfo == null) // se non trovo la descrizione dell'entita' mi fermo subito
                return false;

            bool convertedInInsert = false;

            // richiamo l'unparse dell'entita' master
            string xml = UnparseStringForUpdateEntity(masterEntityCrmInfo, ets.DocTBGuid, out convertedInInsert);
            if (string.IsNullOrWhiteSpace(xml))
            {
                if (masterEntityCrmInfo.Entity.Name.Equals("Order"))
                    return true;
                return false;
            }

            string response = string.Empty;
            // eseguo l'xml del master per il suo Update
            bool execResult = RunExecute(xml, out response);

            bool sent = false;

            // se l'update e' andato a buon fine
            if (execResult)
            {
                // parso la risposta ritornata
                PatSynchroResponseInfo prsi = PatSynchroResponseParser.GetResponseInfo(response);
                Result result = prsi.GetResultByPosition(0);

                if (prsi.GetSynchroStatusByPosition(0) == SynchroStatusType.Synchro)
                {
                    sent = true;

                    // se si tratta dell'inserimento di un'entita' primaria allora vado ad inserire la riga nella DS_Transcoding
                    if (masterEntityCrmInfo.Entity.IsPrimary && convertedInInsert)
                    {
                        for (int y = 0; y < PKValues.Count; y++)
                        {
                            string pk = PKValues[y];
                            result = prsi.GetResultByPosition(y);
                            TranscodingMng.InsertRow(connectionStringManager.CompanyConnectionString, masterEntityCrmInfo.Entity.TranscodingTable, pk, masterEntityCrmInfo.Entity.Name, result.Id, ets.DocTBGuid);
                        }
                    }
                }
                else // se e' fallito l'invio del master mi tengo da parte l'xml per l'eventuale recovery
                {
                    ets.SynchXMLData = (masterEntityCrmInfo.Entity.SubEntities.Count > 0) ? ("[master]" + xml) : xml;
                    ets.ResultString = (result != null) ? result.ExtendedErrorDescription : response;
                }
            }
            else
            {
                ets.SynchXMLData = (masterEntityCrmInfo.Entity.SubEntities.Count > 0) ? ("[master]" + xml) : xml;
                ets.ResultString = response;
            }

            //---------------------------------------------------------------------
            // GESTIONE SUBENTITIES (solo se l'update del master e' andato a buon fine)
            //---------------------------------------------------------------------
            // questo metodo viene richiamato solo dalla Notify, quindi sul singolo documento
            if (masterEntityCrmInfo.Entity.SubEntities.Count > 0 && sent)
            {
                // mi tengo da parte il valore della pk di mago che mi serve per sostituire il placeholder per comporre la where clause
                string pkVal = PKValues[0];

                foreach (string subE in masterEntityCrmInfo.Entity.SubEntities)
                {
                    // per ogni subentity carico le info
                    CRMEntityInfo subEntitycrmInfo = GetEntityByName(subE);
                    sent = sent && UpdateSubEntity(subEntitycrmInfo, pkVal, ref ets.SynchXMLData, ref ets.ResultString);
                }
            }

            return sent;
        }

        ///<summary>
        /// Gestione specifica per l'Update delle righe
        /// Visti i problemi con le chiavi primarie, si e' deciso di eliminare in Pat tutte le righe e re-inserire da zero quelle
        /// presenti in Mago
        ///</summary>
        //---------------------------------------------------------------------
        private bool UpdateSubEntity(CRMEntityInfo subEntitycrmInfo, string pkValue, ref string synchXMLDataToDB, ref string resultString)
        {
            bool sent = false;

            if (!subEntitycrmInfo.Entity.IsPrimary)
                sent = true; // se si tratta di subentity secondaria forzo la Insert
            else
            {
                // Estraggo dalla tabella DS_Transcoding tutte gli EntityID legati al master
                List<DTPatValuesToImport> subEntityIDList = TranscodingMng.GetSubEntityIDList(connectionStringManager.CompanyConnectionString, subEntitycrmInfo.Entity.Name, pkValue);

                if (subEntityIDList.Count == 0)
                    sent = true; // se non ho trovato righe slave salvate nella transcondifica procedo con l'inserimento
                else
                {
                    // altrimenti elimino tutte le righe e le re-inserisco
                    string xmlDeleteAllRows = string.Empty;

                    try
                    {
                        xmlDeleteAllRows = PatUnparseHelper.UnparseStringForEntity(subEntitycrmInfo.Entity, SynchroActionType.Delete, subEntityIDList);
                    }
                    catch (DSException e)
                    {
                        LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, e.Source, e.ExtendedInfo);
                    }

                    if (!string.IsNullOrWhiteSpace(xmlDeleteAllRows))
                    {
                        string response = string.Empty;

                        if (RunExecute(xmlDeleteAllRows, out response))
                        {
                            // parso la risposta ritornata
                            PatSynchroResponseInfo prsi = PatSynchroResponseParser.GetResponseInfo(response);

                            for (int i = 0; i < prsi.Results.Count; i++)
                            {
                                if (prsi.GetSynchroStatusByPosition(i) != SynchroStatusType.Synchro)
                                {
                                    Result result = prsi.GetResultByPosition(i);
                                    synchXMLDataToDB += "";
                                    resultString += result.ExtendedErrorDescription;
                                }
                            }

                            sent = TranscodingMng.DeleteRowBySubEntityName(connectionStringManager.CompanyConnectionString, pkValue, subEntitycrmInfo.Entity.Name);
                        }
                        else
                        {
                            // se e' fallito completamente la cancellazione mi tengo da parte l'xml per l'eventuale recovery
                            synchXMLDataToDB += xmlDeleteAllRows;
                        }
                    }
                }
            }

            if (sent)
                sent = InsertSubEntity(subEntitycrmInfo, pkValue, ref synchXMLDataToDB, ref resultString);

            return sent;
        }

        ///<summary>
        /// Cancellazione della testa del documento!
        /// In Pat NON effettuiamo nessuna cancellazione, se e' possibile facciamo la Set per disabilitare
        /// Viene controllato se nel file e' stato specificato il tag <Delete>:
        /// - se non e' presente torno subito true
        /// - se e' presente devo fare la Get da Pat, sostituire i valori specificati nel nodo <Delete> e rifare una Set
        /// NON abbiamo una DELETE massiva!!
        ///</summary>
        //---------------------------------------------------------------------
        private bool DeleteEntity(DSEntity dsEntity, EntityToSynch ets)
        {
            CRMEntityInfo crmInfo = GetEntityByName(dsEntity.Name);
            if (crmInfo == null) // se non trovo la descrizione dell'entita' mi fermo subito
                return false;

            // faccio pulizia delle pk precedentemente memorizzate
            crmInfo.Entity.PKValues.Clear();

            string deleteXml = UnparseStringForDeleteAction(crmInfo, ets.ActionType, ets.ActionData);

            // se ho una stringa da eseguire la mando a Pat
            if (!string.IsNullOrWhiteSpace(deleteXml))
            {
                // invio l'xml a Pat
                string response = string.Empty;
                // non mi serve parsare la risposta, perche' se e' fallita cmq non posso fare nulla
                RunExecute(deleteXml, out response);
            }

            // elimino la riga del master e tutte le eventuali righe dalla tabella di transcodifica (ma solo se non sono in Exclude!)
            if (crmInfo.Entity.IsPrimary)
            {
                if (TranscodingMng.DeleteRow(connectionStringManager.CompanyConnectionString, crmInfo.Entity.TranscodingTable, crmInfo.Entity.PKValues[0]))
                {
                    if (crmInfo.Entity.SubEntities.Count > 0)
                    {
                        foreach (string subEntity in crmInfo.Entity.SubEntities)
                        {
                            CRMEntityInfo crmSubEInfo = GetEntityByName(subEntity);
                            // devo fare in questo modo perche' nel caso delle Offerte andiamo a richiamare cmq l'entity Order
                            // quindi non posso passare direttamente il nome dell'entity originale perche' non sarebbe corretta
                            if (crmSubEInfo != null)
                                TranscodingMng.DeleteRowBySubEntityName(connectionStringManager.CompanyConnectionString, crmInfo.Entity.PKValues[0], crmSubEInfo.Entity.Name);
                        }
                    }
                }
            }

            return true;
        }

        ///<summary>
        /// In Pat NON effettuiamo nessuna cancellazione, se e' possibile facciamo la Set per disabilitare
        /// Viene controllato se nel file e' stato specificato il tag <Delete>:
        /// - se non e' presente torno subito true
        /// - se e' presente devo fare la Get da Pat, sostituire i valori specificati nel nodo <Delete> e rifare una Set
        ///</summary>
        //---------------------------------------------------------------------
        private bool ExcludeEntity(DSEntity dsEntity, EntityToSynch ets)
        {
            CRMEntityInfo crmInfo = GetEntityByName(dsEntity.Name);
            if (crmInfo == null) // se non trovo la descrizione dell'entita' mi fermo subito
                return false;

            string excludeXml = UnparseStringForExcludeAction(crmInfo, ets.DocNamespace, ets.DocTBGuid);

            // se ho una stringa da eseguire la mando a Pat
            if (!string.IsNullOrWhiteSpace(excludeXml))
            {
                // invio l'xml a Pat
                string response = string.Empty;
                // non mi serve parsare la risposta, perche' se e' fallita cmq non posso fare nulla
                if (!RunExecute(excludeXml, out response))
                    return false;
            }

            return true;
        }

        ///<summary>
        /// Metodo che dato un xml di tipo fragment (piu' operazioni per le subentity)
        /// torna l'i-esimo xml
        ///</summary>
        //---------------------------------------------------------------------
        private string GetOperationNodeFragmentByIdx(string xml, int index)
        {
            XmlDocument doc = new XmlDocument();

            try
            {
                doc.LoadXml(xml);
                XmlNodeList nodeList = doc.SelectNodes(InfiniteCRMConsts.Operations + "/" + InfiniteCRMConsts.Operation);
                if (nodeList.Count >= index)
                    return nodeList[index].InnerXml;
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "InfiniteCRMSynchroProvider.GetOperationNodeFragmentByIdx");
                return string.Empty;
            }

            return string.Empty;
        }

        //--------------------------------------------------------------------------------
        internal override void ExecuteMassiveExport(List<MassiveExportData> medList, DateTime startSynchroDate, bool isRecovery = false, bool bDelta = false)
        {
            // per ogni namespace che devo esportare in Pat
            // vado a parsare il suo file
            // alla query di select devo aggiungere nella where gli eventuali filtri
            // e NON devo aggiungere il coda il filtro sul TBGuid
            // devo tenere traccia della PK di Mago e del TBGuid (per andare ad aggiornare sia la DS_Transcoding che la DS_ActionsLog)
            foreach (MassiveExportData med in medList)
            {
                MassiveInsert(med, startSynchroDate);
            }
        }

        //---------------------------------------------------------------------
        private void MassiveInsert(MassiveExportData med, DateTime startSynchroDate)
        {
            bool sent = true;

            // dato il namespace del documento carico le entita' da eseguire dal file dei profili di sincronizzazione
            PatDocumentToSync doc = GetDocumentToSyncFromNs(med.EntityName) as PatDocumentToSync;
            if (doc == null || doc.Entities.Count == 0)
            {
                UpdateActionQueueStatus(SynchroStatusType.Synchro, med.LogId, string.Empty);
                return;
            }

            // dato il nome dell'entita' carico in memoria il suo file xml (diamo per scontato che ce ne sia sempre una)
            CRMEntityInfo masterEntityCrmInfo = GetEntityByName(doc.Entities[0].Name);
            if (masterEntityCrmInfo == null) // se non trovo la descrizione dell'entita' mi fermo subito
            {
                UpdateActionQueueStatus(SynchroStatusType.Synchro, med.LogId, string.Empty);
                return;
            }

            // se ho specificato dei filtri devo andare prima ad impostare lo stato = NoSynchro in tabella DS_SynchInfo
            // nelle righe che hanno quel namespace con lo stato Synchro
            if (!string.IsNullOrWhiteSpace(med.Filters))
                UpdateStatusForExclude(doc.Name, SynchroStatusType.NoSynchro, SynchroStatusType.Synchro);

            for (int k = 0; k <= Int32.MaxValue; k++) // almeno una volta entra perche' k <= 0
            {
                if (!masterEntityCrmInfo.Entity.IsPrimary && k > 0)
                    break;

                med.OffSet = k * 200 + 1;
                // richiamo l'unparse dell'entita' master
                bool isInsert;
                string xml = UnparseStringForUpdateEntity(masterEntityCrmInfo, string.Empty, out isInsert, med);
                if (string.IsNullOrWhiteSpace(xml))
                {
                    // se la stringa di update e' vuota e sono al primo giro ritorno
                    if (k == 0 && !string.IsNullOrWhiteSpace(med.Filters))
                    {
                        UpdateStatusForExclude(doc.Name, SynchroStatusType.Synchro, SynchroStatusType.NoSynchro);
                        UpdateActionQueueStatus(SynchroStatusType.Synchro, med.LogId, string.Empty);
                        return;
                    }

                    UpdateActionQueueStatus(SynchroStatusType.Synchro, med.LogId, string.Empty);
                    // altrimenti faccio break
                    break;
                }

                // prima di andare ad eseguire l'INSERT in Pat per ogni documento estratto dalla query devo:
                // 1. inserire/aggiornare la riga nella DS_SynchronizatioInfo con TBGuid, namespace, statosynchro = WAIT
                // 2. inserire una riga nella DS_ActionsLog con TBGuid, namespace, statosynchro = WAIT
                List<string> logIdList = InsertSynchronizationData(masterEntityCrmInfo.Entity.TBGuids, med, startSynchroDate);
                UpdateActionQueueStatus(SynchroStatusType.Wait, med.LogId, string.Empty);

                string response = string.Empty;

                // eseguo l'xml del master se l'inserimento dei master e' andato a buon fine
                if (RunExecute(xml, out response))
                {
                    // parso la risposta ritornata
                    PatSynchroResponseInfo prsi = PatSynchroResponseParser.GetResponseInfo(response);

                    for (int i = 0; i < prsi.Results.Count; i++)
                    {
                        Result result = prsi.GetResultByPosition(i);

                        if (result.GetResultSynchroStatus() == SynchroStatusType.Synchro)
                        {
                            sent = (sent && true);

                            // se si tratta dell'inserimento di un'entita' primaria allora vado ad inserire la riga nella DS_Transcoding
                            if (masterEntityCrmInfo.Entity.IsPrimary)
                            {
                                string pk = PKValues[i];
                                TranscodingMng.InsertRow(connectionStringManager.CompanyConnectionString, masterEntityCrmInfo.Entity.TranscodingTable, pk, masterEntityCrmInfo.Entity.Name, result.Id, masterEntityCrmInfo.Entity.TBGuids[i]);
                            }

                            // la sincronizzazione e' andata a buon fine e vado ad aggiornare le righe nelle tabelle
                            SetStatus(Convert.ToInt32(logIdList[i]), SynchroStatusType.Synchro);
                        }
                        else
                        {
                            // gestione errore
                            SetStatus(Convert.ToInt32(logIdList[i]), SynchroStatusType.Error);
                            sent = (sent && false);
                        }
                    }
                }
                else
                {
                    foreach (string logId in logIdList)
                        SetStatus(Convert.ToInt32(logId), SynchroStatusType.Error);
                    UpdateActionQueueStatus(SynchroStatusType.Error, med.LogId, xml);
                    if (!string.IsNullOrWhiteSpace(med.Filters))
                        UpdateStatusForExclude(doc.Name, SynchroStatusType.Synchro, SynchroStatusType.NoSynchro);
                    return;
                }

                string synchXMLDataToDB = string.Empty;
                string resultString = string.Empty;

                // GESTIONE SUBENTITIES (solo se l'inserimento del master e' andato a buon fine)
                if (masterEntityCrmInfo.Entity.SubEntities.Count > 0 && sent)
                {
                    foreach (string subE in masterEntityCrmInfo.Entity.SubEntities)
                    {
                        // per ogni subentity carico le info
                        CRMEntityInfo subEntitycrmInfo = GetEntityByName(subE);
                        foreach (string pk in masterEntityCrmInfo.Entity.PKValues)
                            sent = sent && UpdateSubEntity(subEntitycrmInfo, pk, ref synchXMLDataToDB, ref resultString);

                        // TODO: se non riesce ad inserire la subentity che faccio???
                    }
                }

                if (!sent)
                    UpdateActionQueueStatus(SynchroStatusType.Error, med.LogId, synchXMLDataToDB);
                else
                    UpdateActionQueueStatus(SynchroStatusType.Synchro, med.LogId, string.Empty);
            }

            // faccio la Dispose di tutti i datatable in memoria
            foreach (DTPatValuesToImport dt in dtToImportList)
                dt.MasterDt.Dispose();

            // se ho specificato dei filtri devo andare ad aggiornare le entita' in Pat mettendole disattive
            // e poi andare ad impostare le righe in DS_SynchInfo mettendo lo stato Exclude
            if (!string.IsNullOrWhiteSpace(med.Filters))
            {
                for (int k = 0; k <= Int32.MaxValue; k++)
                {
                    string excludeRowsXml = UnparseStringForExcludeAction(masterEntityCrmInfo, doc.Name, string.Empty, k * 200 + 1);

                    // se ho una stringa da eseguire la mando a Pat
                    if (!string.IsNullOrWhiteSpace(excludeRowsXml))
                    {
                        // invio l'xml a Pat
                        string response = string.Empty;
                        // non mi serve parsare la risposta, perche' se e' fallita cmq non posso fare nulla
                        if (!RunExecute(excludeRowsXml, out response))
                            return;
                    }
                    else
                        break;
                }

                // alla fine faccio cmq l'update delle righe mettendo lo stato Excluded
                UpdateStatusForExclude(doc.Name, SynchroStatusType.Excluded, SynchroStatusType.NoSynchro);
            }
        }

        // <summary>
        /// Chiamata asincrona della SynchronizeErrorsRecovery
        /// </summary>
        //--------------------------------------------------------------------------------
        internal override void SynchronizeErrorsRecoveryAsynch()
        {
            if (!IsProviderValid)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, "LogonFailed", "BaseSynchroProvider.SynchronizeErrorsRecovery", string.Format(Resources.ProviderNotValid, ProviderName));
                return;
            }
        }

        ///<summary>
        /// Utilizzato nel cambio dei filtri della massiva di Pat
        ///</summary>
        //---------------------------------------------------------------------
        private void UpdateStatusForExclude(string docNamespace, SynchroStatusType newSynchroStatus, SynchroStatusType oldSynchroStatus)
        {
            using (TBConnection tbConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {
                try
                {
                    tbConnection.Open();

                    using (TBCommand tbCommand = new TBCommand(tbConnection))
                    {
                        tbCommand.CommandText = string.Format(@"UPDATE [DS_SynchronizationInfo] SET [SynchStatus] = {0}, [SynchDate] = GetDate()
                                                WHERE [DocNamespace] = '{1}' AND [SynchStatus] = {2} AND [ProviderName] = '{3}'",
                                                (int)newSynchroStatus,
                                                docNamespace,
                                                (int)oldSynchroStatus,
                                                ProviderName);
                        tbCommand.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "InfiniteCRMSynchroProvider.UpdateStatusForExclude");
                }
            }
        }

        ///<summary>
        /// Metodo che si occupa di effettuare l'INSERT iniziale nella DS_ActionsLog e DS_SynchronizationInfo
        /// in fase di inserimento massivo in Pat
        /// A fronte di un'azione di sincronizzazione massiva per un tipo documento (namespace), e' necessario
        /// comporre la query e scrivere il testo xml da inviare all'esterno.
        /// Prima bisogna "esplodere" tutti i documenti suddividendoli per TBGuid ed andare ad inserire n-righe
        /// nella tabella DS_ActionsLog e DS_SynchronizationInfo
        ///</summary>
        //---------------------------------------------------------------------
        internal List<string> InsertSynchronizationData(List<string> docTBGuidList, MassiveExportData med, DateTime startSynchroDate)
        {
            List<string> logIdList = new List<string>();

            string insertActionsLog = @"INSERT INTO [DS_ActionsLog]
                                        ([ProviderName], [DocNamespace], [DocTBGuid], [ActionType], [ActionData], [SynchDirection], [SynchXMLData], [SynchStatus], [SynchMessage], [TBCreatedID], [TBModifiedID])
                                        VALUES (@ProviderName, @DocNamespace, @DocTBGuid, @ActionType, '', @SynchDirection, '', @SynchStatus, '', @TBCreatedID, @TBModifiedID);
                                        SELECT CAST(scope_identity() AS int)";

            using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {
                myConnection.Open();

                TBCommand myCommand = new TBCommand(myConnection);

                try
                {
                    foreach (string docTBGuid in docTBGuidList)
                    {
                        myCommand.CommandText = insertActionsLog;

                        myCommand.Parameters.Add("@ProviderName", ProviderName);
                        myCommand.Parameters.Add("@DocNamespace", med.EntityName);
                        myCommand.Parameters.Add("@DocTBGuid", docTBGuid);
                        myCommand.Parameters.Add("@ActionType", (int)SynchroActionType.Massive);
                        myCommand.Parameters.Add("@SynchDirection", (int)SynchroDirectionType.Outbound);
                        myCommand.Parameters.Add("@SynchStatus", (int)SynchroStatusType.Wait);
                        myCommand.Parameters.Add("@TBCreatedID", med.TBCreatedID);
                        myCommand.Parameters.Add("@TBModifiedID", med.TBModifiedID);

                        logIdList.Add(myCommand.ExecuteScalar().ToString());

                        // per ogni riga di DS_ActionsLog vado ad inserire/aggiornare la riga con lo stesso TBGuid nella DS_SynchronizationInfo
                        InsertSynchronizationInfo(SynchroStatusType.Wait, docTBGuid, med.EntityName, med.TBCreatedID, SynchroActionType.Massive, SynchroDirectionType.Outbound, startSynchroDate);

                        myCommand.Parameters.Clear();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "InfiniteCRMSynchroProvider.InsertSynchronizationData");
                }
                finally
                {
                    myCommand.Dispose();
                }
            }

            return logIdList;
        }

        ///<summary>
        /// Metodo re-implementato per la richiesta degli ordini da sincronizzare direttamente a Pat
        ///</summary>
        //---------------------------------------------------------------------
        internal List<SetDataInfo> GetInboundProspectsXml(string companyConnString)
        {
            List<SetDataInfo> sdiList = new List<SetDataInfo>();

            // richiedo a PAT i Prospects con degli ordini da inviare a Mago
            string xmlProspects = GetProspects();

            if (!string.IsNullOrWhiteSpace(xmlProspects))
            {
                XElement rootProspectFragment = PatSynchroResponseParser.GetRootElement(xmlProspects);

                if (rootProspectFragment != null)
                {
                    /*---------RECOVERY----------------*/
                    recoveryMng.AddRowsToRecovery(xmlProspects, "Account");
                    /*---------RECOVERY----------------*/

                    List<SetDataInfo> accountList = GetProspectEntities(rootProspectFragment, companyConnString);
                    if (accountList != null && accountList.Count > 0)
                        sdiList.AddRange(accountList);
                }
            }

            return sdiList;
        }

        //---------------------------------------------------------------------
        internal List<SetDataInfo> GetInboundOrdersXml(string companyConnString)
        {
            List<SetDataInfo> sdiList = new List<SetDataInfo>();

            string xmlOrder = GetOrdersForERP();

            if (!string.IsNullOrWhiteSpace(xmlOrder))
            {
                XElement rootOrderFragment = PatSynchroResponseParser.GetRootElement(xmlOrder);
                if (rootOrderFragment != null)
                {
                    /*---------RECOVERY----------------*/
                    recoveryMng.AddRowsToRecovery(xmlOrder, "Order");
                    /*---------RECOVERY----------------*/

                    List<SetDataInfo> orderList = GetOrderEntities(rootOrderFragment, companyConnString);
                    if (orderList != null && orderList.Count > 0)
                        sdiList.AddRange(orderList);
                }
            }

            return sdiList;
        }

        //--------------------------------------------------------------------------------
        internal List<SetDataInfo> GetOrderEntities(XElement rootOrderFragment, string companyConnString)
        {
            IEnumerable<XElement> orders = null;

            try
            {
                orders = rootOrderFragment.Element(XName.Get("OrdersComplete")).Elements(XName.Get("OrderComplete"));
            }
            catch (Exception)
            {
                orders = null;
            }

            if (orders == null)
                return null;

            List<SetDataInfo> orderList = new List<SetDataInfo>();
            SetDataInfo singleOrder = null;

            foreach (XElement elem in orders)
            {
                try
                {
                    singleOrder = PatSynchroResponseParser.ParseSingleOrder(elem, TranscodingMng, companyConnString);
                }
                catch (DSException e)
                {
                    LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, e.Source, e.ExtendedInfo);
                    singleOrder = null;
                }

                if (singleOrder == null)
                    continue;
                orderList.Add(singleOrder);
                singleOrder = null;
            }

            return orderList;
        }

        /// <summary>
        /// parsa gli account e ritorna la lista di SetDataInfo
        /// </summary>
        //--------------------------------------------------------------------------------
        internal List<SetDataInfo> GetProspectEntities(XElement rootAccountFragment, string companyConnString)
        {
            IEnumerable<XElement> accounts = null;
            try
            {
                accounts = rootAccountFragment.Element(XName.Get("Accounts")).Elements(XName.Get("Account"));
            }
            catch (Exception e)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, "InfiniteCRMSynchroProvider.GetProspectEntities");
                accounts = null;
            }

            if (accounts == null)
                return null;

            List<SetDataInfo> accountList = new List<SetDataInfo>();
            SetDataInfo singleAccount = null;

            foreach (XElement elem in accounts)
            {
                try
                {
                    singleAccount = PatSynchroResponseParser.ParseSingleAccount(elem, TranscodingMng, companyConnString);
                }
                catch (DSException e)
                {
                    LogWriter.WriteToLog(CompanyName, ProviderName, e.Message, e.Source);
                    singleAccount = null;
                }

                if (singleAccount == null)
                    continue;
                accountList.Add(singleAccount);
                singleAccount = null;
            }

            return accountList;
        }

        ///<summary>
        /// Estraiamo gli ordini da importare in ERP comprensivi di righe
        ///<Operation>
        ///<Get>
        ///      <OrdersComplete>
        ///			<Filters>
        ///				<Filter field="OrderStatusID" op="EqualTo" value="MAGO_CONFIRMING" />
        ///			</Filters>
        ///      </OrdersComplete>
        /// </Get>
        ///</Operation>
        ///</summary>
        //--------------------------------------------------------------------------------
        private string GetOrdersForERP()
        {
            string ordersToImportQry =
                @"<Operations><Operation>
                    <Get>
                        <OrdersComplete>
                            <Filters>
                                <Filter field=""OrderStatusID"" op=""EqualTo"" value=""MAGO_CONFIRMING"" />
                                <Filter field=""ObjectTypeID"" op=""EqualTo"" value=""MAGO_ORDER"" />
                            </Filters>
                        </OrdersComplete>
                    </Get>
                </Operation></Operations>";

            string response = string.Empty;
            string resultCode = string.Empty;

            // cerco tutti gli ordini da importare
            if (!RunExecute(ordersToImportQry, out response) || string.IsNullOrWhiteSpace(response) || !PatSynchroResponseParser.ParseResponse(response, out resultCode, "OrdersComplete"))
                return string.Empty;

            return response;
        }

        ///<summary>
        /// 1. Chiedere tutti gli account di tipo MAGO_PROSPECT che hanno assegnato almeno un ordine con lo stato MAGO_CONFIRMING
        /// I dati si ottengono con questa query:
        ///<Operation>
        ///  <Get>
        ///    <Orders>
        ///      <Filters>
        ///        <Filter field="OrderStatusID" op="EqualTo" value="MAGO_CONFIRMING" />
        ///        <Filter field="Account.ObjectTypeID" op="EqualTo" value="MAGO_PROSPECT" />
        ///      </Filters>
        ///    </Orders>
        ///  </Get>
        ///</Operation>
        /// 2. Da questa risposta dobbiamo individuare tutti i nodi di tipo AccountID e poi, facendo una DISTINCT preventiva
        /// (perché potrebbero essere doppi), eseguire una Get degli Accounts per identificare tutti i dati da importare in Mago
        ///<Operation>
        ///  <Get>
        ///    <Accounts>
        ///      <Filters>
        ///        <Filter field="ID" op="EqualTo" value="accountid-1| accountid-2| accountid-n" />
        ///      </Filters>
        ///    </Accounts>
        ///  </Get>
        ///</summary>
        //--------------------------------------------------------------------------------
        private string GetProspects()
        {
            string ordersWithProspectQry =
                @"<Operations><Operation>
                  <Get>
                    <Orders>
                      <Filters>
                        <Filter field=""OrderStatusID"" op=""EqualTo"" value=""MAGO_CONFIRMING"" />
                        <Filter field=""Account.ObjectTypeID"" op=""EqualTo"" value=""MAGO_PROSPECT"" />
                      </Filters>
                    </Orders>
                  </Get>
                </Operation></Operations>";

            string response = string.Empty;
            string resultCode = string.Empty;

            // cerco tutti gli ordini da importare con un prospect nella testa
            if (!RunExecute(ordersWithProspectQry, out response) || string.IsNullOrWhiteSpace(response) || !PatSynchroResponseParser.ParseResponse(response, out resultCode))
                return string.Empty;

            // analizzo l'xml ritornato, estraggo gli accountID di tipo Prospect da inserire in Mago
            string prospectsQuery = PatSynchroResponseParser.GetProspectsQuery(response);

            if (string.IsNullOrWhiteSpace(prospectsQuery))
                return string.Empty;

            if (!RunExecute(prospectsQuery, out response) || string.IsNullOrWhiteSpace(response) || !PatSynchroResponseParser.ParseResponse(response, out resultCode, "Accounts"))
                return string.Empty;

            return response;
        }

        //--------------------------------------------------------------------------------
        public string GetMagoIds(string response, string entityName)
        {
            return PatSynchroResponseParser.GetKeyFromMago(response, entityName);
        }

        //--------------------------------------------------------------------------------
        public string GetMagoTBGuid(string response, string entityName)
        {
            return PatSynchroResponseParser.GetTBGuidForMago(response, entityName);
        }

        //--------------------------------------------------------------------------------
        internal int SetData(string loginName, string loginPassword, bool loginWindowsAuthentication, string companyName, string companyConnectionString, List<SetDataInfo> setDataInfoList)
        {
            if (setDataInfoList.Count == 0)
                return 0;

            LoginManager loginMng = null;
            TbServices tbServices = null;
            int accumulatingResult = 0; //serve per return per sapere

            try
            {
                tbServices = new TbServices();
                loginMng = GetLoginManager();
                if (loginMng.Login(loginName, loginPassword, loginWindowsAuthentication, companyName, "DataSynchronizer", false) != 0)
                    return -1;

                int result = 0;
                foreach (SetDataInfo dataInfo in setDataInfoList)
                {
                    string setDataResponse = string.Empty;
                    // importo in Mago ogni xml generato il precedenza - prj. 6203
                    result = tbServices.SetData(loginMng.AuthenticationToken, dataInfo.MagoXml, DateTime.Now /*leggere data sull'ordine di pat*/, 0, false, out setDataResponse);

                    //se TBServices non e' disponibile esco
                    if (result == -1)
                    {
                        Debug.WriteLine("Error in SetData: " + setDataResponse);
                        LogWriter.WriteToLog(CompanyName, ProviderName, string.Format("Impossible to create TBServices", dataInfo.EntityName), "BaseSynchroProvider.SetData (CreateTB)", setDataResponse);
                        return -1;
                    }

                    if (result == 1)
                    {
                        dataInfo.Inserted = false; // se impossibile inserire in mago imposto parametro a false
                        InsertSynchronizationInfo(SynchroStatusType.Error, Guid.Empty.ToString(), dataInfo.Namespace, 0, SynchroActionType.Insert, SynchroDirectionType.Inbound, DateTime.Now, companyConnectionString);
                        LogWriter.WriteToLog(CompanyName, ProviderName, string.Format("Error in SetData. Impossible to insert document DS_ActionsQueue.EntityName {0}): ", dataInfo.EntityName), "BaseSynchroProvider.SetData", setDataResponse);
                        accumulatingResult = 1;
                        continue;
                    }

                    //magoID non vuoto se ho fatto l'update del documento esistente
                    if (string.IsNullOrWhiteSpace(dataInfo.MagoID))
                    {
                        string magoID = GetMagoIds(setDataResponse, dataInfo.EntityName);
                        if (string.IsNullOrEmpty(magoID))
                            continue;
                        dataInfo.MagoID = magoID;
                    }

                    string magoTbGuid = GetMagoTBGuid(setDataResponse, dataInfo.EntityName);
                    dataInfo.TBGuid = (string.IsNullOrEmpty(magoTbGuid)) ? Guid.Empty.ToString() : magoTbGuid;

                    InsertSynchronizationInfo(SynchroStatusType.Synchro, dataInfo.TBGuid, dataInfo.Namespace, 0, SynchroActionType.Insert, SynchroDirectionType.Inbound, DateTime.Now, companyConnectionString);
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "BaseSynchroProvider.SetData");
                return -1;
            }
            finally
            {
                if (tbServices != null && loginMng != null)
                {
                    tbServices.CloseTB(loginMng.AuthenticationToken);
                    loginMng.LogOff();
                }
            }

            return accumulatingResult;
        }

        //--------------------------------------------------------------------------------
        internal void FinalizeSetData(List<SetDataInfo> dataInfoList, string companyConnString)
        {
            if (dataInfoList.Count == 0)
                return;

            foreach (SetDataInfo dataInfo in dataInfoList)
            {
                if (!dataInfo.Inserted) // se stato l'errore del documento durante la setdata non inserisco in transcodifica la riga del documento e non vado a cambiare lpo stato
                    continue;

                // vado ad inserire la riga nella tabella di transcodifica solo se ho la chiave di Mago valorizzata
                if (!string.IsNullOrWhiteSpace(dataInfo.MagoID))
                    if (!transcodingMng.SafeInsertMasterAndRows(companyConnString, dataInfo))
                        continue;
            }

            string updateQuery = PatSynchroResponseParser.GetPatUpdateString(dataInfoList);
            if (string.IsNullOrEmpty(updateQuery))
                return;

            string response = string.Empty;
            if (!RunExecute(updateQuery, out response))
                Debug.WriteLine("Error in InfiniteCRMSynchroProvider.FinalizeSetData: " + response);
        }

        //---------------------------------------------------------------------
        public override bool TestProviderParameters(string url, string username, string password, bool skipCrtValidation, string parameters, out string message)
        {
            message = string.Empty;
            return PatSyncroRef.TestProviderParameters(url, username, password, out message);
        }

        //---------------------------------------------------------------------
        public override bool SetProviderParameters(string authenticationToken, string url, string username, string password, bool skipCrtValidation, string IAFModule, string parameters, out string message)
        {
            message = string.Empty;
            return PatSyncroRef.SetProviderParameters(url, username, password, out message);
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
            message = "Not implemented";
            return false;
        }

    }
}
