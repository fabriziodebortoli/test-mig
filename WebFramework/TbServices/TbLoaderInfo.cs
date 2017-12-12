using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using System.IO;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.WebServices.TbServices
{
	//=========================================================================
	internal class TbLoaderInfo
	{
		internal static bool SingletonTbLoader = false;
		internal static int SingletonTbPort = 10000;
		internal static WCFBinding TbWCFBinding = WCFBinding.BasicHttp;
		internal static TimeSpan DafaultCallTimeout;
		internal static TimeSpan DataTransferCallTimeout;

		private TBServicesDiagnostic diagnostic;

		private List<TbLoginInfo> logins = new List<TbLoginInfo>();
		private int tbPort = 0;
		private TbLoaderClientInterface masterInterface = null;
		private WCFBinding binding;
		private RemoteClient host;

		//--------------------------------------------------------------------------------
		public RemoteClient Host { get { return host; } }

		//--------------------------------------------------------------------------------
        public bool Remote { get { return RemoteClient.IsRemote(host); } }
		
		//--------------------------------------------------------------------------------
		public string Server
		{
			get { return masterInterface == null ? null : masterInterface.TbServer; }
			set { if (masterInterface == null) throw new NullReferenceException("masterInterface not yet initialized!"); masterInterface.TbServer = value; }
		}
		//--------------------------------------------------------------------------------
		public int TbPort { get { return tbPort; } }
		//--------------------------------------------------------------------------------
		public WCFBinding Binding { get { return binding; } }
		//--------------------------------------------------------------------------------
		public TBServicesDiagnostic Diagnostic { get { return diagnostic; } }
		//--------------------------------------------------------------------------------
		public int LoginCount { get { return logins.Count; } }
		//--------------------------------------------------------------------------------
		public TbLoaderClientInterface MasterInterface { get { return masterInterface; } }

		//--------------------------------------------------------------------------------
		public bool Responding
		{
			get
			{
				if (masterInterface == null)
					return false;
				try
				{
					return masterInterface.GetProcessID() == ProcessId;
				}
				catch
				{
					return false;
				}
			}
		}

		//--------------------------------------------------------------------------------
		internal int ProcessId { get { return masterInterface.TbProcessId; } }

        //--------------------------------------------------------------------------------
		internal IntPtr ProcessHandle { get { return masterInterface.TbProcessHandle; } }

		//--------------------------------------------------------------------------------
		static TbLoaderInfo()
		{
			TBServicesConfiguration configInfo = TBServicesConfiguration.Current;
			SingletonTbLoader = configInfo.SingletonTBLoader;
			TbWCFBinding = (WCFBinding)Enum.Parse(typeof(WCFBinding), configInfo.TbWCFBinding);
			try
			{
				DafaultCallTimeout = TimeSpan.FromMinutes(InstallationData.ServerConnectionInfo.TbWCFDefaultTimeout);
			}
			catch
			{
				DafaultCallTimeout = TimeSpan.FromMinutes(1);
			}
			try
			{
				DataTransferCallTimeout = TimeSpan.FromMinutes(InstallationData.ServerConnectionInfo.TbWCFDataTransferTimeout);
			}
			catch
			{
				DataTransferCallTimeout = TimeSpan.FromMinutes(20);
			}
			SingletonTbPort = configInfo.SingletonTBPort;
		}

		//-----------------------------------------------------------------------
		public TbLoaderInfo(TBServicesDiagnostic diagnostic, RemoteClient host = null)
		{
			this.diagnostic = diagnostic;
			this.binding = TbWCFBinding;
			this.tbPort = SingletonTbPort;
			this.host = host;

			this.masterInterface = TbLoaderInfo.SingletonTbLoader
				? new SingletonTbLoaderClientInterface
					(
					new PathFinder(string.Empty, string.Empty),
					string.Empty,
					tbPort,
					string.Empty,
					binding
					)
				: (Remote ?

				new TbLoaderRemoteClientInterface
					(
					new PathFinder(string.Empty, string.Empty),
					string.Empty,
					tbPort,
					string.Empty,
					binding,
					Path.Combine(host.RemotePath, "TbLoader.exe"),
					host.User,
					host.Password, 
					host.RemoteServicePort
					) : new TbLoaderClientInterface
					(
					new PathFinder(string.Empty, string.Empty),
					string.Empty,
					tbPort,
					string.Empty,
					binding
					));
			this.masterInterface.SetTimeout(DafaultCallTimeout);

		}

		//---------------------------------------------------------------------------
		internal bool ReleaseLogin(string authenticationToken)
		{
			for (int i = logins.Count - 1; i >= 0; i--)
			{
				TbLoginInfo tbLoginInfo = logins[i];
				if (tbLoginInfo.AuthenticationToken == authenticationToken)
				{
					tbLoginInfo.TbApplicationClientInterface.CloseLogin();
					logins.RemoveAt(i);
					diagnostic.SetInfo("Closed login with user {0} and company {1}.", tbLoginInfo.User, tbLoginInfo.Company);
					return true;
				}
			}

			return false;
		}

		//-----------------------------------------------------------------------
		internal bool Clone(TbLoaderInfo tb, Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager loginManager)
		{
			int code = StartTB(tb.MasterInterface.AuthenticationToken, tb.TbPort);
			if (code < (int)EasyLookConnectionCodes.OK)
			{
				//non error, altrimenti mi invalida il client! se non ci riesco, ne creerò uno successivamente e semmai in quella sede darò errore
				diagnostic.SetInfo(tb.MasterInterface.AuthenticationToken, "Error restoring TBLoader; error code: {0}.", code);
				return false;
			}
			logins = tb.logins;
			diagnostic.SetInfo(tb.MasterInterface.AuthenticationToken, "Restored TBLoader process; process ID: {0}.", ProcessId);

			ValidateLogins(loginManager);
			return true;
		}

		//-----------------------------------------------------------------------
		internal void ValidateLogins(Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager loginManager)
		{
			for (int i = logins.Count - 1; i >= 0; i--)
			{
				TbLoginInfo tbLoginInfo = logins[i];
				if (!tbLoginInfo.Responding)
				{
					logins.RemoveAt(i);
					if (loginManager.IsValidToken(tbLoginInfo.AuthenticationToken))
					{
						int ret = InitTbLogin(tbLoginInfo.AuthenticationToken, tbLoginInfo.User, tbLoginInfo.Company, false, tbLoginInfo.ApplicationDate, true);
						if (ret < (int)EasyLookConnectionCodes.OK)
						{
							//non error, altrimenti mi invalida il client! se non ci riesco, ne creerò uno successivamente e semmai in quella sede darò errore
							diagnostic.SetInfo(tbLoginInfo.AuthenticationToken, "Error restoring login to TBLoader; process ID: {0}; error code: {1}.", ProcessId, ret);
						}
						else
						{
							diagnostic.SetInfo(tbLoginInfo.AuthenticationToken, "Restored TBLoader login; process ID: {0}.", ProcessId);
						}
					}
				}
			}
		}
		//-----------------------------------------------------------------------
		internal void RemoveInvalidTokens(Microarea.TaskBuilderNet.Core.WebServicesWrapper.LoginManager loginManager)
		{
			for (int i = logins.Count - 1; i >= 0; i--)
			{
				TbLoginInfo tbLoginInfo = logins[i];
				if (!tbLoginInfo.Responding)
				{
					logins.RemoveAt(i);
					continue;
				}
				if (!loginManager.IsValidToken(tbLoginInfo.AuthenticationToken))
				{
					try
					{
						tbLoginInfo.TbApplicationClientInterface.CloseLogin();
					}
					catch (Exception ex)
					{
						diagnostic.SetError("", ex.ToString());
					}
					logins.RemoveAt(i);
					diagnostic.Clear(tbLoginInfo.AuthenticationToken);
				}
			}
		}

		//-----------------------------------------------------------------------
		internal TbLoginInfo FindLogin(string authenticationToken)
		{
			for (int i = logins.Count - 1; i >= 0; i--)
			{
				TbLoginInfo tbLoginInfo = logins[i];
				if (tbLoginInfo.AuthenticationToken == authenticationToken)
					return tbLoginInfo;
			}
			return null;
		}

		//-----------------------------------------------------------------------
		internal int InitTbLogin(string authenticationToken, string user, string company, bool startTb, DateTime applicationDate, bool checkDate)
		{
			TbLoginInfo newLogin = new TbLoginInfo(user, company, authenticationToken, this);

			int code = newLogin.Login(authenticationToken, ref applicationDate);
			if (code != (int)EasyLookConnectionCodes.OK)
			{
				diagnostic.SetError(authenticationToken, "Error logging in to TBLoader; user: {0}; company: {1}; process ID: {2}; error code: {3}.", user, company, ProcessId, code);
				return code;
			}
			if (checkDate && !newLogin.ChangeApplicationDate(applicationDate))
			{
				diagnostic.SetError(authenticationToken, "Error logging in to TBLoader; user: {0}; company: {1}; process ID: {2}; error code: {3}.", user, company, ProcessId, EasyLookConnectionCodes.SetApplicationDateFailed);
				return (int)EasyLookConnectionCodes.SetApplicationDateFailed;
			}
			logins.Add(newLogin);

			diagnostic.SetInfo(authenticationToken, "Successfully logged in to TBLoader; user: {0}; company: {1}; process ID: {2}; port: {3}.", user, company, ProcessId, TbPort);

			return newLogin.TbApplicationClientInterface.TbPort;
		}

		//--------------------------------------------------------------------------------
		internal int StartTB(string authenticationToken, int suggestedPort)
		{
			int code = (int)EasyLookConnectionCodes.OK;
			try
			{
				masterInterface.SetSuggestedPort(suggestedPort, binding);
				masterInterface.StartTbLoader("TBServices", true);
				if (!masterInterface.Connected)
				{
					diagnostic.SetError(authenticationToken, Strings.TbLoaderStartError);
					TBServicesDiagnostic.CopyDiagnostic(authenticationToken, masterInterface.GetApplicationContextDiagnostic(true), diagnostic);

					masterInterface.KillProcess();
					code = (int)EasyLookConnectionCodes.StartTbLoaderFailed;
					return code;
				}
				tbPort = masterInterface.TbPort;
				if (host != null)
					host.count++;
				return code;
			}
			catch (TbLoaderClientInterfaceException err)
			{
				diagnostic.SetError(authenticationToken, Strings.TbLoaderStartError);
				diagnostic.SetError(authenticationToken, err);
				masterInterface.KillProcess();

				code = (int)EasyLookConnectionCodes.StartTbLoaderFailed;
				return code;
			}
			finally
			{
				if (code < (int)EasyLookConnectionCodes.OK)
					diagnostic.SetError(authenticationToken, "Error creating a new TbLoader; error code {0}.", code);
				else
					diagnostic.SetInfo(authenticationToken, "Created new TbLoader with process ID: {0}; listening port: {1}.", ProcessId, TbPort);
			}
		}

		///aggiunge le informazioni relative ai threads del processo 
		//-----------------------------------------------------------------------
		internal void AppendXmlInfo(XmlElement tbLoaderInstantiatedElement)
		{
			//se non ho effettuato login, non ho thread oltre a quello principale
			if (masterInterface == null)
				return;

			string fragment = masterInterface.GetActiveThreads();

			XmlDocumentFragment frag = tbLoaderInstantiatedElement.OwnerDocument.CreateDocumentFragment();
			frag.InnerXml = fragment;
			tbLoaderInstantiatedElement.AppendChild(frag);
		}

		//-----------------------------------------------------------------------
		internal void KillThread(int threadID, int processId, string authenticationToken)
		{
			if (masterInterface.GetProcessID() != processId)
				return;

			masterInterface.AuthenticationToken = authenticationToken;
			masterInterface.KillThread(threadID);
		}

		//-----------------------------------------------------------------------
		internal bool StopThread(int threadID, int processId, string authenticationToken)
		{
			if (masterInterface.GetProcessID() != processId)
				return false;

			masterInterface.AuthenticationToken = authenticationToken;
			return masterInterface.StopThread(threadID);
		}

		//-----------------------------------------------------------------------
		internal void KillProcess(int processId, string authenticationToken)
		{
			if (masterInterface.GetProcessID() != processId)
				return;

			masterInterface.AuthenticationToken = authenticationToken;
			CloseProcess(true, authenticationToken);
		}

		//-----------------------------------------------------------------------
		internal bool StopProcess(int processId, string authenticationToken)
		{
			if (masterInterface.GetProcessID() != processId)
				return false;

			masterInterface.AuthenticationToken = authenticationToken;
			return CloseProcess(false, authenticationToken);
		}

		//-----------------------------------------------------------------------
		internal bool CloseProcess(bool killIfNotResponding, string authenticationToken)
		{
			try
			{
				if (masterInterface == null)
					return true;
				try
				{
					masterInterface.AuthenticationToken = authenticationToken;

					//try to be polite...
					masterInterface.DestroyTB();
					if (masterInterface.WaitForExit(15000))
					{
						diagnostic.SetInfo(authenticationToken, "Closed TBLoader process");
						return true;
					}

					if (killIfNotResponding)
					{
						if (masterInterface.KillProcess())
							diagnostic.SetInfo(authenticationToken, "Killed not responding TBLoader process");

						return true;
					}
					return false;
				}
				catch
				{
					if (killIfNotResponding)
					{
						//...but when it there wants, it there wants.... (quanno ce vo` ce vo`...)
						if (masterInterface.KillProcess())
							diagnostic.SetInfo(authenticationToken, "Killed not responding TBLoader process");

						return true;
					}
					return false;
				}
			}
			catch (Exception ex)
			{
				diagnostic.SetError(authenticationToken, ex);
				return false;
			}
			finally
			{
				if (host != null)
					host.count--;
			}
		}
	}

	//=========================================================================
	public class TbLoginInfo
	{
		//---------------------------------------------------------------------------
		private string user = string.Empty;
		private string company = string.Empty;
		private string authenticationToken = string.Empty;
		private TBServicesDiagnostic diagnostic = null;
		private TbLoaderClientInterface tbApplicationClientInterface = null;
		private TbLoaderInfo tbInfo;
		public DateTime ApplicationDate = DateTime.Today;

		//---------------------------------------------------------------------------
		public string User { get { return user; } }
		//--------------------------------------------------------------------------------
		public string Company { get { return company; } }
		//--------------------------------------------------------------------------------
		public string AuthenticationToken { get { return authenticationToken; } }
		//--------------------------------------------------------------------------------
		public TbLoaderClientInterface TbApplicationClientInterface { get { return tbApplicationClientInterface; } }

		//---------------------------------------------------------------------------
		internal bool Responding
		{
			get
			{
				try
				{
					return tbApplicationClientInterface.IsLoginValid();
				}
				catch
				{
					return false;
				}
			}
		}
		//---------------------------------------------------------------------------
		internal TbLoginInfo(string user, string company, string authenticationToken, TbLoaderInfo tbInfo)
		{
			this.user = user;
			this.company = company;
			this.authenticationToken = authenticationToken;
			this.tbInfo = tbInfo;
			this.diagnostic = tbInfo.Diagnostic;
			this.tbApplicationClientInterface = TbLoaderInfo.SingletonTbLoader
				? new SingletonTbLoaderClientInterface
					(
					new PathFinder(company, user),
					string.Empty,
					tbInfo.TbPort,
					authenticationToken,
					tbInfo.Binding
					)
				: (tbInfo.Remote ?

				new TbLoaderRemoteClientInterface
					(
					new PathFinder(company, user),
					string.Empty,
					tbInfo.TbPort,
					authenticationToken,
					tbInfo.Binding,
					Path.Combine(tbInfo.Host.RemotePath, "TbLoader.exe"),
					tbInfo.Host.User,
					tbInfo.Host.Password,
					tbInfo.Host.RemoteServicePort
					) : new TbLoaderClientInterface
					(
					new PathFinder(company, user),
					string.Empty,
					tbInfo.TbPort,
					authenticationToken,
					tbInfo.Binding
					));
			this.tbApplicationClientInterface.SetTimeout(TbLoaderInfo.DafaultCallTimeout);
			this.tbApplicationClientInterface.TbServer = tbInfo.Server;
		}

		//---------------------------------------------------------------------------
		public void AppendXmlInfo(XmlElement tbLoaderInstantiatedElement)
		{
			if (tbLoaderInstantiatedElement == null)
				return;

			tbLoaderInstantiatedElement.SetAttribute("user", user);
			tbLoaderInstantiatedElement.SetAttribute("company", company);
			tbLoaderInstantiatedElement.SetAttribute("tbPort", tbApplicationClientInterface.TbPort.ToString());
			tbLoaderInstantiatedElement.SetAttribute("applicationdate", ApplicationDate.ToShortDateString());
			tbLoaderInstantiatedElement.SetAttribute("tbserver", Dns.GetHostName());
			tbLoaderInstantiatedElement.SetAttribute("token", authenticationToken);
		}

		//-----------------------------------------------------------------------
		internal bool ChangeApplicationDate(DateTime applicationDate)
		{
			try
			{
				if (ApplicationDate.ToShortDateString() != applicationDate.ToShortDateString())
				{
					TbApplicationClientInterface.CloseLatestXTechDocument();
					TbApplicationClientInterface.SetApplicationDate(applicationDate);
				}
			}
			catch (TbLoaderClientInterfaceException err)
			{
				diagnostic.SetError(authenticationToken, String.Format(Strings.SetApplicationDateError, err.Message));
				return false;
			}

			ApplicationDate = applicationDate;
			return true;
		}

		//--------------------------------------------------------------------------------
		internal int Login(string authenticationToken, ref DateTime applicationDate)
		{
			TbApplicationClientInterface.TBProcess = tbInfo.MasterInterface.TBProcess;
			try
			{
				if (!TbApplicationClientInterface.InitTbLogin())
				{
					diagnostic.SetError(authenticationToken, Strings.TbLoaderInitLoginError);
					TBServicesDiagnostic.CopyDiagnostic(authenticationToken, TbApplicationClientInterface.GetLoginContextDiagnostic(false), diagnostic);

					TbApplicationClientInterface.CloseLogin();
					return (int)EasyLookConnectionCodes.InitTbLoginFailed;
				}
			}
			catch (TbLoaderClientInterfaceException err)
			{
				diagnostic.SetError(authenticationToken, Strings.TbLoaderInitLoginError);
				diagnostic.SetError(authenticationToken, err);
				try
				{
					TbApplicationClientInterface.CloseLogin();
				}
				catch
				{
					//potrebbe andare in eccezione se non e' riuscito a fare la login
				}
				return (int)EasyLookConnectionCodes.InitTbLoginFailed;
			}
			return (int)EasyLookConnectionCodes.OK;
		}
	}
}
