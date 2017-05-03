using System;

using Microarea.Common.Applications;

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
        public string GetJsonAskDialog(string name)
        {
           AskDialog dlg = StateMachine.Report.Engine.FindAskDialog(name);
           if (dlg == null)
                return string.Empty;
            
            return dlg.ToJson();
        }

        public string GetJsonAskDialog(int index=0)
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

        //---------------------------------------------------------------------
        public Message GetResponseFor(Message msg)
        {
            Message nMsg = new Message();
            nMsg.commandType = msg.commandType;

            switch(msg.commandType)
            {
              
                case MessageBuilder.CommandType.OK:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed OK()";
                        break;
                    }             
                case MessageBuilder.CommandType.STOP:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed STOP()";
                        break;
                    }
               case MessageBuilder.CommandType.ASK:
                    {
                                            
                        nMsg.page = msg.page;
                        nMsg.message = GetJsonAskDialog(/*nMsg.page*/);
                        break;
                    }
                case MessageBuilder.CommandType.UPDATEASK:
                    {
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
                case MessageBuilder.CommandType.NAMESPACE:
                    {
                        // this.stateMachine.Do()
                        nMsg.message = "Executed NAMESPACE()";
                        break;
                    }
             }
            return nMsg;
        }
    }
}
