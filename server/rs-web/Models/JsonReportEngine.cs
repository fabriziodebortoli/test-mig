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
            rd.reportObjects = woorm.Objects;
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
    }
}
