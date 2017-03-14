using System;
using System.Collections.Specialized;
using System.Xml;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;

namespace Microarea.RSWeb.Render
{
    /// <summary>
    /// Summary description for XmlReportEngine.
    /// </summary>
    public class XmlReportEngine : JsonReportEngine
    {
         public StringCollection XmlResultReports = new StringCollection();

		//--------------------------------------------------------------------------
		public XmlReportEngine (TbReportSession session)
            : base (session)
        {
            ReportSession.XmlReport = true;
        }

        // ITRI gestire meglio anche il ritorno di un diagnostic, in caso di errore (multiple righe)
        // o di una collezione di stringhe di errore.
        //--------------------------------------------------------------------------
        private StringCollection ExecuteReport(XmlReturnType xmlReturnType)
		{
            // istanzio una nuova macchina per la elaborazione del report per generare solo XML
            StateMachine = new RSEngine(ReportSession, ReportSession.XmlDomParameters, XmlResultReports, xmlReturnType);

			// se ci sono stati errore nel caricamento fermo tutto (solo dopo aver istanziato la RSEngine)
			//if (!sessionOk)
			//	StateMachine.CurrentState = State.LoadSessionError;

			//// devo essere autenticato
			//if (ui == null)
			//	StateMachine.CurrentState = State.AuthenticationError;

			//// deve essere indicata anche la connection su cui si estraggono i dati
			//if (ui != null && (ui.CompanyDbConnection == null || ui.CompanyDbConnection.Length == 0))
			//	StateMachine.CurrentState = State.ConnectionError;

			// faccio partire la macchina a stati che si ferma o su completamento dell'estrazione
			// o su errore. A differenza del caso Web non rientra mai su se stessa perchè non ci sono postback.
			StateMachine.Step();

			// se ci sono stati errori li trasmetto nel file XML stesso
			if (StateMachine.HtmlPage == HtmlPageType.Error)
				StateMachine.XmlGetErrors();

            // rilascio le risorse, il report e serializzato in XmlResultReports
            StateMachine.Dispose();
			StateMachine = null;

			return XmlResultReports;
		}

        //--------------------------------------------------------------------------
        public StringCollection XmlExecuteReport()
        {
            //if (ReportNamespace == null || ReportNamespace.Length == 0)
            //    return new StringCollection();

            return ExecuteReport(XmlReturnType.ReportData);
        }

        //--------------------------------------------------------------------------
        public String XmlGetParameters()
        {
            StringCollection doms = ExecuteReport(XmlReturnType.ReportParameters);
            if (doms == null || doms.Count <= 0) return string.Empty;

            return doms[0];
        }
    }
}
