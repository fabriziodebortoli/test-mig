using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Xml;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.StringLoader;
using Microarea.Common.Applications;
using Microarea.Common.Generic;

using Microarea.RSWeb.WoormEngine;
using Microarea.RSWeb.WoormViewer;
using Microarea.RSWeb.Models;
using System.Net.WebSockets;
using System.Text;
using Microarea.Common.NameSolver;
using System.Collections.Generic;
using Microarea.Common.CoreTypes;

namespace Microarea.RSWeb.Render
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
        Run,
        RunViewer,
        RunPersister,
        EndPersister,
        InitalizeChannel,
        ExecuteInizialize,
        ExecuteAsk,
        ExecuteExtraction,
        ExecuteErrorStep,
        ExecuteUserBreak,
        //RenderingAskDialog,
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
        public const string WebFramework = "WebFramework";
        public const string EasyLookFull = "EasyLookFull";
        public const string EasyLook = "EasyLook";
    }

    /// <summary>
    /// Descrizione di riepilogo per RSEngine.
    /// </summary>
    /// ================================================================================
    public class RSEngine : IDisposable
    {
        public WoormDocument Woorm = null;
        public Report Report = null;

        private TbReportSession reportSession = null;
        public AskDialog ActiveAskDialog = null;

        private bool disposed = false;  // Track whether Dispose has been called.
        private XmlReturnType xmlReturnType = XmlReturnType.ReportData;
        private Thread extractionThread = null;
        private State currentState = State.Start;

        public StringCollection Errors = new StringCollection();
        public StringCollection Warnings = new StringCollection();

        public HtmlPageType PreviousHtmlPage = HtmlPageType.None;
        public HtmlPageType HtmlPage = HtmlPageType.None;
        public string ReportTitle = string.Empty;

        public  InternalState   CurrentInternalState;
        public  bool            Working = false;
        private bool            reRun = false;

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
        public string Filename { get { return Woorm.Filename; } }
        //--------------------------------------------------------------------------
        public TbReportSession ReportSession { get { return reportSession; } }

        //--------------------------------------------------------------------------
        public RSEngine(TbReportSession reportSession)
        {
            this.reportSession = reportSession;
            string filename = ReportSession.ReportPath;

            if (reportSession.UserInfo.IsAuthenticated())
            {
                // Se sono  un file XML allora sono uno snapshot e quindi serve solo il visualizzatore altrimenti
                // eseguo tutto normalmente ed estraggo dati e li visualizzo
                if (string.Compare(Path.GetExtension(filename), NameSolverStrings.XmlExtension, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    Woorm = new RDEWoormDocument(filename, reportSession);
                    Report = null;
                }
                else
                {
                    Woorm = new WoormDocument(filename, reportSession);
                    Report = new Report(filename, reportSession, Woorm);
                }
            }
        }

        //--------------------------------------------------------------------------
        // da usarsi solo per il motore di esportazione in XML. Il viewer serve solo per 
        // parsare correttamente l'header del report e fare tutti i controlli di release
        // necessari (ad esempio : NOWEB)
        public RSEngine
            (
                TbReportSession reportSession,
                XmlDocument xmlDomParameters,
                StringCollection xmlResultReports,
                XmlReturnType xmlReturnType
            )
        {
            this.reportSession = reportSession;
            this.xmlReturnType = xmlReturnType;

            if (reportSession.UserInfo.IsAuthenticated())
            {
                this.reportSession.Localizer = new WoormLocalizer(reportSession.ReportPath, ReportSession.PathFinder);
                // Woorm serve per gestire il parse delle release
                Woorm   = new WoormDocument (reportSession.ReportPath, reportSession);
                Report  = new Report        (reportSession.ReportPath, reportSession, xmlDomParameters, xmlResultReports);
            }
        }

        //--------------------------------------------------------------------------
        // da usarsi solo per il motore di esportazione in PDF. 
        public RSEngine
            (
                TbReportSession reportSession,
                XmlDocument xmlDomParameters
            )
        {
            this.reportSession = reportSession;

            if (reportSession.UserInfo.IsAuthenticated())
            {
                this.reportSession.Localizer = new WoormLocalizer(reportSession.ReportPath, ReportSession.PathFinder);
                //serve sia visualizzatore (WoormDocument) che motore (Report), per generare il pdf.
                Woorm   = new WoormDocument (reportSession.ReportPath, reportSession);
                Report  = new Report        (reportSession.ReportPath, reportSession, xmlDomParameters);
            }
        }

        //------------------------------------------------------------------------------
        //public void Unparse()
        //{
        //	using (Unparser unparser = new Unparser())
        //	{
        //		//unparser.Open(reportSession.ReportPath);
        //		unparser.Open("c:\\a.wrm");
        //		Woorm.Unparse(unparser);
        //		Report.Unparse(unparser);
        //	}
        //}

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
            if (!disposed)
            {
                // se arrivo dal distruttore non so l'ordine di distruzione
                if (disposing)
                {
                    if (Report != null) Report.Dispose();
                    if (Woorm != null) Woorm.Dispose();
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
                return true;
                /* TODO RSWEB check della security light/full
                    if (Woorm.Namespace == null) 
                        return true;

                    bool applySecurityFilter = false;

                    if (reportSession.UserInfo != null)
                        {
                            if (reportSession.UserInfo.IsSecurityLightEnabled() && !reportSession.UserInfo.IsSecurityLightAccessAllowed(Woorm.Namespace.ToString(), false))
                                return false;

                            applySecurityFilter = reportSession.UserInfo.IsActivated("MicroareaConsole", "SecurityAdmin");
                        }
                    //Controllo se ho i grant per vedere il report 

                    ISecurity security = reportSession.UserInfo.NewSecurity
                        (
                            reportSession.UserInfo.Company, 
                            reportSession.UserInfo.User,
                            applySecurityFilter
                        ); 

                    bool existGrants = security.ExistExecuteGrant(Woorm.Namespace.ToString(), SecurityType.Report);
                    security.Dispose();

                    return existGrants;
                */
            }
        }

        //--------------------------------------------------------------------------
        public bool Step()
        {
            if (Working)
                return false;

            while (true)
            {
                switch (CurrentState)
                {
                    case State.Start:
                        {
                            // controlla che il file sia presente
                            if (!PathFinder.PathFinderInstance.ExistFile(Filename))
                            {
                                CurrentState = State.FileNotFound;
                                //TODO RSWEB DIAGNOSTIC -> MESSAGE TO CLIENT
                                break;
                            }

                            if (!Granted)
                            {
                                CurrentState = State.GrantViolation;
                                break;
                            }

                            if (reportSession.UserInfo == null ||
                                !reportSession.UserInfo.IsValidToken(reportSession.UserInfo.AuthenticationToken) ||
                                !reportSession.UserInfo.IsActivated(StateMachineConstStrings.WebFramework, StateMachineConstStrings.EasyLook))
                            {
                                CurrentState = State.AuthenticationError;
                                break;
                            }

                            CurrentState = State.CheckNoWebAndRelease;
                            break;
                        }

                    case State.CheckNoWebAndRelease:
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
                            CurrentState = State.Run;
                            break;
                        }

                    case State.Run:
                        {
                            if (Report == null)
                            {
                                CurrentState = State.RunViewer;
                                break;
                            }

                            if (!Report.ParseReport())
                            {
                                CurrentState = State.ReportError;
                                break;
                            }

                            ReportTitle = Report.ReportTitle;
                            CurrentState = State.InitReport;
                            break;
                        }

                    case State.InitReport:
                        {

                            Report.InitReport();

                            CurrentState = State.InitalizeChannel;
                            break;
                        }

                    case State.InitalizeChannel:
                        {
                            Report.InitializeChannel();
                            CurrentState = State.ExecuteInizialize;
                            break;
                        }

                    case State.ExecuteInizialize:
                        {
                            Report.Engine.Status = ReportEngine.ReportStatus.Init;

                            if (!Report.ExecuteInitialize())
                            {
                                CurrentState = State.ExecuteErrorStep;
                                break;
                            }

                            // se vengo chiamato per i parametri, restituisco solo quelli
                            if (Report.EngineType == EngineType.FullXML_OfficeXML && xmlReturnType == XmlReturnType.ReportParameters)
                            {
                                Report.Engine.OutChannel.XmlGetParameters();
                                CurrentState = State.End;
                                break;
                            }

                            this.Report.SymTable.SaveAskDialogFieldsState();

                            Report.SaveInfo();

                            if (!reRun && (!Woorm.LoadDocument() || !Woorm.ParseDocument()))
                            {
                                BuildErrors(Woorm.Diagnostic);
                                CurrentState = State.ViewerError;
                                break;
                            }

                            CurrentState = State.ExecuteAsk;
                            return false;
                        }

                    case State.ExecuteAsk:
                        {
                            //-------------------------------------------------------------
                            if (Report.EngineType == EngineType.FullXML_OfficeXML)
                            {
                                // skippo le Ask perchè sono chiamato come motore in background
                                // valorizza i parametri delle Ask con i dati provenienti dal dom passato dal chiamante
                                Report.ExecuteLoadParamters();

                                CurrentState = State.ExecuteExtraction;
                                break;
                            }

                            //-------------------------------------------------------------
                            if (Report.EngineType == EngineType.PDFSharp_OfficePDF)
                            {
                                // skippo le Ask perchè sono chiamato come motore in background
                                // valorizza i parametri delle Ask con i dati provenienti dal dom passato dal chiamante
                                Report.ExecuteLoadParamters();

                                CurrentState = State.ExecuteExtraction;

                                //salvo symboltable e parso il woormdocument in quanto viene utilizzata la parte grafica
                                //Report.SaveInfo();

                                //if (!Woorm.LoadDocument() || !Woorm.ParseDocument())
                                //{
                                //	BuildErrors(Woorm.Diagnostic);
                                //	CurrentState = State.ViewerError;
                                //	break;
                                //}
                                break;
                            }

                            //-------------------------------------------------------------
                            //TODO RSWEB skip ask dialogs
                            //if (Report.Engine.ExistsDialogs())
                            //    Report.Engine.HideAllAskDialogs();
                            ////-------------------------------------------------------------

                            if (!Report.ExecuteAsk())
                            {
                                CurrentState = State.ExecuteErrorStep;
                                break;
                            }

                            if (Report.CurrentAskDialog == null)
                            {
                                Report.ExecuteAfterAsk();

                                //Report.SaveInfo();

                                //if (!Woorm.LoadDocument() || !Woorm.ParseDocument())
                                //{
                                //	BuildErrors(Woorm.Diagnostic);
                                //	CurrentState = State.ViewerError;
                                //	break;
                                //}

                                CurrentState = State.ExecuteExtraction;

                                break;
                            }

                            // esco per processare la corrente AskDialog
                            //CurrentState = State.RenderingAskDialog;
                            HtmlPage = HtmlPageType.Form;

                            RSSocketHandler.SendMessage(this.reportSession.WebSocket, MessageBuilder.CommandType.ASK, Report.CurrentAskDialog.ToJson()); //.Wait();

                            return false;
                        }

                    case State.ExecuteExtraction:
                        {
                            Report.Engine.Status = ReportEngine.ReportStatus.FirstRow;
                            {
                                Field f = Report.Engine.RepSymTable.Fields.Find(SpecialReportField.NAME.IS_FIRST_TUPLE);
                                if (f != null)
                                {
                                    f.SetAllData(true, true);
                                }
                                f = Report.Engine.RepSymTable.Fields.Find(SpecialReportField.NAME.IS_LAST_TUPLE);
                                if (f != null)
                                {
                                    f.SetAllData(false, true);
                                }
                            }

                            //se chiamato via magic link, eseguo in maniera sincrona
                            if (Report.EngineType == EngineType.FullExtraction || Report.EngineType == EngineType.FullXML_OfficeXML || Report.EngineType == EngineType.PDFSharp_OfficePDF)
                            {
                                CurrentInternalState = InternalState.ExecuteBeforeActions;

                                DoExtraction();

                                bool ok = this.Woorm.RdeReader.LoadTotPage();

                                string tot = this.Woorm.RdeReader.TotalPages.ToJson("totalPages", true);
                                RSSocketHandler.SendMessage(this.reportSession.WebSocket, MessageBuilder.CommandType.ENDREPORT, tot); //.Wait();

                                Woorm.LoadPage(1);
                                RSSocketHandler.SendMessage(this.reportSession.WebSocket, MessageBuilder.CommandType.TEMPLATE, Woorm.ToJson(true)); //.Wait();
                                
                                return false;
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

                            return false;
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
                    case State.End:
                        {
                            //string tot = this.Woorm.RdeReader.TotalPages.ToJson("totalPages", true) ;
                            //RSSocketHandler.SendMessage(this.reportSession.WebSocket, MessageBuilder.CommandType.ENDREPORT, tot); //TotalPages //.Wait();

                            return false;
                        }

                    // se non è uno dei precedenti allora esco
                    default:
                        return false;
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

                            //if existing, out variables (e.g passed from the tbloader document) are saved to reportSession and sent back to called document
                            if (Report.UnparseOutParametersToReportSession())
                            {
                                TbSession.TbAssignWoormParameters(this.reportSession);
                            }

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
        //--------------------------------------------------------------------
       public void StopReport()
       {
            Report.Engine.StopEngine(WoormViewerStrings.ExecutionStopped);
            Woorm.ReportSession.StoppedByUser = true;
            ExtractionThread.Join();
            CurrentState = State.UserInterrupted;
            Step();
       }

       public void ReRun()
       {
            reRun = true;
            Report.SymTable.DisplayTables.ResetAllRowsCounter();
            Report.Engine.PrepareForReRun();

            CurrentState = State.Start;
            Step();
        }

    }
 }
