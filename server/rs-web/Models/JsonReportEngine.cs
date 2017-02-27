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
        private string ReportNamespace;
        private string AuthenticationToken;
        private DateTime applicationDate;
        private string impersonatedUser;
        //private TBWebContext httpContext;
        private bool useApproximation;

        public RSEngine StateMachine = null;
        public TbReportSession ReportSession;

        public XmlDocument XmlDomParameters = new XmlDocument();

        public UserInfo ui = null;
         //--------------------------------------------------------------------------
        public JsonReportEngine
                            (
                               
                                string parameters,
                                DateTime applicationDate,
                                NamespaceMessage nsMsg,
                                LoginInfoMessage logMsg,
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
                ReportNamespace = nsMsg.nameSpace;

            AuthenticationToken = nsMsg.authtoken;         
            this.applicationDate = applicationDate;
            this.impersonatedUser = "";// msg.user;
            this.useApproximation = useApproximation;
            // this.httpContext = httpContext;

            ui = new UserInfo();

            /////////////////////////////////////////////////////
            // TODO temporary
            //in future get login information
            ui.Valid                    = true;
            ui.Company                  = logMsg.companyName;
            ui.Admin                    = logMsg.admin;
            ui.AuthenticationToken      = nsMsg.authtoken;
            ui.UseUnicode               = logMsg.useUnicode;
            ui.Provider                 = logMsg.providerName;
            ui.User                     = logMsg.userName;
            ui.CompanyDbConnection      = logMsg.connectionString;
           

            /////////////////////////////////////////////////////

            //if (!(ui.Login(AuthenticationToken)))
            //    return new StringCollection();

            //ui.SetCulture();
            //ui.ApplicationDate = applicationDate;
            //ui.UseApproximation = useApproximation;
           // ui.ImpersonatedUser = impersonatedUser;
            //ui.PathFinder = new PathFinder(ui.Company, ui.User); //temp
            CreateStateMachine();

        }

         //--------------------------------------------------------------------------
        private void  CreateStateMachine()
        {
           
            // istanzio la mia sessione di lavoro 
            ReportSession = new TbReportSession(ui);
           // bool sessionOk = ReportSession.LoadSessionInfo();

            // servono per le funzioni interne implementate da Expression
            NameSpace nameSpace = new NameSpace(ReportNamespace, NameSpaceObjectType.Report);
            ReportSession.ReportNamespace = ReportNamespace;
            //ReportSession.ReportPath = ReportSession.UserInfo.PathFinder.GetCustomUserReportFile(ui.Company, impersonatedUser, nameSpace, true);
 
            // istanzio una nuova macchina per la elaborazione del report per generare solo XML
            //TbSession reportSession, string filename, string sessionID, string uniqueID
            StateMachine = new RSEngine(ReportSession, ReportSession.ReportPath, "sessionID", "uniqueID");

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
