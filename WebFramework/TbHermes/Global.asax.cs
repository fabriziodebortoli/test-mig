using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using System.Globalization;
using Microarea.TaskBuilderNet.TbHermesBL;

namespace Microarea.WebServices.TbHermes
{
	public class Global : System.Web.HttpApplication
	{
		// l'unica variabile static, spero 
		// (è l'unico modo che mi viene in mente per accedere ad application da metodi tick privi di httpcontext)
		static internal ApplicationHolder appHolder;
		static private IDiagnostic diagnostic = new Diagnostic("TbHermes");  	//Gestione errori
		internal static IDiagnostic Diagnostic { get { return diagnostic; } }

		private const string hermesEngineName = "HermesEngine";
		private class HermesEngineLocker { }

		//---------------------------------------------------------------------
		// permette di usare accedere all'engine come se fosse un singleton, 
		// ma è salvato nel contesto Application di ASP.NET invece che in una static
		static internal HermesEngine HermesEngine
		{
			get
			{
				lock (typeof(HermesEngineLocker))
				{
					HermesEngine hermesEngine = appHolder.Application[hermesEngineName] as HermesEngine;
					if (hermesEngine == null)
					{
						hermesEngine = new TaskBuilderNet.TbHermesBL.HermesEngine();
						hermesEngine.Diagnostic = diagnostic;
						appHolder.Application[hermesEngineName] = hermesEngine;
					}
					return hermesEngine;
				}
			}
		}

		//---------------------------------------------------------------------
		protected void Application_Start(object sender, EventArgs e)
		{
			// schedulatore dei poveri, sostituisci con qualcosa di più solido

			// legge il tempo in minuti da parametro web.config
			int tickMinutesMinimum = 1;
			int tickMinutesMaximum = 60;
			int tickMinutesDefault = 5;
			int tickMinutes = tickMinutesDefault;
            TaskBuilderNet.TbHermesBL.Config.HermesSettings myHS = TaskBuilderNet.TbHermesBL.Config.HermesSettings.Load();
            tickMinutes = myHS.TickRate;
			if (tickMinutes < tickMinutesMinimum)
				tickMinutes = tickMinutesMinimum;
			else if (tickMinutes > tickMinutesMaximum)
				tickMinutes = tickMinutesMaximum;

			TimeSpan freq = new TimeSpan(hours: 0, minutes: tickMinutes, seconds: 0);
			TimerManager timer = new TimerManager(freq);
			timer.TimerTickFired += delegate(object snd, EventArgs ev)
			{
				Global.HermesEngine.Tick();
				timer.Start();
			};
			Application["Timer"] = timer; // evito che esca di scope e sia disposed
			timer.Start();
			appHolder = new ApplicationHolder(this.Application);

			// l'istanza del timer preferisco non incapsularla nella classe di engine
			// per evitare di avere un thread timer tra i piedi in eventuali unit test
		}

		//---------------------------------------------------------------------
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
        /*  teoricamente corretto ma inefficace, questo o un altro timer continua a sparare anche a processo chiuso
            TimerManager timer =  (TimerManager)Application["Timer"];
            timer.Stop();
            timer.Dispose();
		*/
		}
	}
}