using System;
using System.Collections.Specialized;
using System.Xml;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Woorm.WoormController;
using System.Web;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	/// <summary>
	/// Summary description for XmlReportEngine.
	/// </summary>
	public class XmlReportEngine
	{
		private string		ReportNamespace;
		private string		AuthenticationToken;
		private DateTime	applicationDate;
		private string		impersonatedUser;
		private HttpContext httpContext;
		private bool		useApproximation;
		
		private bool		EInvoice = false;
        private bool        writeNotValidField = false;  

		internal	RSEngine		    StateMachine = null;
		public		TbReportSession		ReportSession;
		public		StringCollection	XmlResultReports = new StringCollection();
		public		XmlDocument			XmlDomParameters = new XmlDocument();

        //--------------------------------------------------------------------------
        public bool WriteNotValidField
        {
            set { writeNotValidField = value; }
        }
		//--------------------------------------------------------------------------
		public XmlReportEngine
			(
				string		authenticationToken,
				string		parameters,
				DateTime	applicationDate,
				string		impersonatedUser,
				bool		useApproximation,
                HttpContext httpContext,
				bool		eInvoice
			)
		{
			XmlDomParameters.LoadXml(parameters);
            this.ReportNamespace			= XmlDomParameters.DocumentElement.GetAttribute(XmlWriterTokens.Attribute.TbNamespace);

            this.AuthenticationToken    = authenticationToken;
			this.applicationDate	= applicationDate;
			this.impersonatedUser	= impersonatedUser;
			this.useApproximation	= useApproximation;
			this.httpContext		= httpContext;
            this.EInvoice			= eInvoice;
		}
	
		//--------------------------------------------------------------------------
		public StringCollection XmlExecuteReport()
		{
			if (ReportNamespace == null || ReportNamespace.Length == 0) 
				return new StringCollection();

			return ExecuteReport(XmlReturnType.ReportData);
		}
	
		//--------------------------------------------------------------------------
		public String XmlGetParameters()
		{
			if (ReportNamespace == null || ReportNamespace.Length == 0) 
				return string.Empty;

			StringCollection doms = ExecuteReport(XmlReturnType.ReportParameters);
			if (doms == null || doms.Count <= 0) return string.Empty;

			return doms[0];
		}


		// ITRI gestire meglio anche il ritorno di un diagnostic, in caso di errore (multiple righe)
		// o di una collezione di stringhe di errore.
		//--------------------------------------------------------------------------
		private StringCollection ExecuteReport(XmlReturnType xmlReturnType)
		{
			UserInfo ui = new UserInfo();
			if (!(ui.Login(AuthenticationToken)))
				return new StringCollection();

			ui.SetCulture();
			ui.ApplicationDate	= applicationDate;
			ui.UseApproximation = useApproximation;
			ui.ImpersonatedUser	= impersonatedUser;

			// istanzio la mia sessione di lavoro 
			ReportSession = new TbReportSession(ui);
			ReportSession.EInvoice = EInvoice;
            ReportSession.WriteNotValidField = writeNotValidField;
			bool sessionOk = ReportSession.LoadSessionInfo();

			// servono per le funzioni interne implementate da Expression
			NameSpace nameSpace = new NameSpace(ReportNamespace, NameSpaceObjectType.Report);
			ReportSession.ReportNamespace = ReportNamespace;
			ReportSession.ReportPath = ReportSession.UserInfo.PathFinder.GetCustomUserReportFile(ui.Company, impersonatedUser, nameSpace, true);
            ReportSession.XmlReport = true;

			// istanzio una nuova macchina per la elaborazione del report per generare solo XML
			StateMachine = new RSEngine(ReportSession, XmlDomParameters, XmlResultReports, xmlReturnType);

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

			// se ci sono stati errori li trasmetto nel file XML stesso
			if (StateMachine.HtmlPage == HtmlPageType.Error)
				StateMachine.XmlGetErrors();

			// rilascio la macchina per risparmiare memoria
			StateMachine.Dispose();
			StateMachine = null;

			return XmlResultReports;
		}
	}
}
