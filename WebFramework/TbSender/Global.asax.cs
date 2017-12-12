using System;
using Microarea.TaskBuilderNet.TbSenderBL;
using System.Collections.Generic;
using System.Web;
using Microarea.TaskBuilderNet.TbSenderBL.PostaLite;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;
using System.Globalization;
using Microarea.TaskBuilderNet.TbSenderBL.PostaLiteTracker;

namespace Microarea.WebServices.TbSender
{
	public class Global : System.Web.HttpApplication
	{
		// l'unica variabile static, spero 
		// (è l'unico modo che mi viene in mente per accedere ad application da metodi tick privi di httpcontext)
		static internal ApplicationHolder appHolder;
		static private IDiagnostic diagnostic = new Diagnostic("TbSender");  	//Gestione errori

		internal static IDiagnostic Diagnostic { get { return diagnostic; } }

		public Global()
		{

		}

		public override void Init()
		{
			base.Init();
		}

		protected void Application_Start(object sender, EventArgs e)
		{
			// schedulatore dei poveri, sostituisci con qualcosa di più solido

			// legge il tempo in minuti da parametro web.config
			int tickMinutesMinimum = 1;
			int tickMinutesMaximum = 60;
			int tickMinutesDefault = 5;
			int tickMinutes = tickMinutesDefault;
			string tickMinutesStr = System.Configuration.ConfigurationManager.AppSettings["TickMinutes"];
			if (false == string.IsNullOrWhiteSpace(tickMinutesStr))
			{
				if (false == Int32.TryParse(tickMinutesStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out tickMinutes))
					// necessario perché in caso di false valorizza a zero! (pensavo facesse come un try, invece... ma la doc lo dice)
					tickMinutes = tickMinutesDefault;
			}
			if (tickMinutes < tickMinutesMinimum)
				tickMinutes = tickMinutesMinimum;
			else if (tickMinutes > tickMinutesMaximum)
				tickMinutes = tickMinutesMaximum;

			TimeSpan freq = new TimeSpan(hours: 0, minutes: tickMinutes, seconds: 0);
			TimerManager timer = new TimerManager(freq);
			timer.TimerTickFired += delegate(object snd, EventArgs ev)
			{
				Tick();
				timer.Start();
			};
			Application["Timer"] = timer; // evito che esca di scope e disposed
			timer.Start();
			appHolder = new ApplicationHolder(this.Application);
		}

		protected void Session_Start(object sender, EventArgs e)
		{

		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{

		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{

		}

		protected void Application_Error(object sender, EventArgs e)
		{

		}

		protected void Session_End(object sender, EventArgs e)
		{

		}

		protected void Application_End(object sender, EventArgs e)
		{

		}

		//---------------------------------------------------------------------
		// estensioni
		//---------------------------------------------------------------------

		internal class SenderEngineLocker { }
		private class SubscriptionEngineLocker { }
		private class SettingsProviderLocker { }

		//---------------------------------------------------------------------
		internal static void Tick()
		{
			// creo engine pool se non esiste
			Dictionary<string, SenderEngine> enginePool = EnginePool; // invocare fuori da lock!
			// clono la collection dei pool
			List<SenderEngine> enginesList = new List<SenderEngine>();
			lock (typeof(SenderEngineLocker))
			{
				enginesList.AddRange(enginePool.Values);
			}
			// ora sono sicuro di non bloccare altre richieste di accodamento

			//Parallel.ForEach(enginesList, senderEngine => {senderEngine.Tick()});
			foreach (SenderEngine senderEngine in enginesList)
			{
				senderEngine.Tick(); // TODO esecuzione AsParallel
			}
		}

		//---------------------------------------------------------------------
		static internal Dictionary<string, SenderEngine> EnginePool
		{
			get
			{
				lock (typeof(SenderEngineLocker))
				{
					Dictionary<string, SenderEngine> enginePool = appHolder.Application["EnginePool"] as Dictionary<string, SenderEngine>;
					if (enginePool == null)
					{
						enginePool = new Dictionary<string, SenderEngine>(StringComparer.InvariantCultureIgnoreCase);
						List<string> companies = LoginManagerConnector.GetSubscribedCompaniesDescriptors();
						ICredentialsProvider credProv = Global.SubscriptionEngine;
						foreach (string company in companies)
						{
							//SenderEngine engine = 
							GetOrAddCompanyEngineUnlocked(company, enginePool, credProv);
						}
						appHolder.Application["EnginePool"] = enginePool;
					}
					return enginePool;
				}
			}
		}

		//-------------------------------------------------------------------------------
		static internal SenderEngine GetOrAddCompanyEngineUnlocked
			(
			string company,
			Dictionary<string, SenderEngine> enginePool,
			ICredentialsProvider credProv
			)
		{
			SenderEngine engine;
			if (false == enginePool.TryGetValue(company, out engine))
			{
				IPostaLiteService plSvc = new PostaLiteServiceWrapper(); // TODO use IoC, switch mock and actual service
				//IPostaLiteService plSvc = new PostaLiteServiceOld(); // TODO use IoC, switch mock and actual service
				IPostaLiteSettingsProvider settingsProvider = Global.SettingsProvider;
				IDiagnostic diagnostic = Global.diagnostic;
				IInvalidDocumentNotifier tracker = new Tracker();
				tracker.Url = "http://www.microarea.it/PostaLiteTrackerWS/Tracker.asmx";
#if DEBUG
				tracker.Url = "http://spp-hotfix/PostaLiteTrackerWS/Tracker.asmx";
#endif
				engine = new SenderEngine(plSvc, credProv, settingsProvider, diagnostic, company);
				engine.InvalidDocumentNotifier = tracker;
				enginePool[company] = engine;
			}
			return engine;
		}

		//-------------------------------------------------------------------------------
		static internal SubscriptionEngine SubscriptionEngine
		{
			get
			{
				lock (typeof(SubscriptionEngineLocker))
				{
					SubscriptionEngine subscrEngine = appHolder.Application["SubscriptionEngine"] as SubscriptionEngine;
					if (subscrEngine == null)
					{
						IPostaLiteService plSvc = new PostaLiteServiceWrapper(); // TODO use IoC, switch mock and actual service
						//IPostaLiteService plSvc = new PostaLiteServiceOld(); // TODO use IoC, switch mock and actual service
						IPostaLiteSettingsProvider settingsProvider = Global.SettingsProvider;
						IDiagnostic diagnostic = Global.diagnostic;
						IDateTimeProvider dateTimeProvider = new DateTimeProvider();
						IChargeTracker tracker = new Tracker();
						tracker.Url = "http://www.microarea.it/PostaLiteTrackerWS/Tracker.asmx";
#if DEBUG
						tracker.Url = "http://spp-hotfix/PostaLiteTrackerWS/Tracker.asmx";
#endif
						subscrEngine = new SubscriptionEngine(plSvc, settingsProvider, diagnostic, dateTimeProvider);
						subscrEngine.ChargeTracker = tracker;
						appHolder.Application["SubscriptionEngine"] = subscrEngine;
					}
					return subscrEngine;
				}
			}
		}

		//-------------------------------------------------------------------------------
		static public IPostaLiteSettingsProvider SettingsProvider
		{
			get
			{
				lock (typeof(SettingsProviderLocker))
				{
					IPostaLiteSettingsProvider settingsProvider = appHolder.Application["SettingsProvider"] as IPostaLiteSettingsProvider;
					if (settingsProvider == null)
					{
						settingsProvider = new PostaLiteSettingsProvider();
						appHolder.Application["SettingsProvider"] = settingsProvider;
					}
					return settingsProvider;
				}
			}
		}
	}
}
