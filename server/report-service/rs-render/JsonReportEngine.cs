using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using Microarea.Common.Applications;
using Microarea.Common.Generic;

using Microarea.RSWeb.WoormViewer;
using Microarea.RSWeb.Models;
using Microarea.RSWeb.WoormEngine;
using Microarea.Common.Hotlink;

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
            StateMachine = new RSEngine(ReportSession);

            StateMachine.Step();

            // se ci sono stati errori li trasmetto nel file XML stesso
            if (StateMachine.HtmlPage == HtmlPageType.Error)
                StateMachine.XmlGetErrors();
        }

        //---------------------------------------------------------------------
        public string GetJsonInitTemplate()
        {
            WoormDocument woorm = StateMachine.Woorm;
            return woorm.ToJson(true, "page", true, StateMachine.ReportTitle);
        }

        public string GetJsonTemplatePage(ref int page)
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

            //TODO RSWEB bloccare prima
            if (page > woorm.RdeReader.TotalPages)
            {
                page = woorm.RdeReader.TotalPages;
            }

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

            //TODO RSWEB bloccare prima
            if (page > woorm.RdeReader.TotalPages)
            {
                page = woorm.RdeReader.TotalPages;
            }

            woorm.LoadPage(page);

            return woorm.ToJson(false);
        }

        //---------------------------------------------------------------------
        //chiamata sul change delle askentry di tipo "runatserver"
        public string UpdateJsonAskDialog(List<AskDialogElement> values, string currentDialogName)
        {
            AskDialog askDialog = StateMachine.Report.Engine.FindAskDialog(currentDialogName);
            if (askDialog == null)
                return string.Empty;
            if (StateMachine.Report.CurrentAskDialog != askDialog)
                return string.Empty;

            askDialog.AssignAllAskData(values);

            return askDialog.ToJson();
        }

        //---------------------------------------------------------------------
        //chiamata sul NEXT delle askdialog
        public string GetJsonAskDialog(List<AskDialogElement> values, string currentClientDialogName)
        {
            if (currentClientDialogName.IsNullOrEmpty() || values == null || values.Count == 0)
            {
                //TODO RSWEB in caso di RERUN occorre cambiare la cartella dei file temporanei
                //if (StateMachine.CurrentState == State.End)
                //StateMachine.CurrentState = State.ExecuteAsk;

                //viene cercata la prima, se esiste
                StateMachine.Step();
                return string.Empty;
            }

            if (StateMachine.Report.CurrentAskDialog == null || !currentClientDialogName.CompareNoCase(StateMachine.Report.CurrentAskDialog.FormName))
            {
                //il client ha fatto prev
                AskDialog dlg = StateMachine.Report.Engine.GetAskDialog(currentClientDialogName);
                if (dlg != null)
                    StateMachine.Report.CurrentAskDialog = dlg;
            }

            if (StateMachine.Report.CurrentAskDialog == null)
                return string.Empty;

            StateMachine.Report.CurrentAskDialog.AssignAllAskData(values);

            //passa alla prossima dialog se esiste oppure inizia estrazione dati
            StateMachine.Step();

            return string.Empty;
        }

        //---------------------------------------------------------------------
        private string PreviousAskDialog(string currentClientDialogName)
        {
            AskDialog askDialog = StateMachine.Report.Engine.GetAskDialog(currentClientDialogName);
            if (askDialog != null)
                StateMachine.Report.CurrentAskDialog = askDialog;

            return askDialog.ToJson();
        }

        //---------------------------------------------------------------------
        public Message GetResponseFor(Message msg)
        {
            switch (msg.commandType)
            {
                case MessageBuilder.CommandType.ASK:
                    {
                        //contiene il nome della dialog, se è vuota/=="0" viene richiesta la prima per la prima volta
                        msg.page = msg.page;
                        List<AskDialogElement> values = msg.page.IsNullOrEmpty() ? null
                                                        : JsonConvert.DeserializeObject<List<AskDialogElement>>(msg.message);

                        GetJsonAskDialog(values, msg.page);

                        msg.commandType = MessageBuilder.CommandType.NONE;
                        break;
                    }

                case MessageBuilder.CommandType.PREVASK:
                    {
                        msg.message = PreviousAskDialog(msg.page);
                        break;
                    }

                case MessageBuilder.CommandType.UPDATEASK:
                    {

                        List<AskDialogElement> values = msg.page.IsNullOrEmpty() ? null
                                                           : JsonConvert.DeserializeObject<List<AskDialogElement>>(msg.message);

                        msg.message = UpdateJsonAskDialog(values, msg.page);

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
                        {
                            msg.message = GetJsonTemplatePage(ref pageNum);
                            msg.page = pageNum.ToString();
                        }
                        break;
                    }

                case MessageBuilder.CommandType.DATA:
                    {
                        msg.page = pageNum.ToString();
                        msg.message = GetJsonDataPage(pageNum);
                        break;
                    }
                case MessageBuilder.CommandType.RERUN:
                    {
                        //reset state machine
                        StateMachine.StopReport();
                        StateMachine.ReportSession.uniqueID = Guid.NewGuid().ToString();
                        StateMachine.CurrentState = State.ExecuteAsk;

                        GetJsonAskDialog(null, "");

                        msg.commandType = MessageBuilder.CommandType.NONE;
                        break;
                    }

                //----------------------------------------------
                //TODO
                case MessageBuilder.CommandType.STOP:
                    {
                        // this.stateMachine.Do()
                        msg.message = "Executed STOP()";

                        StateMachine.StopReport();
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
