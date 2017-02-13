using System;
using System.Collections.Specialized;
using System.Xml;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;

using Microarea.RSWeb.WoormController;

namespace Microarea.RSWeb.Models
{
    public class JsonReportEngine
    {
        private string ReportNamespace;
        private string AuthenticationToken;
        private DateTime applicationDate;
        private string impersonatedUser;
        //private TBWebContext httpContext;
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
                               // TBWebContext httpContext,
                                bool useApproximation = true
                            )
        {
            if (!parameters.IsNullOrEmpty())
            {
                XmlDomParameters.LoadXml(parameters);
                ReportNamespace = XmlDomParameters.DocumentElement.GetAttribute(XmlWriterTokens.Attribute.TbNamespace);
            }
            else
                ReportNamespace = nameSpace;

            AuthenticationToken = authenticationToken;         
            this.applicationDate = applicationDate;
            this.impersonatedUser = impersonatedUser;
            this.useApproximation = useApproximation;
           // this.httpContext = httpContext;
        }

         //--------------------------------------------------------------------------
        private StringCollection ExecuteReport()
        {
            UserInfo ui = new UserInfo();

            /////////////////////////////////////////////////////
            // TODO temporary
            //in future get login information
            ui.Valid = true;
            ui.Company = "Company_ERP"; //to change if needed
            ui.CompanyId = 20;          //to change 
            ui.User = "sa";             //to change
            ui.LoginId = 1;             // to change
            ui.Password = "";           // to change
            ui.CompanyDbConnection = string.Format("Server = USR-SARMANTANA1;Database = {0};User Id = {1};Password = {2};", ui.Company, ui.User, ui.Password);

            ui.Provider = "SQL"; //?
            ui.Admin = true;    //to change

            /////////////////////////////////////////////////////

            //if (!(ui.Login(AuthenticationToken)))
            //    return new StringCollection();

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
            //TbSession reportSession, string filename, string sessionID, string uniqueID
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
