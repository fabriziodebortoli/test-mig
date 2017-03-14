using System;
using System.Collections.Specialized;
using System.Xml;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;

using Microarea.RSWeb.WoormViewer;
using Microarea.RSWeb.Objects;
using Microarea.RSWeb.Models;

namespace Microarea.RSWeb.Render
{
    public class JsonReportEngine
    {
        public TbReportSession ReportSession;

        public RSEngine StateMachine = null;

        //--------------------------------------------------------------------------
        public JsonReportEngine(TbReportSession session)
        {
            ReportSession = session;

         }

        public void Execute()
        {
            StateMachine = new RSEngine(ReportSession, ReportSession.ReportPath, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

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
            if (StateMachine.HtmlPage == HtmlPageType.Error)
                StateMachine.XmlGetErrors();

        }

        public string GetJsonTemplatePage(int page = 1)
        {
            WoormDocument woorm = StateMachine.Woorm;
            
            //TODO RSWEB OTTIMIZZAZIONE sostituire con file system watcher
            while (!woorm.RdeReader.IsPageReady(page))  //wait until xml page rde data exists
            {
                //if (woorm.RdeReader.LoadTotPage())  //report completed
                //    break;  //maybe page in a wrong page number
            };  

            woorm.LoadPage(page);

            return woorm.ToJson(true);
        }

        public string GetJsonDataPage(int page = 1)
        {
            WoormDocument woorm = StateMachine.Woorm;

            //salvo la pagina corrente int current = woorm.RdeReader.CurrentPage;

             //TODO RSWEB OTTIMIZZAZIONE sostituire con file system watcher
            while (!woorm.RdeReader.IsPageReady(page))
            {
                //if (woorm.RdeReader.LoadTotPage())
                //    break;
            };  //wait 

            woorm.LoadPage(page);

            return woorm.ToJson(false);
        }

        public Message GetResponseFor(Message msg)
        {
            Message nMsg = new Message();
            nMsg.commandType = msg.commandType;
           
            //nMsg.response = "This Is Response for " + msg.message;

            switch(msg.commandType)
            {
                case MessageBuilder.CommandType.ASK:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed ASK()";
                        break;
                    }
                case MessageBuilder.CommandType.DATA:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed DATA()";
                        break;
                    }
                case MessageBuilder.CommandType.ERROR:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed ERROR()";
                        break;
                    }
                case MessageBuilder.CommandType.GUID:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed GUID()";
                        break;
                    }
                case MessageBuilder.CommandType.NAMESPACE:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed NAMESPACE()";
                        break;
                    }
                case MessageBuilder.CommandType.NEXTPAGE:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed NEXTPAGE()";
                        break;
                    }
                case MessageBuilder.CommandType.PREVPAGE:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed PREVPAGE()";
                        break;
                    }
                case MessageBuilder.CommandType.OK:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed OK()";
                        break;
                    }
                case MessageBuilder.CommandType.PAGE:
                    {         
                        // this.stateMachine.Do()
                        nMsg.message = "Executed PAGE()";
                        break;
                    }
                case MessageBuilder.CommandType.PAUSE:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed PAUSE()";
                        break;
                    }
                case MessageBuilder.CommandType.PDF:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed PDF()";
                        break;
                    }
                case MessageBuilder.CommandType.RUN:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed ASK()";
                        break;
                    }
                case MessageBuilder.CommandType.STOP:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed STOP()";
                        break;
                    }
                case MessageBuilder.CommandType.TEMPLATE:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed TEMPLATE()";
                        break;
                    }
                case MessageBuilder.CommandType.TEST:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed TEST()";
                        break;
                    }


            }
            return nMsg;
        }
    }
}
