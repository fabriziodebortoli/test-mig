using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing.Printing;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.ApplicationsWinUI.EnumsViewer;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SoapCall;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using System.Threading;
using System.Web.Configuration;
using System.Web;
using System.Collections.Generic;

namespace Microarea.WebServices.TbServices
{
	//=========================================================================
	public class TbServicesEngine
	{
		#region membri privati

		//array dei tbloader istanziati da login manager identificati da utente password company e data di applicazione
        private ArrayList tbLoaders = new ArrayList();

       private TBServicesDiagnostic diagnostic = new TBServicesDiagnostic();
		private Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager loginManager = null;
		private TBSCache tbsCache = new TBSCache();
        private EnumsXmlCache lastEnumsXml = new EnumsXmlCache();
		private HttpRuntimeSection httpRuntimeSection = new HttpRuntimeSection();
		Timer validateTimer = null;
		#endregion

		#region metodi pubblici

        //---------------------------------------------------------------------------
        internal ArrayList TbLoaders
        {
            get { return tbLoaders; }
        }
		
		//---------------------------------------------------------------------------
		public void Init()
		{
			lock (this)
			{
				loginManager = new Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager();
				tbLoaders = new ArrayList();
				httpRuntimeSection.ExecutionTimeout = TimeSpan.FromMinutes(InstallationData.ServerConnectionInfo.TbWCFDataTransferTimeout);

                if (TBServicesConfiguration.Current.TBLoaderCheckIntervalSeconds > -1)
                    validateTimer = new Timer(new TimerCallback(Tick), null, TimeSpan.FromHours(0), TimeSpan.FromSeconds(TBServicesConfiguration.Current.TBLoaderCheckIntervalSeconds));
			}
		}
		//-----------------------------------------------------------------------
		void Tick(object state)
		{
			try
			{
				lock (this)
				{
					ValidateTBLoaders();
				}
			}
			catch
			{
			}
		}
		//---------------------------------------------------------------------------
		public void CloseLogin(string authenticationToken)
		{
			lock (this)
			{
				for (int i = tbLoaders.Count - 1; i >= 0; i--)
				{
					TbLoaderInfo tbLoaderInfo = (TbLoaderInfo)tbLoaders[i];
					if (tbLoaderInfo.ReleaseLogin(authenticationToken))
						return;
				}
			}
		}
		//---------------------------------------------------------------------------
		internal void CloseAllProcesseses()
		{
			lock (this)
			{
				for (int i = tbLoaders.Count - 1; i >= 0; i--)
				{
					TbLoaderInfo tbLoaderInfo = (TbLoaderInfo)tbLoaders[i];
					tbLoaderInfo.CloseProcess(true, "");
				}

				tbLoaders.Clear();
			}
		}
		//---------------------------------------------------------------------------
		public void ValidateTBLoaders()
		{
			lock (this)
			{
				for (int i = tbLoaders.Count - 1; i >= 0; i--)
				{
					TbLoaderInfo tb = (TbLoaderInfo)tbLoaders[i];

					if (!tb.Responding)
					{
						tb.CloseProcess(true, tb.MasterInterface.AuthenticationToken);

						TbLoaderInfo newTb = new TbLoaderInfo(diagnostic);
						if (newTb.Clone(tb, loginManager))
						{
							tbLoaders[i] = newTb;
						}
						else
						{
							tbLoaders.RemoveAt(i);
						}
						continue;
					}

					tb.ValidateLogins(loginManager);
				}
			}
		}
		/// <summary>
		/// Istanzia un tb per poter eseguire una funzione esterna
		/// Se un tb con la stessa company e data di applicazione è già stato istanziato usa quello.
		/// </summary>
		public int CreateTB(string authenticationToken, DateTime applicationDate, bool checkDate, out string user)
		{
			string server;
			return CreateTB(authenticationToken, applicationDate, checkDate, false, out user, out server);
		}

		/// <summary>
		/// Istanzia un tb per poter eseguire una funzione esterna
		/// Se un tb con la stessa company e data di applicazione è già stato istanziato usa quello.
		/// </summary>
		/// <param name="authenticationToken">Token di autenticazione dell'utente logginato in easy look</param>
		/// <param name="applicationDate">data di applicazione necessaria</param>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		public int CreateTB(string authenticationToken, DateTime applicationDate, bool checkDate, bool useRemoteServers, out string user, out string server)
		{
			lock (this)
			{
				user = string.Empty;
				server = string.Empty;
				string company = string.Empty;

				//Cerco un tb legato al token passato
				if (!loginManager.GetAuthenticationNames(authenticationToken, out user, out company))
					return (int)EasyLookConnectionCodes.UserNotAuthenticated;

				int code = 0;
                //prima effettuo una validazione dei tboader in canna (tb potrebbe essersi crashato)
                if (validateTimer != null)
                    ValidateTBLoaders();
				
                for (int i = tbLoaders.Count - 1; i >= 0; i--)
				{
					TbLoaderInfo tb = (TbLoaderInfo)tbLoaders[i];

					if (!tb.Responding)
					{
						tb.CloseProcess(true, authenticationToken);
						tbLoaders.RemoveAt(i);
						continue;
					}

					//se il binding non corrisponde, continuo
					if (tb.Binding != TbLoaderInfo.TbWCFBinding)
						continue;

					//prima di tutto vengono analizzati tutti i token memorizzati per il TBLoader in esame
					//e rimuovo tutti quelli invalidi (derivati ad esempio da un logout)
					tb.RemoveInvalidTokens(loginManager);

					//Caso 1: il mio utente ha già fatto la login su TBLoader
					TbLoginInfo loginInfo = tb.FindLogin(authenticationToken);
					if (loginInfo != null)
					{
						if (checkDate && !loginInfo.ChangeApplicationDate(applicationDate))
							return (int)EasyLookConnectionCodes.SetApplicationDateFailed;

						int ret = loginInfo.TbApplicationClientInterface.TbPort;
						server = tb.Server;
						diagnostic.SetInfo(authenticationToken, "Attaching to TBLoader login; user: {0}; company: {1}; process ID: {2}; port: {3}.", user, company, tb.ProcessId, tb.TbPort);
						return ret;
					}
				}

				for (int i = tbLoaders.Count - 1; i >= 0; i--)
				{
					TbLoaderInfo tb = (TbLoaderInfo)tbLoaders[i];

					//se il binding non corrisponde, continuo
					if (tb.Binding != TbLoaderInfo.TbWCFBinding)
						continue;

					int loginCount = SingleThreaded ? 1 : InstallationData.ServerConnectionInfo.MaxLoginPerTBLoader;
					//Caso 2: 
					//effettuo una nuova login su un TBLoader esistente (se non ho raggiunto il tetto massimo)
					if (tb.LoginCount >= loginCount)
						continue;
					server = tb.Server;
					return tb.InitTbLogin(authenticationToken, user, company, false, applicationDate, checkDate);
				}

				if (tbLoaders.Count >= InstallationData.ServerConnectionInfo.MaxTBLoader)
					throw new ApplicationException(Strings.MaxTbLoaderReached);

				//Caso 3: creo un nuovo TBLoader
				int suggestedPort;
				TbLoaderInfo newTB = CreateTBLoaderInfo(useRemoteServers, authenticationToken, out server, out suggestedPort);
				newTB.Server = server;
				code = newTB.StartTB(authenticationToken, suggestedPort);
				if (code < (int)EasyLookConnectionCodes.OK)
					return code;

				tbLoaders.Add(newTB);
				
				//effettuo la login
				return newTB.InitTbLogin(authenticationToken, user, company, true, applicationDate, checkDate);
			}
		}

		//-----------------------------------------------------------------------
		private TbLoaderInfo CreateTBLoaderInfo(bool useRemoteServers, string authenticationToken, out string server, out int suggestedPort)
		{
			if (remoteClients == null)
			{
				try
				{
					remoteClients = RemoteClient.Read(true);
				}
				catch (Exception ex)
				{
					diagnostic.SetError(authenticationToken, ex);
					remoteClients = new List<RemoteClient>();
				}
			}
			if (!useRemoteServers || remoteClients.Count == 0)
			{
				//default
				server = Environment.MachineName;
				suggestedPort = -1;
				return new TbLoaderInfo(diagnostic);
			}
			int min = int.MaxValue;
			RemoteClient candidate = null;
			foreach (RemoteClient h in remoteClients)
			{
                if (true == h.EasyLook)
                    continue;

				if (h.count < min)
				{
					candidate = h;
					min = h.count;
				}
			}
			//Host h = hosts[0];
			server = candidate.Host;
			suggestedPort = candidate.Port + candidate.count;
			return new TbLoaderInfo(diagnostic, candidate);
		}

		
		//-----------------------------------------------------------------------
		public WCFBinding GetWCFBinding()
		{
			return TbLoaderInfo.TbWCFBinding;
		}

		/// <summary>
		/// Comunica che un particolare Easy Look non necessita più del tbLoader che utilizzava
		/// Viene chiamato quando easy look fa log off
		/// </summary>
		//-----------------------------------------------------------------------
		internal void ReleaseTB(string clientToken)
		{
			lock (this)
			{
				foreach (TbLoaderInfo tbLoaderInfo in tbLoaders)
				{
					if (tbLoaderInfo.ReleaseLogin(clientToken))
						break;
				}
			}
		}

		//---------------------------------------------------------------------------
		internal bool IsValidTokenForConsole(string authenticationToken)
		{
			return authenticationToken == InstallationData.ServerConnectionInfo.SysDBConnectionString;
		}

		/// <summary>
		/// Xml contente la lista dei tb istanziati per easy look
		/// </summary>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		public string GetTbLoaderInstantiatedListXml()
		{
			lock (this)
			{
				XmlDocument doc = new XmlDocument();
				XmlElement root = doc.CreateElement("Processes");

				doc.AppendChild(root);

				if (tbLoaders == null)
					return doc.InnerXml;

				foreach (TbLoaderInfo tbInfo in tbLoaders)
				{
					XmlElement tbLoaderInstantiatedElement = doc.CreateElement("Process");
					tbLoaderInstantiatedElement.SetAttribute("id", tbInfo.ProcessId.ToString());
					root.AppendChild(tbLoaderInstantiatedElement);

					tbInfo.AppendXmlInfo(tbLoaderInstantiatedElement);

				}

				return doc.InnerXml;
			}
		}

		/// <summary>
		/// Restituisce true se c'e' almeno un tbloader istanziato cui il token fa riferimento
		/// </summary>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		public bool IsTbLoaderInstantiated(string authenticationToken)
		{
			lock (this)
			{
				if (tbLoaders == null)
					return false;

				foreach (TbLoaderInfo tbInfo in tbLoaders)
				{
					TbLoginInfo loginInfo = tbInfo.FindLogin(authenticationToken);
					if (loginInfo != null)
						return true;
				}
				return false;
			}
		}

		/// <summary>
		///Killa il thread
		/// </summary>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		public void KillThread(int threadID, int processID, string authenticationToken)
		{
			lock (this)
			{
				foreach (TbLoaderInfo tbInfo in tbLoaders)
					tbInfo.KillThread(threadID, processID, authenticationToken);
			}
		}

		/// <summary>
		///Stoppa il thread
		/// </summary>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		public bool StopThread(int threadID, int processID, string authenticationToken)
		{
			lock (this)
			{
				foreach (TbLoaderInfo tbInfo in tbLoaders)
					if (tbInfo.StopThread(threadID, processID, authenticationToken))
						return true;
				return false;
			}
		}

		/// <summary>
		///Killa il processo
		/// </summary>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		public void KillProcess(int processID, string authenticationToken)
		{
			lock (this)
			{
				foreach (TbLoaderInfo tbInfo in tbLoaders)
					tbInfo.KillProcess(processID, authenticationToken);
			}
		}

		/// <summary>
		///Stoppa il thread
		/// </summary>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		public bool StopProcess(int processID, string authenticationToken)
		{
			lock (this)
			{
				foreach (TbLoaderInfo tbInfo in tbLoaders)
					if (tbInfo.StopProcess(processID, authenticationToken))
						return true;
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool IsValidToken(string authenticationToken)
		{
			return loginManager.IsValidToken(authenticationToken);
		}

		#endregion

		#region metodi per il passaggio dati

		//-----------------------------------------------------------------------
		public bool SetData(
			string authenticationToken,
			string data,
			DateTime applicationDate,
			int saveAction,
			out string result,
			bool useApproximation)
		{
			result = string.Empty;
			XmlDocument dataDoc = new XmlDocument();

			try
			{
				dataDoc.LoadXml(data);
			}
			catch (Exception exc)
			{
				XmlWriterHelper xmlWriterHelper = new XmlWriterHelper(string.Empty, string.Empty, string.Empty, XmlWriterTokens.Prefix);
				xmlWriterHelper.AddError(TbServicesErrorCodes.XmlDataFileNotValid, NameSolverStrings.TbServices, exc.Message);
				result = xmlWriterHelper.Dom.OuterXml;
				return false;
			}

			string user = string.Empty;
			int tbPort = CreateTB(authenticationToken, applicationDate, true, out user);
			if (tbPort < 0)
			{
				string message = string.Format(Strings.TbLoaderIstancingError, tbPort);
				XmlWriterHelper xmlWriterHelper = new XmlWriterHelper(dataDoc);
				xmlWriterHelper.AddError(TbServicesErrorCodes.CreateTBFailed, NameSolverStrings.TbServices, message);
				result = xmlWriterHelper.Dom.OuterXml;
				return false;
			}

			bool bRes = false;

			using (TbLoaderClientInterface tb = new TbLoaderClientInterface(BasePathFinder.BasePathFinderInstance, tbPort, authenticationToken, TbLoaderInfo.TbWCFBinding))
			{
				tb.SetTimeout(TbLoaderInfo.DataTransferCallTimeout);
				try
				{
					bRes = tb.SetData(data, saveAction, user, out result);
				}
				catch (Exception exc)
				{
					string message = string.Format(Strings.WebServiceCallError, "", exc.Message);
					XmlWriterHelper xmlWriterHelper = new XmlWriterHelper(dataDoc);
					xmlWriterHelper.AddError(TbServicesErrorCodes.SaveXMLTBDocumentFailed, NameSolverStrings.TbServices, message);
					result = xmlWriterHelper.Dom.OuterXml;
					return false;
				}
			}

			return bRes;
		}

		//-----------------------------------------------------------------------
		public StringCollection GetData
			(
			string authenticationToken,
			string parameters,
			DateTime applicationDate,
			int loadAction,
			int resultType,
			int formatType,
			bool useApproximation
			)
		{
			StringCollection dataStrings = new StringCollection();

			XmlDocument parametersDoc = new XmlDocument();
			try
			{
				parametersDoc.LoadXml(parameters);
			}
			catch (XmlException exc)
			{
				XmlWriterHelper xmlWriterHelper = new XmlWriterHelper(string.Empty, string.Empty, string.Empty, XmlWriterTokens.Prefix);
				xmlWriterHelper.AddError(TbServicesErrorCodes.XmlParametersFileNotValid, NameSolverStrings.TbServices, exc.Message);
				dataStrings.Add(xmlWriterHelper.Dom.OuterXml);
				return dataStrings;
			}

			string user = string.Empty;
			int tbPort = CreateTB(authenticationToken, applicationDate, true, out user);
			if (tbPort < 0)
			{
				string message = string.Format(Strings.TbLoaderIstancingError, tbPort);
				XmlWriterHelper xmlWriterHelper = new XmlWriterHelper(parametersDoc);
				xmlWriterHelper.AddError(TbServicesErrorCodes.CreateTBFailed, NameSolverStrings.TbServices, message);
				dataStrings.Add(xmlWriterHelper.Dom.OuterXml);
				return dataStrings;
			}

			using (TbLoaderClientInterface tb = new TbLoaderClientInterface(BasePathFinder.BasePathFinderInstance, tbPort, authenticationToken, TbLoaderInfo.TbWCFBinding))
			{
				tb.SetTimeout(TbLoaderInfo.DataTransferCallTimeout);
				bool res = false;

				try
				{
					res = tb.GetData(parameters, useApproximation, user, out dataStrings);
				}
				catch (Exception exc)
				{
					string message = string.Format(Strings.WebServiceCallError, "LoadXMLTBDocument", exc.Message);
					XmlWriterHelper xmlWriterHelper = new XmlWriterHelper(parametersDoc);
					xmlWriterHelper.AddError(TbServicesErrorCodes.LoadXMLTBDocumentFailed, NameSolverStrings.TbServices, message);
					dataStrings.Add(xmlWriterHelper.Dom.OuterXml);
					return dataStrings;
				}
			}
			return dataStrings;
		}

		//-----------------------------------------------------------------------
		public string XmlGetParameters(
			string authenticationToken,
			string parameters,
			DateTime applicationDate,
			bool useApproximation)
		{
			XmlDocument parametersDoc = new XmlDocument();
			try
			{
				parametersDoc.LoadXml(parameters);
			}
			catch (XmlException exc)
			{
				XmlWriterHelper xmlWriterHelper = new XmlWriterHelper(string.Empty, string.Empty, string.Empty, XmlWriterTokens.Prefix);
				xmlWriterHelper.AddError(TbServicesErrorCodes.XmlParametersFileNotValid, NameSolverStrings.TbServices, exc.Message);
				return xmlWriterHelper.Dom.OuterXml;
			}

			string user = string.Empty;
			int tbPort = CreateTB(authenticationToken, applicationDate, true, out user);
			if (tbPort < 0)
			{
				string message = string.Format(Strings.TbLoaderIstancingError, tbPort);
				XmlWriterHelper xmlWriterHelper = new XmlWriterHelper(parametersDoc);
				xmlWriterHelper.AddError(TbServicesErrorCodes.CreateTBFailed, NameSolverStrings.TbServices, message);
				return xmlWriterHelper.Dom.OuterXml;
			}

			using (TbLoaderClientInterface tb = new TbLoaderClientInterface(BasePathFinder.BasePathFinderInstance, tbPort, authenticationToken, TbLoaderInfo.TbWCFBinding))
			{
				tb.SetTimeout(TbLoaderInfo.DafaultCallTimeout);
				bool res = false;
				string result = string.Empty;

				try
				{
					res = tb.GetXMLParameters(parameters, useApproximation, user, out result);
				}
				catch (Exception exc)
				{
					string message = string.Format(Strings.WebServiceCallException, "GetXMLParameters", exc.Message);
					XmlWriterHelper xmlWriterHelper = new XmlWriterHelper(parametersDoc);
					xmlWriterHelper.AddError(TbServicesErrorCodes.GetXMLParametersFailed, NameSolverStrings.TbServices, message);
					return xmlWriterHelper.Dom.OuterXml;
				}
				return result;
			}
		}

		//-----------------------------------------------------------------------
		public string GetXMLHotLink
			(
			string authenticationToken,
			string docNamespace,
			string nsUri,
			string fieldXPath
			)
		{
			string user = string.Empty;
			int tbPort = CreateTB(authenticationToken, DateTime.Now, false, out user);
			if (tbPort < 0)
				return string.Empty;

			using (TbLoaderClientInterface tb = new TbLoaderClientInterface(BasePathFinder.BasePathFinderInstance, tbPort, authenticationToken, TbLoaderInfo.TbWCFBinding))
			{
				tb.SetTimeout(TbLoaderInfo.DafaultCallTimeout);
				string result = string.Empty;
				try
				{
					result = tb.GetXMLHotLink(docNamespace, nsUri, fieldXPath, user);
				}
				catch
				{
					return string.Empty;
				}
				return result;
			}

		}
		//-----------------------------------------------------------------------
		public string GetReportSchema
			(
			string authenticationToken,
			string reportNamespace,
			string forUser

			)
		{
			string user = string.Empty;
			int tbPort = CreateTB(authenticationToken, DateTime.Now, false, out user);
			if (tbPort < 0)
				return string.Empty;

			using (TbLoaderClientInterface tb = new TbLoaderClientInterface(BasePathFinder.BasePathFinderInstance, tbPort, authenticationToken, TbLoaderInfo.TbWCFBinding))
			{
				tb.SetTimeout(TbLoaderInfo.DafaultCallTimeout);

				string result = string.Empty;
				try
				{
					result = tb.GetReportSchema(reportNamespace, forUser);
				}
				catch
				{
					return string.Empty;
				}
				return result;
			}

		}


		//-----------------------------------------------------------------------
		public string GetDocumentSchema
			(
			string authenticationToken,
			string documentNamespace,
			string profileName,
			string forUser
			)
		{
			string user = string.Empty;
			int tbPort = CreateTB(authenticationToken, DateTime.Now, false, out user);
			if (tbPort < 0)
				return string.Empty;

			using (TbLoaderClientInterface tb = new TbLoaderClientInterface(BasePathFinder.BasePathFinderInstance, tbPort, authenticationToken, TbLoaderInfo.TbWCFBinding))
			{
				tb.SetTimeout(TbLoaderInfo.DafaultCallTimeout);
				string result = string.Empty;
				try
				{
					result = tb.GetDocumentSchema(documentNamespace, profileName, forUser);
				}
				catch
				{
					return string.Empty;
				}
				return result;
			}
		}

        //-----------------------------------------------------------------------
        public string GetEnumsXml (string culture)
        {
            // richiesta fino a quando cambia e/o fino alla inizializzazione 
            if (string.Compare(lastEnumsXml.Culture, culture) != 0)
            {
                lastEnumsXml.Culture = culture;
                lastEnumsXml.Xml = EnumsViewerManager.CreateXml(InstallationData.InstallationName, culture);
            }

            return lastEnumsXml.Xml;
        }

		//-----------------------------------------------------------------------
		public string RunFunction(string authenticationToken, string request, string nameSpace, string functionName, out string errorMsg)
		{
			errorMsg = string.Empty;
			string user = string.Empty;
			int tbPort = CreateTB(authenticationToken, DateTime.Now, false, out user);
			if (tbPort < 0)
			{
				errorMsg = string.Format(Strings.GenericTBLoaderCreationError, tbPort);
				return string.Empty;
			}

			SoapClient sc = new SoapClient();
			sc.SetConnectionInfo(Environment.MachineName, tbPort, authenticationToken, nameSpace, functionName);
			string newReq = sc.ChangeToken(request);
			string response = sc.DispatchRequest(newReq);
			return response;
		}

		#endregion

		#region FileSystemAccess


		//---------------------------------------------------------------------------
		public string GetCachedFile(string authenticationToken, string nameSpace, string user, string company)
		{
			if (!loginManager.IsValidToken(authenticationToken))
				return string.Empty;

			lock (this)
			{
				return tbsCache.GetItem(nameSpace, user, company);
			}
		}

		//---------------------------------------------------------------------------
		public byte[] GetFileStream(string authenticationToken, string nameSpace, string user, string company)
		{
			if (!loginManager.IsValidToken(authenticationToken))
				return null;

			lock (this)
			{
				TBSCacheContainer tbcc = tbsCache.GetCachedContainer(user, company);
				string realFileName = tbcc.pf.GetFilename(new NameSpace(nameSpace), string.Empty);
				if (realFileName == string.Empty)
					return null;

				try
				{
					FileStream fs = File.OpenRead(realFileName);
					byte[] fileContent = new byte[fs.Length];
					fs.Read(fileContent, 0, (int)fs.Length);
					return fileContent;
				}
				catch
				{
					return null;
				}
			}
		}

		//---------------------------------------------------------------------------
		internal DiagnosticSimpleItem[] GetDiagnosticItems(string authenticationToken, bool clear)
		{
			//l'oggetto diagnostic e' thread safe
			return diagnostic.GetDiagnosticItems(authenticationToken, clear);
		}

		//---------------------------------------------------------------------
		internal bool GetServerPrinterNames(out string[] printers)
		{
			try
			{
				//printers = PrinterSettings.InstalledPrinters.ToArray ???
                int np = PrinterSettings.InstalledPrinters.Count;
                if (np == 0)
                {
                    printers = new string[1];
                    printers[0] = "There are no installed printers on server";
                    return false;
                }

				printers = new string[np];
				int i = 0;

				foreach (String printer in PrinterSettings.InstalledPrinters)
				{
					printers[i++] = printer.ToString();
					//paramento
					if (i == PrinterSettings.InstalledPrinters.Count)
						break;
				}
			}
			catch (System.ComponentModel.Win32Exception ex)
			{
				printers = new string[1];
				printers[0] = ex.Message + " Error code:" + ex.NativeErrorCode.ToString();
				return false;
			}
            catch (System.Exception ex)
            {
                printers = new string[1];
                printers[0] = ex.Message + " Stack error code:" + ex.StackTrace.ToString();
                return false;
            }
            return true;
		}

		#endregion

		private bool? singleThreaded;
		private System.Collections.Generic.List<RemoteClient> remoteClients;
		public bool SingleThreaded
		{
			get
			{
				if (singleThreaded == null)
				{
					PathFinder pf = new PathFinder("", "");
					singleThreaded = pf.SingleThreaded;
				}
				return singleThreaded.Value;
			}
		}
	}

	/// <summary>
	/// Callse che contiene l'istanza statica dell'applicazione
	/// </summary>
	//=========================================================================
	public class TbServicesApplication
	{
		public static TbServicesEngine TbServicesEngine = new TbServicesEngine();
	}

	//=========================================================================
	internal class TBSCacheItem
	{
		public string NameSpace = string.Empty;
		public string FileName = string.Empty;

		//---------------------------------------------------------------------------
		public TBSCacheItem(string nameSpace, string fileName)
		{
			this.NameSpace = nameSpace;
			this.FileName = fileName;
		}
	}

	//=========================================================================
	internal class TBSCacheContainer
	{
		public string User = string.Empty;
		public string Company = string.Empty;
		public PathFinder pf;

		public ArrayList Items = new ArrayList();

		//---------------------------------------------------------------------------
		public TBSCacheContainer(string user, string company)
		{
			this.User = user;
			this.Company = company;

			pf = new PathFinder(company, user);
		}

		//---------------------------------------------------------------------------
		public string GetItem(string nameSpace, string user, string company)
		{
			foreach (TBSCacheItem item in Items)
			{
				if (item.NameSpace == nameSpace)
					return item.FileName;
			}

			string realFileName = pf.GetFilename(new NameSpace(nameSpace), string.Empty);
			if (realFileName == string.Empty || !File.Exists(realFileName))
				return string.Empty;

			string newFileName = Guid.NewGuid().ToString();
			try
			{
				if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Temp\\"))
					Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "\\Temp\\");

				File.Copy(realFileName, AppDomain.CurrentDomain.BaseDirectory + "\\Temp\\" + newFileName);
			}
			catch
			{
				return string.Empty;
			}

			TBSCacheItem tbItem = new TBSCacheItem(nameSpace, "Temp/" + newFileName);
			Items.Add(tbItem);

			return tbItem.FileName;
		}
	}

	//=========================================================================
	internal class TBSCache
	{
		public TBSCache()
		{

		}

		~TBSCache()
		{
			foreach (TBSCacheContainer tbcc in containers)
			{
				foreach (TBSCacheItem item in tbcc.Items)
				{
					if (File.Exists(item.FileName))
						File.Delete(item.FileName);
				}
			}
		}

		private ArrayList containers = new ArrayList();

		//---------------------------------------------------------------------------
		public TBSCacheContainer GetCachedContainer(string user, string company)
		{
			foreach (TBSCacheContainer tbcc in containers)
			{
				if (tbcc.User == user && tbcc.Company == company)
					return tbcc;
			}

			TBSCacheContainer newContainer = new TBSCacheContainer(user, company);
			containers.Add(newContainer);

			return newContainer;
		}

		//---------------------------------------------------------------------------
		public string GetItem(string nameSpace, string user, string company)
		{
			TBSCacheContainer tbcc = GetCachedContainer(user, company);
			return tbcc.GetItem(nameSpace, user, company);
		}
	}

	//=========================================================================
	internal class EnumsXmlCache
	{
		internal string Culture = string.Empty;
		internal string Xml = string.Empty;
	};

	//=========================================================================
	public class TBServicesConfiguration : ConfigurationSection
	{
		static TBServicesConfiguration configInfo = null;
		//---------------------------------------------------------------------------
		public static TBServicesConfiguration Current
		{
			get
			{
				if (configInfo == null)
					configInfo = (TBServicesConfiguration)ConfigurationManager.GetSection("tbServices");
				return configInfo;
			}
		}
		//---------------------------------------------------------------------------
		[ConfigurationProperty("singletonTBLoader", DefaultValue = false, IsRequired = false)]
		public bool SingletonTBLoader
		{
			get
			{
				return (bool)this["singletonTBLoader"];
			}
		}
		//---------------------------------------------------------------------------
		[ConfigurationProperty("tbLoaderCheckIntervalSeconds", DefaultValue = -1, IsRequired = false)]
        public int TBLoaderCheckIntervalSeconds
		{
			get
			{
				return (int)this["tbLoaderCheckIntervalSeconds"];
			}
		}
		//---------------------------------------------------------------------------
		[ConfigurationProperty("binding", DefaultValue = "BasicHttp", IsRequired = false)]
		public string TbWCFBinding
		{
			get
			{
				return (string)this["binding"];
			}
		}

		//---------------------------------------------------------------------------
		[ConfigurationProperty("singletonTBPort", DefaultValue = 10000, IsRequired = false)]
		public int SingletonTBPort
		{
			get
			{
				return (int)this["singletonTBPort"];
			}
		}
	}
}
