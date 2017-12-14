using System;
using System.IO;
using System.Text;
using System.Xml;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Woorm.WoormController;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;
using System.Web;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
    /// <summary>
    /// Summary description for PdfReportEngine.
    /// </summary>
    public class PdfReportEngine
	{
		private string ReportNamespace;
		private string AuthenticationToken;
		private DateTime applicationDate;
		private string impersonatedUser;
		private HttpContext httpContext;
		private bool useApproximation;

		internal RSEngine StateMachine = null;
		public TbReportSession ReportSession;
		public XmlDocument XmlDomParameters = new XmlDocument();

		//--------------------------------------------------------------------------
		public PdfReportEngine
			(
				string authenticationToken,
				string parameters,
				DateTime applicationDate,
				string impersonatedUser,
				bool useApproximation,
                HttpContext httpContext
			)
		{
			XmlDomParameters.LoadXml(parameters);

			AuthenticationToken		= authenticationToken;
			ReportNamespace			= XmlDomParameters.DocumentElement.GetAttribute(XmlWriterTokens.Attribute.TbNamespace);
			this.applicationDate	= applicationDate;
			this.impersonatedUser	= impersonatedUser;
			this.useApproximation	= useApproximation;
			this.httpContext		= httpContext;
		}
		
		///<summary>
		///Resituisce il report in formato pdf, sotto forma di stream binario
		///</summary>
		//--------------------------------------------------------------------------
		public byte[] ExecuteReport(ref string diagnostic)
		{
			if (ReportNamespace == null || ReportNamespace.Length == 0)
			{
				diagnostic = WoormWebControlStrings.EmptyReportNamespace;
				return new byte[0]; //array vuoto
			}
			UserInfo ui = new UserInfo();
			if (!(ui.Login(AuthenticationToken)))
			{
				diagnostic = string.Format("{0}:{1}", WoormWebControlStrings.AuthenticationError, ui.ErrorExplain);
				return new byte[0]; //array vuoto
			}
			ui.SetCulture();
			ui.ApplicationDate	= applicationDate;
			ui.UseApproximation = useApproximation;
			ui.ImpersonatedUser	= impersonatedUser;

			// istanzio la mia sessione di lavoro 
			ReportSession = new TbReportSession(ui);
			bool sessionOk = ReportSession.LoadSessionInfo();

			// servono per le funzioni interne implementate da Expression
			NameSpace nameSpace = new NameSpace(ReportNamespace,NameSpaceObjectType.Report);
			ReportSession.ReportNamespace = ReportNamespace;
			ReportSession.ReportPath = ReportSession.UserInfo.PathFinder.GetCustomUserReportFile(ui.Company,impersonatedUser,nameSpace,true);

			// istanzio una nuova macchina per la elaborazione del report per generare solo XML
			//uso il sessionId della sessione e genero un GUID come uniqueID, sono usati per determinare il percorso
			//dove salvare  su file system i file delle pagina del report e di symbol table
			StateMachine = new RSEngine(ReportSession, XmlDomParameters, httpContext.Session.SessionID, Guid.NewGuid().ToString());

			// se ci sono stati errore nel caricamento fermo tutto (solo dopo aver istanziato la RSEngine)
			if (!sessionOk)
				StateMachine.CurrentState = State.LoadSessionError;

			// devo essere autenticato
			if (ui == null)
				StateMachine.CurrentState = State.AuthenticationError;

			// deve essere indicata anche la connection su cui si estraggono i dati
			if (ui != null && (ui.CompanyDbConnection == null || ui.CompanyDbConnection.Length == 0))
				StateMachine.CurrentState = State.ConnectionError;

			// faccio partire la macchina a stati che si ferma o su completamento dell'estrazione
			// o su errore. A differenza del caso Web non rientra mai su se stessa perchè non ci sono postback.
			StateMachine.Step();

			if (StateMachine.HtmlPage == HtmlPageType.Error)
			{
				diagnostic = string.Format("{0}, {1}", GetErrorMessage(StateMachine.Report.Diagnostic), GetErrorMessage(StateMachine.Woorm.Diagnostic));
				return new byte[0]; //array vuoto
			}

			//genero il pdf
			WoormDocument woorm = StateMachine.Woorm;
			PdfRender viewer = new PdfRender(woorm);
			//salvo la pagina corrente
			int current = woorm.RdeReader.CurrentPage;
			//ciclo sulle pagine per generare un pdf
			woorm.RdeReader.LoadTotPage();
			for (int i = 1; i <= woorm.RdeReader.TotalPages; i++)
			{
				woorm.LoadPage(i);
				viewer.ReportPage();
			}

			//reimposto la pagina iniziale
			woorm.RdeReader.CurrentPage = current;

			using (MemoryStream stream = new MemoryStream())
			{
				viewer.SaveToStreamAndClose(stream,true);
				// rilascio la macchina per risparmiare memoria
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
