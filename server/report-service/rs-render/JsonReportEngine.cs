﻿using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using Microarea.Common.Applications;
using Microarea.Common.Generic;

using Microarea.RSWeb.WoormViewer;
using Microarea.RSWeb.Models;
using Microarea.RSWeb.WoormEngine;

namespace Microarea.RSWeb.Render
{
    public class JsonReportEngine
    {
        public TbReportSession ReportSession;

        public RSEngine StateMachine = null;

        private int pageNum = 1;

        //--------------------------------------------------------------------------
        public JsonReportEngine(TbReportSession session)
        {
            ReportSession = session;

            session.EngineType = EngineType.FullExtraction;//TODO RSWEB .Paginated_Standard;
        }

        public void Execute()
        {
            StateMachine = new RSEngine(ReportSession, ReportSession.ReportPath, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            StateMachine.Step();

            // se ci sono stati errori li trasmetto nel file XML stesso
            if (StateMachine.HtmlPage == HtmlPageType.Error)
                StateMachine.XmlGetErrors();
        }

        //---------------------------------------------------------------------
        public string GetJsonTemplatePage(int page = 1)
        {
            WoormDocument woorm = StateMachine.Woorm;

            //TODO RSWEB OTTIMIZZAZIONE sostituire con file system watcher
            if (StateMachine.Report.EngineType != EngineType.FullExtraction)
                while (!woorm.RdeReader.IsPageReady(page))
                {
                    System.Threading.Tasks.Task.Delay(1000).Wait();

                    //if (woorm.RdeReader.LoadTotPage())
                    //    break;
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

                    //if (woorm.RdeReader.LoadTotPage())
                    //    break;
                };  //wait 

            woorm.LoadPage(page);

            return woorm.ToJson(false);
        }

        //---------------------------------------------------------------------
        public string UpdateJsonAskDialog(AskDialogElement data, string currentDialogName)
        {
            AskDialog dlg = StateMachine.Report.Engine.FindAskDialog(currentDialogName);
            if (dlg == null)
                return string.Empty;

            //TODO update engine

            return dlg.ToJson();
        }

        //---------------------------------------------------------------------
        public string GetJsonAskDialog(List<AskDialogElement> data, string currentDialogName)
        {
            AskDialog dlg = null;

            if (currentDialogName.IsNullOrEmpty() || currentDialogName == "0")
            {
                //TODO find first enabled dialog
                dlg = StateMachine.Report.Engine.GetAskDialog(0);
            }
            else
            {
                //TODO find NEXT enabled dialog !
                //dlg = StateMachine.Report.Engine.FindAskDialog(currentDialogName);
            }
           
            if (dlg == null)
            {
                return string.Empty;
            }
            return dlg.ToJson();
        }

        //---------------------------------------------------------------------
        public Message GetResponseFor(Message msg)
        {
            Message nMsg = new Message();
            nMsg.commandType = msg.commandType;

            switch(msg.commandType)
            {
               case MessageBuilder.CommandType.ASK:
                    {         
                        //contiene il nome della dialog, se è vuota/=="0" viene richiesta la prima per la prima volta
                        nMsg.page = msg.page;   
                        List<AskDialogElement> data = nMsg.page == "0" ? null 
                                                        : JsonConvert.DeserializeObject<List<AskDialogElement>>(msg.message);

                        nMsg.message = GetJsonAskDialog(data, nMsg.page);

                        if (nMsg.message.IsNullOrEmpty())
                        {
                            //dialog "finite": inizia la visualizzazione della prima pagina
                            nMsg.commandType = MessageBuilder.CommandType.TEMPLATE;
                            nMsg.message = GetJsonTemplatePage(1);
                        }

                        break;
                    }
                case MessageBuilder.CommandType.UPDATEASK:
                    {
                       nMsg.page = msg.page;
                       AskDialogElement data = JsonConvert.DeserializeObject<AskDialogElement>(msg.message);

                       nMsg.message = UpdateJsonAskDialog(data, nMsg.page);

                       

                       break;
                    }
          
                case MessageBuilder.CommandType.INITTEMPLATE:
                case MessageBuilder.CommandType.TEMPLATE:
                    {
                        if (int.TryParse(msg.page, out pageNum))
                            nMsg.message = GetJsonTemplatePage(pageNum);
                        break;
                    }

                case MessageBuilder.CommandType.DATA:
                    {
                        nMsg.page = pageNum.ToString();
                        nMsg.message = GetJsonDataPage(pageNum);
                        break;
                    }

                //----------------------------------------------
                //TODO
                case MessageBuilder.CommandType.STOP:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed STOP()";
                        break;
                    }

                case MessageBuilder.CommandType.NAMESPACE:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed NAMESPACE()";
                        break;
                    }
                case MessageBuilder.CommandType.OK:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed OK()";
                        break;
                    }

            }
            return nMsg;
        }

        //---------------------------------------------------------------------
        //per debug
        public string GetJsonAskDialog(int index = 0)
        {
            AskDialog dlg = StateMachine.Report.Engine.GetAskDialog(index);
            if (dlg == null)
                return string.Empty;

            return dlg.ToJson();
        }

        public string GetJsonAskDialogs()
        {
            return StateMachine.Report.Engine.ToJsonDialogs();
        }
    }
}
