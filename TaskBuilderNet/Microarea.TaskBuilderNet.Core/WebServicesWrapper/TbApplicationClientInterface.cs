using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SoapCall;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.TBApplicationWrapper
{
	abstract class TBAsyncResult : IAsyncResult
	{
		private Thread thread;
		private object asyncState;
		ManualResetEvent evt = new ManualResetEvent(false);
		internal TBAsyncResult()
		{
		}
		
		internal void Start(ThreadStart ts, object asyncState)
		{
			this.asyncState = asyncState;
			this.thread = new Thread(ts);
			this.thread.Start();
		}

		internal void Finished(AsyncCallback callback)
		{
			callback(this);
			evt.Set();
		}
		#region IAsyncResult Members

		public object AsyncState
		{
			get { return asyncState; }
		}

		public System.Threading.WaitHandle AsyncWaitHandle
		{
			get { return evt; }
		}

		public bool CompletedSynchronously
		{
			get { return false; }
		}

		public bool IsCompleted
		{
			get { return !thread.IsAlive; }
		}

		#endregion
	}

	public class TbApplicationClientInterface : TbLoaderClientInterface
	{
		ITBApplicationProxy tbAppProxy;
       
        static Type proxyType;
        static Semaphore loginSemaphore = new Semaphore(1, 1);

        //-----------------------------------------------------------------------
		private bool SingleThreaded
        {
            get 
            {
                PathFinder pf = PathFinder as PathFinder;
                return pf != null ? pf.SingleThreaded : false;
            }
        }
		 //-----------------------------------------------------------------------
		public string[] DocumentHistory { get { return tbAppProxy.DocumentHistory; } }
       
		//-----------------------------------------------------------------------
		private static void InitProxyType(string tbApplicationPath)
		{
			AssemblyName an = AssemblyName.GetAssemblyName(Path.Combine(tbApplicationPath, "TBApplicationWrapper.dll"));
            Assembly asmb = null;

            try
            {
                asmb = Assembly.Load(an);
            }
            catch (Exception ex)
            {
                string s = ex.Message;
                proxyType = null;
                return;
            }

			proxyType = asmb.GetType("Microarea.Framework.TBApplicationWrapper.TBApplicationProxy");
		}
		//-----------------------------------------------------------------------
		private static ITBApplicationProxy CreateTBApplicationProxy(string tbApplicationPath, string arguments)
		{
			string path = Environment.GetEnvironmentVariable("PATH");
			path += ";" + tbApplicationPath;
			Environment.SetEnvironmentVariable("PATH", path);
			if (proxyType == null)
				InitProxyType(tbApplicationPath);

			ConstructorInfo ci = proxyType.GetConstructor(new Type[] { typeof(string), typeof(string) });
			return ci.Invoke(new object[] {tbApplicationPath, arguments}) as ITBApplicationProxy;
		}

		//-----------------------------------------------------------------------
		private static ITBApplicationProxy AttachProxyToTbApplication(string tbApplicationPath)
		{
			if (proxyType == null)
				InitProxyType(tbApplicationPath); 
			MethodInfo mi = proxyType.GetMethod("AttachProxyToTbApplication");
			return mi.Invoke(null, null) as ITBApplicationProxy;
		}
		//-----------------------------------------------------------------------
		public static void CloseTBApplication()
		{
			if (proxyType == null)
				return;

			MethodInfo mi = proxyType.GetMethod("CloseTbApplication");
			mi.Invoke(null, null);
		}
        //-----------------------------------------------------------------------
        public TbApplicationClientInterface(String authenticationToken)
            : base(BasePathFinder.BasePathFinderInstance, "", 0, authenticationToken, WCFBinding.None)
        {
            tbAppProxy = null;
        }
		//-----------------------------------------------------------------------
		public TbApplicationClientInterface(String launcher, IBasePathFinder pathFinder, int port, String authenticationToken, WCFBinding binding)
			: base(pathFinder, launcher, port, authenticationToken, binding)
		{
			tbAppProxy = null;
		}

		//-----------------------------------------------------------------------
		public TbApplicationClientInterface(String launcher, IBasePathFinder pathFinder, String authenticationToken)
			: base(pathFinder, launcher, authenticationToken)
		{
			tbAppProxy = null;
		}

		//-----------------------------------------------------------------------
		public override bool Connected
		{
			get
			{
				return tbAppProxy != null
					&& !tbAppProxy.HasExited();
			}
		}

		//-----------------------------------------------------------------------
		public bool Valid
		{
			get
			{
				return tbAppProxy != null && tbAppProxy.Valid;
			}
		}
		//-----------------------------------------------------------------------
		public override bool Logged
		{
			get
			{
				return tbAppProxy != null && tbAppProxy.Logged;
			}
		}

		//-----------------------------------------------------------------------
		public bool CanShowLockStructure
		{
			get
			{
				return tbAppProxy != null && tbAppProxy.Logged && tbAppProxy.CanShowLockStructure;
			}
		}

		//-----------------------------------------------------------------------
		public bool IsEasyBuilderDeveloper
		{
			get { return tbAppProxy != null && tbAppProxy.IsEasyBuilderDeveloper; }
		}
		//-----------------------------------------------------------------------
		protected override void InternalStartTBLoader(String arguments)
		{
			latestTbLoaderArgs = arguments;

			tbAppProxy = CreateTBApplicationProxy(TbApplicationPath, arguments);
		}

		//-----------------------------------------------------------------------
       // public IHttpDocumentHandler HttpDocumentHandler { get { return tbAppProxy.HttpDocumentHandler; } }

		//-----------------------------------------------------------------------
		public override void StartTbLoader(String launcher, bool unattendedMode, bool clearCachedData)
		{
			StartTbLoader(launcher, unattendedMode, clearCachedData, -1);
		}

		//-----------------------------------------------------------------------
		public void StartTbLoader(String launcher, bool unattendedMode, bool clearCachedData, int tbPort)
		{
			//try first to attach to an existing tbapplication 
			AttachTBLoader(tbPort);

			//if attach fails, create a new TBApplication
			if (!Connected)
				base.StartTbLoader(launcher, unattendedMode, clearCachedData);
			else
			{
				SetMenuHandle(MenuHandle);
				tbAppProxy.SetUnattendedMode(unattendedMode);
			}
		}

		//-----------------------------------------------------------------------
		public static ITBApplicationProxy StartTBLoader
			(
			WCFBinding binding,
			int port,
			String tbApplicationPath,
			String launcher,
			String language,
			IntPtr menuHandle,
			bool unattendedMode,
			bool clearCachedData,
			bool useNetTcpProtocol,
            String tbLoaderParams
			)
		{
			String arguments = BuildArguments(binding, port, launcher, language, menuHandle, unattendedMode, clearCachedData, false, tbLoaderParams);
			return CreateTBApplicationProxy(tbApplicationPath, arguments);
		}

		//-----------------------------------------------------------------------
		public override bool InitTbLogin()
		{
            if (SingleThreaded)
                loginSemaphore.WaitOne();//un login alla volta se sono silgne threaded
			
            if (tbAppProxy.Login(AuthenticationToken))
			{
  			    SetMenuHandle(MenuHandle);
				return true;
			}
            if (SingleThreaded)
                loginSemaphore.Release();
			
            return false;
		}

		//-----------------------------------------------------------------------
        public override void CloseLoginAndExternalProcess()
		{
			CloseLogin();
		}
		//-----------------------------------------------------------------------
        public override void CloseLogin()
        {
            if (!Logged)
                return;

            tbAppProxy.CloseLogin();
            try
            {
                if (SingleThreaded)
                    loginSemaphore.Release();
            }
            catch
            {
            }
        }
			

		//-----------------------------------------------------------------------
		public override bool CloseAllDocuments()
		{
			return tbAppProxy.CloseAllDocuments();
		}
		//-----------------------------------------------------------------------
		public void FireAction(int documentHandle, string action)
		{
			tbAppProxy.FireAction(documentHandle, action);
		}
		//-----------------------------------------------------------------------
		public override bool RunDocument(String command, String arguments, out int handle)
		{
			handle = tbAppProxy.RunDocument(command, arguments);
			return handle != 0;
		}

		//-----------------------------------------------------------------------
		public override bool RunFunction(String command, String arguments)
		{
			return tbAppProxy.RunFunction(command, arguments);
		}

		//-----------------------------------------------------------------------
		public override void RunFunctionInNewThread (string command, string arguments)
		{
			tbAppProxy.RunFunctionInNewThread(command, arguments);
		}
		//-----------------------------------------------------------------------
		public override bool RunReport(String command, String arguments, out int handle)
		{
			handle = tbAppProxy.RunReport(command, arguments);
			return handle != 0;
		}
		//-----------------------------------------------------------------------
		public override bool RunTextEditor(String command)
		{
			return tbAppProxy.RunTextEditor(command);
		}

		//-----------------------------------------------------------------------
		public override Diagnostic GetLoginContextDiagnostic(bool clear)
		{
			if (tbAppProxy == null)
				return new Diagnostic("TBApplicationClientInterface"); 
			return (Diagnostic)tbAppProxy.GetLoginContextDiagnostic(clear);
		}
		//-----------------------------------------------------------------------
		public override Diagnostic GetApplicationContextDiagnostic(bool clear)
		{
			if (tbAppProxy == null)
				return new Diagnostic("TBApplicationClientInterface"); 
			return (Diagnostic)tbAppProxy.GetApplicationContextDiagnostic(clear);
		}
		//-----------------------------------------------------------------------
		public override Diagnostic GetGlobalDiagnostic(bool clear)
		{
			if (tbAppProxy == null)
				return new Diagnostic("TBApplicationClientInterface"); 
			return (Diagnostic)tbAppProxy.GetGlobalDiagnostic(clear);
		}
		//-----------------------------------------------------------------------
		public override void SetApplicationDate(DateTime date)
		{
			tbAppProxy.SetApplicationDate(date);
		}
		
		//-----------------------------------------------------------------------
		public void ChangeOperationsDate ()
		{
			tbAppProxy.ChangeOperationsDate();
		}

        //-----------------------------------------------------------------------
        public override bool OpenEnumsViewer(string culture, string installation)
        {
            return tbAppProxy.OpenEnumsViewer(culture, installation);
        }

        //-----------------------------------------------------------------------
		public override void ShowAboutFramework()
		{
			tbAppProxy.ShowAboutFramework();
		}

		//-----------------------------------------------------------------------
		public void ShowLockStructure()
		{
			tbAppProxy.ShowLockStructure();
		}

		//-----------------------------------------------------------------------
		public override void WaitForDisconnection()
		{
			tbAppProxy.WaitForExit();
		}

		//-----------------------------------------------------------------------
		public override void AttachTBLoader()
		{
			AttachTBLoader(-1);
		}

		//-----------------------------------------------------------------------
		public override bool CanChangeLogin(bool lockTbLoader)
		{
			return tbAppProxy.CanChangeLogin(lockTbLoader);
		}

		//-----------------------------------------------------------------------
		public int GetOpenDocuments()
		{
			return tbAppProxy.GetOpenDocuments();
		}

		//-----------------------------------------------------------------------
		public int GetOpenDocumentsInDesignMode()
		{
			return tbAppProxy.GetOpenDocumentsInDesignMode();
		}

		//-----------------------------------------------------------------------
		public override bool CanCloseLogin()
		{
			return tbAppProxy.CanCloseLogin();
		}

		//-----------------------------------------------------------------------
		public override bool CanCloseTB()
		{
			return tbAppProxy.CanCloseTB();
		}

        //-----------------------------------------------------------------------
        public bool OnBeforeCanCloseTB()
        {
            return tbAppProxy.OnBeforeCanCloseTB();
        }

        //-----------------------------------------------------------------------
        public bool SilentCloseLoginDocuments()
        {
            return tbAppProxy.SilentCloseLoginDocuments();
        }

		//-----------------------------------------------------------------------
		protected override int InternalChangeLogin(String oldAuthenticationToken, String newAuthenticationToken, bool unlock)
		{
			return tbAppProxy.ChangeLogin(oldAuthenticationToken, newAuthenticationToken, unlock);
		}

		//-----------------------------------------------------------------------
		public override void ClearCache()
		{
			tbAppProxy.ClearCache();
		}
		//-----------------------------------------------------------------------
		public override DateTime GetApplicationDate()
		{
			return tbAppProxy.GetApplicationDate();
		}
		//-----------------------------------------------------------------------
		public void AttachTBLoader(int tbSoapPort)
		{
			tbAppProxy = AttachProxyToTbApplication(TbApplicationPath);

			if (tbAppProxy != null)
			{
				tbPort = (tbSoapPort > 0) ? tbSoapPort : tbAppProxy.SoapPort;
				InitSoapInterfaceURL();
			}
		}

		

		//-----------------------------------------------------------------------
		public override void CloseTB()
		{
			if (tbAppProxy != null)
				tbAppProxy.Destroy();
		}

		//-----------------------------------------------------------------------
		protected override void ResetObject()
		{
			tbAppProxy = null;
		}

		//-----------------------------------------------------------------------
		public override void SetMenuHandle(IntPtr menuHandle)
		{
			tbAppProxy.SetMenuWindowHandle(menuHandle);
		}

		//-----------------------------------------------------------------------
		public void SetDocked(bool isDocked)
		{
			tbAppProxy.SetDocked(isDocked);
		}
		
		//-----------------------------------------------------------------------
		public MenuStrip CreateMenuStrip(IntPtr Handle, IntPtr MenuHandle)
		{
			return tbAppProxy.CreateMenuStrip(Handle, MenuHandle);
		}

		//-----------------------------------------------------------------------
		class RunBatchInUnattendedModeAsyncResult : TBAsyncResult
		{ 
			public bool ret = false;
			public int documentHandle = 0;
			public string[] messages = null;

		}
		//-----------------------------------------------------------------------
		public override IAsyncResult BeginRunBatchInUnattendedMode(string documentNamespace, string xmlParams, AsyncCallback callback, object asyncState)
		{
			RunBatchInUnattendedModeAsyncResult res = new RunBatchInUnattendedModeAsyncResult();
			res.Start(delegate 
			{
				res.ret = RunBatchInUnattendedMode(documentNamespace, xmlParams, ref res.documentHandle, ref res.messages);
				res.Finished(callback);
			},
			asyncState);
			return res;
		}

		//-----------------------------------------------------------------------
		public override bool EndRunBatchInUnattendedMode(IAsyncResult asyncResult, out int documentHandle, out string[] messages)
		{
			RunBatchInUnattendedModeAsyncResult result = (RunBatchInUnattendedModeAsyncResult)asyncResult;
			documentHandle = result.documentHandle;
			messages = result.messages;
			return result.ret;
		}

		
		
		//-----------------------------------------------------------------------
		public override bool RunBatchInUnattendedMode(string documentNamespace, string xmlParams, ref int documentHandle, ref string[] messages)
		{
			return tbAppProxy.RunBatchInUnattendedMode(documentNamespace, xmlParams, ref documentHandle, ref messages);
		}


		//-----------------------------------------------------------------------
		class RunReportInUnattendedModeAsyncResult : TBAsyncResult
		{
			public bool ret;
			public int reportHandle;
			public string[] messages;
		}
		//-----------------------------------------------------------------------
		public override IAsyncResult BeginRunReportInUnattendedMode(WoormInfo woormInfo, string xmlParams, AsyncCallback callback, object asyncState)
		{
			RunReportInUnattendedModeAsyncResult res = new RunReportInUnattendedModeAsyncResult();
			res.Start(delegate
			{
				res.ret = RunReportInUnattendedMode(woormInfo, xmlParams, ref res.reportHandle, ref res.messages);
				res.Finished(callback);
			},
			asyncState);
			return res;
		}
		//-----------------------------------------------------------------------
		public override bool EndRunReportInUnattendedMode(IAsyncResult asyncResult, out int reportHandle, out string[] messages)
		{
			RunReportInUnattendedModeAsyncResult result = (RunReportInUnattendedModeAsyncResult)asyncResult;
			reportHandle = result.reportHandle;
			messages = result.messages;
			return result.ret;
		}
		//-----------------------------------------------------------------------
		public override bool RunReportInUnattendedMode(WoormInfo woormInfo, string xmlParams, ref int reportHandle, ref string[] messages)
		{
			return tbAppProxy.RunReportInUnattendedMode((int)woormInfo, xmlParams, ref reportHandle, ref messages);
		}

		//-----------------------------------------------------------------------
		public void InitLockManager()
		{
			tbAppProxy.InitLockManager();
		}

		//-----------------------------------------------------------------------
		public override object Call(IFunctionPrototype functionPrototype, object[] parameters)
		{
			NameSpace ns = new NameSpace(functionPrototype.FullName, NameSpaceObjectType.Function);
			return WCFSoapClient.CallInProcess(ns.Module, functionPrototype.FullName, functionPrototype.Name, this.AuthenticationToken, 0, parameters);
		}
	}
}