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
        public string GetJsonInitTemplate()
        {
            WoormDocument woorm = StateMachine.Woorm;
            return woorm.ToJson(true);
        }

        public string GetJsonTemplatePage(int page)
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
        //chiamata sul change delle askentry di tipo "runatserver"
        public string UpdateJsonAskDialog(List<AskDialogElement> data, string currentDialogName)
        {
            AskDialog askDialog = StateMachine.Report.Engine.FindAskDialog(currentDialogName);
            if (askDialog == null)
                return string.Empty;

            askDialog.AssignAllAskData(data);

            return askDialog.ToJson();
        }

        //---------------------------------------------------------------------
        //chiamata sul NEXT delle askdialog
        public string GetJsonAskDialog(List<AskDialogElement> data, string currentClientDialogName)
        {
            if (currentClientDialogName.IsNullOrEmpty() || data == null || data.Count == 0)
            {
                //viene cercata la prima, se esiste
                StateMachine.Step();
                return string.Empty;
            }

            if (!currentClientDialogName.CompareNoCase(StateMachine.Report.CurrentAskDialog.FormName))
            {
                //il client ha fatto prev
                AskDialog dlg = StateMachine.Report.Engine.GetAskDialog(currentClientDialogName);
                if (dlg != null)
                    StateMachine.Report.CurrentAskDialog = dlg;
            }

            StateMachine.Report.CurrentAskDialog.AssignAllAskData(data);
            
            //passa alla prossima dialog se esiste oppure inizia estrazione dati
            StateMachine.Step();

            return string.Empty;
        }

        //---------------------------------------------------------------------
        public Message GetResponseFor(Message msg)
        {
       
            switch(msg.commandType)
            {
               case MessageBuilder.CommandType.ASK:
                {         
                    //contiene il nome della dialog, se è vuota/=="0" viene richiesta la prima per la prima volta
                    msg.page = msg.page;   
                    List<AskDialogElement> data = msg.page.IsNullOrEmpty() ? null 
                                                    : JsonConvert.DeserializeObject<List<AskDialogElement>>(msg.message);

                    GetJsonAskDialog(data, msg.page);

                    msg.commandType = MessageBuilder.CommandType.NONE;

                    //if (nMsg.message.IsNullOrEmpty())
                    //{
                    //    //dialog "finite": inizia la visualizzazione della prima pagina
                    //    nMsg.commandType = MessageBuilder.CommandType.TEMPLATE;
                    //    nMsg.message = GetJsonTemplatePage(1);
                    //}

                    break;
                }
                case MessageBuilder.CommandType.UPDATEASK:
                    {
                      msg.page = msg.page;
                      List<AskDialogElement> data = msg.page.IsNullOrEmpty() ? null
                                                         : JsonConvert.DeserializeObject<List<AskDialogElement>>(msg.message);

                      msg.message = UpdateJsonAskDialog(data, msg.page);

                      break;
                    }
                /*
                 case MessageBuilder.CommandType.ABORTASK:  //click on CANCEL
                    {
                            Debug.Assert(StateMachine.CurrentState == State.RenderingForm);

                            StateMachine.CurrentState = State.ExecuteUserBreak;
                            StateMachine.Step();
                           
                        }
                 * */

                case MessageBuilder.CommandType.INITTEMPLATE:
                    {
                        msg.message = GetJsonInitTemplate();
                        break;
                    }
                case MessageBuilder.CommandType.TEMPLATE:
                    {
                        if (int.TryParse(msg.page, out pageNum))
                            msg.message = GetJsonTemplatePage(pageNum);
                        break;
                    }

                case MessageBuilder.CommandType.DATA:
                    {
                        msg.page = pageNum.ToString();
                        msg.message = GetJsonDataPage(pageNum);
                        break;
                    }

                //----------------------------------------------
                //TODO
                case MessageBuilder.CommandType.STOP:
                    {
                        // this.stateMachine.Do()
                        msg.message = "Executed STOP()";
                        break;
                    }
            }
            return msg;
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
