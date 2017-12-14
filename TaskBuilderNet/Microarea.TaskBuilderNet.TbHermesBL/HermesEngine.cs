using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Limilabs.Client.IMAP;
using Limilabs.Client.POP3;
using Limilabs.Client.SMTP;
using Limilabs.Mail;
using Limilabs.Mail.Headers;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.TbHermesBL.Config;
using System.IO;
using System.Diagnostics;


namespace Microarea.TaskBuilderNet.TbHermesBL
{
	public class HermesEngine
	{
        LoginManagerConnector loginManagerConn = new LoginManagerConnector(); // TbHermes
		private object tickLocker = new object();
        private short divisor = 10;
        
		//---------------------------------------------------------------------
		public IDiagnostic Diagnostic { get; set; }

		//---------------------------------------------------------------------
		public bool Init()
		{
			// TODO - re-inizializzazione delle strutture dati usate
			loginManagerConn.Restart();
			return true;
		}

		//---------------------------------------------------------------------
		public string Tick()
		{
			bool entered = false;
            string sReturn = "BEGIN";
			IEasyAttachmentManager eaMng = null;
            TbSenderDatabaseInfo cmp;
  			HermesSettings hs = HermesSettings.Load();
            MailLogListener MailTracer = new MailLogListener();

            // controllo semaforo sollevato da attività precedente Tick, se esiste
            // ancora attività in corso, rimandare task
            try
            {
                entered = Monitor.TryEnter(tickLocker);
                if (false == entered)
                {
                    MailTracer.WriteLine("INFO: Tick already lock", "Hermes");
                    return "INFO: Tick already lock";
                }
            }
            catch (Exception ex)
            {
                // non riesco a salvare le info della login
                TreatException(ex);
                if (entered)
					Monitor.Exit(tickLocker);
                return "EXCEPTION tickLocker:" + ex.ToString();
            }

            // recupero utili informazioni dal loginmanager
            try
            {
                if (divisor == 10)
                {
                    loginManagerConn.GetMyData();
                    divisor = 0;
                }
                else
                    divisor++;
            }
            catch (Exception ex)
            {
                // non riesco a salvare le info della login
                TreatException(ex);
                if (entered)
					Monitor.Exit(tickLocker);
                return "EXCEPTION GetMyData:" + ex.ToString();
            }

            
            // inizializzo il log
            try
            {
                if (true == hs.LoggerEnabled)
                {
                    DirectoryInfo di = new DirectoryInfo(hs.LoggingPath);
                    if (!di.Exists)
                        di.Create();
                    string fileLogPath = hs.LoggingPath + "mail_log_" + DateTime.Today.ToString("ddMMyyyy") + ".txt";

                    MailTracer.Initialize(fileLogPath);
                    Trace.Listeners.Add(MailTracer);
                    Trace.AutoFlush = true;

                    if (true == hs.LoggerRawData)
                    {
                        Limilabs.Mail.Log.Enabled = true;   // Limilabs.Mail.Log si servira' del listeren seguente
                        //Limilabs.Mail.Log.WriteLine += Console.WriteLine;
                    }
                }
            }
            catch (Exception ex)
            {
                // non riesco a creare il trace listener
                TreatException(ex);
                if (entered)
					Monitor.Exit(tickLocker);
                if (MailTracer != null)
                {
                    MailTracer.Flush();
                    MailTracer.Close();
                } 
                return "EXCEPTION Logger:" + ex.ToString();
            }

            // da ora posso scrivere nel log (qualora fosse attivo)
            MailTracer.WriteLine("Start:", "TICK--");

            // controllo dei settings
            try
			{                
				if (false == hs.Enabled)
                {
                    MailTracer.WriteLine("INFO: mail server disabled", "Hermes");
                    System.Diagnostics.Process[] myProcesses;
                    myProcesses = System.Diagnostics.Process.GetProcessesByName("TbLoader");
                    foreach (System.Diagnostics.Process instance in myProcesses)
                    {
                        //instance.CloseMainWindow();
                        //instance.Close();
                        instance.Kill();
                    }
                    throw new Exception("INFO: mail server disabled");
                }

                if (string.IsNullOrEmpty(hs.Company))
                {
                    MailTracer.WriteLine("ERROR: No company found", "Hermes");
                    throw new Exception("ERROR: No company found");
                }
            }
            catch (Exception ex)
            {
                // mancano delle info uscire subito
                TreatException(ex);
                if (entered)
					Monitor.Exit(tickLocker);
                if (MailTracer != null)
                {
                    MailTracer.Flush();
                    MailTracer.Close();
                } 
                return "Settings:" + ex.Message;
            }

            // inizializzo cmp e EA
            try
			{                
				// Blindiamo su un'unica company (db aziendale)
				List<TbSenderDatabaseInfo> cmpList = loginManagerConn.GetSubscribedCompanies();
				cmp = cmpList.FirstOrDefault(x => string.Compare(x.Company, hs.Company, StringComparison.InvariantCultureIgnoreCase) == 0);
				if (cmp == null)
                {
                    MailTracer.WriteLine("ERROR: the company selected mismatch the company list", "Hermes");
                    throw new Exception("ERROR: the company selected mismatch the company list");
                }
            
				eaMng = new EasyAttachmentManager(loginManagerConn, cmp);
					//new DummyEasyAttachmentManager();
                if (false == eaMng.DmsDbFound)
                {
                    MailTracer.WriteLine("ERROR: EasyAttachment not ready", "Hermes");
                    throw new Exception("ERROR: EasyAttachment not ready");
                }
			}
            catch (Exception ex) 
            {
                // errorri di inizializzazione
                TreatException(ex);
                if (entered)
					Monitor.Exit(tickLocker);
                if (MailTracer != null)
                {
                    MailTracer.Flush();
                    MailTracer.Close();
                } 
                return "Initialize:" + ex.Message;
            }


            try
            {
                //Calendar Process
                MailTracer.WriteLine("Calendar Process:", "TICK--");
                string module = "Calendar";
                if (IsActiveModule(cmp, module))
                    sReturn += CalendarFunctions.CalendarProcess(cmp, this.TreatException, MailTracer, this.loginManagerConn, eaMng);
            }
            catch (Exception e)
            {
                MailTracer.WriteLine("EXCEPTION:" + e.ToString(), "Hermes");
                sReturn = "EXCEPTION:" + e.ToString();
            }

            try
            {
                //Mail Process
                MailTracer.WriteLine("Mail Process:", "TICK--");
                string module = "Mail";
                if (IsActiveModule(cmp, module))
                    sReturn += MailFunctions.MailProcess(cmp, eaMng, this.TreatException, MailTracer, this.loginManagerConn);
            }
            catch (Exception e)
            {
                MailTracer.WriteLine("EXCEPTION:" + e.ToString(), "Hermes");
                sReturn = "EXCEPTION:" + e.ToString();
            }


            // finale
            MailTracer.WriteLine("End.", "TICK--");
            if (entered)
				Monitor.Exit(tickLocker);
            if (MailTracer != null)
            {
                MailTracer.Flush();
                MailTracer.Close();
            } 
			if (eaMng != null)
				eaMng.Dispose();

            sReturn += "_END";
            return sReturn;
		}


       	//---------------------------------------------------------------------
        public static bool IsActiveModule(TbSenderDatabaseInfo cmp, string module)
        {
            bool ret = false;
            try
            {
                OM_ModulesOFM moduleOFM = OM_ModulesOFM.GetModulesOFM(cmp, module);
                if (moduleOFM != null)
                    ret = moduleOFM.BoolIsActive;
            }
            catch 
            {
                ret = false;
            }
            return ret;
        }

		//---------------------------------------------------------------------
		public ServiceStatus GetUpdatedStatus(string company, ClientIdentifier clientIdentifier)
		{
			// questo metodo l'abbiamo impotizzato con lo scopo di fornire al client uno status aggiornato
			// comprendente sia lo stato della connessione a Internet vista da server, sia la data di ultimo
			// messaggio in base al clientIdentifier.
			// Qualore il demone avesse in memoria la data (in seguito a ultima elaborazione tick), 
			// risparmieremmo carico sul db
			return new ServiceStatus(); // TODO
		}

		public void UploadMessage(string company, MailMessage mailMessage)
		//public void ForceSendSingleEMail(..)
		{
			// mentre la lettura dei messaggi da parte del client passa direttemante da db (al massimo chiedendo
			// al server la data di ultimo messaggio ricevuto in base al clientIdentifier), per la scrittura/invio
			// occorre per forza passare dal server.
			// In termini logici orrisponde a mettere in outbox la mail. In termini pratici vuole dire scrivere una
			// riga su db (o tenere in una coda in memoria) in attesa del prossimo tick che faccia l'invio/db-scrittura

			//outbox.Enqueue(mailMessage);
			// TODO - la scrittura preferisco farla fare al service per semplificare la semaforizzazione
		}

		//public void ForceSendSingleAccountOutbox(..)
		//public void ForceSendSingleSingleWorkerEmails(..)
		//public void ForceReceive(..)


		//---------------------------------------------------------------------
		public delegate void TreatExceptionDelegate(Exception ex);
		public void TreatException(Exception ex)
		{
			TreatException(ex, this.Diagnostic);
		}
		public static void TreatException(Exception ex, IDiagnostic diagnostic)
		{
            Trace.WriteLine("EXCEPTION:");
            Trace.WriteLine(ex, "Hermes");
            Trace.WriteLine("");
		}
	}
}
