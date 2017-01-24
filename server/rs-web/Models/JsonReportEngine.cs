using System;
using System.Collections.Specialized;
using System.Xml;

using TaskBuilderNetCore.Interfaces;

using Microarea.RSWeb.Applications;
using Microarea.RSWeb.CoreTypes;
using Microarea.RSWeb.Generic;
using Microarea.RSWeb.WoormController;

namespace Microarea.RSWeb.Models
{
    public class JsonReportEngine
    {
        private string ReportNamespace;
        private string AuthenticationToken;
        private DateTime applicationDate;
        private string impersonatedUser;
        private TBWebContext httpContext;
        private bool useApproximation;

        public RSEngine StateMachine = null;
        public TbReportSession ReportSession;

        public XmlDocument XmlDomParameters = new XmlDocument();

         //--------------------------------------------------------------------------
        public JsonReportEngine
                            (
                                string nameSpace,
                                string authenticationToken,
                                string parameters,
                                DateTime applicationDate,
                                string impersonatedUser,
                                TBWebContext httpContext,
                                bool useApproximation = true
                            )
        {
            XmlDomParameters.LoadXml(parameters);

            AuthenticationToken = authenticationToken;
            ReportNamespace = XmlDomParameters.DocumentElement.GetAttribute(XmlWriterTokens.Attribute.TbNamespace);
            this.applicationDate = applicationDate;
            this.impersonatedUser = impersonatedUser;
            this.useApproximation = useApproximation;
            this.httpContext = httpContext;
        }

        // ITRI gestire meglio anche il ritorno di un diagnostic, in caso di errore (multiple righe)
        // o di una collezione di stringhe di errore.
        //--------------------------------------------------------------------------
        private StringCollection ExecuteReport(XmlReturnType xmlReturnType)
        {
            UserInfo ui = new UserInfo();

            if (!(ui.Login(AuthenticationToken)))
                return new StringCollection();

            ui.SetCulture();
            ui.ApplicationDate = applicationDate;
            ui.UseApproximation = useApproximation;
            ui.ImpersonatedUser = impersonatedUser;

            // istanzio la mia sessione di lavoro 
            ReportSession = new TbReportSession(ui);
            bool sessionOk = ReportSession.LoadSessionInfo();

            // servono per le funzioni interne implementate da Expression
            NameSpace nameSpace = new NameSpace(ReportNamespace, NameSpaceObjectType.Report);
            ReportSession.ReportNamespace = ReportNamespace;
            ReportSession.ReportPath = ReportSession.UserInfo.PathFinder.GetCustomUserReportFile(ui.Company, impersonatedUser, nameSpace, true);
 
            // istanzio una nuova macchina per la elaborazione del report per generare solo XML
            //TbReportSession reportSession, string filename, string sessionID, string uniqueID
            StateMachine = new RSEngine(ReportSession, ReportSession.ReportPath, "sessionID", "uniqueID");

            // se ci sono stati errore nel caricamento fermo tutto (solo dopo aver istanziato la RSEngine)
            if (!sessionOk)
                StateMachine.CurrentState = State.LoadSessionError;

            // devo essere autenticato
            if (ui == null)
                StateMachine.CurrentState = State.AuthenticationError;

            // deve essere indicata anche la connection su cui si estraggono i dati
            if (ui != null && (ui.CompanyDbConnection == null || ui.CompanyDbConnection.Length == 0))
                StateMachine.CurrentState = State.ConnectionError;

            // faccio partire la macchina a stati che si ferma o su completamento dell'estrazione
            // o su errore. A differenza del caso Web non rientra mai su se stessa perchè non ci sono postback.
            StateMachine.Step();

            // se ci sono stati errori li trasmetto nel file XML stesso
            if (StateMachine.HtmlPage == HtmlPageType.Error)
                StateMachine.XmlGetErrors();

            // rilascio la macchina per risparmiare memoria
            StateMachine.Dispose();
            StateMachine = null;

            return null;
        }
    }
 }
