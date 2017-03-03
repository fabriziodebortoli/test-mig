using System;
using System.Collections.Specialized;
using System.Xml;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;

using Microarea.RSWeb.WoormController;
using Microarea.Common.NameSolver;

namespace Microarea.RSWeb.Models
{
    public class JsonReportEngine
    {
        public TbReportSession ReportSession;

        public RSEngine StateMachine = null;
 
        public XmlDocument XmlDomParameters = new XmlDocument();

         //--------------------------------------------------------------------------
        public JsonReportEngine
                            (
                               TbReportSession session,
                               string parameters = null
                            )
        {
            ReportSession = session;

            //TODO RSWEB report lanciato con parametri
            //if (!parameters.IsNullOrEmpty())
            //{
            //    XmlDomParameters.LoadXml(parameters);
            //    ReportNamespace = XmlDomParameters.DocumentElement.GetAttribute(XmlWriterTokens.Attribute.TbNamespace);
            //}

            StateMachine = new RSEngine(ReportSession, ReportSession.ReportPath, "sessionID", "uniqueID"); //, Guid.NewGuid().ToString());

            // se ci sono stati errore nel caricamento fermo tutto (solo dopo aver istanziato la RSEngine)
            //if (!sessionOk)
            //    StateMachine.CurrentState = State.LoadSessionError;

            // devo essere autenticato
            //if (ui == null)
            //    StateMachine.CurrentState = State.AuthenticationError;

            // deve essere indicata anche la connection su cui si estraggono i dati
            //if (ui != null && (ui.CompanyDbConnection == null || ui.CompanyDbConnection.Length == 0))
            //    StateMachine.CurrentState = State.ConnectionError;

            // faccio partire la macchina a stati che si ferma o su completamento dell'estrazione
            // o su errore. A differenza del caso Web non rientra mai su se stessa perchè non ci sono postback.
            StateMachine.Step();

            // se ci sono stati errori li trasmetto nel file XML stesso
            //if (StateMachine.HtmlPage == HtmlPageType.Error)
            //    StateMachine.XmlGetErrors();

            // rilascio la macchina per risparmiare memoria
            StateMachine.Dispose();
            StateMachine = null;

        }
    }
 }
