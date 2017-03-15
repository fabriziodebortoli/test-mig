﻿using System;
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
using Microarea.RSWeb.WoormEngine;

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

            StateMachine.Report.EngineType = EngineType.FullExtraction; //TODO RSWEB problema sync con engine thread

            StateMachine.Step();

            // se ci sono stati errori li trasmetto nel file XML stesso
            if (StateMachine.HtmlPage == HtmlPageType.Error)
                StateMachine.XmlGetErrors();
        }

        public string GetJsonTemplatePage(int page = 1)
        {
            WoormDocument woorm = StateMachine.Woorm;

            //TODO RSWEB OTTIMIZZAZIONE sostituire con file system watcher
            if (StateMachine.Report.EngineType != EngineType.FullExtraction)
                while (!woorm.RdeReader.IsPageReady(page))
                {
                    System.Threading.Tasks.Task.Delay(1000).Wait();

                    if (woorm.RdeReader.LoadTotPage())
                        break;
                };  //wait 

            woorm.LoadPage(page);

            return woorm.ToJson(true);
        }

        public string GetJsonDataPage(int page = 1)
        {
            WoormDocument woorm = StateMachine.Woorm;

            //TODO RSWEB OTTIMIZZAZIONE sostituire con file system watcher
            if (StateMachine.Report.EngineType != EngineType.FullExtraction)
                while (!woorm.RdeReader.IsPageReady(page))
                {
                    System.Threading.Tasks.Task.Delay(1000).Wait();

                    if (woorm.RdeReader.LoadTotPage())
                            break;
                };  //wait 

            woorm.LoadPage(page);

            return woorm.ToJson(false);
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
