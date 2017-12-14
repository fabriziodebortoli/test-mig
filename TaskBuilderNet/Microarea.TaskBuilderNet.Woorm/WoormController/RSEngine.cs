using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Woorm.WoormEngine;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Woorm.WoormWebControl;

namespace Microarea.TaskBuilderNet.Woorm.WoormController
{
    // tipologia di renderizzazione
    public enum HtmlPageType { Error, Viewer, Form, HotLink, Persister, Print, None };

	// tipo di dati restituiti in formato XML
	public enum XmlReturnType { ReportData, ReportParameters };

	// possibili stati interni nel ciclo di estrazione dati della macchina a stati
	public enum InternalState
	{
		ExecuteBeforeActions,
		ExecuteRulesAndEvents,
		ExecuteAfterActions,
		ExecuteFinalizeActions,
		ExecuteLastStep,
		End
	}

	// possibili stati della macchina
	public enum State 
	{ 
		Start, 
		CheckNoWebAndRelease, 
		InitReport, 
		RunReport, 
		RunViewer, 
		RunPersister,
		EndPersister,
		InitalizeChannel,
		ExecuteInizialize,
		ExecuteAsk,
		ExecuteExtraction,
		ExecuteErrorStep,
		ExecuteUserBreak,
		RenderingForm,
		FileNotFound,
		GrantViolation,
		ViewerError,
		ReportError,
		ConnectionError,
		NoWeb,
		BadRelease,
		NoDataFound,
		AuthenticationError,
		LoadSessionError,
		Print,
		UserInterrupted,
		End
	};

    //=============================================================================        
    class StateMachineConstStrings
    {
        public const string WebFramework    = "WebFramework";
        public const string EasyLookFull    = "EasyLookFull";
        public const string EasyLook        = "EasyLook";
    }

	/// <summary>
	/// Descrizione di riepilogo per RSEngine.
	/// </summary>
	/// ================================================================================
	public class RSEngine : IDisposable
	{
		private bool			disposed = false;	// Track whether Dispose has been called.
		private TbReportSession	reportSession = null;
		private XmlReturnType	xmlReturnType = XmlReturnType.ReportData;
		private Thread			extractionThread = null;
		private State			currentState = State.Start;
		
		public StringCollection	Errors = new StringCollection();
		public StringCollection	Warnings = new StringCollection();
		public WoormDocument	Woorm = null;
		public Report			Report = null;
		public HtmlPageType		PreviousHtmlPage = HtmlPageType.None;
		public HtmlPageType		HtmlPage = HtmlPageType.None;
		public string			ReportTitle = string.Empty;
        
        //public System.Web.UI.StateBag StateBag = new System.Web.UI.StateBag();
        public System.Collections.Hashtable StateBag = new System.Collections.Hashtable();

		public AskDialog		ActiveAskDialog = null;

		public InternalState 	CurrentInternalState;
		public bool 			Working = false;

		//Siccome il currentState puo essere modificato dal thread di esecuzione e da quello del viewer, devo sincronizzarne
		//l'accesso
		//--------------------------------------------------------------------------
		public State CurrentState
		{
			get
			{
				lock (this)
				{
					return currentState;
				}
			}
			set 
			{
				lock (this)
				{
					currentState = value;
				}
			}
		}

		//--------------------------------------------------------------------------
		public Thread ExtractionThread { get { return extractionThread; } }
		//--------------------------------------------------------------------------
		public string	Filename			{ get { return Woorm.Filename; }}
		//--------------------------------------------------------------------------
		public TbReportSession	ReportSession	{ get { return reportSession; }}
		
		//--------------------------------------------------------------------------
		public RSEngine(TbReportSession reportSession, string filename, string sessionID, string uniqueID)
		{
			this.reportSession = reportSession;
			if (reportSession.IsAuthenticated)
			{
				// Se sono  un file XML allora sono uno snapshot e quindi serve solo il visualizzatore altrimenti
				// eseguo tutto normalmente ed estraggo dati e li visualizzo
				if (string.Compare(Path.GetExtension(filename), NameSolverStrings.XmlExtension, true, CultureInfo.InvariantCulture) == 0)
				{
					Woorm = new RDEWoormDocument(filename, reportSession, sessionID, uniqueID);
					Report = null;
				}
				else
				{
					Woorm =  new WoormDocument(filename, reportSession, sessionID, uniqueID);
					Report = new Report(filename, reportSession, sessionID, uniqueID, Woorm);
				}
			}
		}
	
		// da usarsi solo per il motore di esportazione in XML. Il viewer serve solo per 
		// parsare correttamente l'header del report e fare tutti i controlli di release
		// necessari (ad esempio : NOWEB)
		//--------------------------------------------------------------------------
		public RSEngine
			(
				TbReportSession		reportSession,
				XmlDocument			xmlDomParameters, 
				StringCollection	xmlResultReports, 
				XmlReturnType		xmlReturnType
			)
		{
			this.reportSession = reportSession;
			this.xmlReturnType = xmlReturnType;

			if (reportSession.IsAuthenticated)
			{
				this.reportSession.Localizer = 	new WoormLocalizer(reportSession.ReportPath, ReportSession.PathFinder);
				// Woorm serve per gestire il parse delle release
				Woorm =  new WoormDocument(reportSession.ReportPath, reportSession, "", "");
				Report = new Report(reportSession.ReportPath, reportSession, xmlDomParameters, xmlResultReports);
			}
		}

		// da usarsi solo per il motore di esportazione in PDF. 
		//--------------------------------------------------------------------------
		public RSEngine
			(
				TbReportSession reportSession,
				XmlDocument xmlDomParameters,
				string sessionID, 
				string uniqueID
			)
		{
			this.reportSession = reportSession;

			if (reportSession.IsAuthenticated)
			{
				this.reportSession.Localizer = 	new WoormLocalizer(reportSession.ReportPath,ReportSession.PathFinder);
				//serve sia visualizzatore (WoormDocument) che motore (Report), per generare il pdf.
				Woorm =  new WoormDocument(reportSession.ReportPath, reportSession, sessionID, uniqueID);
				Report = new Report(reportSession.ReportPath,reportSession,xmlDomParameters, sessionID, uniqueID);
			}
		}

		//------------------------------------------------------------------------------
		public void Unparse()
		{
			using (Unparser unparser = new Unparser())
			{
				//unparser.Open(reportSession.ReportPath);
				unparser.Open("c:\\a.wrm");
				Woorm.Unparse(unparser);
				Report.Unparse(unparser);
			}
		}

		//------------------------------------------------------------------------------
		~RSEngine()
		{
			Dispose(false);
		}

		//------------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//------------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if(!disposed)
			{
				// se arrivo dal distruttore non so l'ordine di distruzione
				if(disposing)
				{
					if (Report != null) Report.Dispose();
					if (Woorm != null) Woorm.Dispose();
					StateBag.Clear();
				}

			}
			disposed = true;         
		}

		//--------------------------------------------------------------------------
		public void XmlGetErrors()
		{
			if (Report != null)
				Report.Engine.OutChannel.XmlGetErrors(Errors, Warnings);
		}

		//--------------------------------------------------------------------------
		private void BuildErrors(IDiagnostic diagnostic)
		{
			Errors.Add(WoormWebControlStrings.SyntaxError);
			Errors.Add(Filename);

			if (diagnostic.Error) 
				foreach (IDiagnosticItem error in diagnostic.AllMessages(DiagnosticType.Error)) 
				{
					if (error.ExtendedInfo != null)
						Errors.Add(string.Format
							(
							WoormWebControlStrings.ErrorLocation, 
							error.ExtendedInfo["Row"].ToString(), 
							error.ExtendedInfo["Column"].ToString()
							));
					Errors.Add(error.FullExplain);
				}
			Errors.Add(string.Format(WoormWebControlStrings.CompileError, diagnostic.TotalErrors));
		}

		//--------------------------------------------------------------------------
		private bool Granted
		{
			get
			{
                 if (Woorm.Namespace == null) 
                    return true;

                bool applySecurityFilter = false;

                if (reportSession.UserInfo.LoginManager != null)
                {
                    if (reportSession.UserInfo.LoginManager.IsSecurityLightEnabled() && !reportSession.UserInfo.LoginManager.IsSecurityLightAccessAllowed(Woorm.Namespace.ToString(), false))
                        return false;

                    applySecurityFilter = reportSession.UserInfo.LoginManager.IsActivated("MicroareaConsole", "SecurityAdmin") && 
                        (
                        reportSession.UserInfo.LoginManager.LoginManagerState != LoginManagerState.Logged ||
                        reportSession.UserInfo.LoginManager.Security
                        );
                }
                //Controllo se ho i grant per vedere il report 
                ISecurity security = reportSession.UserInfo.LoginManager.NewSecurity
                                        (
                                            reportSession.UserInfo.Company, 
                                            reportSession.UserInfo.User,
                                            applySecurityFilter
                                        ); 

                bool existGrants = security.ExistExecuteGrant(Woorm.Namespace.ToString(), SecurityType.Report);
                security.Dispose();

                return existGrants;
            }
        }

		//--------------------------------------------------------------------------
		public void Step()
		{
			if (Working)
				return;

			while (true)
			{
				switch (CurrentState)
				{
					case State.Start :	
					{
						// controlla che il file sia presente
						if(!File.Exists (Filename))
						{
							CurrentState = State.FileNotFound;
							break;
						}

						if(!Granted)
						{
							CurrentState = State.GrantViolation;
							break;
						}

						if (reportSession.UserInfo == null || 
							!reportSession.UserInfo.LoginManager.IsValidToken(reportSession.UserInfo.LoginManager.AuthenticationToken) ||
							!reportSession.UserInfo.LoginManager.IsActivated(StateMachineConstStrings.WebFramework, StateMachineConstStrings.EasyLook))
						{
							CurrentState = State.AuthenticationError;		
							break;
						}
						
						CurrentState = State.CheckNoWebAndRelease;
						break;
					}

					case State.CheckNoWebAndRelease :	
					{
						// non devo controllare il file originale perchè esiste solo la
						// parte grafica embedded nel RDE e devo controllare la sua release
						// inoltre a poca importanza che sia NoWeb perchè i dati sono già stati
						// estratti e la incompatibilità web è solo per il WoormEngine
						if (!(Woorm is RDEWoormDocument))
						{
							ReleaseChecker checker = new ReleaseChecker(Woorm);
							if (!checker.CheckNoWebAndRelease())
							{
								BuildErrors(checker.Diagnostic);
								checker.Dispose();
								CurrentState = State.BadRelease;
								break;
							}


							// alcuni report non possono essere eseguiti in Modalità Web
							// pertanto li compilo normalmente ma inibisco l'esecuzione
							if (checker.NoWeb)
							{
								checker.Dispose();
								CurrentState = State.NoWeb;
								break;
							}

							checker.Dispose();
						}
						CurrentState = State.RunReport;
						break;
					}

					case State.RunReport :	
					{
						if (Report == null)
						{
							CurrentState = State.RunViewer;
							break;
						}

						if (!Report.Compile())
						{
							CurrentState = State.ReportError;
							break;
						}
						
						ReportTitle = Report.ReportTitle;
						CurrentState = State.InitReport;
						break;
					}

					case State.InitReport :	
					{

						Report.InitReport();

						CurrentState = State.InitalizeChannel;
						break;
					}

					case State.InitalizeChannel :	
					{
						Report.InitializeChannel();
						CurrentState = State.ExecuteInizialize;
						break;
					}

					case State.ExecuteInizialize :	
					{
                        Report.Engine.Status = ReportEngine.ReportStatus.Init;

						if (!Report.ExecuteInitialize())
						{
							CurrentState = State.ExecuteErrorStep;
							break;
						}
						
						// se vengo chiamato per i parametri, restituisco solo quelli
						if (Report.EngineType == EngineType.OfficeXML && xmlReturnType == XmlReturnType.ReportParameters)
						{
							Report.Engine.OutChannel.XmlGetParameters();
							CurrentState = State.End;
							break;
						}

						CurrentState = State.ExecuteAsk;
						break;
					}

					case State.ExecuteAsk :	
					{
						// skippo le Ask perchè sono chiamato come motore in background
						if (Report.EngineType == EngineType.OfficeXML)
						{
							// valorizza i parametri delle Ask con i dati provenienti dal dom passato dal chiamante
							Report.ExecuteLoadParamters();
							CurrentState = State.ExecuteExtraction;
							break;
						}
						if (Report.EngineType == EngineType.OfficePDF)
						{
							// skippo le Ask perchè sono chiamato come motore in background
							// valorizza i parametri delle Ask con i dati provenienti dal dom passato dal chiamante
							Report.ExecuteLoadParamters();
							CurrentState = State.ExecuteExtraction;
							//salvo symboltable e parso il woormdocument in quanto viene utilizzata la parte grafica
							Report.SaveInfo();
							if (!Woorm.LoadDocument() || !Woorm.ParseDocument())
							{
								BuildErrors(Woorm.Diagnostic);
								CurrentState = State.ViewerError;
								break;
							}
							break;
						}

						if (!Report.ExecuteAsk())
						{
							CurrentState = State.ExecuteErrorStep;
							break;
						}

						if (Report.CurrentAskDialog == null)
						{
							Report.ExecuteAfterAsk();

							Report.SaveInfo();
						
							if (!Woorm.LoadDocument() || !Woorm.ParseDocument())
							{
								BuildErrors(Woorm.Diagnostic);
								CurrentState = State.ViewerError;
								break;
							}

							CurrentState = State.ExecuteExtraction;
							break;
						}

						// esco per processare la corrente AskDialog
						HtmlPage = HtmlPageType.Form;
						CurrentState = State.RenderingForm;
						return;
					}

					case State.ExecuteExtraction:
						{
                            Report.Engine.Status = ReportEngine.ReportStatus.FirstRow;

                            //se chiamato via magic link, eseguo in maniera sincrona
							if (Report.EngineType == EngineType.OfficeXML || Report.EngineType == EngineType.OfficePDF)
							{
								CurrentInternalState = InternalState.ExecuteBeforeActions;
								DoExtraction();
								break;
							}
							else
							{
								//ho eseguito le ask dialog, posso far partire la fase di estrazione su un altro thread, e 
								//mandare all'utente una pagina che dia feedback all'utente
								extractionThread = new Thread(() =>
								{
									CurrentInternalState = InternalState.ExecuteBeforeActions;
									DoExtraction();
								});
								Working = true;
								extractionThread.Start();

								//Esco con una pagina che da feedback all'utente, e che impedisce che il client vada in timeout
								HtmlPage = HtmlPageType.Viewer;
							}

							return;
						}

					// qua ci si arriva da richieste esplicite dell'utente (Dialog)
					case State.ExecuteUserBreak:
						{
							if (!Report.ExecuteFinalizeActions())
							{
								// Se ho errore durante la finalizzazione devo fare lo stesso codice
								// di cleanup dello stato di errore senza rientrare nella macchina stati.
								Report.ExecuteErrorStep();
								Report.FinalizeChannel();
								CurrentState = State.ReportError;
								break;
							}

							// gestisco la diagnostica di avvertimento
							if (Report.Diagnostic.Warning)
							{
								// non ho estratto dati o sono stato interrotto (per OfficePass non è un errore ma una warnings
								foreach (IDiagnosticItem warning in Report.Diagnostic.AllMessages(DiagnosticType.Warning))
									Warnings.Add(warning.FullExplain);
							}
							else
								// Interrotto nelle AskDialog
								Errors.Add(WoormWebControlStrings.UserBreak);

							Report.FinalizeChannel();
							CurrentState = State.End;
							HtmlPage = HtmlPageType.Error;
							break;
						}

					// Qua posso arrivare con errori veri e propri oppure se ho stoppato il motore
					// e quindi sono in stato di UserBreak (Dati non trovati etc...)
					// Se sono in UserBreak non lo considero errore.
					case State.ExecuteErrorStep:
						{
							if (Report.Engine.UserBreak)
							{
								CurrentState = State.ExecuteUserBreak;
								break;
							}

							Report.ExecuteFinalizeActions();
							Report.ExecuteErrorStep();
							Report.FinalizeChannel();
							CurrentState = State.ReportError;
							break;
						}

					case State.RunViewer:
						{
							//Nel caso di Report storicizzati deve caricare il documento qui
							if (Woorm is RDEWoormDocument && !Woorm.LoadDocument())
							{
								BuildErrors(Woorm.Diagnostic);
								CurrentState = State.ViewerError;
								break;
							}
							HtmlPage = HtmlPageType.Viewer;
							CurrentState = State.End;

							break;
						}

					case State.UserInterrupted:
						{
							HtmlPage = HtmlPageType.Viewer;
							CurrentState = State.End;
							break;
						}

					case State.RunPersister:
						{
							HtmlPage = HtmlPageType.Persister;
							CurrentState = State.End;

							break;
						}

					case State.EndPersister:
						{
							HtmlPage = HtmlPageType.Viewer;
							CurrentState = State.End;

							break;
						}

					case State.ViewerError:
						{
							HtmlPage = HtmlPageType.Error;
							CurrentState = State.End;
							break;
						}

					case State.ConnectionError:
						{
							HtmlPage = HtmlPageType.Error;
							Errors.Add(WoormWebControlStrings.ConnectionError);

							CurrentState = State.End;
							break;
						}

					case State.NoDataFound:
						{
							HtmlPage = HtmlPageType.Error;
							Warnings.Add(WoormWebControlStrings.NoDataFound);

							CurrentState = State.End;
							break;
						}

					case State.NoWeb:
						{
							HtmlPage = HtmlPageType.Error;
							Errors.Add(WoormWebControlStrings.NoWebSupport);

							CurrentState = State.End;
							break;
						}

					case State.BadRelease:
						{
							HtmlPage = HtmlPageType.Error;
							CurrentState = State.End;
							break;
						}

					case State.AuthenticationError:
						{
							HtmlPage = HtmlPageType.Error;
							Errors.Add(WoormWebControlStrings.AuthenticationError);

							CurrentState = State.End;
							break;
						}

					case State.LoadSessionError:
						{
							HtmlPage = HtmlPageType.Error;
							Errors.Add(WoormWebControlStrings.LoadSessionError);

							CurrentState = State.End;
							break;
						}

					// posso avere più stati di errore
					case State.ReportError:
						{
							HtmlPage = HtmlPageType.Error;
							Errors.Add(WoormWebControlStrings.SyntaxError);
							Errors.Add(Filename);

							if (Report.Diagnostic.Error)
								foreach (IDiagnosticItem error in Report.Diagnostic.AllMessages(DiagnosticType.Error))
								{
									Errors.Add(error.FullExplain);
									if (error.ExtendedInfo != null)
										Errors.Add(string.Format
											(
												WoormWebControlStrings.ErrorLocation,
												error.ExtendedInfo["Row"].ToString(),
												error.ExtendedInfo["Column"].ToString()
											));
								}

							Errors.Add(string.Format
								(
									WoormWebControlStrings.CompileError,
									Report.Diagnostic.TotalErrors
								));

							CurrentState = State.End;
							break;
						}

					case State.FileNotFound:
						{
							HtmlPage = HtmlPageType.Error;
							Errors.Add(string.Format(WoormWebControlStrings.FileNotFound, Filename));

							CurrentState = State.End;
							break;
						}

					case State.GrantViolation:
						{
							HtmlPage = HtmlPageType.Error;
							Errors.Add(WoormWebControlStrings.GrantViolation);

							CurrentState = State.End;
							break;
						}

					case State.Print:
						{
							HtmlPage = HtmlPageType.Print;
							CurrentState = State.End;
							break;
						}

					// se non è uno dei precedenti allora esco
					default:
						return;
				}
			}
		}

		//--------------------------------------------------------------------------
		private void DoExtraction()
		{
			while (CurrentInternalState != InternalState.End)
			{
				switch (CurrentInternalState)
				{
					case InternalState.ExecuteBeforeActions:
						{
							if (!Report.ExecuteBeforeActions())
							{
								CurrentState = State.ExecuteErrorStep;
								CurrentInternalState = InternalState.End;
								break;
							}

							CurrentInternalState = InternalState.ExecuteRulesAndEvents;
							break;
						}

					case InternalState.ExecuteRulesAndEvents:
						{
							RuleReturn ruleReturn = Report.ExecuteRulesAndEvents();
							switch (ruleReturn)
							{
								case RuleReturn.Abort:
									{
										CurrentState = State.ExecuteErrorStep;
										CurrentInternalState = InternalState.End;
										break;
									}

								case RuleReturn.Backtrack:
									{
										CurrentInternalState = InternalState.ExecuteFinalizeActions;
										break;
									}

								case RuleReturn.Success:
									{
										CurrentInternalState = InternalState.ExecuteAfterActions;
										break;
									}
							}
							break;
						}
					case InternalState.ExecuteAfterActions:
						{
							if (!Report.ExecuteAfterActions())
							{
								CurrentState = State.ExecuteErrorStep;
								CurrentInternalState = InternalState.End;
								break;
							}

							CurrentInternalState = InternalState.ExecuteFinalizeActions;
							break;
						}

					case InternalState.ExecuteFinalizeActions:
						{
							if (!Report.ExecuteFinalizeActions())
							{
								CurrentState = State.ExecuteErrorStep;
								CurrentInternalState = InternalState.End;
								break;
							}

							CurrentInternalState = InternalState.ExecuteLastStep;
							break;
						}
					case InternalState.ExecuteLastStep:
						{
							
							Report.FinalizeChannel();				
							CurrentInternalState = InternalState.End;
							if (Report.ExitStatus == RuleReturn.Backtrack)
							{
								CurrentState = State.NoDataFound;
								break;
							}

							CurrentState = State.RunViewer;
							break;
						}
				}
			}

			Working = false;
		}
	}
}
