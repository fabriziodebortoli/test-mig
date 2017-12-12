using System;
using System.Data;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;
using System.Collections.Generic;

namespace Microarea.WebServices.DataSynchronizer.DataLayer
{
    public class DBManager
    {
        private static object _locker = new object();
        /// <summary>
        /// Esegue una query sulla tabella DS_Providers per ottenere l'elenco delle configurazioni dei provider
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="providerName"></param>
        /// <param name="logger"></param>
        /// <param name="bDisabled"> Questo parametro attiva il filtro sul campo Disabled della tabella DS_Providers. Di default si considerano solo le configurazioni attive.
        /// In caso di test dei parametri del provider si considerano anche le configurazioni disattive. </param>
        /// <returns></returns>
        public static ProviderConfiguration GetProviderConfiguration(string connectionString, string providerName, IProviderLogWriter logger, bool bDisabled = false)
        {
            ProviderConfiguration result = null;

            IDataReader myDataReader = null;

            try
            {

                using (TBConnection myConnection = new TBConnection(connectionString, DBMSType.SQLSERVER))
                {
                    myConnection.Open();

                    using (TBCommand myCommand = new TBCommand(myConnection))
                    {
                        myCommand.CommandText = $"SELECT [Name], [Description], [ProviderUrl], [ProviderUser], [ProviderPassword], [ProviderParameters], [IsEAProvider], [IAFModules], [SkipCrtValidation] FROM [DS_Providers] WHERE Name = '{providerName}'";
                        
                        if (!bDisabled)
                            myCommand.CommandText += " and Disabled = '0'";

                        myDataReader = myCommand.ExecuteReader();
                        if (myDataReader == null)
                            return result;

                        if (myDataReader.Read())
                        {
                            result = new ProviderConfiguration(
                                myDataReader["Name"].ToString(),
                                myDataReader["Description"].ToString(),
                                myDataReader["ProviderUrl"].ToString(),
                                myDataReader["ProviderUser"].ToString(),
                                myDataReader["ProviderPassword"].ToString(),
                                myDataReader["ProviderParameters"].ToString(),
                                myDataReader["IsEAProvider"].ToString().Equals("1"),
                                myDataReader["IAFModules"].ToString(),
                                myDataReader["SkipCrtValidation"].ToString().Equals("1")
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.WriteToLog("Failed to get Provider Configuration", ex);
            }
            finally
            {
                if (myDataReader != null && !myDataReader.IsClosed)
                {
                    myDataReader.Close();
                    myDataReader.Dispose();
                }
            }

            return result;
        }

        public static List<ProviderConfiguration> GetAllProviderConfiguration(string connectionString, IProviderLogWriter logger)
        {
            List<ProviderConfiguration> result = new List<ProviderConfiguration>();

            IDataReader myDataReader = null;

            try
            {
                using (TBConnection myConnection = new TBConnection(connectionString, DBMSType.SQLSERVER))
                {
                    myConnection.Open();

                    using (TBCommand myCommand = new TBCommand(myConnection))
                    {
                        myCommand.CommandText = $"SELECT [Name], [Description], [ProviderUrl], [ProviderUser], [ProviderPassword], [ProviderParameters], [IsEAProvider], [IAFModules], [SkipCrtValidation] FROM [DS_Providers] WHERE Disabled = '0'";
                        myDataReader = myCommand.ExecuteReader();
                        if (myDataReader == null)
                            return result;

                        while (myDataReader.Read())
                        {
                            ProviderConfiguration pConfig = new ProviderConfiguration
                                                            (
                                                                myDataReader["Name"].ToString(),
                                                                myDataReader["Description"].ToString(),
                                                                myDataReader["ProviderUrl"].ToString(),
                                                                myDataReader["ProviderUser"].ToString(),
                                                                myDataReader["ProviderPassword"].ToString(),
                                                                myDataReader["ProviderParameters"].ToString(),
                                                                myDataReader["IsEAProvider"].ToString().Equals("1"),
                                                                myDataReader["IAFModules"].ToString(),
                                                                myDataReader["SkipCrtValidation"].ToString().Equals("1")
                                                            );

                            result.Add(pConfig);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.WriteToLog("Failed to get Provider Configuration", ex);
            }
            finally
            {
                if (myDataReader != null && !myDataReader.IsClosed)
                {
                    myDataReader.Close();
                    myDataReader.Dispose();
                }
            }

            return result;
        }
    }
}