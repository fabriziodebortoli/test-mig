using System;
using System.Collections.Generic;
using System.Data;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.DataSynchroUtilities;

namespace Microarea.TaskBuilderNet.DataSynchroProviders
{
    //================================================================================
    internal abstract class RecoveryManager
    {
        protected List<EntityToImport> rmList;
        protected BaseSynchroProvider provider;
        protected string loginName;
        protected string loginPassword;
        protected bool loginWindowsAuthentication;
        protected string companyName;
        protected string companyConnectionString;
        protected abstract void InboundRecovery();

        //---------------------------------------------------------------------
        public void StartRecovery()
        {
            //legge dalla tabella DS_Queue
            // se ci sono i query da eseguire fa il procedimento standart. 
            //cambia lo stato della riga in DS_Queue  
            //esce

            rmList = GetEntitiesToSyncronize();
            if (rmList == null || rmList.Count == 0)
                return;

            InboundRecovery();

            ChangeActionsQueueStatus();
        }

        /// <summary>
        /// cambia lo stato da ToSynchro a Synchro  
        /// </summary>
        /// <param name="ds"></param>
        //---------------------------------------------------------------------
        public void ChangeActionsQueueStatus(SynchroStatusType statusType)
        {
            if (rmList == null || rmList.Count == 0)
                return;
            string query = "UPDATE [DS_ActionsQueue] SET [SynchStatus] =" + (int)statusType + " WHERE [LogId] = {0}";
            using (TBConnection connection = new TBConnection(companyConnectionString, DBMSType.SQLSERVER))
            {
                try
                {
                    connection.Open();
                    foreach (EntityToImport rmInfo in rmList)
                    {

                        using (TBCommand command = new TBCommand(string.Format(query, rmInfo.LogId), connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (TBException ex)
                {
                    rmList.Clear();
                    provider.LogWriter.WriteToLog(provider.CompanyName, provider.ProviderName, ex.Message, "RecoveryManager.ChangeActionsQueueStatus");
                    return;
                }
            }
            rmList.Clear();
        }


        /// <summary>
        /// cambia lo stato da ToSynchro a Synchro 
        /// usato solo durante la recovery
        /// </summary>
        /// <param name="ds"></param>
        //---------------------------------------------------------------------
        protected void ChangeActionsQueueStatus()
        {
            if (rmList == null || rmList.Count == 0)
                return;

            string query = "UPDATE [DS_ActionsQueue] SET [SynchStatus] = {0} WHERE [LogId] = {1}";
            using (TBConnection connection = new TBConnection(companyConnectionString, DBMSType.SQLSERVER))
            {
                try
                {
                    connection.Open();
                    foreach (EntityToImport rmInfo in rmList)
                    {
                        // se lo fallita la creazione di TBServices lo stato rimane toSyunchro per il prox recovery
                        // se fallito l'inserimento del documento allora lo stato e' error
                        SynchroStatusType status = SynchroStatusType.ToSynchro;
                        if (rmInfo.Status == 0)
                            status = SynchroStatusType.Synchro;
                        else
                            if (rmInfo.Status == 1)
                            status = SynchroStatusType.Error;
                        else
                            continue;

                        using (TBCommand command = new TBCommand(string.Format(query, (int)status, rmInfo.LogId), connection))
                            command.ExecuteNonQuery();
                    }
                }
                catch (TBException ex)
                {
                    rmList.Clear();
                    provider.LogWriter.WriteToLog(provider.CompanyName, provider.ProviderName, ex.Message, "RecoveryManager.ChangeActionsQueueStatus");
                    return;
                }
            }
            rmList.Clear();
        }


        /// <summary>
        /// leggo la tabella DS_ActionQueue e ricavo i dati con direzione Inbound, provider specifico e lo stato ToSynchronize
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        //---------------------------------------------------------------------
        protected List<EntityToImport> GetEntitiesToSyncronize()
        {
            List<EntityToImport> infoList = new List<EntityToImport>();

            using (TBConnection connection = new TBConnection(companyConnectionString, DBMSType.SQLSERVER))
            {
                try
                {
                    connection.Open();

                    string query = string.Format(@"SELECT [LogId],[ActionName],[SynchXMLData] FROM [DS_ActionsQueue] WHERE [ProviderName] = '{0}' 
                                                    AND [SynchStatus] = {1} AND [SynchDirection] = {2}",
                                                    provider.ProviderName, (int)SynchroStatusType.ToSynchro, (int)SynchroDirectionType.Inbound);

                    using (TBCommand command = new TBCommand(query, connection))
                    {
                        IDataReader dr = command.ExecuteReader();
                        if (dr != null && connection.DataReaderHasRows(dr))
                        {
                            while (dr.Read())
                            {
                                EntityToImport recMng = new EntityToImport();
                                recMng.Name = dr["ActionName"].ToString();
                                recMng.XmlToImport = dr["SynchXMLData"].ToString();
                                recMng.LogId = (int)dr["LogId"];
                                infoList.Add(recMng);
                                recMng = null;
                            }

                            dr.Close();
                            dr.Dispose();
                        }
                    }
                }
                catch (TBException ex)
                {
                    provider.LogWriter.WriteToLog(provider.CompanyName, provider.ProviderName, ex.Message, "RecoveryManager.GetEntitiesToSyncronize");
                    return null;
                }
            }
            return infoList;
        }

        /// <summary>
        /// inserisce i dati ricevuti nella tabella ti DS_ActionQueue
        /// </summary>
        //---------------------------------------------------------------------
        public void AddRowsToRecovery(string crmXml, string entityName)
        {
            if (string.IsNullOrEmpty(crmXml) || string.IsNullOrEmpty(entityName))
                return;

            if (rmList != null && rmList.Count > 0)
                rmList.Clear();
            else
                rmList = new List<EntityToImport>();

            string query = @"INSERT INTO DS_ActionsQueue (ProviderName, ActionName, SynchXMLData, SynchStatus, SynchDirection)
                             VALUES(@provider, @entity, @crmXml," + (int)SynchroStatusType.ToSynchro + "," + (int)SynchroDirectionType.Inbound + @"); 
                            SELECT CAST(scope_identity() AS int)";

            using (TBConnection connection = new TBConnection(companyConnectionString, DBMSType.SQLSERVER))
            {
                try
                {
                    connection.Open();

                    EntityToImport rmData = new EntityToImport();

                    using (TBCommand command = new TBCommand(query, connection))
                    {
                        command.Parameters.Add("@provider", provider.ProviderName);
                        command.Parameters.Add("@entity", entityName);
                        command.Parameters.Add("@crmXml", crmXml);

                        rmData.LogId = (Int32)command.ExecuteScalar();
                        rmData.Name = entityName;
                        rmData.XmlToImport = crmXml;
                        rmList.Add(rmData);
                    }
                }
                catch (TBException ex)
                {
                    provider.LogWriter.WriteToLog(provider.CompanyName, provider.ProviderName, ex.Message, "RecoveryManager.AddRowsToRecovery");
                    return;
                }
            }
        }
    }
}
