using System;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.DataSynchroUtilities;
using Microarea.TaskBuilderNet.Interfaces;
using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.DataSynchroProviders
{
    //================================================================================
    public abstract class ExternalSynchroProvider : BaseSynchroProvider
    {
        /// <summary>
        /// Inserts or updates the table DS_SynchronizationInfo
        /// The table will be updated if docTBGuid already exists
        /// </summary>
        /// <param name="synchStatus"></param>
        /// <param name="docTBGuid"></param>
        /// <param name="docNamespace"></param>
        /// <param name="workerID"></param>
        /// <param name="actionType"></param>
        /// <param name="synchDirection"></param>
        /// <param name="auxConnectionString">optional</param>
        //--------------------------------------------------------------------------------
        public void InsertOrUpdateDSSynchronizationInfo(SynchroStatusType synchStatus, string docTBGuid, string docNamespace, int workerID, SynchroActionType actionType, SynchroDirectionType synchDirection, string auxConnectionString = "")
        {
            using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {
                myConnection.Open();

                TBCommand aSqlCommand = new TBCommand(myConnection);
                // controllo se esiste nella DS_SynchronizationInfo una riga con quel TBGuid
                string queryexistInfo = "SELECT COUNT(*) FROM [DS_SynchronizationInfo] WHERE [DocTBGuid] = @docTBGuid AND[ProviderName] = @providerName";

                bool exist = false;

                try
                {
                    aSqlCommand = new TBCommand(myConnection);
                    aSqlCommand.CommandText = queryexistInfo;
                    aSqlCommand.Parameters.Add("@docTBGuid", docTBGuid);
                    aSqlCommand.Parameters.Add("@providerName", ProviderName);
                    exist = (int)aSqlCommand.ExecuteScalar() > 0;

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "ExternalSynchroProvider.InsertOrUpdateDSSynchronizationInfo");
                }
                finally
                {
                    aSqlCommand.Dispose();
                }

                try // faccio una INSERT o un UPDATE a seconda dell'esistenza del record 
                {
                    aSqlCommand = new TBCommand(myConnection);

                    if (exist)
                        aSqlCommand.CommandText =
                                    @"UPDATE [DS_SynchronizationInfo] SET [SynchStatus] = @SynchStatus, [SynchDate] = @SynchDate, [SynchDirection] = @SynchDirection, 
                                    [LastAction] = @LastAction, [TBModified] = @TBModified, [TBModifiedID] = @TBModifiedID, [WorkerID] =  @WorkerID WHERE [DocTBGuid] = @DocTBGuid AND[ProviderName] = @providerName";
                    else
                    {
                        aSqlCommand.CommandText = @"INSERT INTO [DS_SynchronizationInfo] ([DocTBGuid], [ProviderName], [DocNamespace], [SynchStatus], [SynchDate], [SynchDirection],
                                                    [LastAction], [TBCreated], [TBModified], [TBCreatedID], [TBModifiedID], [WorkerID]) 
                                                    VALUES (@DocTBGuid, @providerName, @DocNamespace, @SynchStatus, @SynchDate, @SynchDirection, @LastAction, @TBCreated, 
                                                    @TBModified, @TBCreatedID, @TBModifiedID, @WorkerID)";

                        aSqlCommand.Parameters.Add("@DocNamespace", docNamespace);
                        aSqlCommand.Parameters.Add("@TBCreated", DateTime.Now);
                        aSqlCommand.Parameters.Add("@TBCreatedID", workerID);
                    }

                    aSqlCommand.Parameters.Add("@providerName", ProviderName);
                    aSqlCommand.Parameters.Add("@DocTBGuid", docTBGuid);
                    aSqlCommand.Parameters.Add("@SynchStatus", (int)synchStatus);
                    aSqlCommand.Parameters.Add("@SynchDate", DateTime.Now);
                    aSqlCommand.Parameters.Add("@SynchDirection", (int)synchDirection);
                    aSqlCommand.Parameters.Add("@LastAction", (int)actionType);
                    aSqlCommand.Parameters.Add("@TBModified", DateTime.Now);
                    aSqlCommand.Parameters.Add("@TBModifiedID", workerID);
                    aSqlCommand.Parameters.Add("@WorkerID", workerID);
                    aSqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "ExternalSynchroProvider.InsertSynchronizationInfo");
                }
                finally
                {
                    aSqlCommand.Dispose();
                }
            }
        }

        /// <summary>
        /// Delete a line from DS_SynchronizationInfo with a specified DocTBGuid
        /// </summary>
        /// <param name="docTBGuid"></param>
        /// <param name="auxConnectionString">If this parameter is string.Empty then a CompanyConnectionString will be used</param>
        //--------------------------------------------------------------------------------
        public void DeleteDSSynchronizationInfo(string docTBGuid, string auxConnectionString = "")
        {
            using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {
                myConnection.Open();

                TBCommand aSqlCommand = new TBCommand(myConnection);
                // controllo se esiste nella DS_SynchronizationInfo una riga con quel TBGuid
                string queryexistInfo = "DELETE FROM [DS_SynchronizationInfo] WHERE [DocTBGuid] = @docTBGuid AND[ProviderName] = @providerName";

                try
                {
                    aSqlCommand = new TBCommand(myConnection);
                    aSqlCommand.CommandText = queryexistInfo;
                    aSqlCommand.Parameters.Add("@docTBGuid", docTBGuid);
                    aSqlCommand.Parameters.Add("@providerName", ProviderName);
                    aSqlCommand.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "ExternalSynchroProvider.DeleteDSSynchronizationInfo");
                }
                finally
                {
                    aSqlCommand.Dispose();
                }
            }
        }

        /// <summary>
        /// Inserts a line in DS_ActionsLog
        /// </summary>
        /// <param name="statusType"></param>
        /// <param name="synchMessage"></param>
        /// <param name="docTbGuid"></param>
        /// <param name="docNameSpace"></param>
        /// <param name="direction"></param>
        /// <param name="actionType"></param>
        /// <param name="auxConnectionString">If this parameter is string.Empty then a CompanyConnectionString will be used</param>
        /// <returns>logId of the line just inserted into the DS_ActionsLog table. 0 if operation failed</returns>
        //--------------------------------------------------------------------------------
        public int InsertDSActionsLog(SynchroStatusType statusType, string synchMessage, string docTbGuid, string docNameSpace, SynchroDirectionType direction, SynchroActionType actionType, string auxConnectionString = "")
        {
            int logId = 0;

            using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {

                myConnection.Open();

                TBCommand myCommand = new TBCommand(myConnection);

                string ActionsLogQuery = string.Format(@"INSERT INTO [DS_ActionsLog] 
                                        ([ProviderName], [DocNamespace], [DocTBGuid], [ActionType],  [SynchDirection], [SynchStatus], [SynchMessage], [TBCreatedID], [TBModifiedID], [ActionData], [SynchXMLData],[TBCreated],[TBModified]) 
                                        VALUES ('{0}', '{1}', '{2}', {3}, {4}, {5}, '{6}', {7}, {8},'','',getdate(),getdate());SELECT SCOPE_IDENTITY();", ProviderName, docNameSpace, string.IsNullOrWhiteSpace(docTbGuid) ? Guid.Empty.ToString() : docTbGuid, (int)actionType,
                                                                                                       (int)direction, (int)statusType, synchMessage.Replace("'", "''"), 0, 0);

                try
                {
                    myCommand.CommandText = ActionsLogQuery;
                    logId = Int32.Parse(myCommand.ExecuteScalar().ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "ExternalSynchroProvider.InsertDSActionsLog");
                }
                finally
                {
                    myCommand.Dispose();
                }
            }

            return logId;
        }


        /// <summary>
        /// Updates a line in the DS_ActionsLog table with a specified logID
        /// </summary>
        /// <param name="SynchStatus"></param>
        /// <param name="importXml">can be "string.Empty"</param>
        /// <param name="synchMessage">can be "string.Empty"</param>
        /// <param name="logID"></param>
        /// <param name="auxConnectionString">If this parameter is string.Empty then a CompanyConnectionString will be used</param>
        //--------------------------------------------------------------------------------
        public void UpdateDSActionsLog(int logID, SynchroStatusType SynchStatus, string importXml, string synchMessage, string auxConnectionString = "")
        {
            // update della DS_ActionsLog con dati relativi alla sincronizzazione appena avvenuta.
            string query1 = "UPDATE [DS_ActionsLog] SET [SynchStatus] = @synchStatus, [TBModified] = @TBModified WHERE [LogId] = @logid AND [ProviderName] = @providerName";
            string query2 = @"UPDATE [DS_ActionsLog] SET [SynchMessage] = @synchMessage, [SynchStatus] = @synchStatus, [SynchXMLData] = @synchXMLData, [TBModified] = @TBModified
                            WHERE [LogId] = @logid AND[ProviderName] = @providerName";

            bool pre = (string.IsNullOrWhiteSpace(importXml) && string.IsNullOrWhiteSpace(synchMessage));
            string query = pre ? query1 : query2;

            using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {
                myConnection.Open();

                TBCommand aSqlCommand = new TBCommand(myConnection);

                try
                {
                    aSqlCommand.CommandText = query;
                    if (!pre)
                    {
                        aSqlCommand.Parameters.Add("@synchMessage", string.IsNullOrWhiteSpace(synchMessage) ? String.Empty : synchMessage);
                        aSqlCommand.Parameters.Add("@synchXmlData", string.IsNullOrWhiteSpace(importXml) ? String.Empty : importXml);
                    }
                    aSqlCommand.Parameters.Add("@TBModified", DateTime.Now);
                    aSqlCommand.Parameters.Add("@synchStatus", SynchStatus);
                    aSqlCommand.Parameters.Add("@logid", logID);
                    aSqlCommand.Parameters.Add("@providerName", ProviderName);
                    aSqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "ExternalSynchroProvider.UpdateDSActionsLog");
                }
                finally
                {
                    aSqlCommand.Dispose();
                }
            }
        }

        /// <summary>
        /// Delete a line in DS_ActionsLog with a specified LogId
        /// </summary>
        /// <param name="logID"></param>
        /// <param name="auxConnectionString">If this parameter is string.Empty then a CompanyConnectionString will be used</param>
        //--------------------------------------------------------------------------------
        public void DeleteDSActionsLog(int logID, string auxConnectionString = "")
        {
            // update della DS_ActionsLog con dati relativi alla sincronizzazione appena avvenuta.
            string query = "DELETE FROM [DS_ActionsLog] WHERE [LogId]=@logid";


            using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {
                myConnection.Open();

                TBCommand aSqlCommand = new TBCommand(myConnection);

                try
                {
                    aSqlCommand.CommandText = query;
                    aSqlCommand.Parameters.Add("@logid", logID);
                    aSqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "ExternalSynchroProvider.DeleteDSActionsLog");
                }
                finally
                {
                    aSqlCommand.Dispose();
                }
            }
        }

        /// <summary>
        /// Insert a line into DS_ActionsQueue
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="SynchXMLData">optional. Can be string.Empty</param>
        /// <param name="statusType"></param>
        /// <param name="synchDirection"></param>
        /// <param name="filters">optional. Can be string.Empty</param>
        /// <param name="auxConnectionString">If this parameter is string.Empty then a CompanyConnectionString will be used</param>
        /// <returns>Returns the LogId of the inserted line. Returns 0 is the operations has failed</returns>
        //--------------------------------------------------------------------------------
        public int InsertDSActionsQueue(string actionName, string synchXMLData, SynchroStatusType statusType, SynchroDirectionType synchDirection, string filters, string auxConnectionString = "")
        {
            int result = 0;
            string query = "INSERT INTO [DS_ActionsQueue] ([ProviderName],[ActionName]";

            if (!string.IsNullOrWhiteSpace(synchXMLData))
                query += ",[SynchXMLData]";

            query += ",[SynchStatus],[SynchDirection]";

            if (!string.IsNullOrWhiteSpace(filters))
                query += " ,[SynchFilters]";

            query += @" ,[TBCreated],[TBModified],[TBCreatedID],[TBModifiedID])

            VALUES ('" + ProviderName + "','" + actionName + "'";

            if (!string.IsNullOrWhiteSpace(synchXMLData))
                query += ",'" + synchXMLData + "'";

            query += "," + (int)statusType + "," + (int)synchDirection;

            if (!string.IsNullOrWhiteSpace(filters))
                query += ",'" + filters + "'";

            query += ",getdate(),getdate(),0,0);SELECT SCOPE_IDENTITY();";

            using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {
                myConnection.Open();

                TBCommand aSqlCommand = new TBCommand(query, myConnection);

                try
                {
                    result = Int32.Parse(aSqlCommand.ExecuteScalar().ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "ExternalSynchroProvider.InsertDSActionsQueue");
                }
                finally
                {
                    aSqlCommand.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// Update a line in a DS_ActionsQueue with a specified logId 
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="synchXMLData">optional. Can be string.Empty</param>
        /// <param name="statusType"></param>
        /// <param name="auxConnectionString">If this parameter is string.Empty then a CompanyConnectionString will be used</param>
        //--------------------------------------------------------------------------------
        public void UpdateDSActionsQueue(int logId, string synchXMLData, SynchroStatusType statusType, string auxConnectionString = "")
        {
            string query = "UPDATE [DS_ActionsQueue] SET [TBModified]=getdate(), [SynchStatus]=" + (int)statusType;

            if (!string.IsNullOrWhiteSpace(synchXMLData))
                query += ",[SynchXMLData]='" + synchXMLData + "' ";

            query += "WHERE [LogId]=" + logId;

            using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {
                myConnection.Open();

                TBCommand aSqlCommand = new TBCommand(query, myConnection);

                try
                {
                    aSqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "ExternalSynchroProvider.UpdateDSActionsQueue");
                }
                finally
                {
                    aSqlCommand.Dispose();
                }
            }
        }

        /// <summary>
        /// Delete a line from DS_ActionsQueue with specified logId
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="auxConnectionString">If this parameter is string.Empty then a CompanyConnectionString will be used</param>
        //--------------------------------------------------------------------------------
        public void DeleteDSActionsQueue(int logId, string auxConnectionString = "")
        {
            string query = "DELETE FROM [DS_ActionsQueue] WHERE [LogId]=" + logId;

            using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {
                myConnection.Open();

                TBCommand aSqlCommand = new TBCommand(query, myConnection);

                try
                {
                    aSqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "ExternalSynchroProvider.DeleteDSActionsQueue");
                }
                finally
                {
                    aSqlCommand.Dispose();
                }
            }
        }

        /// <summary>
        /// Inserts a row in DS_Transcoding
        /// </summary>
        /// <param name="erpTableName"></param>
        /// <param name="erpKey"></param>
        /// <param name="docTBGuid"></param>
        /// <param name="externalDocName"></param>
        /// <param name="externalId"></param>
        /// <param name="auxConnectionString">If this parameter is string.Empty then a CompanyConnectionString will be used</param>
        //--------------------------------------------------------------------------------
        public void InsertDSTranscoding(string erpTableName, string erpKey, string docTBGuid, string externalDocName, string externalId, string auxConnectionString = "")
        {
            string query = @"INSERT INTO [DS_Transcoding]
                                                       ([ProviderName]
                                                       ,[ERPTableName]
                                                       ,[ERPKey]
                                                       ,[DocTBGuid]
                                                       ,[EntityName]
                                                       ,[EntityID]
                                                       ,[TBCreated]
                                                       ,[TBModified]
                                                       ,[TBCreatedID]
                                                       ,[TBModifiedID])
                                                VALUES
                                                    (@ProviderName,
                                                     @ERPTableName,
                                                     @ERPKey,
                                                     @DocTBGuid,
                                                     @EntityName,
                                                     @EntityID,
                                                     getdate(),
                                                     getdate(),
                                                     1,
                                                     1)";

            using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {
                myConnection.Open();

                TBCommand aSqlCommand = new TBCommand(myConnection);

                try
                {
                    aSqlCommand.CommandText = query;
                    aSqlCommand.Parameters.Add("@ProviderName", ProviderName);
                    aSqlCommand.Parameters.Add("@ERPTableName", erpTableName);
                    aSqlCommand.Parameters.Add("@ERPKey", erpKey);
                    aSqlCommand.Parameters.Add("@DocTBGuid", docTBGuid);
                    aSqlCommand.Parameters.Add("@EntityName", externalDocName);
                    aSqlCommand.Parameters.Add("@EntityID", externalId);
                    aSqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "ExternalSynchroProvider.InsertDSTranscoding");
                }
                finally
                {
                    aSqlCommand.Dispose();
                }
            }
        }

        /// <summary>
        /// Delete a line from DS_Transcoding with specified erpTableName and erpKey
        /// </summary>
        /// <param name="erpTableName"></param>
        /// <param name="erpKey"></param>
        /// <param name="auxConnectionString"></param>
        //--------------------------------------------------------------------------------
        public void DeleteDSTranscoding(string erpTableName, string erpKey, string auxConnectionString = "")
        {
            string query = @"DELETE FROM [DS_Transcoding] WHERE [ProviderName]=@ProviderName AND [ERPTableName]=@ERPTableName AND [ERPKey]= @ERPKey";

            using (TBConnection myConnection = new TBConnection(connectionStringManager.CompanyConnectionString, DBMSType.SQLSERVER))
            {
                myConnection.Open();

                TBCommand aSqlCommand = new TBCommand(myConnection);

                try
                {
                    aSqlCommand.CommandText = query;
                    aSqlCommand.Parameters.Add("@ProviderName", ProviderName);
                    aSqlCommand.Parameters.Add("@ERPTableName", erpTableName);
                    aSqlCommand.Parameters.Add("@ERPKey", erpKey);
                    aSqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    LogWriter.WriteToLog(CompanyName, ProviderName, ex.Message, "ExternalSynchroProvider.DeleteDSTranscoding");
                }
                finally
                {
                    aSqlCommand.Dispose();
                }
            }
        }

        //--------------------------------------------------------------------------------
        public override void SynchronizeErrorsRecovery()
        {
        }
    }
}
