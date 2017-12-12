using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.DataSynchroUtilities;

namespace Microarea.TaskBuilderNet.DataSynchroProviders
{
    ///<summary>
    /// Classe che si occupa di effettuare le operazioni nella tabella DS_Transcoding,
    /// ovvero la tabella che memorizza le chiavi di trascodifica tra Mago e terze parti
    /// Viene istanziata nei singoli provider che la vogliono utilizzare 
    ///</summary>
    //================================================================================
    public class TranscodingManager
    {
        //------------------------------------------------------------------------------------
        private string providerName;
        private string companyName;
        private IProviderLogWriter LogWriter { get; set; }

        //-------------------------------------------------------------------------------------
        public TranscodingManager(string providerName, string companyName, IProviderLogWriter logWriter)
        {
            this.providerName = providerName;
            this.companyName = companyName;
            this.LogWriter = logWriter;
        }

        //---------------------------------------------------------------------
        private TBConnection _dbConnection = null;

        //---------------------------------------------------------------------
        public void SetDbConnection(TBConnection dbConnection)
        {
            if (_dbConnection != null)
            {
                if (_dbConnection.State != ConnectionState.Closed)
                    _dbConnection.Close();

                _dbConnection.Dispose();
            }

            _dbConnection = dbConnection;
        }

        //---------------------------------------------------------------------
        private TBConnection CreateDbConnection(string companyConnectionString, out bool isMyTBConnection)
        {
            try
            {
                isMyTBConnection = false;

                if (_dbConnection == null)
                {
                    _dbConnection = new TBConnection(companyConnectionString, DBMSType.SQLSERVER);
                    isMyTBConnection = true;
                }

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

        //---------------------------------------------------------------------
        /// <exception cref="Microarea.TaskBuilderNet.Data.DatabaseLayer.TBException" />
        public TBConnection OpenDbConnection(string companyConnectionString, out bool isMyTBConnection)
        {
            TBConnection tbConnection = CreateDbConnection(companyConnectionString, out isMyTBConnection);

            if (tbConnection.State != ConnectionState.Open)
                tbConnection.Open();

            return tbConnection;
        }

        //---------------------------------------------------------------------
        /// <exception cref="Microarea.TaskBuilderNet.Data.DatabaseLayer.TBException" />
        public void CloseDbConnection(bool isMyTBConnection)
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

        //-------------------------------------------------------------------------------------
        public string GetEntityID(string companyConnectionString, string erpTableName, string erpKey)
        {
            string entityID = "-1";

            bool isMyTBConnection = false;

            // TODO: prevedere anche la sintassi Oracle!!
            try
            {
                TBConnection myConnection = OpenDbConnection(companyConnectionString, out isMyTBConnection);

                string query = string.Format("SELECT [EntityID] FROM [DS_Transcoding] WHERE [ProviderName] = '{0}' AND [ERPTableName] = '{1}' AND [ERPKey] = '{2}'", providerName, erpTableName, erpKey);
                using (TBCommand myCommand = new TBCommand(query, myConnection))
                {
                    IDataReader dr = myCommand.ExecuteReader();
                    if (dr != null && myConnection.DataReaderHasRows(dr))
                    {
                        if (dr.Read())
                            entityID = dr["EntityID"].ToString();

                        dr.Close();
                        dr.Dispose();
                    }
                }
            }
            catch (TBException ex)
            {
                Debug.WriteLine(string.Format("GetEntityID(erpTableName={0}, erpKey={1}", erpTableName, erpKey), ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "TranscodingManager.GetEntityID");
                return entityID;
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return entityID;
        }

        //-------------------------------------------------------------------------------------
        public string GetRecordKey(string companyConnectionString, string entityName, string entityID)
        {
            string recordKey = string.Empty;

            bool isMyTBConnection = false;

            // TODO: prevedere anche la sintassi Oracle!!
            try
            {
                TBConnection myConnection = OpenDbConnection(companyConnectionString, out isMyTBConnection);

                string query = string.Format("SELECT [ERPKey] FROM [DS_Transcoding] WHERE [ProviderName] = '{0}' AND [EntityName] = '{1}' AND [EntityID] = '{2}'", providerName, entityName, entityID);
                using (TBCommand myCommand = new TBCommand(query, myConnection))
                {
                    IDataReader dr = myCommand.ExecuteReader();
                    if (dr != null && myConnection.DataReaderHasRows(dr))
                    {
                        if (dr.Read())
                            recordKey = dr["ERPKey"].ToString();

                        dr.Close();
                        dr.Dispose();
                    }
                }
            }
            catch (TBException ex)
            {
                Debug.WriteLine(string.Format("GetRecordKey(entityName={0}, entityID={1}", entityName, entityID), ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "TranscodingManager.GetRecordKey");
                return string.Empty;
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return recordKey;
        }

        //-------------------------------------------------------------------------------------
        public DataTable GetEntityValuesToExclude(string companyConnectionString, string docTBGuid, string docNamespace, int offset)
        {
            DataTable dtEntityValues = new DataTable();

            // TODO: prevedere anche la sintassi Oracle!!

            TBDataAdapter dataAdapter = null;

            bool isMyTBConnection = false;

            try
            {
                TBConnection myConnection = OpenDbConnection(companyConnectionString, out isMyTBConnection);

                string query = string.IsNullOrWhiteSpace(docTBGuid)
                                ? string.Format(@" SELECT * FROM (SELECT [EntityName], [EntityID], ROW_NUMBER() OVER (ORDER BY [EntityID]) as num  FROM [DS_Transcoding]  JOIN (SELECT [DocTBGuid] FROM [DS_SynchronizationInfo] 
										        WHERE [ProviderName] = '{0}' AND [DocNamespace] = '{1}' AND [SynchStatus] = {2}) k ON [DS_Transcoding].[DocTBGuid] = k.DocTBGuid ) l
                                                WHERE num BETWEEN {3} AND {3}+199",
                                                providerName, docNamespace, (int)SynchroStatusType.NoSynchro, offset)
                                : string.Format("SELECT [EntityName],[EntityID] FROM [DS_Transcoding] WHERE [ProviderName] = '{0}' AND [DocTBGuid] = '{1}'", providerName, docTBGuid);

                dataAdapter = new TBDataAdapter(query, myConnection);
                dataAdapter.Fill(dtEntityValues);
            }
            catch (TBException ex)
            {
                Debug.WriteLine(string.Format("GetEntityValuesToExclude(docNamespace={0})", docNamespace), ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "TranscodingManager.GetEntityValuesToExclude");
            }
            finally
            {
                dataAdapter.Dispose();

                CloseDbConnection(isMyTBConnection);
            }

            return dtEntityValues;
        }

        ///<summary>
        /// Per trovare tutte le righe inserite per una subentity
        /// E' necessario fare una una query prendendo nella colonna ErpKey la Substring escludendo il codice
        /// 0058_0fe9d13f-dddf-4550-8242-990513358e7d 
        /// SELECT SUBSTRING(ERPKey, LEN('0058_') + 1, 36) AS ERPKey, EntityID FROM [DS_Transcoding]
        /// WHERE EntityName = 'Contact' AND ERPKey LIKE '0058_%' AND ProviderID = 1
        ///</summary>
        //-------------------------------------------------------------------------------------
        internal List<DTPatValuesToImport> GetSubEntityIDList(string companyConnectionString, string entityName, string erpKey)
        {
            List<DTPatValuesToImport> subEntityIds = new List<DTPatValuesToImport>();

            bool isMyTBConnection = false;

            // TODO: prevedere anche la sintassi Oracle!!

            try
            {
                TBConnection myConnection = OpenDbConnection(companyConnectionString, out isMyTBConnection);

                string query = string.Format
                    (
                    "SELECT [EntityID] AS ID, [ERPKey] AS ERPKey FROM [DS_Transcoding] WHERE [EntityName] = '{1}' AND [ERPKey] LIKE '{0}_%' AND [ProviderName] = '{2}'",
                    erpKey, entityName, providerName
                    );

                DataTable myDt = null;

                using (TBDataAdapter sda = new TBDataAdapter(query, myConnection))
                {
                    myDt = new DataTable(entityName);
                    sda.Fill(myDt);
                }

                if (myDt != null && myDt.Rows.Count > 0)
                {
                    DTPatValuesToImport masterDt = new DTPatValuesToImport(myDt);
                    subEntityIds.Add(masterDt);
                }
            }
            catch (TBException ex)
            {
                Debug.WriteLine(string.Format("GetSubEntityIDList(entityName={0}, erpKey={1}", entityName, erpKey), ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "TranscodingManager.GetSubEntityIDList");
                return subEntityIds;
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return subEntityIds;
        }

        //-------------------------------------------------------------------------------------
        public bool InsertRow(string companyConnectionString, string erpTableName, string erpKey, string entityName, string entityID, string docTBGuid = "")
        {
            bool isMyTBConnection = false;

            // TODO: prevedere anche la sintassi Oracle!!
            try
            {
                TBConnection myConnection = OpenDbConnection(companyConnectionString, out isMyTBConnection);

                string query = string.Format(@"INSERT INTO [DS_Transcoding] ([ProviderName], [ERPTableName], [ERPKey], [EntityName], [EntityID], [DocTBGuid]) VALUES
                                            ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", providerName.Replace("'", "''"), erpTableName.Replace("'", "''"), erpKey.Replace("'", "''"), entityName.Replace("'", "''"), entityID.Replace("'", "''"),
                                                                                        string.IsNullOrWhiteSpace(docTBGuid) ? Guid.Empty.ToString() : docTBGuid);

                using (TBCommand myCommand = new TBCommand(query, myConnection))
                    myCommand.ExecuteNonQuery();
            }
            catch (TBException ex)
            {
                if (ex.Number == 2627)
                    return true;
                Debug.WriteLine(string.Format("InsertRow(erpTableName={0}, erpKey={1}, entityName={2}, entityID={3}", erpTableName, erpKey, entityName, entityID), ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "TranscodingManager.InsertRow");
                return false;
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return true;
        }

        //-------------------------------------------------------------------------------------
        public bool DeleteRow(string companyConnectionString, string erpTableName, string erpKey)
        {
            bool isMyTBConnection = false;

            // TODO: prevedere anche la sintassi Oracle!!
            try
            {
                TBConnection myConnection = OpenDbConnection(companyConnectionString, out isMyTBConnection);

                string query = string.Format("DELETE FROM [DS_Transcoding] WHERE [ProviderName] = '{0}' AND [ERPTableName] = '{1}' AND [ERPKey] = '{2}'", providerName, erpTableName, erpKey);

                using (TBCommand myCommand = new TBCommand(query, myConnection))
                    myCommand.ExecuteNonQuery();
            }
            catch (TBException ex)
            {
                Debug.WriteLine(string.Format("DeleteRow(erpTableName={0}, erpKey={1}", erpTableName, erpKey), ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "TranscodingManager.DeleteRow");
                return false;
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return true;
        }

        ///<summary>
        /// Metodo per eliminare le righe relative alla subentity, facendo un like con la pk del master
        ///</summary>
        //-------------------------------------------------------------------------------------
        public bool DeleteRowBySubEntityName(string companyConnectionString, string likeErpKey, string entityName)
        {
            bool isMyTBConnection = false;

            // TODO: prevedere anche la sintassi Oracle!!
            try
            {
                TBConnection myConnection = OpenDbConnection(companyConnectionString, out isMyTBConnection);

                string query = string.Format("DELETE FROM [DS_Transcoding] WHERE [ProviderName] = '{0}' AND [ERPKey] LIKE '{1}_%' AND [EntityName] = '{2}'", providerName, likeErpKey, entityName);

                using (TBCommand myCommand = new TBCommand(query, myConnection))
                    myCommand.ExecuteNonQuery();
            }
            catch (TBException ex)
            {
                Debug.WriteLine(string.Format("DeleteRowBySubEntityName(likeErpKey={0}, entityName={1}", likeErpKey, entityName), ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "TranscodingManager.DeleteRowBySubEntityName");
                return false;
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return true;
        }

        ///<summary>
        /// Metodo per eliminare le righe relative al tbguid
        ///</summary>
        //-------------------------------------------------------------------------------------
        public bool DeleteRowByTBGuid(string companyConnectionString, string DocTBGuid)
        {
            bool isMyTBConnection = false;

            // TODO: prevedere anche la sintassi Oracle!!
            try
            {
                TBConnection myConnection = OpenDbConnection(companyConnectionString, out isMyTBConnection);

                string query = string.Format("DELETE FROM [DS_Transcoding] WHERE [DocTBGuid] = '{0}' AND ProviderName = '{1}'", DocTBGuid, providerName);

                using (TBCommand myCommand = new TBCommand(query, myConnection))
                    myCommand.ExecuteNonQuery();
            }
            catch (TBException ex)
            {
                Debug.WriteLine(string.Format("DeleteRowByTBGuid(DocTBGuid={0}}", DocTBGuid), ex.Message);
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "TranscodingManager.DeleteRowByTBGuid");
                return false;
            }
            finally
            {
                CloseDbConnection(isMyTBConnection);
            }

            return true;
        }

        ///<summary>
        /// Inserimento dei riferimenti di testa e righe nella tabella di transcodifica
        /// con la gestione della transazione, in modo da rollbackare tutto in caso di errore
        /// (viene utilizzata di solito quando importiamo in Mago prendendo dati da Pat e 
        /// dobbiamo popolare le tabelle di transcodifica)
        ///</summary>
        //--------------------------------------------------------------------------------
        internal bool SafeInsertMasterAndRows(string companyConnectionString, SetDataInfo dataInfo)
        {
            string insertQuery = @"INSERT INTO [DS_Transcoding] ([ProviderName], [ERPTableName], [ERPKey], [EntityName], [EntityID], [DocTBGuid]) 
									VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')";

            bool isMyTBConnection = false;

            IDbTransaction transaction = null;

            try
            {
                TBConnection myConnection = OpenDbConnection(companyConnectionString, out isMyTBConnection);
                transaction = myConnection.BeginTransaction();

                string query = string.Format(insertQuery, providerName, dataInfo.MagoTableName, dataInfo.MagoID, dataInfo.EntityName, dataInfo.PatID, dataInfo.TBGuid);

                TBCommand myCommand = new TBCommand(query, myConnection);
                myCommand.Transaction = transaction;

                myCommand.ExecuteNonQuery();

                if (dataInfo.PatRows.Count > 0)
                {
                    foreach (SetDataInfo patRow in dataInfo.PatRows)
                    {
                        // per le righe degli ordini assegno magoid_patid come ERPKey (per evitare chiave duplicata nelle righe)
                        query = string.Format(insertQuery, providerName, patRow.MagoTableName, dataInfo.MagoID + "_" + patRow.PatID, patRow.EntityName, patRow.PatID, Guid.Empty.ToString());
                        myCommand.CommandText = query;
                        myCommand.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch (TBException ex)
            {
                if (ex.Number == 2627)
                    return true;
                Debug.WriteLine(string.Format("SafeInsertMasterAndRows error: {0}", ex.Message));
                transaction.Rollback();
                LogWriter.WriteToLog(companyName, providerName, ex.Message, "TranscodingManager.SafeInsertMasterAndRows");
                return false;
            }
            finally
            {
                if (transaction != null)
                    transaction.Dispose();

                CloseDbConnection(isMyTBConnection);
            }
            
            return true;
        }
    }
}
