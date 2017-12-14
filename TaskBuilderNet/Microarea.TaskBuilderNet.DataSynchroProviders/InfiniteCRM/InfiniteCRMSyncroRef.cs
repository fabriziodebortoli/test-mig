using System;
using System.Diagnostics;
using System.Net;
using System.ServiceModel;
using System.Xml;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Parsers;
using Microarea.TaskBuilderNet.DataSynchroProviders.Properties;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.DataSynchroUtilities;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM
{
	///<summary>
	/// Wrapper per le chiamate ai webmethod esposti dal servizio PatWSConnector
	/// dell'installazione di Pat
	///</summary>
	//=========================================================================
	internal class InfiniteCRMSyncroRef
	{
		private const string testGetXmlString = @"<Operations><Operation><Get><PortTypes /></Get></Operation></Operations>";

		private string companyName = string.Empty;

        private IProviderLogWriter LogWriter { get; set; }

        //---------------------------------------------------------------------
        private string patUser = string.Empty;
		private string patPassword = string.Empty;
		private string patUrl = string.Empty; 

		//---------------------------------------------------------------------
		private PatWSConnector.WSConnectorClient wsClient = null;

        //---------------------------------------------------------------------
        public InfiniteCRMSyncroRef(string companyName, IProviderLogWriter logWriter)
        {
			if (wsClient == null)
				wsClient = new PatWSConnector.WSConnectorClient();

			this.companyName = companyName;
            this.LogWriter = logWriter;
        }

		//---------------------------------------------------------------------
        public bool TestProviderParameters(string url, string username, string password,out string message)
        {
            message = string.Empty;

            if (string.IsNullOrWhiteSpace(url))
            {
				message = string.Format(Resources.InfiniteCRMConnectionError, Resources.EmptyURI);
                LogWriter.WriteToLog(companyName, DatabaseLayerConsts.CRMInfinity, message, "InfinitySyncroRef.TestProviderParameters");
                return false;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
				message = string.Format(Resources.InfiniteCRMConnectionError, Resources.EmptyCredentials);
                LogWriter.WriteToLog(companyName, DatabaseLayerConsts.CRMInfinity, message, "InfinitySyncroRef.TestProviderParameters");
                return false;
            }

            patUrl = url;
            patUser = username;
            patPassword = password;	

            try
            {
                // devo fare sempre la new!
                wsClient = new PatWSConnector.WSConnectorClient();

                // assegno il nuovo url e le nuove credenziali
                wsClient.Endpoint.Address = new EndpointAddress(new Uri(patUrl), wsClient.Endpoint.Address.Identity, wsClient.Endpoint.Address.Headers);
              
                // vado ad eseguire una Get semplice
                string response = ExecuteWithParameters(testGetXmlString, patUrl, patUser, patPassword);

                // parso la risposta
                PatSynchroResponseInfo info = PatSynchroResponseParser.GetResponseInfo(response);
                if (info.GetSynchroStatusByPosition(0) == SynchroStatusType.Synchro)
                    return true;
                else
                    message = string.Format(Resources.InfiniteCRMConnectionErrorDescri, info.Results[0].Code, info.Results[0].Description);
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.ToString());
                message = string.Format(Resources.InfiniteCRMConnectionError, we.Message);
                LogWriter.WriteToLog(companyName, DatabaseLayerConsts.InfiniteCRM, we.Message, "InfiniteCRMSyncroRef.TestProviderParameters");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                message = string.Format(Resources.InfiniteCRMConnectionError, ex.Message);
                LogWriter.WriteToLog(companyName, DatabaseLayerConsts.InfiniteCRM, ex.Message, "InfiniteCRMSyncroRef.TestProviderParameters");
                return false;
            }

            return false;
        }

		//---------------------------------------------------------------------
        public bool SetProviderParameters(string url, string username, string password, out string message)
        {
            message = string.Empty;

            if (string.IsNullOrWhiteSpace(url))
            {
                message = string.Format(Resources.InfinityConnectionError, Resources.EmptyURI);
                LogWriter.WriteToLog(companyName, DatabaseLayerConsts.CRMInfinity, message, "InfinitySyncroRef.TestProviderParameters");
                return false;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                message = string.Format(Resources.InfinityConnectionError, Resources.EmptyCredentials);
                LogWriter.WriteToLog(companyName, DatabaseLayerConsts.CRMInfinity, message, "InfinitySyncroRef.TestProviderParameters");
                return false;
            }

            patUrl = url;
            patUser = username;
            patPassword = password;	


            wsClient = new PatWSConnector.WSConnectorClient();
            wsClient.Endpoint.Address = new EndpointAddress(new Uri(patUrl), wsClient.Endpoint.Address.Identity, wsClient.Endpoint.Address.Headers);

			return true;
        }


		///<summary>
		/// Metodo per invocare la funzione Execute per comunicare con il sistema Pat
		/// N.B. Ogni volta che mando una stringa xml verso Pat devo aggiungere preventivamente
		/// al nodo <Operations> gli attributi username="{0}" password="{1}" parallelexecution="false"
		/// Inoltre in questo modo non memorizzo nella DS_ActionsLog le credenziali in chiaro e,
		/// nel caso di ripetizione di un'azione fallita con credenziali nel frattempo variate, 
		/// non cado nel problema di sostituirle nella stringa
		///</summary>
        //---------------------------------------------------------------------
        public string Execute(string xml) 
        {
			if (string.IsNullOrWhiteSpace(patUrl) || string.IsNullOrWhiteSpace(patUser) || string.IsNullOrWhiteSpace(patPassword) || String.IsNullOrWhiteSpace(xml))
			{
				LogWriter.WriteToLog(companyName, DatabaseLayerConsts.InfiniteCRM, string.Format(Resources.InfiniteCRMConnectionError, Resources.EmptyCredentials), "InfiniteCRMSyncroRef.Execute"); 
				return string.Empty;
			}

			string response = string.Empty;

            try
            {
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xml);

				// aggiungo al nodo <Operations> gia' esistente gli attributi con le credenziali (che andranno lette dai parametri!!)
				doc.DocumentElement.SetAttribute(InfiniteCRMConsts.Username, patUser);
				doc.DocumentElement.SetAttribute(InfiniteCRMConsts.Password, patPassword);
				doc.DocumentElement.SetAttribute(InfiniteCRMConsts.ParallelExecution, bool.FalseString.ToLowerInvariant());
				
				// invio la stringa xml a Pat
				response = wsClient.Execute(doc.InnerXml);

				Debug.WriteLine("--------------------------------");
				Debug.WriteLine(string.Format("XML for ICRM: {0}\r\n", doc.InnerXml));
				Debug.WriteLine("--------------------------------");
				Debug.WriteLine(string.Format("Response: {0}\r\n", response));
				Debug.WriteLine("--------------------------------");
            }
			catch (WebException we)
			{
				Debug.WriteLine(we.ToString());
				LogWriter.WriteToLog(companyName, DatabaseLayerConsts.InfiniteCRM, string.Format(Resources.InfiniteCRMConnectionError, we.Message), "InfiniteCRMSyncroRef.Execute");
				throw(we);//eseguo il rethrow dell'eccezione per poterla gestire a monte
			}
			catch (Exception exc)
            {
                Debug.WriteLine(exc.ToString());
				LogWriter.WriteToLog(companyName, DatabaseLayerConsts.InfiniteCRM, exc.Message, "InfiniteCRMSyncroRef.Execute", xml);
                throw(exc); //eseguo il rethrow dell'eccezione per poterla gestire a monte
            }

			return response;
        }

        // metodo execute per testare la connessione
		//---------------------------------------------------------------------
		private string ExecuteWithParameters(string xml, string url, string user, string password)
        {
            if (string.IsNullOrWhiteSpace(xml) || string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(user) || String.IsNullOrWhiteSpace(password))
            {
                LogWriter.WriteToLog(companyName, DatabaseLayerConsts.InfiniteCRM, string.Format(Resources.InfiniteCRMConnectionError, Resources.EmptyCredentials), "InfiniteCRMSyncroRef.ExecuteWithParameters");
                return string.Empty;
            }

            string response = string.Empty;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                // aggiungo al nodo <Operations> gia' esistente gli attributi con le credenziali (che andranno lette dai parametri!!)
                doc.DocumentElement.SetAttribute(InfiniteCRMConsts.Username, user);
                doc.DocumentElement.SetAttribute(InfiniteCRMConsts.Password, password);
                doc.DocumentElement.SetAttribute(InfiniteCRMConsts.ParallelExecution, bool.FalseString.ToLowerInvariant());

                // invio la stringa xml a Pat
                response = wsClient.Execute(doc.InnerXml);

                Debug.WriteLine("--------------------------------");
                Debug.WriteLine(string.Format("XML for ICRM: {0}\r\n", doc.InnerXml));
                Debug.WriteLine("--------------------------------");
                Debug.WriteLine(string.Format("Response: {0}\r\n", response));
                Debug.WriteLine("--------------------------------");
            }
            catch (WebException we)
            {
                Debug.WriteLine(we.ToString());
                LogWriter.WriteToLog(companyName, DatabaseLayerConsts.InfiniteCRM, string.Format(Resources.InfiniteCRMConnectionError, we.Message), "InfiniteCRMSyncroRef.ExecuteWithParameters");
				throw new WebException(we.Message);//eseguo il rethrow dell'eccezione per poterla gestire a monte
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.ToString());
                LogWriter.WriteToLog(companyName, DatabaseLayerConsts.InfiniteCRM, exc.Message, "InfiniteCRMSyncroRef.ExecuteWithParameters", xml);
                throw new Exception(exc.Message); //eseguo il rethrow dell'eccezione per poterla gestire a monte
            }

            return response;
        }
	}
}
