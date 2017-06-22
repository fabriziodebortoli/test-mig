using System;
using System.IO;
using System.Text;
using System.Xml;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;

using Microarea.RSWeb.WoormViewer;

namespace Microarea.RSWeb.Render
{
    /// <summary>
    /// Summary description for PdfReportEngine.
    /// </summary>
    public class PdfReportEngine: JsonReportEngine
    {

        //--------------------------------------------------------------------------
        public PdfReportEngine(TbReportSession session) : base (session)
		{
            session.EngineType = EngineType.PDFSharp_OfficePDF;
        }

        ///<summary>
        ///Resituisce il report in formato pdf, sotto forma di stream binario
        ///</summary>
        //--------------------------------------------------------------------------
        public byte[] ExecuteReport(ref string diagnostic)
		{
            // istanzio una nuova macchina per la elaborazione del report per generare solo XML
            //uso il sessionId della sessione e genero un GUID come uniqueID, sono usati per determinare il percorso
            //dove salvare  su file system i file delle pagina del report e di symbol table
            StateMachine = new RSEngine(ReportSession, ReportSession.XmlDomParameters);

            // se ci sono stati errore nel caricamento fermo tutto (solo dopo aver istanziato la RSEngine)
            //if (!sessionOk)
            //	StateMachine.CurrentState = State.LoadSessionError;

            //// devo essere autenticato
            //if (ui == null)
            //	StateMachine.CurrentState = State.AuthenticationError;

            //// deve essere indicata anche la connection su cui si estraggono i dati
            //if (ui != null && (ui.CompanyDbConnection == null || ui.CompanyDbConnection.Length == 0))
            //	StateMachine.CurrentState = State.ConnectionError;
            string name = ReportSession.ReportName;
            // faccio partire la macchina a stati che si ferma o su completamento dell'estrazione
            // o su errore. A differenza del caso Web non rientra mai su se stessa perchè non ci sono postback.
            StateMachine.Step();
            if (StateMachine.CurrentState == State.ExecuteAsk)
                StateMachine.Step();

            if (StateMachine.HtmlPage == HtmlPageType.Error)
			{
				diagnostic = string.Format("{0}, {1}", GetErrorMessage(StateMachine.Report.Diagnostic), GetErrorMessage(StateMachine.Woorm.Diagnostic));
				return new byte[0]; //array vuoto
			}

			//genero il pdf
			WoormDocument woorm = StateMachine.Woorm;

			PdfRender pdfRender = new PdfRender(woorm);     

			//salvo la pagina corrente
			int current = woorm.RdeReader.CurrentPage;

			woorm.RdeReader.LoadTotPage();

			//ciclo sulle pagine per generare un pdf
			for (int i = 1; i <= woorm.RdeReader.TotalPages; i++)
			{
				woorm.LoadPage(i);
                pdfRender.ReportPage();      //TODO rsweb
			}

			//reimposto la pagina iniziale
			woorm.RdeReader.CurrentPage = current;

			using (MemoryStream stream = new MemoryStream())
			{
                pdfRender.SaveToStreamAndClose(stream, true);

				// rilascio la macchina per risparmiare memoria           TODO rsweb
				StateMachine.Dispose();
				StateMachine = null;

				return stream.ToArray();
			}
		}

		///<summary>
		///Metodo che a partire dal diagnostic costruisce una stringa che contiene tutta la messagistica del
		///diagnostic
		/// </summary>
		//-----------------------------------------------------------------------------
		public static string GetErrorMessage(IDiagnostic diagnostic)
		{
			IDiagnosticItems items = diagnostic.AllMessages();
			StringBuilder errorMessage = new StringBuilder();
			if (items != null)
				foreach (IDiagnosticItem item in items)
				{
					if (!string.IsNullOrEmpty(item.FullExplain))
					{
						errorMessage.AppendLine(item.FullExplain);
						foreach (IExtendedInfoItem info in item.ExtendedInfo)
						{
							if (info.Info == null)
								continue;
							errorMessage.AppendLine(string.Format("{0}: {1}",info.Name,info.Info));
						}
					}
				}
			return errorMessage.ToString();
		}
	}
}
