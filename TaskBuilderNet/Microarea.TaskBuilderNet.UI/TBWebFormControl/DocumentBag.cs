using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Web.SessionState;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	class DocumentBag : IDisposable	
	{
		internal enum State { NONE, STARTED, CLOSED, ERROR, NOTALLOWED }
		private State threadState = State.NONE;

		private TBWebProxy actionService = null;
		private Diagnostic diagnostic = new Diagnostic(null);

		private int threadId = 0;
		private int proxyObjectId = 0;
		private HttpSessionState session;
		private WndObjDescriptionContainerRoot controlDescription;
		private static CultureInfo currentCulture;
		
		public int PingInterval = TBWebFormControl.MaxPingInterval;
		public TBWebFormControl.ActionCode ActionCode = TBWebFormControl.ActionCode.NONE;

		//lista degli script associati ai controlli
		public List<TBWebFormControl.RegisteredScript> scriptResources = new List<TBWebFormControl.RegisteredScript>();

		//--------------------------------------------------------------------------------------
		public State ThreadState { get { return threadState; } }
	
		//--------------------------------------------------------------------------------------
		internal IDiagnostic Diagnostic { get { return diagnostic; } }
		
		//--------------------------------------------------------------------------------------
		public TBWebProxy ActionService
		{
			get 
			{
				if (actionService != null)
				{
					if (actionService.Available)
						return actionService;
				}

				return actionService = CreateWebProxy();
			}
		}
		//--------------------------------------------------------------------------------------
		public WndObjDescriptionContainerRoot ControlDescription
		{
			get
			{
				if (controlDescription == null)
					controlDescription = new WndObjDescriptionContainerRoot();
				return controlDescription;
			}
			set { controlDescription = value; }
		}
		//--------------------------------------------------------------------------------------
		public int ThreadId
		{
			get { return threadId; }
		}
		//--------------------------------------------------------------------------------------
		public int ProxyObjectId
		{
			get { return proxyObjectId; }
		}
		//--------------------------------------------------------------------------------------
		public CultureInfo CurrentCulture
		{
			get { return currentCulture; }
		}
		//--------------------------------------------------------------------------------------
		public DocumentBag(HttpSessionState session)
		{
			this.session = session;
		}
	
		//--------------------------------------------------------------------------------------
		public void InitializeThread()
		{
			this.proxyObjectId = ActionService.WebProxyObjCreate(ref threadId);
			ActionService.SetUserInteractionMode(this.proxyObjectId, 1);
			threadState = State.STARTED;
		}

		//--------------------------------------------------------------------------------------
		public byte[] AttachToDocumentThread(int docThreadId)
		{
			threadId = docThreadId;
			threadState = State.STARTED;
			return ActionService.WebProxyObjCreateForThread(docThreadId, ref proxyObjectId);
		}
		//--------------------------------------------------------------------------------------
		public static TBWebProxy CreateWebProxy()
		{
			TbLoaderClientInterface i = GetTbInterface();
			TBWebProxy actionService = (i != null && i.Available)
				? i.CreateTBWebProxy() 
				: null;

			return actionService;
		}

		//--------------------------------------------------------------------------------------
		private static TbLoaderClientInterface GetTbInterface()
		{
			UserInfo ui = UserInfo.FromSession();
			if (ui != null)
			{
				currentCulture = new CultureInfo(ui.LoginManager.PreferredLanguage);
				return ui.GetTbLoaderInterface() as TbLoaderClientInterface;
			}
			return null;
		}
		//--------------------------------------------------------------------------------------
		public void Clear(State state)
		{
			try
			{
				threadState = state;

				if (session != null)
					session[proxyObjectId.ToString()] = null;
				if (ActionService != null)
					ActionService.Dispose();
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.ToString());
			}		
		}
		//--------------------------------------------------------------------------------------
		~DocumentBag()
		{
			Dispose();
		}

		#region IDisposable Members

		//--------------------------------------------------------------------------------------
		public void Dispose()
		{
			Clear(State.CLOSED);
			
		}

		#endregion


		///<summary> 
		/// Metodo per impostare da esterno lo stato del thread
		///</summary>

		//--------------------------------------------------------------------------------------
		internal void SetThreadAvailability(State state)
		{
			threadState = state;
		}
	}
}
