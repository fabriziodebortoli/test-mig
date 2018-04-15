using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SoapCall;

using Microarea.TaskBuilderNet.Core.TbGenlibInterface;
using Microarea.TaskBuilderNet.Core.TbGesInterface;
using Microarea.TaskBuilderNet.Core.TbMacroRecorderInterface;
using Microarea.TaskBuilderNet.Core.TbOleDbInterface;
using Microarea.TaskBuilderNet.Core.TbWoormViewerInterface;
using Microarea.TaskBuilderNet.Core.TbXmlTransferInterface;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	/// <summary>
	/// E' la classe che comunica via socket con taskbuilder
	/// </summary>
	//=========================================================================
	public class TbLoaderClientInterface : IDisposable, ITbLoaderClient
	{
		#region Eventi
		public event EventHandler TBLoaderExited;
		#endregion
		#region membri privati o protetti
		protected TbOleDbClient tbOleDb = null;
		protected Microarea.TaskBuilderNet.Core.TbOleDbInterface.TBHeaderInfo tbOleDbHeader = null;
		protected TbGesClient tbGes = null;
		protected Microarea.TaskBuilderNet.Core.TbGesInterface.TBHeaderInfo tbGesHeader = null;
		protected TbWoormViewerClient tbWoormViewer = null;
		protected Microarea.TaskBuilderNet.Core.TbWoormViewerInterface.TBHeaderInfo tbWoormViewerHeader = null;
		protected TbGenlibUIClient tbGenlibUI = null;
		protected Microarea.TaskBuilderNet.Core.TbGenlibInterface.TBHeaderInfo tbGenlibUiHeader = null;
		protected TBXmlTransferClient tbXMLTransfer = null;
		protected Microarea.TaskBuilderNet.Core.TbXmlTransferInterface.TBHeaderInfo tbXmlTransferHeader = null;
		protected TBMacroRecorderTBMacroRecorderClient tbMacroRecorder = null;
		protected Microarea.TaskBuilderNet.Core.TbMacroRecorderInterface.TBHeaderInfo tbMacroRecorderHeader = null;
		protected TbDMSInterface.TbDMSClient tbDMS = null;
		protected Microarea.TaskBuilderNet.Core.TbDMSInterface.TBHeaderInfo tbDMSHeader = null;
		protected ErpPricePoliciesComponents.PricePoliciesComponentsClient erpPricePoliciesComponentsClient = null;
		protected Microarea.TaskBuilderNet.Core.ErpPricePoliciesComponents.TBHeaderInfo erpPricePoliciesComponentsHeader = null;
		protected ErpItemsServices.ItemsServicesClient itemsServicesClient = null;
		protected Microarea.TaskBuilderNet.Core.ErpItemsServices.TBHeaderInfo itemsServicesClientHeader = null;
		protected ERPCustomersSuppliersDbl.CustomersSuppliersDblClient customersSuppliersDblClient = null;
		protected Microarea.TaskBuilderNet.Core.ERPCustomersSuppliersDbl.TBHeaderInfo customersSuppliersDblHeader = null;
        protected ErpCreditLimitComponents.CreditLimitComponentsClient erpCreditLimitComponentsClient = null;
       protected Microarea.TaskBuilderNet.Core.ErpCreditLimitComponents.TBHeaderInfo erpCreditLimitComponentsHeader = null;


        private Binding currentBinding = null;
		private Binding mexBinding = null;
		private bool registerWCFNamespacesOnStart = false;
		private TimeSpan timeout = TimeSpan.MaxValue;
		private IBasePathFinder pathFinder = null;
		private string tbServer = string.Empty;
		private IntPtr menuHandle = IntPtr.Zero;

		protected int tbPort = -1;
		private string tbApplicationPath = string.Empty;
		private string tbLoaderPath = string.Empty;
		protected Process currentTBProcess = null;
		private string authenticationToken = string.Empty;
		protected string latestTbLoaderArgs = "";
		protected WCFBinding binding = WCFBinding.BasicHttp;
		private int suggestedSoapPort = -1;
		private int suggestedTcpPort = -1;
		private string launcher = "";
		#endregion

		#region proprietà

		//-----------------------------------------------------------------------
		/// <summary>
		/// Handle della finestra che riceve i messaggi di diagnostica di TBLoader
		/// </summary>
		public IntPtr MenuHandle
		{
			get { return menuHandle; }
			set { menuHandle = value; }
		}

		//-----------------------------------------------------------------------
		public bool RegisterWCFNamespacesOnStart
		{
			get { return registerWCFNamespacesOnStart; }
			set { registerWCFNamespacesOnStart = value; }
		}

		//-----------------------------------------------------------------------
		public Binding MexBinding
		{
			get
			{
				if (mexBinding == null)
					mexBinding = CreateBinding(true, true);

				return mexBinding;
			}
		}
		//-----------------------------------------------------------------------
		public IBasePathFinder PathFinder { get { return pathFinder; } }
		//--------------------------------------------------------------------------------
		public int TbPort
		{
			get
			{
				return tbPort;
			}
			set
			{
				tbPort = value;
				InitSoapInterfaceURL();
			}
		}

		//--------------------------------------------------------------------------------
		public bool Available
		{
			get
			{
				return	IsAvailable<TbOleDb>(tbOleDb) &&
						IsAvailable<TbGes>(tbGes) &&
						IsAvailable<TbWoormViewer>(tbWoormViewer) &&
						IsAvailable<TbGenlibUI>(tbGenlibUI) &&
						IsAvailable<TBXmlTransfer>(tbXMLTransfer);
			}
		}

		//--------------------------------------------------------------------------------
		internal static bool IsAvailable<T>(ClientBase<T> client) where T : class
		{
			return
				(client.State == System.ServiceModel.CommunicationState.Opened) ||
				(client.State == System.ServiceModel.CommunicationState.Opening) ||
				(client.State == System.ServiceModel.CommunicationState.Created);
		}

		//--------------------------------------------------------------------------------
		public string TbApplicationPath
		{
			get { return tbApplicationPath; }
		}

		//--------------------------------------------------------------------------------
		public string TbServer { get { return tbServer; } set { tbServer = value; InitSoapInterfaceURL(); } }

		//--------------------------------------------------------------------------------
		public virtual int TbProcessId
		{
			get { return (CurrentTBProcess == null) ? 0 : CurrentTBProcess.Id; }
			set { CurrentTBProcess = Process.GetProcessById(value); }
		}

		//--------------------------------------------------------------------------------
		public IntPtr TbProcessHandle
		{
			get { return (CurrentTBProcess == null) ? IntPtr.Zero : CurrentTBProcess.Handle; }
		}

		//--------------------------------------------------------------------------------
		public Process TBProcess { get { return CurrentTBProcess; } set { currentTBProcess = value; } }
		//--------------------------------------------------------------------------------
		protected Process CurrentTBProcess
		{
			get { return currentTBProcess; }

			set
			{
				if (value != currentTBProcess)
				{
					currentTBProcess = value;
					if (currentTBProcess != null)
					{
						currentTBProcess.EnableRaisingEvents = true;
						currentTBProcess.Exited += new EventHandler(TBLoader_Exited);
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		public string AuthenticationToken
		{
			get { return authenticationToken; }
			set { authenticationToken = value; InitSoapInterfaceHeader(); }
		}

		//-----------------------------------------------------------------------
		public virtual bool Connected
		{
			get { return !HasCurrentTBProcessExited(); }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool HasCurrentTBProcessExited()
		{
			try
			{
				return (CurrentTBProcess == null || CurrentTBProcess.HasExited);
			}
			catch (Win32Exception)
			{
				// The process associated with currentTBProcess has exited but
				// the exit code for the process could not be retrieved.
				return true;
			}
			catch (InvalidOperationException)
			{
				// There is no process associated with currentTBProcess
				return true;
			}
		}

		#endregion

		#region costruttori
		/// <summary>
		/// Inizializza l'istanza dell'oggetto in modo da comunicare con un tb istanziato
		/// da un login manager potenzialmente su un'altra macchina
		/// Di default viene usato il BasicHttp binding
		/// </summary>
		/// <param name="tbServer"></param>
		/// <param name="tbPort"></param>
		//-----------------------------------------------------------------------
		public TbLoaderClientInterface(IBasePathFinder pathFinder, int tbPort, string authenticationToken, WCFBinding binding)
			: this(pathFinder, "", tbPort, authenticationToken, binding)
		{
		}

		/// <summary>
		/// Di default viene usato il BasicHttp binding
		/// </summary>
		//-----------------------------------------------------------------------
		public TbLoaderClientInterface(IBasePathFinder pathFinder, string launcher, string authenticationToken)
			: this(pathFinder, launcher, 0, authenticationToken, WCFBinding.BasicHttp)
		{
			this.tbPort = pathFinder.TbLoaderSOAPPort;
		}
		//-----------------------------------------------------------------------
		public TbLoaderClientInterface(IBasePathFinder pathFinder, string launcher, int tbPort, string authenticationToken, WCFBinding binding)
		{
			this.pathFinder = pathFinder;
			this.launcher = launcher;
			this.authenticationToken = authenticationToken;
			this.binding = binding;
			this.tbPort = tbPort;

			Init();
		}

		//-----------------------------------------------------------------------
		public TbLoaderClientInterface(IBasePathFinder pathFinder, string authenticationToken)
		{
			this.pathFinder = pathFinder;
			this.authenticationToken = authenticationToken;
			this.tbPort = pathFinder.TbLoaderSOAPPort;

			Init();
		}

		#endregion

		#region inizializzazione
		//-----------------------------------------------------------------------
		private void Init()
		{
			if (Microarea.TaskBuilderNet.Core.Generic.ClickOnceDeploy.IsClickOnceClient)
				Microarea.TaskBuilderNet.Core.Generic.ClickOnceDeploy.DownloadGroup("TBApps", WebServicesWrapperStrings.TBComponents);

			tbApplicationPath = pathFinder.GetTBLoaderPath();
			tbLoaderPath = Path.Combine(tbApplicationPath, "TBLoader.exe");

			tbServer = pathFinder.RemoteWebServer;

			InitSoapInterfaces();
		}

		//-----------------------------------------------------------------------
		private Binding CreateBinding(bool forMex, bool changeTimeout)
		{
			Binding bnd = null;
			switch (binding)
			{
				case WCFBinding.NetTcp:
					{
						bnd = forMex
							//non uso MetadataExchangeBindings.CreateMexTcpBinding() perche non ha un buffer sufficiente e non riesco a modificarlo nella SetBufferMaxValuesToBinding
							? new NetTcpBinding(SecurityMode.None)
							: new NetTcpBinding();
						break;
					}
				case WCFBinding.BasicHttp:
					{
						bnd = forMex
							? MetadataExchangeBindings.CreateMexHttpBinding()
							: new BasicHttpBinding();
						break;
					}
			}

			if (bnd == null)
				return null;

			WCFSoapClient.SetBufferMaxValuesToBinding(bnd);

			if (changeTimeout && pathFinder != null)
				WCFSoapClient.SetTimeoutToBinding(bnd, timeout);

			return bnd;
		}

		//-----------------------------------------------------------------------
		protected virtual bool CreateInterfaces()
		{
			currentBinding = CreateBinding(false, true);
			if (currentBinding == null)
				return false;

			tbOleDb = new TbOleDbClient(currentBinding, new EndpointAddress(BuildURL("http://localhost:10000/Framework.TbOleDb.TbOleDb/TbOleDb")));

			tbGes = new TbGesClient(currentBinding, new EndpointAddress(BuildURL("http://localhost:10000/Framework.TbGes.TbGes/TbGes")));

			tbWoormViewer = new TbWoormViewerClient(currentBinding, new EndpointAddress(BuildURL("http://localhost:10000/Framework.TbWoormViewer.TbWoormViewer/TbWoormViewer")));

			tbGenlibUI = new TbGenlibUIClient(currentBinding, new EndpointAddress(BuildURL("http://localhost:10000/Framework.TbGenlibUI.TbGenlibUI/TbGenlibUI")));

			tbXMLTransfer = new TBXmlTransferClient(currentBinding, new EndpointAddress(BuildURL("http://localhost:10000/Extensions.XEngine.TBXmlTransfer/TBXmlTransfer")));

			tbMacroRecorder = new TBMacroRecorderTBMacroRecorderClient(currentBinding, new EndpointAddress(BuildURL("http://localhost:10000/TestManager.TBMacroRecorder.TBMacroRecorder/TBMacroRecorderTBMacroRecorder")));

			tbDMS = new TbDMSInterface.TbDMSClient(currentBinding, new EndpointAddress(BuildURL("http://localhost:10000/Extensions.EasyAttachment.TbDMS/TbDMS")));

			erpPricePoliciesComponentsClient = new ErpPricePoliciesComponents.PricePoliciesComponentsClient(currentBinding, new EndpointAddress(BuildURL("http://localhost:10000/ERP.PricePolicies.Components/PricePoliciesComponents")));

			itemsServicesClient = new ErpItemsServices.ItemsServicesClient(currentBinding, new EndpointAddress(BuildURL("http://localhost:10000/ERP.Items.Services/ItemsServices")));

			customersSuppliersDblClient = new ERPCustomersSuppliersDbl.CustomersSuppliersDblClient(currentBinding, new EndpointAddress(BuildURL("http://localhost:10000/ERP.CustomersSuppliers.Dbl/CustomersSuppliersDbl")));

           erpCreditLimitComponentsClient = new ErpCreditLimitComponents.CreditLimitComponentsClient(currentBinding, new EndpointAddress(BuildURL("http://localhost:10000/ERP.CreditLimit.Components/CreditLimitComponents")));


			return true;
		}

		//-----------------------------------------------------------------------
		public void InitSoapInterfaces()
		{
			if (CreateInterfaces())
			{
				InitSoapInterfaceURL();
				InitSoapInterfaceHeader();
			}
		}

		//-----------------------------------------------------------------------
		protected void InitSoapInterfaceURL()
		{
			if (binding == WCFBinding.None)
				return;

			tbOleDb.Endpoint.Address = new EndpointAddress(BuildURL(tbOleDb.Endpoint.ListenUri));
			tbGes.Endpoint.Address = new EndpointAddress(BuildURL(tbGes.Endpoint.ListenUri));
			tbWoormViewer.Endpoint.Address = new EndpointAddress(BuildURL(tbWoormViewer.Endpoint.ListenUri));
			tbGenlibUI.Endpoint.Address = new EndpointAddress(BuildURL(tbGenlibUI.Endpoint.ListenUri));
			tbXMLTransfer.Endpoint.Address = new EndpointAddress(BuildURL(tbXMLTransfer.Endpoint.ListenUri));
			tbMacroRecorder.Endpoint.Address = new EndpointAddress(BuildURL(tbMacroRecorder.Endpoint.ListenUri));
			tbDMS.Endpoint.Address = new EndpointAddress(BuildURL(tbDMS.Endpoint.ListenUri));
			erpPricePoliciesComponentsClient.Endpoint.Address = new EndpointAddress(BuildURL(erpPricePoliciesComponentsClient.Endpoint.ListenUri));
			itemsServicesClient.Endpoint.Address = new EndpointAddress(BuildURL(itemsServicesClient.Endpoint.ListenUri));
			customersSuppliersDblClient.Endpoint.Address = new EndpointAddress(BuildURL(customersSuppliersDblClient.Endpoint.ListenUri));
            erpCreditLimitComponentsClient.Endpoint.Address = new EndpointAddress(BuildURL(erpCreditLimitComponentsClient.Endpoint.ListenUri));

        }

        //-----------------------------------------------------------------------
        private void InitSoapInterfaceHeader()
		{
			if (binding == WCFBinding.None)
				return;

			tbOleDbHeader = new Microarea.TaskBuilderNet.Core.TbOleDbInterface.TBHeaderInfo();
			tbOleDbHeader.AuthToken = authenticationToken;
			tbGesHeader = new Microarea.TaskBuilderNet.Core.TbGesInterface.TBHeaderInfo();
			tbGesHeader.AuthToken = authenticationToken;
			tbWoormViewerHeader = new Microarea.TaskBuilderNet.Core.TbWoormViewerInterface.TBHeaderInfo();
			tbWoormViewerHeader.AuthToken = authenticationToken;
			tbGenlibUiHeader = new Microarea.TaskBuilderNet.Core.TbGenlibInterface.TBHeaderInfo();
			tbGenlibUiHeader.AuthToken = authenticationToken;
			tbXmlTransferHeader = new Microarea.TaskBuilderNet.Core.TbXmlTransferInterface.TBHeaderInfo();
			tbXmlTransferHeader.AuthToken = authenticationToken;
			tbMacroRecorderHeader = new Microarea.TaskBuilderNet.Core.TbMacroRecorderInterface.TBHeaderInfo();
			tbMacroRecorderHeader.AuthToken = authenticationToken;
			tbDMSHeader = new TbDMSInterface.TBHeaderInfo();
			tbDMSHeader.AuthToken = authenticationToken;
			erpPricePoliciesComponentsHeader = new ErpPricePoliciesComponents.TBHeaderInfo();
			erpPricePoliciesComponentsHeader.AuthToken = authenticationToken;
			itemsServicesClientHeader = new ErpItemsServices.TBHeaderInfo();
			itemsServicesClientHeader.AuthToken = authenticationToken;
			customersSuppliersDblHeader = new ERPCustomersSuppliersDbl.TBHeaderInfo();
			customersSuppliersDblHeader.AuthToken = authenticationToken;
            erpCreditLimitComponentsHeader = new ErpCreditLimitComponents.TBHeaderInfo();
            erpCreditLimitComponentsHeader.AuthToken = authenticationToken; 
        }
		#endregion

		#region Funzioni per istanziare un tb
		//-----------------------------------------------------------------------
		public void SetSuggestedPort(int port, WCFBinding binding)
		{
			switch (binding)
			{
				case WCFBinding.BasicHttp: suggestedSoapPort = port; break;
				case WCFBinding.NetTcp: suggestedTcpPort = port; break;
			}
		}
		//-----------------------------------------------------------------------
		public int GetNewTbLoaderPort(WCFBinding binding)
		{
			switch (binding)
			{
				case WCFBinding.BasicHttp: return GetNewTbLoaderPort(suggestedSoapPort > 0 ? suggestedSoapPort : pathFinder.TbLoaderSOAPPort);
				case WCFBinding.NetTcp: return GetNewTbLoaderPort(suggestedTcpPort > 0 ? suggestedTcpPort : pathFinder.TbLoaderTCPPort);
				default: return 0;
			}
		}

		//cambiato il modificatore da internal a public, per richiedere una porta libera per xsocket
		//-----------------------------------------------------------------------
		public static int GetNewTbLoaderPort(int suggestedPort)
		{
			int port = suggestedPort;
			int maxTb = 100;
			while (maxTb-- >= 0)
			{
				if (!TbLoaderClientInterface.IsBusy(port))
					return port;

				port++;
			}

			throw new ApplicationException(string.Format(WebServicesWrapperStrings.NoPortAvailable, suggestedPort, port));
		}

		//-----------------------------------------------------------------------
		static bool IsBusy(int port)
		{
			try
			{
				IPGlobalProperties ipGP = IPGlobalProperties.GetIPGlobalProperties();
				IPEndPoint[] endpoints = ipGP.GetActiveTcpListeners();
				if (endpoints == null || endpoints.Length == 0) return false;
				for (int i = 0; i < endpoints.Length; i++)
					if (endpoints[i].Port == port)
						return true;
				return false;
			}
			catch //esiste un buco del framework per cui si schianta la GetActiveTcpListeners su Win2003SP2
			{
				return false;
			}
		}

		// devo ignorare la URL precofezionata dalla WebReference perchè dipende 
		// dalla macchina su cui è stato generato il WebReference ed inoltre aggiunge
		// la porta giusta per comunicare. 
		//-----------------------------------------------------------------------
		private string BuildURL(string currentUri)
		{
			return BuildURL(new Uri(currentUri)).ToString();
		}
		//-----------------------------------------------------------------------
		private Uri BuildURL(Uri currentUri)
		{
			UriBuilder ub = new UriBuilder(currentUri);
			ub.Port = tbPort;
			ub.Host = tbServer;
			switch (binding)
			{
				case WCFBinding.BasicHttp:
					ub.Scheme = "http";
					break;
				case WCFBinding.NetTcp:
					ub.Scheme = "net.tcp";
					break;
			}
			return ub.Uri;

		}

		//-----------------------------------------------------------------------
		public void StartTbLoader()
		{
			// di default la messaggistica è abilitata
			StartTbLoader(launcher, false);
		}

		//-----------------------------------------------------------------------
		public void StartTbLoader(string laucher)
		{
			StartTbLoader(laucher, false);
		}

		//-----------------------------------------------------------------------
		public void StartTbLoader(string laucher, bool unattendedMode)
		{
			StartTbLoader(laucher, unattendedMode, false);
		}

		//-----------------------------------------------------------------------
		protected static string BuildArguments(
			WCFBinding binding,
			int tbPort,
			string launcher,
			string language,
			IntPtr menuHandle,
			bool unattendedMode,
			bool clearCachedData,
			bool registerWCFNamespaces,
            string tbLoaderParams)
		{
			StringBuilder args = new StringBuilder();
			switch (binding)
			{
				case WCFBinding.None:
					args.Append("TBSOAPPort=0");
					break;
				case WCFBinding.BasicHttp:
					args.AppendFormat("TBSOAPPort={0}", tbPort);
					break;
				case WCFBinding.NetTcp:
					args.AppendFormat("TBTCPPort={0}", tbPort);
					break;
			}

			if (unattendedMode)
				args.Append(" UnattendedMode=true");

			if (!string.IsNullOrEmpty(launcher))
				args.AppendFormat(" Launcher={0}", launcher);

			if (menuHandle != IntPtr.Zero)
				args.AppendFormat(" MenuHandle={0}", menuHandle);

			if (language != null)
				args.AppendFormat(" PrimaryCulture={0}", language);

			if (clearCachedData)
				args.Append(" ClearCache");
			if (registerWCFNamespaces)
				args.Append(" RegisterWCFNamespaces");
            //args.Append(" HideMainWindow=true");
            if (!tbLoaderParams.IsNullOrEmpty())
            {
                args.Append(" ");
                args.Append(tbLoaderParams);
            }
			return args.ToString();
		}
		//-----------------------------------------------------------------------
		virtual public void StartTbLoader(string launcher, bool unattendedMode, bool clearCachedData, string additionalArgs="")
		{
			try
			{
				lock (typeof(TbLoaderClientInterface))
				{
					if (binding != WCFBinding.None)
					{
						tbPort = GetNewTbLoaderPort(binding);

						InitSoapInterfaceURL();
					}
					string language = InstallationData.ServerConnectionInfo.PreferredLanguage;
					string args = BuildArguments(binding, tbPort, launcher, language, menuHandle, unattendedMode, clearCachedData, RegisterWCFNamespacesOnStart, additionalArgs);
					InternalStartTBLoader(args);
				}
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.ConnectionToTbFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		protected virtual void InternalStartTBLoader(string arguments)
		{
			latestTbLoaderArgs = arguments;
			if (!File.Exists(tbLoaderPath))
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.TBLoaderExeNotFoundFmt, tbLoaderPath)));

			string semaphore = GetNewSemaphorePath();
			Directory.CreateDirectory(semaphore);
			arguments += string.Format(" Semaphore=\"{0}\"", semaphore);

			try
			{
				if (StartProcess(arguments))
				{
					WaitForListenerAvailable(semaphore, 90000);
				}
			}
			finally
			{
				if (Directory.Exists(semaphore))
					try
					{
						Directory.Delete(semaphore);
					}
					catch
					{
					}
			}


		}

		//-----------------------------------------------------------------------
		protected virtual string GetNewSemaphorePath()
		{
			return Path.Combine(PathFinder.GetTempPath(), Guid.NewGuid().ToString());
		}

		//-----------------------------------------------------------------------
		protected virtual bool StartProcess(string arguments)
		{
			CurrentTBProcess = Process.Start(tbLoaderPath, arguments);
			return CurrentTBProcess != null;
		}

		//-----------------------------------------------------------------------
		protected void WaitForListenerAvailable(string folder, int timeoutMilliseconds)
		{
			//non uso un FileSystemWatcher perche su certe configurazioni di WIN2003 non funziona
			DateTime start = DateTime.Now;
			while (Directory.Exists(folder))//aspetto la partenza di tbloader, quando avra' finito lo startup cancellera' la cartella
			{
				Thread.Sleep(1000);

				if ((DateTime.Now - start).TotalMilliseconds > timeoutMilliseconds)
					throw new TimeoutException(string.Format(WebServicesWrapperStrings.TimeoutStartingTbloader, timeoutMilliseconds / 1000));
			}
		}

		//-----------------------------------------------------------------------
		protected void TBLoader_Exited(object sender, EventArgs e)
		{
			WaitForDisconnection();

			ResetObject();

			if (TBLoaderExited != null)
				TBLoaderExited(this, e);
		}

		//-----------------------------------------------------------------------
		protected virtual void ResetObject()
		{
			CurrentTBProcess = null;
		}

		#endregion

		#region Funzioni generiche
		//-----------------------------------------------------------------------
		public string GetExtraFiltering(string[] tables, string where)
		{
			try
			{
				return tbOleDb.GetExtraFiltering(ref tbOleDbHeader, tables, where);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.GetGetExtraFilteringFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual object Call(IFunctionPrototype functionPrototype, object[] parameters)
		{
			WCFSoapClient client = new WCFSoapClient
				(
				functionPrototype,
				TbServer,
				TbPort,
				AuthenticationToken,
				MexBinding,
				currentBinding,
				timeout
				);
			return client.Call(parameters);
		}
		//-----------------------------------------------------------------------
		public string GetHotlinkQuery(string hotLinkNamespace, string arguments, int action)
		{
			try
			{
				return tbGes.GetHotlinkQuery(ref tbGesHeader, hotLinkNamespace, arguments, action);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.GetHotlinkQueryFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public void UseRemoteInterface(bool set)
		{
			tbGes.UseRemoteInterface(ref tbGesHeader, set);
		}

		//-----------------------------------------------------------------------
		public int GetNrOpenDocuments()
		{
			try
			{
				int[] nrDocs = tbGes.GetOpenDocuments(ref tbGesHeader);
				return (nrDocs != null) ? nrDocs.Length : 0;
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.GetNrOpenDocumentsFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual void SetApplicationDate(DateTime date)
		{
			try
			{
				tbGenlibUI.SetApplicationDate(ref tbGenlibUiHeader, date.ToString(@"yyyy-MM-ddTHH\:mm\:ss"));
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.SetApplicationDateFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual void CloseLatestXTechDocument()
		{
			try
			{
				tbXMLTransfer.CloseLatestDocument(ref tbXmlTransferHeader);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(e.Message, e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual bool OpenEnumsViewer(string culture, string installation)
		{
			return false;
		}

		//-----------------------------------------------------------------------
		public virtual void ShowAboutFramework()
		{
			try
			{
				tbGenlibUI.ShowAboutFramework(ref tbGenlibUiHeader);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(e.Message, e));
			}
		}
		//-----------------------------------------------------------------------
		public void SetApplicationDateToSystemDate()
		{
			try
			{
				tbGenlibUI.SetApplicationDateToSystemDate(ref tbGenlibUiHeader);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.SetApplicationDateFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public bool RunDocument(string command)
		{
			return RunDocument(command, string.Empty);
		}
		//-----------------------------------------------------------------------
		public bool RunDocument(string command, string arguments)
		{
			int handle;
			return RunDocument(command, arguments, out handle);
		}
		//-----------------------------------------------------------------------
		public virtual bool RunDocument(string command, string arguments, out int handle)
		{
			try
			{
				handle = tbGes.RunDocument(ref tbGesHeader, command, arguments);
				return (handle != 0);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.RunDocumentFailedFmt, command), e));
			}
		}

		//-----------------------------------------------------------------------
		public bool BeginRunDocument(string command)
		{
			try
			{
				tbGes.BeginRunDocument(tbGesHeader, command, "", null, null);
				return true;
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.RunDocumentFailedFmt, command), e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual bool RunFunction(string command) { return RunFunction(command, string.Empty); }
		public virtual bool RunFunction(string command, string arguments)
		{
			try
			{
				return tbGes.RunFunction(ref tbGesHeader, command, arguments);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.RunFunctionFailedFmt, command), e));
			}
		}
		//-----------------------------------------------------------------------
		public virtual void RunFunctionInNewThread(string command) { RunFunctionInNewThread(command, string.Empty); }
		public virtual void RunFunctionInNewThread(string command, string arguments)
		{
			try
			{
				tbGes.RunFunctionInNewThread(ref tbGesHeader, command, arguments);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.RunFunctionFailedFmt, command), e));
			}
		}
		//-----------------------------------------------------------------------
		public IAsyncResult BeginRunFunction(string command, string arguments, AsyncCallback callback, object asyncState)
		{
			try
			{
				return tbGes.BeginRunFunction(tbGesHeader, command, arguments, callback, asyncState);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.BeginRunFunctionFailedFmt, command), e));
			}
		}

		//-----------------------------------------------------------------------
		public bool EndRunFunction(IAsyncResult asyncResult)
		{
			try
			{
				bool res;
				tbGes.EndRunFunction(asyncResult, out res);
				return res;
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.EndRunFunctionFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public bool RunReport(string command) { return RunReport(command, string.Empty); }
		//-----------------------------------------------------------------------
		public bool RunReport(string command, string arguments)
		{
			int handle;
			return RunReport(command, arguments, out handle);
		}
		//-----------------------------------------------------------------------
		public virtual bool RunReport(string command, string arguments, out int handle)
		{
			try
			{
				handle = tbGes.RunReport(ref tbGesHeader, command, arguments);
				return (handle != 0);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.RunReportFailedFmt, command), e));
			}
		}

		// fa partire il TextEditor incluso in TBC++
		//-----------------------------------------------------------------------
		public virtual bool RunTextEditor(string command)
		{
			try
			{
				return tbGes.RunEditor(ref tbGesHeader, command);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.GetDocumentParametersFailedFmt, command), e));
			}
		}

		//-----------------------------------------------------------------------
		public bool GetDocumentParameters(string command, ref string xmlParameters, string code)
		{
			try
			{
				return tbGes.GetDocumentParameters(ref tbGesHeader, command, ref xmlParameters, code);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.GetDocumentParametersFailedFmt, command), e));
			}
		}

		//-----------------------------------------------------------------------
		public bool GetReportParameters(string command, ref string xmlParameters, string code)
		{
			try
			{
				return tbGes.GetReportParameters(ref tbGesHeader, command, ref xmlParameters, code);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.GetReportParametersFailedFmt, command), e));
			}
		}

		//-----------------------------------------------------------------------
		public bool GetXMLExportParameters(string command, ref string xmlParameters, ref string[] messages, string code)
		{
			try
			{
				return tbXMLTransfer.GetXMLExportParameters(ref tbXmlTransferHeader, command, ref xmlParameters, ref messages, code);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.GetXMLExportParametersFailedFmt, command), e));
			}
		}

		//-----------------------------------------------------------------------
		public bool GetXMLImportParameters(string command, ref string xmlParameters, ref string[] messages, string code)
		{
			try
			{
				return tbXMLTransfer.GetXMLImportParameters(ref tbXmlTransferHeader, command, ref xmlParameters, ref messages, code);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.GetXMLExportParametersFailedFmt, command), e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual bool RunBatchInUnattendedMode(string documentNamespace, string xmlParams, ref int documentHandle, ref string[] messages)
		{
			try
			{
				return tbGes.RunBatchInUnattendedMode(ref tbGesHeader, documentNamespace, xmlParams, ref documentHandle, ref messages);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.RunBatchInUnattendedModeFailedFmt, documentNamespace), e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual IAsyncResult BeginRunBatchInUnattendedMode(string documentNamespace, string xmlParams, AsyncCallback callback, object asyncState)
		{
			try
			{
				return tbGes.BeginRunBatchInUnattendedMode(tbGesHeader, documentNamespace, xmlParams, 0, null, callback, asyncState);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.BeginRunBatchInUnattendedModeFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual bool EndRunBatchInUnattendedMode(IAsyncResult asyncResult, out int documentHandle, out string[] messages)
		{
			try
			{
				bool ret;
				tbGes.EndRunBatchInUnattendedMode(asyncResult, out ret, out documentHandle, out messages);
				return ret;
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.EndRunBatchInUnattendedModeFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual bool RunReportInUnattendedMode(WoormInfo woormInfo, string xmlParams, ref int reportHandle, ref string[] messages)
		{
			try
			{
				return tbGes.RunReportInUnattendedMode(ref tbGesHeader, woormInfo, xmlParams, ref reportHandle, ref messages);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.RunReportInUnattendedModeFailedFmt, woormInfo), e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual IAsyncResult BeginRunReportInUnattendedMode(WoormInfo woormInfo, string xmlParams, AsyncCallback callback, object asyncState)
		{
			try
			{
				return tbGes.BeginRunReportInUnattendedMode(tbGesHeader, woormInfo, xmlParams, 0, null, callback, asyncState);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.BeginRunReportInUnattendedModeFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual bool EndRunReportInUnattendedMode(IAsyncResult asyncResult, out int reportHandle, out string[] messages)
		{
			try
			{
				bool ret;
				tbGes.EndRunReportInUnattendedMode(asyncResult, out ret, out reportHandle, out messages);
				return ret;
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.EndRunReportInUnattendedModeFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual bool RunXMLExportInUnattendedMode(string documentNamespace, string xmlParams, ref int documentHandle, ref string[] messages)
		{
			try
			{
				return tbXMLTransfer.RunXMLExportInUnattendedMode(ref tbXmlTransferHeader, documentNamespace, xmlParams, ref documentHandle, ref messages);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.RunXmlExportInUnattendedModeFailedFmt, documentNamespace), e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual IAsyncResult BeginRunXMLExportInUnattendedMode(string documentNamespace, string xmlParams, AsyncCallback callback, object asyncState)
		{
			try
			{
				return tbXMLTransfer.BeginRunXMLExportInUnattendedMode(tbXmlTransferHeader, documentNamespace, xmlParams, 0, null, callback, asyncState);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.BeginRunXmlExportInUnattendedModeFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual bool EndRunXMLExportInUnattendedMode(IAsyncResult asyncResult, out int documentHandle, out string[] messages)
		{
			try
			{
				bool ret;
				tbXMLTransfer.EndRunXMLExportInUnattendedMode(asyncResult, out ret, out documentHandle, out messages);
				return ret;
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.EndRunXmlExportInUnattendedModeFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual bool RunXMLImportInUnattendedMode(string documentNamespace, bool downloadEnvelopes, bool validateData, string xmlParams, ref int documentHandle, ref string[] messages)
		{
			try
			{
				return tbXMLTransfer.RunXMLImportInUnattendedMode(ref tbXmlTransferHeader, documentNamespace, downloadEnvelopes, validateData, xmlParams, ref documentHandle, ref messages);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.RunXmlImportInUnattendedModeFailedFmt, documentNamespace), e));
			}
		}

		//-----------------------------------------------------------------------
		public bool Import(int documentHandle, string envelopeFolder, ref string resultDescription)
		{
			try
			{
				return tbXMLTransfer.Import(ref tbXmlTransferHeader, documentHandle, envelopeFolder, ref resultDescription);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.RunFailed, "Import"), e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual IAsyncResult BeginRunXMLImportInUnattendedMode(string documentNamespace, bool downloadEnvelopes, bool validateData, string xmlParams, AsyncCallback callback, object asyncState)
		{
			try
			{
				return tbXMLTransfer.BeginRunXMLImportInUnattendedMode(tbXmlTransferHeader, documentNamespace, downloadEnvelopes, validateData, xmlParams, 0, null, callback, asyncState);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.BeginRunXmlImportInUnattendedModeFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual bool EndRunXMLImportInUnattendedMode(IAsyncResult asyncResult, out int documentHandle, out string[] messages)
		{
			try
			{
				bool ret;
				tbXMLTransfer.EndRunXMLImportInUnattendedMode(asyncResult, out ret, out documentHandle, out messages);
				return ret;
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.EndRunXmlImportInUnattendedModeFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public bool EnableSoapFunctionExecutionControl(bool enable)
		{
			return tbGes.EnableSoapFunctionExecutionControl(ref tbGesHeader, enable);
		}

		//-----------------------------------------------------------------------
		public bool IsConnectionActive()
		{
			string userName = string.Empty;
			string companyName = string.Empty;

			bool enabled = tbGes.EnableSoapFunctionExecutionControl(ref tbGesHeader, false);
			bool alive = false;
			try
			{
				alive = tbGes.GetCurrentUser(ref tbGesHeader, ref userName, ref companyName);
			}
			finally
			{
				tbGes.EnableSoapFunctionExecutionControl(ref tbGesHeader, enabled);
			}
			return alive;
		}

		//-----------------------------------------------------------------------
		public virtual bool IsLoginValid()
		{
			try { return tbGes.IsLoginValid(ref tbGesHeader); }
			catch { return false; }
		}

		//-----------------------------------------------------------------------
		public virtual bool InitTbLogin()
		{
			try
			{
				if (tbGes.Login(ref tbGesHeader, authenticationToken))
				{
					SetMenuHandle(MenuHandle);
					return true;
				}
				return false;
			}
			catch (Exception e)
			{
				TbLoaderClientInterfaceException tblExc =
					new TbLoaderClientInterfaceException(WebServicesWrapperStrings.InitTbLoginFailed, e);
				tblExc.LoginFailed = true;
				throw (tblExc);
			}
		}

		//-----------------------------------------------------------------------
		public virtual void SetMenuHandle(IntPtr menuHandle)
		{
			try
			{
				tbGes.SetMenuHandle(ref tbGesHeader, (int)menuHandle);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.SetMenuHandleFailed, e));
			}

		}

		//-----------------------------------------------------------------------
		public virtual void AttachTBLoader()
		{
			try
			{
				TbProcessId = GetProcessID();
			}
			catch
			{
				InternalStartTBLoader(latestTbLoaderArgs);
			}
		}

		//-----------------------------------------------------------------------
		public string GetActiveThreads()
		{
			return tbGes.GetActiveThreads(ref tbGesHeader);
		}

		//-----------------------------------------------------------------------
		public string GetLoginActiveThreads()
		{
			return tbGes.GetLoginActiveThreads(ref tbGesHeader);
		}

		//-----------------------------------------------------------------------
		public void KillThread(int threadID)
		{
			tbGes.KillThread(ref tbGesHeader, threadID);
		}

		//-----------------------------------------------------------------------
		public bool CanStopThread(int threadID)
		{
			return tbGes.CanStopThread(ref tbGesHeader, threadID);
		}

		//-----------------------------------------------------------------------
		public bool StopThread(int threadID)
		{
			return tbGes.StopThread(ref tbGesHeader, threadID);
		}

		//-----------------------------------------------------------------------
		public virtual void KillProcess(int procID)
		{
			Process p = Process.GetProcessById(procID);
			if (p != null)
				p.Kill();

		}

		//-----------------------------------------------------------------------
		public virtual bool KillProcess()
		{
			if (currentTBProcess == null)
				return false;
			currentTBProcess.Kill();
			return true;

		}

		//-----------------------------------------------------------------------
		public bool StopProcess(int procID)
		{
			Process p = Process.GetProcessById(procID);
			if (p != null)
				p.Close();
			return true;
		}


		//-----------------------------------------------------------------------
		public int GetProcessID()
		{
			try
			{
				return tbGes.GetProcessID(ref tbGesHeader);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.GetProcessIDFailed, e));
			}

		}
		//-----------------------------------------------------------------------
		public int GetLogins()
		{
			return tbGes.GetLogins(ref tbGesHeader);
		}

		//-----------------------------------------------------------------------
		public virtual bool CanChangeLogin(bool lockTbLoader)
		{
			try
			{
				return tbGes.CanChangeLogin(ref tbGesHeader, lockTbLoader);
			}
			catch (Exception)
			{
				return false;
			}
		}
		//-----------------------------------------------------------------------
		protected virtual int InternalChangeLogin
			(
			string oldAuthenticationToken,
			string newAuthenticationToken,
			bool unlock
			)
		{
			try
			{
				return tbGes.ChangeLogin(ref tbGesHeader, oldAuthenticationToken, newAuthenticationToken, unlock);
			}
			catch (Exception)
			{
				return -1;
			}
		}
		//-----------------------------------------------------------------------
		public int ChangeLogin
			(
			string oldAuthenticationToken,
			string newAuthenticationToken,
			IBasePathFinder aPathFinder,
			bool unlock
			)
		{

			int returnCode = InternalChangeLogin(oldAuthenticationToken, newAuthenticationToken, unlock);
			if (returnCode == 0)
			{
				if (pathFinder != null)
				{
					pathFinder = aPathFinder;
				}
				this.AuthenticationToken = newAuthenticationToken;
				SetMenuHandle(MenuHandle);
			}
			return returnCode;
		}

		/// <summary>
		/// verifica lo stato di lock di TbLoader 
		/// </summary>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		public bool IsTBLocked()
		{
			try
			{
				return tbGes.IsTBLocked(ref tbGesHeader);
			}
			catch (Exception)
			{
				return true;
			}
		}

		/// <summary>
		/// consente di mettere TB in stato di Lock
		/// </summary>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		public bool LockTB(string authenticationToken)
		{
			try
			{
				return tbGes.LockTB(ref tbGesHeader, authenticationToken);
			}
			catch (Exception)
			{
				return true;
			}
		}

		/// <summary>
		/// consente di togliere TB dallo stato di Lock
		/// </summary>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		public bool UnLockTB(string authenticationToken)
		{
			try
			{
				return tbGes.UnLockTB(ref tbGesHeader, authenticationToken);
			}
			catch (Exception)
			{
				return true;
			}
		}

		/// <summary>
		/// restituisco true perché l'istanza di TB potrebbe essere stata  chiusa e 
		/// conseguentemente la chiamata soap fallisce
		/// </summary>
		/// <returns></returns>
		//-----------------------------------------------------------------------
		public virtual bool CanCloseTB()
		{
			try
			{
				return tbGes.CanCloseTB(ref tbGesHeader);
			}
			catch
			{
				return true;
			}
		}

		/// <returns></returns>
		//-----------------------------------------------------------------------
		public virtual bool CanCloseLogin()
		{
			try
			{
				return tbGes.CanCloseLogin(ref tbGesHeader);
			}
			catch
			{
				return true;
			}

		}
		//-----------------------------------------------------------------------
		public virtual void CloseTB()
		{
			try
			{
				tbGes.CloseTB(ref tbGesHeader);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.CloseTBFailed, e));
			}
		}
		//-----------------------------------------------------------------------
		public virtual void DestroyTB()
		{
			tbGes.DestroyTB(ref tbGesHeader);
		}
		//-----------------------------------------------------------------------
		public virtual void CloseLoginAndExternalProcess()
		{
			CloseTB();
		}
		//-----------------------------------------------------------------------
		public virtual void CloseLogin()
		{
			try
			{
				tbGes.CloseLogin(ref tbGesHeader);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.CloseLoginFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public bool GetCurrentUser(out string currentUser, out string currentCompany)
		{
			currentUser = currentCompany = string.Empty;

			try
			{
				return tbGes.GetCurrentUser(ref tbGesHeader, ref currentUser, ref currentCompany);
			}
			catch (Exception)
			{
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public virtual DateTime GetApplicationDate()
		{
			try
			{
				string tmpDate = tbGes.GetApplicationDate(ref tbGesHeader);
				return DateTime.Parse(tmpDate);
			}
			catch (Exception e)
			{
				// se non riesco a chiamare non do errore ma torno una data di default
				Debug.Fail(e.Message);
				return DateTime.Today;
			}
		}

		//-----------------------------------------------------------------------
		public int GetApplicationYear()
		{
			try
			{
				return tbGes.GetApplicationYear(ref tbGesHeader);
			}
			catch (Exception e)
			{
				// se non riesco a chiamare non do errore ma torno l'anno odierno
				Debug.Fail(e.Message);
				return DateTime.Today.Year;
			}
		}

		//-----------------------------------------------------------------------
		public int GetApplicationMonth()
		{
			try
			{
				return tbGes.GetApplicationMonth(ref tbGesHeader);
			}
			catch (Exception e)
			{
				// se non riesco a chiamare non do errore ma torno il mese di oggi
				Debug.Fail(e.Message);
				return DateTime.Today.Month;
			}
		}

		//-----------------------------------------------------------------------
		public int GetApplicationDay()
		{
			try
			{
				return tbGes.GetApplicationDay(ref tbGesHeader);
			}
			catch (Exception e)
			{
				// se non riesco a chiamare non do errore ma torno oggi
				Debug.Fail(e.Message);
				return DateTime.Today.Day;
			}
		}

		//-----------------------------------------------------------------------
		public bool ExistDocument(int handle)
		{
			return tbGes.ExistDocument(ref tbGesHeader, handle);
		}

		//-----------------------------------------------------------------------
		public bool CloseDocument(int handle)
		{
			try
			{
				return tbGes.CloseDocument(ref tbGesHeader, handle);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.CloseDocumentFailed, e));
			}
		}

        //-----------------------------------------------------------------------
        public bool RunIconizedDocument(int handle)
		{
			try
			{
                return tbGes.RunIconizedDocument(ref tbGesHeader, handle);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.RunIconizedDocumentFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public bool SetDocumentInForeground(int handle)
		{
			try
			{
				return tbGes.SetDocumentInForeground(ref tbGesHeader, handle);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.RunFailed, "SetDocumentInForeground"), e));
			}
		}

		//-----------------------------------------------------------------------
		public WoormInfo CreateWoormInfo(string reportNamespace)
		{
			return new WoormInfo(tbWoormViewer, tbWoormViewerHeader, reportNamespace);
		}

		//-----------------------------------------------------------------------
		public virtual void ClearCache()
		{
			try
			{
				tbGes.ClearCache(ref tbGesHeader);
			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(String.Format(WebServicesWrapperStrings.RunFailed, "ClearCache"), e));
			}
		}

		//-----------------------------------------------------------------------
		public virtual void WaitForDisconnection()
		{
			if (CurrentTBProcess == null)
				return;
			CurrentTBProcess.WaitForExit();
		}
		//-----------------------------------------------------------------------
		private static void AddDiagnostic(string[] messages, int[] types, Diagnostic d)
		{
			for (int i = 0; i < messages.Length; i++)
			{
				DiagnosticType type = (DiagnosticType)types[i];
				if (type != DiagnosticType.Error &&
					type != DiagnosticType.Warning &&
					type != DiagnosticType.Information)
					type = DiagnosticType.Error;
				d.Set(type, messages[i]);
			}
		}
		//-----------------------------------------------------------------------
		public virtual Diagnostic GetLoginContextDiagnostic(bool clear)
		{
			return GetContextDiagnostic(clear, false);
		}


		//-----------------------------------------------------------------------
		public virtual Diagnostic GetApplicationContextDiagnostic(bool clear)
		{
			return GetContextDiagnostic(clear, true);

		}

		//-----------------------------------------------------------------------
		private Diagnostic GetContextDiagnostic(bool clear, bool application)
		{
			Diagnostic d = new Diagnostic("");
			string[] messages = new string[0];
			int[] types = new int[0];
			try
			{
				if (application)
					tbGes.GetApplicationContextMessages(ref tbGesHeader, clear, ref messages, ref types);
				else
					tbGes.GetLoginContextMessages(ref tbGesHeader, clear, ref messages, ref types);
			}
			catch (Exception ex)
			{
				messages = new string[] { ex.ToString() };
				types = new int[] { 0 };
			}

			AddDiagnostic(messages, types, d);


			return d;
		}
		//-----------------------------------------------------------------------
		public virtual Diagnostic GetGlobalDiagnostic(bool clear)
		{
			Diagnostic d = new Diagnostic("");
			d.Set(GetApplicationContextDiagnostic(clear));
			d.Set(GetLoginContextDiagnostic(clear));
			return d;
		}

		//-----------------------------------------------------------------------
		public virtual int[] GetDocumentThreads()
		{
			try
			{
				return tbGes.GetDocumentThreads(ref tbGesHeader);
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.ToString());
				return new int[0];
			}
		}

		#endregion

		#region funzioni per import/export dati di tb verso office

		//-----------------------------------------------------------------------
		public bool SetData(string dataXML, int saveAction, string loginName, out string result)
		{
			result = string.Empty;
			return tbXMLTransfer.SetData(ref tbXmlTransferHeader, dataXML, saveAction, loginName, ref result);
		}

		//-----------------------------------------------------------------------
		public bool GetData(string paramXML, bool useApproximation, string loginName, out StringCollection result)
		{
			result = new StringCollection();
			string[] messages = new string[1];
			bool res = false;
			try
			{
				string[] docs = null;
				res = tbXMLTransfer.GetData(ref tbXmlTransferHeader, paramXML, useApproximation, loginName, ref docs);
				result.AddRange(docs);
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
				return false;
			}

			return res;
		}

		//-----------------------------------------------------------------------
		public bool GetXMLParameters(string txXTechTempPath, bool useApproximation, string loginName, out string result)
		{
			result = string.Empty;
			return tbXMLTransfer.GetXMLParameters(ref tbXmlTransferHeader, txXTechTempPath, useApproximation, loginName, ref result);
		}

		//-----------------------------------------------------------------------
		public string GetXMLHotLink(string documentNamespace, string nsUri, string fieldXPath, string loginName)
		{
			return tbXMLTransfer.GetXMLHotLink(ref tbXmlTransferHeader, documentNamespace, nsUri, fieldXPath, loginName);
		}

		//-----------------------------------------------------------------------
		public string GetDocumentSchema(string documentNamespace, string profileName, string forUser)
		{
			return tbXMLTransfer.GetDocumentSchema(ref tbXmlTransferHeader, documentNamespace, profileName, forUser);
		}

		//-----------------------------------------------------------------------
		public string GetReportSchema(string reportNamespace, string forUser)
		{
			return tbXMLTransfer.GetReportSchema(ref tbXmlTransferHeader, reportNamespace, forUser);
		}


		#endregion

		#region Metodi per il Test Automatico

		//-----------------------------------------------------------------------
		public bool SetTestPlan(string xmlTestPlan)
		{
			try
			{
				return tbMacroRecorder.SetTestPlan(ref tbMacroRecorderHeader, xmlTestPlan);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//restituisce il numero di step a cui è arrivato
		//-----------------------------------------------------------------------
		public int PlayStep(int step, bool isStepByStep, ref string[] messages)
		{
			int result = 0;
			try
			{
				return result = tbMacroRecorder.PlayStep(ref tbMacroRecorderHeader, step, isStepByStep, ref messages);
			}
			catch
			{
			}

			return -1;
		}

		//-----------------------------------------------------------------------
		public void Record()
		{
			try
			{
				tbMacroRecorder.Record(ref tbMacroRecorderHeader);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
			}
		}

		//-----------------------------------------------------------------------
		public void StopRecord()
		{
			try
			{
				tbMacroRecorder.StopRecord(ref tbMacroRecorderHeader);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
			}
		}

		//-----------------------------------------------------------------------
		public void SetTPManagerHandle(IntPtr handle, ref int tbProcessID, ref bool isTb2005)
		{
			tbMacroRecorder.SetTPManagerHandle(ref tbMacroRecorderHeader, (int)handle, ref tbProcessID, ref isTb2005);
		}

		///<summary>
		/// Richiama il metodo per disconnettere momentaneamente l'azienda dal database
		/// (per eseguire il restore database senza incorrere nel problema del database in uso)
		/// 
		/// Valori di ritorno:
		/// 0 = Success
		/// 1 = Invalid Authentication Token
		/// 2 = Cannot Close Connections / Unknown thread or Unable to start thread
		///</summary>
		//-----------------------------------------------------------------------
		public int DisconnectCompany()
		{

			try
			{
				return tbGes.DisconnectCompany(ref tbGesHeader, authenticationToken);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return -1;
			}

		}

		///<summary>
		/// Riconnette l'azienda al database dal quale era stata momentaneamente slegata
		/// 
		/// Valori di ritorno:
		/// 0 = Success
		/// 1 = Invalid Authentication Token
		/// 2 = Unknown thread or Unable to start thread
		///</summary>
		//-----------------------------------------------------------------------
		public int ReconnectCompany()
		{
			try
			{
				return tbGes.ReconnectCompany(ref tbGesHeader, authenticationToken);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return -1;
			}

		}

		//-----------------------------------------------------------------------
		/// <summary>
		/// Set user interaction mode for login thread
		/// </summary>
		/// <param name="mode">0 = Undefined, 1 = Attended, 2 = Unattended</param>
		public void SetUserInteractionMode(int mode)
		{
			tbGes.SetUserInteractionMode(ref tbGesHeader, mode);
		}

		#endregion

		#region TbDMS

		//-----------------------------------------------------------------------
		public bool ArchiveFile(string fileName, string description, ref string result)
		{
			try
			{
				return tbDMS.ArchiveFile(ref tbDMSHeader, fileName, description, ref result);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool ArchiveFolder(string folder, ref string result)
		{
			try
			{
				return tbDMS.ArchiveFolder(ref tbDMSHeader, folder, ref result);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool AttachArchivedDocument(int archivedDocId, int documentHandle, ref string result)
		{
			try
			{
				return tbDMS.AttachArchivedDocument(ref tbDMSHeader, archivedDocId, documentHandle, ref result);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool AttachFile(string fileName, string description, int documentHandle, ref string result)
		{
			try
			{
				return tbDMS.AttachFile(ref tbDMSHeader, fileName, description, documentHandle, ref result);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool AttachFileInDocument(string documentNamespace, string documentKey, string fileName, string description, ref string result)
		{
			try
			{
				return tbDMS.AttachFileInDocument(ref tbDMSHeader, documentNamespace, documentKey, fileName, description, ref result);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool AttachFolder(string folder, int documentHandle, ref string result)
		{
			try
			{
				return tbDMS.AttachFolder(ref tbDMSHeader, folder, documentHandle, ref result);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool AttachFolderInDocument(string documentNamespace, string documentKey, string folder, ref string result)
		{
			try
			{
				return tbDMS.AttachFolderInDocument(ref tbDMSHeader, documentNamespace, documentKey, folder, ref result);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool AttachFromTable(string documentNamespace, string documentKey, ref string result)
		{
			try
			{
				return tbDMS.AttachFromTable(ref tbDMSHeader, documentNamespace, documentKey, ref result);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool AttachPaperyBarcode(int documentHandle, string barcode)
		{
			try
			{
				return tbDMS.AttachPaperyBarcode(ref tbDMSHeader, documentHandle, barcode);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool AttachPaperyInDocument(string documentNamespace, string documentKey, string barcode, string description, ref string result)
		{
			try
			{
				return tbDMS.AttachPaperyInDocument(ref tbDMSHeader, documentNamespace, documentKey, barcode, description, ref result);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public string GetAttachmentTemporaryFilePath(int attachmentID)
		{
			try
			{
				return tbDMS.GetAttachmentTemporaryFilePath(ref tbDMSHeader, attachmentID);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return string.Empty;
			}
		}

		//-----------------------------------------------------------------------
		public bool GetDefaultBarcodeType(ref string type, ref string prefix)
		{
			try
			{
				return tbDMS.GetDefaultBarcodeType(ref tbDMSHeader, ref type, ref prefix);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public string GetEasyAttachmentTempPath()
		{
			try
			{
				return tbDMS.GetEasyAttachmentTempPath(ref tbDMSHeader);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return string.Empty;
			}
		}

		//-----------------------------------------------------------------------
		public string GetNewBarcodeValue()
		{
			try
			{
				return tbDMS.GetNewBarcodeValue(ref tbDMSHeader);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return string.Empty;
			}
		}

		//-----------------------------------------------------------------------
		public bool MassiveAttachUnattendedMode(string folder, bool splitFile, ref string result)
		{
			try
			{
				return tbDMS.MassiveAttachUnattendedMode(ref tbDMSHeader, folder, splitFile, ref result);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public int RunDocumentWithEAPanel(string documentNamespace, string documentKey, ref string result)
		{
			try
			{
				return tbDMS.RunDocumentWithEAPanel(ref tbDMSHeader, documentNamespace, documentKey, ref result);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return -1;
			}
		}

		//-----------------------------------------------------------------------
		public string SaveAttachmentFileInFolder(int attachmentID, string sharedFolder)
		{
			try
			{
				return tbDMS.SaveAttachmentFileInFolder(ref tbDMSHeader, attachmentID, sharedFolder);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return string.Empty;
			}
		}

		//-----------------------------------------------------------------------
		public int[] SearchAttachmentsForDocument(string documentNamespace, string documentKey, string searchText, int location, string searchFields)
		{
			try
			{
				return tbDMS.SearchAttachmentsForDocument(ref tbDMSHeader, documentNamespace, documentKey, searchText, location, searchFields);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return null;
			}
		}

        //-----------------------------------------------------------------------
        public bool GetAttachmentBinaryContent(int attachmentID, ref byte[] binaryContent, ref string fileName, ref bool veryLargeFile)
        {
            try
            {
                return tbDMS.GetAttachmentBinaryContent(ref tbDMSHeader, attachmentID, ref binaryContent, ref fileName, ref veryLargeFile);
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.ToString());
                return false;
            }
        }

        //-----------------------------------------------------------------------
        public bool ArchiveBinaryContent(byte[] binaryContent, string sourceFileName, string description, ref int archiveDocID, ref string result)
        {
            try
            {
                return tbDMS.ArchiveBinaryContent(ref tbDMSHeader, binaryContent, sourceFileName, description, ref archiveDocID, ref result);
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.ToString());
                return false;
            }
        }

        //-----------------------------------------------------------------------
        public bool AttachBinaryContent(byte[] binaryContent, string sourceFileName, string description, int documentHandle, ref int attachmentID, ref string result)
        {
            try
            {
                return tbDMS.AttachBinaryContent(ref tbDMSHeader, binaryContent, sourceFileName, description, documentHandle, ref attachmentID, ref result);
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.ToString());
                return false;
            }
        }

        //-----------------------------------------------------------------------
        public bool AttachBinaryContentInDocument(byte[] binaryContent, string sourceFileName, string description, string documentNamespace, string documentKey, ref int attachmentID, ref string result)
        {
            try
            {
                return tbDMS.AttachBinaryContentInDocument(ref tbDMSHeader, binaryContent, sourceFileName, description, documentNamespace, documentKey, ref attachmentID, ref result);
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.ToString());
                return false;
            }
        }

        //-----------------------------------------------------------------------
        public int GetAttachmentIDByFileName(string documentNamespace, string documentKey, string fileName)
        {
            try
            {
                return tbDMS.GetAttachmentIDByFileName(ref tbDMSHeader, documentNamespace, documentKey, fileName);
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.ToString());
                return -1;
            }
        }

        #endregion

        #region ErpPricePoliciesComponents

        //-----------------------------------------------------------------------
        public int DefaultSalePrices_Create()
		{
			try
			{
				return erpPricePoliciesComponentsClient.DefaultSalePrices_Create(ref erpPricePoliciesComponentsHeader);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return -1;
			}
		}

		//-----------------------------------------------------------------------
		public bool DefaultSalePrices_Dispose(int handle)
		{
			try
			{
				return erpPricePoliciesComponentsClient.DefaultSalePrices_Dispose(ref erpPricePoliciesComponentsHeader, handle);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public void DefaultSalePrices_GetDefaultDiscountPerc(int handle, string Customer, string Item, string UoM, double Quantity, ref double aDiscount1, ref double aDiscount2)
		{
			try
			{
				erpPricePoliciesComponentsClient.DefaultSalePrices_GetDefaultDiscountPerc(ref erpPricePoliciesComponentsHeader, handle, Customer, Item, UoM, Quantity, ref aDiscount1, ref aDiscount2);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
			}
		}

        //-----------------------------------------------------------------------
        public double DefaultSalePrices_GetDefaultDiscountPercEx(int handle, string Customer, string Item, string UoM, double Quantity, ref double aDiscount1, ref double aDiscount2, ref string aDiscountFormula)
        {
            try
            {
                return erpPricePoliciesComponentsClient.DefaultSalePrices_GetDefaultDiscountPercEx(ref erpPricePoliciesComponentsHeader, handle, Customer, Item, UoM, Quantity, ref aDiscount1, ref aDiscount2, ref aDiscountFormula);
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.ToString());
                return 0.0f;
            }
        }

		//-----------------------------------------------------------------------
		public string DefaultSalePrices_GetDefaultDiscountString(int handle, string Customer, string Item, string UoM, double Quantity)
		{
			try
			{
				return erpPricePoliciesComponentsClient.DefaultSalePrices_GetDefaultDiscountString(ref erpPricePoliciesComponentsHeader, handle, Customer, Item, UoM, Quantity);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return string.Empty;
			}
		}

		//-----------------------------------------------------------------------
		public double DefaultSalePrices_GetDefaultPrice(int handle, string Customer, string Item, string UoM, double Quantity)
		{
			try
			{
				return erpPricePoliciesComponentsClient.DefaultSalePrices_GetDefaultPrice(ref erpPricePoliciesComponentsHeader, handle, Customer, Item, UoM, Quantity);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return 0.0f;
			}
		}

		//-----------------------------------------------------------------------
		public void DefaultSalePrices_SetCurrencyInfo(int handle, string Currency, string FixingDate, double Fixing, bool FixingIsManual)
		{
			try
			{
				erpPricePoliciesComponentsClient.DefaultSalePrices_SetCurrencyInfo(ref erpPricePoliciesComponentsHeader, handle, Currency, FixingDate, Fixing, FixingIsManual);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return;
			}
		}

		//-----------------------------------------------------------------------
		public void DefaultSalePrices_SetDocumentDate(int handle, string documentDate)
		{
			try
			{
				erpPricePoliciesComponentsClient.DefaultSalePrices_SetDocumentDate(ref erpPricePoliciesComponentsHeader, handle, documentDate);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return;
			}
		}

		//-----------------------------------------------------------------------
		public void DefaultSalePrices_SetNetOfTax(int handle, bool netOfTax)
		{
			try
			{
				erpPricePoliciesComponentsClient.DefaultSalePrices_SetNetOfTax(ref erpPricePoliciesComponentsHeader, handle, netOfTax);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return;
			}
		}

		//-----------------------------------------------------------------------
		public void DefaultSalePrices_SetPriceList(int handle, string priceList)
		{
			try
			{
				erpPricePoliciesComponentsClient.DefaultSalePrices_SetPriceList(ref erpPricePoliciesComponentsHeader, handle, priceList);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return;
			}
		}

		//-----------------------------------------------------------------------
		public void DefaultSalePrices_SetValidityDate(int handle, string validityDate)
		{
			try
			{
				erpPricePoliciesComponentsClient.DefaultSalePrices_SetValidityDate(ref erpPricePoliciesComponentsHeader, handle, validityDate);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return;
			}
		}

		//-----------------------------------------------------------------------
		public void ItemPriceFromPriceListToDate(ref string tPriceList, ref string item, ref double quantity, ref string date)
		{
			try
			{
				erpPricePoliciesComponentsClient.ItemPriceFromPriceListToDate(ref erpPricePoliciesComponentsHeader, ref tPriceList, ref item, ref quantity, ref date);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return;
			}
		}

		#endregion

		#region ItemsServices
		//-----------------------------------------------------------------------
		public string GetPathItemImageFromNamespace(string aNamespace)
		{
			try
			{
				return itemsServicesClient.GetPathItemImageFromNamespace(ref itemsServicesClientHeader, aNamespace);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return string.Empty;
			}
		}

		#endregion 

		#region CustomersSuppliersDbl
		//-----------------------------------------------------------------------
		public bool ExistCustForTaxIdOrFiscalCode(string taxIdNumber, string fiscalCode)
		{
			try
			{
				return customersSuppliersDblClient.ExistCustForTaxIdOrFiscalCode(ref customersSuppliersDblHeader, taxIdNumber, fiscalCode);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool CheckForTaxIdNoWithCountryCode(string countryCode, string taxIdNumber)
		{
			try
			{
				return customersSuppliersDblClient.CheckForTaxIdNoWithCountryCode(ref customersSuppliersDblHeader, countryCode, taxIdNumber);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool FiscalCodeCheck(string taxIdNumber)
		{
			try
			{
				return customersSuppliersDblClient.FiscalCodeCheck(ref customersSuppliersDblHeader, taxIdNumber);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool TaxIdNoCheck(string taxIdNumber)
		{
			try
			{
				return customersSuppliersDblClient.TaxIdNoCheck(ref customersSuppliersDblHeader, taxIdNumber);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}

		//-----------------------------------------------------------------------
		public bool TaxIdNoSecondCheck(string taxIdNumber)
		{
			try
			{
				return customersSuppliersDblClient.TaxIdNoSecondCheck(ref customersSuppliersDblHeader, taxIdNumber);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.ToString());
				return false;
			}
		}
        #endregion
       
        #region CreditLimitComponents
        //-----------------------------------------------------------------------
        public bool CreditLimitManager_GetData(
                                                  int handle,
                                                  string Customer,
                                                  ref bool CreditLimitManage,
                                                  ref bool Blocked,
                                                  ref double OrderedExposure,
                                                  ref double OrderedMargin,
                                                  ref double TotalExposure,
                                                  ref double TotalExposureMargin)
        {
            try
            {
                return erpCreditLimitComponentsClient.CreditLimitManager_GetData(ref erpCreditLimitComponentsHeader, handle, ref Customer, ref CreditLimitManage, ref Blocked, ref OrderedExposure, ref OrderedMargin, ref TotalExposure, ref TotalExposureMargin);
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.ToString());
                return false;
            }
        }

        //-----------------------------------------------------------------------
        public int CreditLimitManager_Create()
        {
            try
            {
                return erpCreditLimitComponentsClient.CreditLimitManager_Create(ref erpCreditLimitComponentsHeader);
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.ToString());
                return -1;
            }
        }

        //-----------------------------------------------------------------------
        public bool CreditLimitManager_Dispose(int handle)
        {
            try
            {
                return erpCreditLimitComponentsClient.CreditLimitManager_Dispose(ref erpCreditLimitComponentsHeader, handle);
            }
            catch (Exception exc)
            {
                Debug.Fail(exc.ToString());
                return false;
            }
        }
       
        #endregion

     
        //-----------------------------------------------------------------------
        public TBWebProxy CreateTBWebProxy()
		{
			TbGesClient tbGesClone = new TbGesClient(CreateBinding(false, false), tbGes.Endpoint.Address);
			return new TBWebProxy(tbGesClone, tbGesHeader);
		}

		//-----------------------------------------------------------------------
		public void Dispose()
		{
			try
			{
				if (tbGes != null)
					tbGes.Close();
			}
			catch
			{
			}
			try
			{
				if (tbWoormViewer != null)
					tbWoormViewer.Close();
			}
			catch
			{
			}
			try
			{
				if (tbXMLTransfer != null)
					tbXMLTransfer.Close();
			}
			catch
			{
			}
			try
			{
				if (tbGenlibUI != null)
					tbGenlibUI.Close();
			}
			catch
			{
			}
		}

		//-----------------------------------------------------------------------
		public void SetTimeout(TimeSpan timeout)
		{
			WCFSoapClient.SetTimeoutToBinding(currentBinding, timeout);
		}

		//-----------------------------------------------------------------------
		public virtual bool Logged
		{
			get
			{
				try
				{
					return tbGes.IsLoginValid(ref tbGesHeader);
				}
				catch
				{
					return false;
				}
			}
		}

		//-----------------------------------------------------------------------
		public virtual bool CloseAllDocuments()
		{
			return tbGes.CloseAllDocuments(ref tbGesHeader);
		}

		//-----------------------------------------------------------------------
		public virtual bool WaitForExit(int milliseconds)
		{
			if (currentTBProcess == null)
				return true;
			return currentTBProcess.WaitForExit(milliseconds);
		}
	}
	//=================================================================================
	public class TbLoaderRemoteClientInterface : TbLoaderClientInterface
	{
		private string user;
		private string password;
		private string remotePath;
		ProcessWMI wmi;
		private int remoteServicePort;
		//-----------------------------------------------------------------------
		public TbLoaderRemoteClientInterface(
			IBasePathFinder pathFinder,
			string launcher,
			int tbPort,
			string authenticationToken,
			WCFBinding binding,
			string remotePath,
			string user,
			string password,
			int remoteServicePort)
			: base(pathFinder, launcher, tbPort, authenticationToken, binding)
		{
			this.remotePath = remotePath;
			this.user = user;
			this.password = password;
			this.remoteServicePort = remoteServicePort;
		}

		//-----------------------------------------------------------------------
		protected override string GetNewSemaphorePath()
		{
			string temp = Path.Combine(((BasePathFinder)PathFinder).CalculateRemoteCustomPath(), NameSolverStrings.Temp);
			return Path.Combine(temp, Guid.NewGuid().ToString());
		}
		//-----------------------------------------------------------------------
		public override int TbProcessId
		{
			get
			{
				return wmi == null ? 0 : wmi.ProcessId;
			}
			set
			{
				base.TbProcessId = value;
			}
		}
		//-----------------------------------------------------------------------
		protected override bool StartProcess(string arguments)
		{
			wmi = new ProcessWMI(user, password, TbServer, remoteServicePort);
			wmi.ExecuteRemoteProcessWMI(remotePath, arguments);
			return wmi.ProcessId != 0;
		}
		//-----------------------------------------------------------------------
		public override void KillProcess(int procID)
		{
			wmi.KillRemoteProcess(procID);
		}
		//-----------------------------------------------------------------------
		public override bool WaitForExit(int milliseconds)
		{
			if (wmi == null)
				return true;
			return wmi.WaitForExit(milliseconds);
		}
		//-----------------------------------------------------------------------
		public override bool KillProcess()
		{
			if (wmi == null || wmi.ProcessId == 0)
				return false;
			wmi.KillRemoteProcess(wmi.ProcessId);
			return true;
		}
		//-----------------------------------------------------------------------
		public override bool Connected
		{
			get
			{
				return wmi != null && wmi.ProcessId > 0;
			}
		}
	}
	//=================================================================================
	public class SingletonTbLoaderClientInterface : TbLoaderClientInterface
	{
		public const int WM_USER = 0x0400;
		public const int UM_GET_SOAP_PORT = WM_USER + 904;

		[DllImport("User32", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		//-----------------------------------------------------------------------
		public SingletonTbLoaderClientInterface(IBasePathFinder pathFinder, string launcher, int tbPort, string authenticationToken, WCFBinding binding)
			: base(pathFinder, launcher, tbPort, authenticationToken, binding)
		{
		}

		//-----------------------------------------------------------------------
		public override void StartTbLoader(string launcher, bool unattendedMode, bool clearCachedData, string additionalArgs)
		{
			try
			{
				Process[] processes = Process.GetProcessesByName("tbloader");

				if (processes.Length == 0)
					throw new ApplicationException(WebServicesWrapperStrings.ManualStartTBLoader);

				InitSoapInterfaceURL();

				try
				{
					int id = GetProcessID();
					currentTBProcess = Process.GetProcessById(id);
				}
				catch (Exception ex)
				{
					throw new ApplicationException(string.Format(WebServicesWrapperStrings.NoValidTbLoaderListening, tbPort), ex);
				}

			}
			catch (Exception e)
			{
				throw (new TbLoaderClientInterfaceException(WebServicesWrapperStrings.ConnectionToTbFailed, e));
			}
		}

		//-----------------------------------------------------------------------
		public override void DestroyTB()
		{
			//does nothing
		}

		//-----------------------------------------------------------------------
		public override bool Connected
		{
			get
			{
				return currentTBProcess != null;
			}
		}

		//-----------------------------------------------------------------------
		public override bool InitTbLogin()
		{
			if (base.InitTbLogin())
			{
				tbGes.SetUserInteractionMode(ref tbGesHeader, 2);
				return true;
			}
			return false;
		}
	}

}
