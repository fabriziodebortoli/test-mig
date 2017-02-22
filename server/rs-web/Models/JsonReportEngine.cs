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
        //private TBWebContext httpContext;
        public  TbReportSession ReportSession = null;

        public RSEngine StateMachine = null;
 
        public XmlDocument XmlDomParameters = new XmlDocument();

          //--------------------------------------------------------------------------
        public JsonReportEngine
<<<<<<< HEAD
                            (                              
                                string authenticationToken,
                                string parameters,
                                DateTime applicationDate,
                                 InitialMessage msg,
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
                ReportNamespace = msg.nameSpace;

            AuthenticationToken = authenticationToken;         
            this.applicationDate = applicationDate;
            this.impersonatedUser = msg.user;
            this.useApproximation = useApproximation;
            // this.httpContext = httpContext;

            ui = new UserInfo();

=======
                            (
                                string authenticationToken,
                                string nameSpace,
                                string parameters
                            )
        {
>>>>>>> 3ffc4a13c60c3df194d9e10aa9564407db7b776e
            /////////////////////////////////////////////////////
            // TODO temporary
            //in future get login information
            UserInfo ui = new UserInfo();
            ui.AuthenticationToken = authenticationToken;

            ui.Valid = true;
            ui.Company = msg.company; //to change if needed
            ui.CompanyId = 20;          //to change 
<<<<<<< HEAD
            ui.User = msg.user;             //to change
            ui.LoginId = 1;             // to change
            ui.Password = msg.password;           // to change
=======
            ui.User = "sa";             //to change
            ui.ImpersonatedUser = ui.User;
            ui.LoginId = 1;             // to change
            ui.Password = "";           // to change
            ui.Admin = true;    //to change
            ui.Provider = "SQL"; //?
>>>>>>> 3ffc4a13c60c3df194d9e10aa9564407db7b776e
            ui.CompanyDbConnection = string.Format("Server = USR-SARMANTANA1;Database = {0};User Id = {1};Password = {2};", ui.Company, ui.User, ui.Password);

            ui.SetCulture("it-IT", "it-IT");
            ////////////////////////////////////////////////
            ReportSession = new TbReportSession(ui);

            ReportSession.ApplicationDate = DateTime.Today; //TODO
            ReportSession.PathFinder = new PathFinder(ui.Company, ui.User);  

            if (!parameters.IsNullOrEmpty())
            {
                XmlDomParameters.LoadXml(parameters);
                ReportSession.ReportNamespace = XmlDomParameters.DocumentElement.GetAttribute(XmlWriterTokens.Attribute.TbNamespace);
            }
            else
                ReportSession.ReportNamespace = nameSpace;

            NameSpace ns = new NameSpace(ReportSession.ReportNamespace, NameSpaceObjectType.Report);
            ReportSession.ReportPath = ReportSession.PathFinder.GetCustomUserReportFile(ReportSession.UserInfo.Company, ReportSession.UserInfo.ImpersonatedUser, ns, true);

            CreateStateMachine();
        }

         //--------------------------------------------------------------------------
        private void  CreateStateMachine()
        {
             // servono per le funzioni interne implementate da Expression
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
