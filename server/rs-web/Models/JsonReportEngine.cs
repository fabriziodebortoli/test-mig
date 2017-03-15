using System;
using System.Collections.Specialized;
using System.Xml;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;

using Microarea.RSWeb.WoormController;
using Microarea.Common.NameSolver;
using Microarea.RSWeb.WoormViewer;
using System.Runtime.Serialization.Json;
using Microarea.RSWeb.Objects;
using System.IO;

namespace Microarea.RSWeb.Models
{
    public class JsonReportEngine
    {
        public TbReportSession ReportSession;

        public RSEngine StateMachine = null;

        //--------------------------------------------------------------------------
        public JsonReportEngine(TbReportSession session)
        {
            ReportSession = session;

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

        public string GetJsonPage(int page = 1)
        {
            WoormDocument woorm = StateMachine.Woorm;
            //salvo la pagina corrente
            int current = woorm.RdeReader.CurrentPage;
            //ciclo sulle pagine per generare un pdf
            woorm.RdeReader.LoadTotPage();
            woorm.LoadPage(page);

            ReportData rd = new ReportData();

            //rd.reportObjects = woorm.Objects;
                rd.reportObjects = new Layout();
                rd.reportObjects.Add(new SqrRect());
            //---------------------------------

            rd.paperLength = woorm.PageInfo.DmPaperLength;
            rd.paperWidth = woorm.PageInfo.DmPaperWidth;

            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer jsonSer = new DataContractJsonSerializer(rd.GetType());
            jsonSer.WriteObject(stream, rd);
            stream.Position = 0;
            // convert stream to string
            StreamReader reader = new StreamReader(stream);
            string text = reader.ReadToEnd();

            return text;
        }

        // qui mancano altri : rectangle, image, file etc.  manca anche la posizione.
        string template = @"
        {            
          ""text"": ""Languages"",
                     
          ""rect"": ""w_CompanyName"",

          ""text"":  ""09/03/2017 14:15"",

          ""grid"": {
                ""Languages"":      ""w_Language"",
                ""Description"":    ""w_Description""
            }
        } 
    ";

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
                        nMsg.message = template;
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
