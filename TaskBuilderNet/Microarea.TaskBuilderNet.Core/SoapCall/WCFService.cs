using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.TaskBuilderNet.Core.SoapCall
{
	//================================================================================
	public class TBSoapException : Exception
	{
		//--------------------------------------------------------------------------------
		public TBSoapException(string message)
			: base(message)
		{

		}
	}
	//================================================================================
	[DataContract]
	public class TBSoapFault
	{
		private string report;

		//--------------------------------------------------------------------------------
		public TBSoapFault(Exception ex)
		{
			this.Message = ex.Message;
		}

		//--------------------------------------------------------------------------------
		[DataMember]
		public string Message
		{
			get { return this.report; }
			set { this.report = value; }
		}
	}


	//-----------------------------------------------------------------------------
	//================================================================================
	[DataContract(Namespace = "urn:Microarea.Web.Services")]
	public class TBHeaderInfo
	{
		[DataMember]
		public string AuthToken;
	}

	//-----------------------------------------------------------------------------
	//================================================================================
	[MessageContract(IsWrapped = false)]
	public abstract class TbSoapArgument
	{
		[MessageHeader]
		public TBHeaderInfo HeaderInfo;

		//-----------------------------------------------------------------------------
		[DebuggerStepThrough]
		public virtual IntPtr GetThreadHwnd()
		{
			return TBWCFService.GetThreadHwnd(this);
		}

		//--------------------------------------------------------------------------------
		public virtual int GetContextHandle() { return 0; }
		//--------------------------------------------------------------------------------
		public abstract string GetFunctionNamespace();
	}

	//-----------------------------------------------------------------------------
	//================================================================================
	public abstract class TBWCFService
	{

		public delegate IntPtr GetThreadHwndFunction(string authenticationToken, int contextHandle, string functionNamespace, bool checkAuthToken);
		public delegate void LoadTBDllFunction(string dllName);
		public static GetThreadHwndFunction GetThreadHwndFunctionPointer;
		public static LoadTBDllFunction LoadTBDllFunctionPointer;

		[DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr GetModuleHandle(string moduleName);

		public static int SoapPort = 0;
		public static int TcpPort = 0;

		//--------------------------------------------------------------------------------
        public Uri GetHTTPAddress()
        {
            return new Uri(string.Format("http://{0}:{1}/{2}", GetServerName(), SoapPort > 0 ? SoapPort : TcpPort, GetNamespace()));
        }

		//--------------------------------------------------------------------------------
        public Uri GetTCPAddress()
        {
            return new Uri(string.Format("net.tcp://{0}:{1}/{2}", GetServerName(), TcpPort, GetNamespace()));
        }

        //--------------------------------------------------------------------------------
        private static string GetServerName()
        {
            string serverFqdn = Environment.MachineName;

            BasePathFinder pf = BasePathFinder.BasePathFinderInstance;
            SettingItem si =
                pf == null ?
                null :
                pf.GetSettingItem("Framework", "TbLoader", "TbLoader", "ServerFullyQualifiedDomainName");

            var serverFullyQualifiedDomainName =
                si != null ?
                (si.Values[0] as string) :
                null;

            if (!String.IsNullOrEmpty(serverFullyQualifiedDomainName))
            {
                serverFqdn = serverFullyQualifiedDomainName;
            }
            return serverFqdn;
        }
        
		//--------------------------------------------------------------------------------
		public abstract string GetName();
		//--------------------------------------------------------------------------------
		public abstract string GetNamespace();

		//-----------------------------------------------------------------------------
		public static IntPtr GetThreadHwnd(TbSoapArgument info)
		{
			if (GetThreadHwndFunctionPointer != null)
			{
				string authToken = null;
				int contextHandle = 0;
				if (info.HeaderInfo != null)
				{
					authToken = info.HeaderInfo.AuthToken;
					contextHandle = info.GetContextHandle();
				}

				return GetThreadHwndFunctionPointer(authToken, contextHandle, info.GetFunctionNamespace(), true);
			}
			return IntPtr.Zero;
		}

		//-----------------------------------------------------------------------------
		protected static void LoadTBDll(string dllName)
		{
			StartTimer();

			if (GetModuleHandle(dllName) != IntPtr.Zero)
				return;

			if (LoadTBDllFunctionPointer != null)
				LoadTBDllFunctionPointer(dllName);
		}

		static Timer timer = null;
		//-----------------------------------------------------------------------------
		public static void StartTimer()
		{
			lock (typeof(TBWCFService))
			{
				//-1 vuol dire infinito, non parte nessun timer per autochiudere il tbloader
				if (InstallationData.ServerConnectionInfo.TBLoaderTimeOut <= -1)
					return;

				long timeout = InstallationData.ServerConnectionInfo.TBLoaderTimeOut * 60000;
				if (timer == null)
					timer = new Timer(new TimerCallback((s) => { ForceCloseTB(); }), null, timeout, Timeout.Infinite);
				else
					timer.Change(timeout, Timeout.Infinite);
			}
		}

		//-----------------------------------------------------------------------------
		private static void ForceCloseTB()
		{
			if (timer != null)
			{
				timer.Dispose();
				timer = null;
			}

			//Chiude il processo senza tante cerimonie (ma solo se si chiama tbloader.exe)
			Process currProc = Process.GetCurrentProcess();
			if (currProc != null && (currProc.ProcessName.CompareNoCase("tbloader.exe") || currProc.ProcessName.CompareNoCase("tbloader")))
				currProc.Kill();
		}
	}

	//================================================================================
	public class DiscoveryRestService : IRestService
	{
		List<ServiceHost> services;

		//-----------------------------------------------------------------------------
		public DiscoveryRestService(List<ServiceHost> services)
		{
			this.services = services;
		}

		//-----------------------------------------------------------------------------
		public bool ProcessRequest(HttpListenerRequest request, HttpListenerResponse response)
		{
			try
			{
				string subPath = request.Url.LocalPath;
				if (string.Compare(subPath, "/Disco", StringComparison.InvariantCultureIgnoreCase) != 0 &&
					string.Compare(subPath, "/ListServices", StringComparison.InvariantCultureIgnoreCase) != 0)
					return false;
				// Construct a response.
				string responseString = GetResponse();
				byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
				// Get a response stream and write the response to it.
				response.ContentLength64 = buffer.Length;
				response.ContentEncoding = Encoding.UTF8;
				response.ContentType = "text/xml";
				System.IO.Stream output = response.OutputStream;
				output.Write(buffer, 0, buffer.Length);
				// You must close the output stream.
				output.Close();
				return true;
			}
			catch
			{
				return false;
			}
		}

		//-----------------------------------------------------------------------------
		private string GetResponse()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(
@"<?xml version=""1.0"" encoding=""utf-8""?> 
<discovery xmlns=""http://schemas.xmlsoap.org/disco/"">");

			foreach (ServiceHost service in services)

				sb.AppendFormat(
@"
	<contractRef xmlns=""http://schemas.xmlsoap.org/disco/scl/"" ref=""http://{0}:{1}/{2}?wsdl"" docRef=""http://{0}:{1}/{2}"" />",
									Environment.MachineName,
									TBWCFService.SoapPort,
									((TBWCFService)service.SingletonInstance).GetNamespace());

			sb.Append(
@"
</discovery>");

			return sb.ToString();
		}
	}
	//================================================================================
	public interface IRestService
	{
		bool ProcessRequest(HttpListenerRequest request, HttpListenerResponse response);
	}
	//================================================================================
	public class RestServiceContainer : MarshalByRefObject
	{
		HttpListener listener;
		Thread listeningThread;
		bool stopped = false;
		List<IRestService> services = new List<IRestService>();

		//-----------------------------------------------------------------------------
		public RestServiceContainer()
		{
		}

		//-----------------------------------------------------------------------------
		public void Start()
		{
			listeningThread = new Thread(new ThreadStart(Listen));
			listeningThread.Start();
		}

		//-----------------------------------------------------------------------------
		private void Listen()
		{

			listener = new HttpListener();
			listener.Prefixes.Add(string.Format("http://{0}:{1}/", Environment.MachineName, TBWCFService.SoapPort));
			listener.Prefixes.Add(string.Format("http://localhost:{0}/", TBWCFService.SoapPort));
			try
			{
				listener.Start();
			}
			catch
			{
				stopped = true;
			}
			while (!stopped)
			{
				IAsyncResult ar = listener.BeginGetContext(new AsyncCallback(OnRequest), null);
				ar.AsyncWaitHandle.WaitOne();
			}
		}

		//-----------------------------------------------------------------------------
		public void Stop()
		{
			stopped = true;
			listener.Close();
			listeningThread.Join();
		}

		//-----------------------------------------------------------------------------
		private void OnRequest(IAsyncResult ar)
		{
			if (!listener.IsListening)
				return;

			HttpListenerContext context = null;
			try
			{
				context = listener.EndGetContext(ar);
			}
			catch
			{
				return;
			}

			try
			{
				foreach (IRestService service in services)
					try
					{
						if (service.ProcessRequest(context.Request, context.Response))
							return;
					}
					catch
					{
					}

				using (StreamWriter w = new StreamWriter(context.Response.OutputStream))
					w.Write("<html><head><title>Mago.Net Developement Environment</title></head><body><h1>Welcome to the Mago.Net development Environment</h1></body></html>");
			}
			finally
			{
				context.Response.OutputStream.Close();
			}

		}

		public bool Active { get { return listener != null && listener.IsListening; } }

		//-----------------------------------------------------------------------------
		internal void Add(IRestService service)
		{
			services.Add(service);
		}
	}


	//================================================================================
	public class ServiceCache
	{
		private static List<ServiceHost> services = new List<ServiceHost>();
		private static RestServiceContainer restService;
		private static bool inErrorState = false;
		public static bool IsRestServiceActive { get { return restService != null && restService.Active; } }
		//--------------------------------------------------------------------------------
		public static void Clear()
		{
			lock (typeof(ServiceCache))
			{
				foreach (ServiceHost host in services)
					host.Close();

				services.Clear();
			}
		}

		//-----------------------------------------------------------------------------
		public static void StartRestService()
		{
			if (TBWCFService.SoapPort > 0)
			{
				restService = new RestServiceContainer();
				restService.Add(new DiscoveryRestService(services));
				restService.Start();
				
			}
		}
		
		//-----------------------------------------------------------------------------
		public static void StopRestService()
		{
			if (restService != null)
				restService.Stop();
		}
		
		//-----------------------------------------------------------------------------
		public static void AddService(TBWCFService service)
		{
			if (inErrorState)
				return;

			lock (typeof(ServiceCache))
			{
				ServiceHost host = null;
				try
				{
					Uri[] baseAddresses = GetBaseAddresses(service);
					host = new ServiceHost(service, baseAddresses);

					ServiceMetadataBehavior be = new ServiceMetadataBehavior();
					host.Description.Behaviors.Add(be);

					if (TBWCFService.SoapPort > 0)
					{
						be.HttpGetEnabled = true;
						AddHttpBinding(service, host);
					}

					if (TBWCFService.TcpPort > 0)
						AddTcpBinding(service, host);

					host.Open();
					services.Add(host);

				}
				catch (AddressAlreadyInUseException ex)
				{
					inErrorState = true;
					throw ex;
				}
			}
		}
		public static void AddRestService(IRestService service)
		{
			if (restService!= null && restService.Active)
				restService.Add(service);
		}
		//-----------------------------------------------------------------------------
		private static Uri[] GetBaseAddresses(TBWCFService service)
		{
			List<Uri> addresses = new List<Uri>();
			if (TBWCFService.SoapPort > 0)
				addresses.Add(service.GetHTTPAddress());

			if (TBWCFService.TcpPort > 0)
				addresses.Add(service.GetTCPAddress());

			return addresses.ToArray();
		}

		//-----------------------------------------------------------------------------
		private static void AddTcpBinding(TBWCFService service, ServiceHost host)
		{
			ServiceEndpoint ep = null;
			NetTcpBinding binding = new NetTcpBinding();
			binding.MaxReceivedMessageSize = int.MaxValue;
			binding.ReaderQuotas.MaxDepth = int.MaxValue;
			binding.ReaderQuotas.MaxArrayLength = int.MaxValue;
			binding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
			binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
			binding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;

			binding.Namespace = "urn:Microarea.Web.Services";
			binding.Name = service.GetName();
			ep = host.AddServiceEndpoint(service.GetType(), binding, service.GetName());
			ep.Name = string.Format("{0}Tcp", service.GetName());

			ep = host.AddServiceEndpoint
			  (
			  ServiceMetadataBehavior.MexContractName,
			  MetadataExchangeBindings.CreateMexTcpBinding(),
			  "mex"
			  );
		}

		//-----------------------------------------------------------------------------
		private static void AddHttpBinding(TBWCFService service, ServiceHost host)
		{
			ServiceEndpoint ep = null;
			BasicHttpBinding binding = new BasicHttpBinding();
			binding.MaxReceivedMessageSize = int.MaxValue;
			binding.ReaderQuotas.MaxDepth = int.MaxValue;
			binding.ReaderQuotas.MaxArrayLength = int.MaxValue;
			binding.ReaderQuotas.MaxBytesPerRead = int.MaxValue;
			binding.ReaderQuotas.MaxStringContentLength = int.MaxValue;
			binding.ReaderQuotas.MaxNameTableCharCount = int.MaxValue;

			binding.Namespace = "urn:Microarea.Web.Services";
			binding.Name = service.GetName();
			ep = host.AddServiceEndpoint(service.GetType(), binding, service.GetName());
			ep.Name = string.Format("{0}Soap", service.GetName());

			ep = host.AddServiceEndpoint
			  (
			  ServiceMetadataBehavior.MexContractName,
			  MetadataExchangeBindings.CreateMexHttpBinding(),
			  "mex"
			  );
		}
	}

}