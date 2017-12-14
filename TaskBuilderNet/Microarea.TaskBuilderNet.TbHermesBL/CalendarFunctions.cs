using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Security.Cryptography.X509Certificates;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.TbHermesBL;
using Microarea.TaskBuilderNet.Core.NameSolver;

using System.IO;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;

using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Compilation;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Util;

using Google.Apis;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Util.Store;
using Google.Apis.Auth;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    static class CalendarFunctions
    {
        //-------------------------------------------------------------------------------
        //Processo dei sincronizzazione con Google Calendar
        public static string CalendarProcess(TbSenderDatabaseInfo cmp, HermesEngine.TreatExceptionDelegate ted, MailLogListener logTrace, LoginManagerConnector lmc, IEasyAttachmentManager eaMng)
        {
            string sReturn = "_CALENDAR";
            //TODO Prendere i dati del proxy di sistema
            try
            {
                //Imposto i dati del proxy inseriti nella Console di VS  
                CheckProxy();

                //Recupero la lista degli account Google da sincronizzare
                List<OM_GoogleAccounts> allGoogleAccounts = new List<OM_GoogleAccounts>();
                allGoogleAccounts.AddRange(OM_GoogleAccounts.GetGoogleAccounts(cmp));

                foreach (OM_GoogleAccounts accountGoogle in allGoogleAccounts)
                {
                    try
                    {
                        //Recupero la mail dell'account (è la chiave del calendario principale) per identificare gli appuntamenti di cui è propietario
                        accountGoogle.PrimaryAddress = OM_GoogleCalendars.GetGoogleCalendarPrimaryAddress(cmp, accountGoogle.WorkerId);

                        logTrace.WriteLine("- get calendar for=" + accountGoogle.PrimaryAddress, "Hermes");
                        //Attivo il servizio di accesso alle API Google
                        CalendarService gooService = GoogleLogin(accountGoogle);
                        if (gooService == null)
                            continue;

                        //Allineamento Calendari
                        logTrace.WriteLine("- GoogleCalendarSychronization", "Hermes");
                        bool calSync = GoogleCalendarSychronization(cmp, ted, logTrace, accountGoogle, gooService);
                        
                        //Allineamento Eventi
                        logTrace.WriteLine("- GoogleEventsSynchronization", "Hermes");
                        bool evSync = GoogleEventsSynchronization(cmp, ted, logTrace, eaMng, accountGoogle, gooService);
                        if (evSync) //Impostao la richiesta di aggiornamento dell'organizer sul Client attraverso la tabella OM_WorkersAlerts
                        {
                            sReturn += "_REFRESH";
                            logTrace.WriteLine("- UpdateCalendarWorkersAlerts", "Hermes");
                            OM_WorkersAlerts.UpdateCalendarWorkersAlerts(cmp, accountGoogle.WorkerId, evSync, ted, logTrace);
                        }

                    }
                    catch (Exception ex)
                    {
                        sReturn += "ERRCAL01: " + ex.Message + "\r\n";
                    }
                }
                sReturn += "_OK";
            }
            catch (Exception ex)
            {
                sReturn += "ERRCAL02: " + ex.Message + "\r\n";
            }
            return sReturn;
        }

        //-------------------------------------------------------------------------------
        //Imposto i dati del proxy inseriti nella Console di VS  
        public static void CheckProxy()
        {
            string filePath = BasePathFinder.BasePathFinderInstance.GetProxiesFilePath();
            ProxySettings proxySettings = ProxySettings.GetServerProxySetting(filePath);
            if ((proxySettings != null) && 
                (!proxySettings.HttpProxy.Server.IsNullOrEmpty()))
            {
                var uri = new Uri(proxySettings.HttpProxy.Server + ":" + proxySettings.HttpProxy.Port);
                WebProxy webProxy = new WebProxy(uri, false);

                if (proxySettings.FirewallCredentialsSettings.NeedsCredentials)
                {
                    NetworkCredential nc = null;
                    if (
                        proxySettings.FirewallCredentialsSettings.Name == null ||
                        proxySettings.FirewallCredentialsSettings.Name.Trim().Length == 0 ||
                        proxySettings.FirewallCredentialsSettings.Password == null ||
                        proxySettings.FirewallCredentialsSettings.Password.Trim().Length == 0
                        )
                        nc = CredentialCache.DefaultNetworkCredentials;
                    else
                    {
                        nc =
                            new NetworkCredential
                            (
                            proxySettings.FirewallCredentialsSettings.Name,
                            Storer.Unstore(proxySettings.FirewallCredentialsSettings.Password),
                            proxySettings.FirewallCredentialsSettings.Domain
                            );
                    }

                    webProxy.Credentials = nc;
                }

                System.Net.WebRequest.DefaultWebProxy = webProxy;
            }
        }

        //-------------------------------------------------------------------------------
        //Attivo il servizio di accesso alle API Google
        public static CalendarService GoogleLogin(OM_GoogleAccounts accountGoogle)
        {
            string sAccount             = accountGoogle.WorkerId.ToString();
            string sClientSecretFile    = accountGoogle.ClientSecretPath;
            string sDataStoreFile       = accountGoogle.FileDataStorePath;
            string sDataStorePath       = accountGoogle.WorkerLogPath;
                
            IList<String> scopes;
            CalendarService service = null;
            scopes = new List<string>();
            //' Add the calendar specific scope to the scopes list.
            scopes.Add(CalendarService.Scope.Calendar);

            //Controllo l'esistenza dei percorsi e dei file necessari alla sincronizzazione
            if ((File.Exists(sClientSecretFile)) &&
                (File.Exists(sDataStoreFile)) &&
                (Directory.Exists(sDataStorePath)) &&
                (!sAccount.IsNullOrEmpty()))
            {
                //Carico il File contenente la chiave ClientSecret
                FileStream stream = new FileStream(sClientSecretFile, FileMode.Open, FileAccess.Read);

                //Imposto il percorso in cui è presente il file di DataStore
                FileDataStore aFile = new Google.Apis.Util.Store.FileDataStore(sDataStorePath);

                //Richiedo le credenziali Google per creare il servizio
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets, scopes, sAccount,
                    System.Threading.CancellationToken.None, aFile).Result;

                //Creo il servizio a Google Calendar
                var initializer = new BaseClientService.Initializer();
                initializer.HttpClientInitializer = credential;
                //initializer.ApplicationName = "calendarapotest";

                service = new CalendarService(initializer);
            }

            return service;
        }

        //-------------------------------------------------------------------------------
        //Allineamento Calendari
        public static bool GoogleCalendarSychronization(TbSenderDatabaseInfo cmp, HermesEngine.TreatExceptionDelegate ted, MailLogListener logTrace,
            OM_GoogleAccounts accountGoogle, CalendarService service)
        {
            bool ret = false;
            //Creo la query di richiesta (includo i calendari cancellati)
            CalendarListResource.ListRequest calRequest = service.CalendarList.List();
            calRequest.ShowDeleted = true;

            //Eseguo la richiesta a Google
            IList<CalendarListEntry> list = calRequest.Execute().Items;
            logTrace.WriteLine("-- count of google cals=" + list.Count(), "Hermes");
            foreach (CalendarListEntry gooCal in list)
            {
                ret = true;

                //Verifico se il calendario è da eliminare
                bool toDelete = false;
                if ((gooCal.Deleted != null) && (gooCal.Deleted.HasValue))
                    toDelete = gooCal.Deleted.Value;

                //Cerco il calendario nel Database di VS
                OM_GoogleCalendars cal = OM_GoogleCalendars.GetGoogleCalendar(cmp, accountGoogle.WorkerId, gooCal.Id);
                if (cal == null)
                {
                    if (!toDelete) //Se il calendario è attivo salvo il record corrispondente nella tabella OM_GoogleCalendars 
                        OM_GoogleCalendars.SaveGoogleCalendar(cmp, accountGoogle.WorkerId, gooCal, ted);
                }
                else
                {
                    if (toDelete)
                    {
                        //Se il calendario è stato cancellato in Google tolgo la sincronizzazione al calendario
                        RemoveCalendarSynchronizationVS(cmp, gooCal.Id, accountGoogle.WorkerId, ted);
                    }
                    else
                    {
                        //Verifico e aggiorno i campi del calendario che sono cambiati 
                        OM_GoogleCalendars.CheckUpdateCalendar(cmp, gooCal, cal, ted);
                    }
                }
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Allineamento Eventi (il valore di ritorno indica se è necessario un'aggiornamento del calendario)
        public static bool GoogleEventsSynchronization(TbSenderDatabaseInfo cmp, HermesEngine.TreatExceptionDelegate ted, MailLogListener logTrace, IEasyAttachmentManager eaMng, 
            OM_GoogleAccounts accountGoogle, CalendarService service)
        {
            bool ret = false;
            try
            {
                // Imposto i giorni di ricerca 
                int befDays = accountGoogle.DayBefore;
                int nextDays = accountGoogle.DayAfter;
                DateTime minLimit = Convert.ToDateTime(DateTime.Now.AddDays(-befDays));
                DateTime maxLimit = Convert.ToDateTime(DateTime.Now.AddDays(nextDays));

                // Ottengo la lista dei calendari del worker
                List<OM_WorkersCalendars> listCal = OM_WorkersCalendars.GetWorkersCalendars(cmp, accountGoogle.WorkerId);
                foreach (OM_WorkersCalendars wCal in listCal)
                {
                    //Controllo se il calendario è sincronizzato
                    if (wCal.IdGoogleCalendar.IsNullOrEmpty())
                        continue;
                    //Cerco il calendario corrispondenete della tabella OM_GoogleCalendars
                    OM_GoogleCalendars gCal = OM_GoogleCalendars.GetGoogleCalendar(cmp, accountGoogle.WorkerId, wCal.IdGoogleCalendar);
                    if (gCal == null)
                        continue;

                    //VS--->Google
                    {
                        logTrace.WriteLine("--- VS--->Google: workerid=" + accountGoogle.WorkerId + " SubId=" + wCal.SubId, "Hermes");
                        //Cerco i commitment singoli di cui sono proprietario in VS 
                        logTrace.WriteLine("GetSingleOwnerCommitments", "Hermes");
                        List<OM_Commitments> listCommitment = OM_Commitments.GetSingleOwnerCommitments(cmp, accountGoogle.WorkerId, wCal.SubId, minLimit, maxLimit);
                        
                        //Cerco i commitment ricorrenti di cui sono proprietario in VS 
                        logTrace.WriteLine("GetRecurrenceOwnerCommitments", "Hermes");
                        List<OM_Commitments> listRecurrenceCommitment = OM_Commitments.GetRecurrenceOwnerCommitments(cmp, accountGoogle.WorkerId, wCal.SubId, minLimit, maxLimit);

                        //Cerco i commitment ricorrenti padre di cui sono proprietario in VS 
                        logTrace.WriteLine("GetRecurrenceOwnerMainCommitments", "Hermes");
                        List<OM_Commitments> listRecurrenceMainCommitment = OM_Commitments.GetRecurrenceOwnerMainCommitments(cmp, accountGoogle.WorkerId, wCal.SubId, minLimit, maxLimit);

                        //TODO PIPPO: Ottenere la lista dei commitments di cui non sono il proprietario 
                        logTrace.WriteLine("GetGuestCommitments", "Hermes");
                        List<OM_Commitments> listGuestCommitment = OM_Commitments.GetGuestCommitments(cmp, accountGoogle.WorkerId, wCal.SubId, minLimit, maxLimit);

                        //TODO PIPPO: Ottenere la lista dei commitments di cui non sono il proprietario 
                        logTrace.WriteLine("GetRecurrenceGuestMainCommitments", "Hermes");
                        List<OM_Commitments> listRecurrenceMainGuestCommitment = OM_Commitments.GetRecurrenceGuestMainCommitments(cmp, accountGoogle.WorkerId, wCal.SubId, minLimit, maxLimit);

                        //TODO PIPPO: Ottenere la lista dei commitments di cui non sono il proprietario 
                        logTrace.WriteLine("GetRecurrenceGuestCommitments", "Hermes");
                        List<OM_Commitments> listRecurrenceGuestCommitment = OM_Commitments.GetRecurrenceGuestCommitments(cmp, accountGoogle.WorkerId, wCal.SubId, minLimit, maxLimit);
                        
                        //Cerco la lista di tutti gli appuntamenti sincronizzati fino a questo momento
                        logTrace.WriteLine("GetGoogleCalendarsEvents", "Hermes");
                        List<OM_GoogleCalendarsEvents> listGoogleCalendarEvents = OM_GoogleCalendarsEvents.GetGoogleCalendarsEvents(cmp, accountGoogle.WorkerId, wCal.IdGoogleCalendar);

                        // Ciclo di invio dei nuovi commitment a Google Calendar
                        logTrace.WriteLine("listCommitment==" + listCommitment.Count, "Hermes");
                        foreach (OM_Commitments comm in listCommitment)
                        {
                            //Cerco l'evento nella tabella OM_GoogleCalendarsEvents con le chiavi WorkerId e CommitmentId
                            OM_GoogleCalendarsEvents gCalEv = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(cmp, accountGoogle.WorkerId, comm.CommitmentId);//, wCal.IdGoogleCalendar);
                            if (gCalEv == null)
                            {
                                //Il commitment non è sincronizzato, creo un nuovo evento in Google Calendar
                                ret |= CreateEventGoogle(cmp, service, accountGoogle, gCal, comm, wCal, eaMng, ted);                            
                            }
                            else
                            {
                                //Controllo se il commitment è sincronizzato su un calendario diverso dall'attuale
                                if (gCalEv.IdCalendar != wCal.IdGoogleCalendar)
                                {
                                    //Cancello l'evento sul vecchio calendario
                                    RemoveEventGoogle(cmp, service, gCalEv, ted);
                                    //Creo il commitment sul nuovo calendario
                                    CreateEventGoogle(cmp, service, accountGoogle, gCal, comm, wCal, eaMng, ted);        
                                }
                            }
                        }

                        // Ciclo di invio dei nuovi commitment Ricorrenti a Google Calendar
                        logTrace.WriteLine("listRecurrenceMainCommitment==" + listRecurrenceMainCommitment.Count, "Hermes");
                        foreach (OM_Commitments comm in listRecurrenceMainCommitment)
                        {
                            //Cerco l'evento nella tabella OM_GoogleCalendarsEvents con le chiavi WorkerId e CommitmentId
                            OM_GoogleCalendarsEvents gCalEv = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(cmp, accountGoogle.WorkerId, comm.CommitmentId);//, wCal.IdGoogleCalendar);
                            if (gCalEv == null)
                            {
                                //Il commitment non è sincronizzato, creo un nuovo evento in Google Calendar
                                ret |= CreateRecurrenceEventGoogle(cmp, service, false, accountGoogle, gCal, comm, wCal, eaMng, ted);                            
                            }
                            else
                            {
                                //Controllo se la ricorrenza è stata generata in VS da una sequanza già esistente (dalla scelta "da questo in poi")
                                if (gCalEv.ParentCommitmentId != comm.RecurrenceId)
                                { 
                                    //Rimuovo l'evento dalla serie precedente 
                                    ret |= RemoveEventGoogle(cmp, service, gCalEv, ted);
                                    //Creo la nuova sequenza partendo da questo commitment
                                    ret |= CreateRecurrenceEventGoogle(cmp, service, false, accountGoogle, gCal, comm, wCal, eaMng, ted);                            
                                }
                                else
                                {
                                    //Controllo se sono state generate eccezioni nella sequenza in VS
                                    CheckRecurringEventExceptionVS(cmp, service, accountGoogle, gCal, gCalEv, wCal, comm, ted);
                                }
                            }
                        }
                        
                        // Ciclo di invio dei nuovi commitment in cui risulto invitato in VS
                        logTrace.WriteLine("listGuestCommitment==" + listGuestCommitment.Count, "Hermes");
                        foreach (OM_Commitments comm in listGuestCommitment)
                        {
                            OM_GoogleCalendarsEvents gCalEv = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(cmp, accountGoogle.WorkerId, comm.CommitmentId, wCal.IdGoogleCalendar);
                            if (gCalEv == null)
                            {
                                //Creo un nuovo evento in Google Calendar
                                ret |= CreateEventGoogle(cmp, service, accountGoogle, gCal, comm, wCal, eaMng, ted);
                            }
                            else
                            {
                                //Controllo se effettuare l'aggiornamento nel ciclo Google--->VS
                                if (gCalEv.IdCalendar != wCal.IdGoogleCalendar)
                                {
                                    //Cancello l'evento sul vecchio calendario
                                    RemoveEventGoogle(cmp, service, gCalEv, ted);
                                    //Creo il commitment sul nuovo calendario
                                    ret |= CreateEventGoogle(cmp, service, accountGoogle, gCal, comm, wCal, eaMng, ted);
                                }
                            }
                        }

                        // Ciclo di invio dei nuovi commitment Ricorrenti a Google Calendar
                        logTrace.WriteLine("listRecurrenceMainGuestCommitment==" + listRecurrenceMainGuestCommitment.Count, "Hermes");
                        foreach (OM_Commitments comm in listRecurrenceMainGuestCommitment)
                        {
                            //Cerco l'evento nella tabella OM_GoogleCalendarsEvents con le chiavi WorkerId e CommitmentId
                            OM_GoogleCalendarsEvents gCalEv = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(cmp, accountGoogle.WorkerId, comm.CommitmentId);//, wCal.IdGoogleCalendar);
                            if (gCalEv == null)
                            {
                                //Il commitment non è sincronizzato, creo un nuovo evento in Google Calendar
                                ret |= CreateRecurrenceEventGoogle(cmp, service, true, accountGoogle, gCal, comm, wCal, eaMng, ted);
                            }
                            else
                            {
                                //Controllo se la ricorrenza è stata generata in VS da una sequanza già esistente (dalla scelta "da questo in poi")
                                if (gCalEv.ParentCommitmentId != comm.RecurrenceId)
                                {
                                    //Rimuovo l'evento dalla serie precedente 
                                    ret |= RemoveEventGoogle(cmp, service, gCalEv, ted);
                                    //Creo la nuova sequenza partendo da questo commitment
                                    ret |= CreateRecurrenceEventGoogle(cmp, service, true, accountGoogle, gCal, comm, wCal, eaMng, ted);
                                }
                                else
                                {
                                    //Controllo se sono state generate eccezioni nella sequenza in VS
                                    CheckRecurringEventExceptionVS(cmp, service, accountGoogle, gCal, gCalEv, wCal, comm, ted);
                                }
                            }
                        }

                        // Ciclo di cancellazione degli eventi in google collegati a commitment non più presenti in VS
                        logTrace.WriteLine("listGoogleCalendarEvents==" + listGoogleCalendarEvents.Count, "Hermes");
                        foreach (OM_GoogleCalendarsEvents gce in listGoogleCalendarEvents)
                        {
                            //Cerco le informazioni dell'evento nella tabella OM_GoogleEvents
                            OM_GoogleEvents ge = OM_GoogleEvents.GetGoogleEvent(cmp, gce.IdEvent);

                            //Controllo se l'evento esiste o se ha le date comprese nella finestra di sincronizzazione 
                            if ((ge == null) || 
                                (ge.EventStart < minLimit) ||
                                (ge.EventStart > maxLimit))
                                continue; 
                            
                            //Cerco il commitmentId nella tabella Commitment per gli eventi Singoli
                            if ((listCommitment != null) &&
                                (listCommitment.Count != 0) &&
                                (listCommitment.Find(x => x.CommitmentId == gce.CommitmentId) != null))
                                continue;

                            //Cerco il commitmentId nella tabella Commitment per gli eventi Ricorrenti                           
                            if ((listRecurrenceCommitment != null) &&
                                (listRecurrenceCommitment.Count != 0) &&
                                (listRecurrenceCommitment.Find(x => x.CommitmentId == gce.CommitmentId) != null))
                                continue;

                            //Cerco il commitmentId nella tabella Commitment per gli eventi Singoli Condivisi
                            if ((listGuestCommitment != null) &&
                                (listGuestCommitment.Count != 0) &&
                                (listGuestCommitment.Find(x => x.CommitmentId == gce.CommitmentId) != null))
                                continue;

                            //Cerco il commitmentId nella tabella Commitment per gli eventi Singoli Condivisi
                            if ((listRecurrenceGuestCommitment != null) &&
                                (listRecurrenceGuestCommitment.Count != 0) &&
                                (listRecurrenceGuestCommitment.Find(x => x.CommitmentId == gce.CommitmentId) != null))
                                continue;

                            //Se il CommitmentId è 0, il record nella OM_GoogleCalendarsEvents è il padre di una sequenza Ricorrente
                            if (gce.CommitmentId == 0)
                            {
                                //Cerco l'evento padre in VS
                                int recurrenceId = OM_Commitments.GetRecurrenceIDFromRecurrenceEvent(cmp, accountGoogle.WorkerId, wCal.IdGoogleCalendar, gce.IdEvent);
                                if (recurrenceId == 0) //Se non trovo nessun figlio in VS elimino il record del padre
                                    RemoveEventVS(cmp, gce, ted);
                                continue;
                            }
                            
                            //Cancello l'evento su Google Calendar
                            RemoveEventGoogle(cmp, service, gce, ted);
                        }

                    }

                    //Google--->VS
                    {
                        //Costruisco la query di richiesta (Calendario, DataMin, DataMax, ShowDeleted)
                        EventsResource.ListRequest calRequest = service.Events.List(wCal.IdGoogleCalendar);
                        calRequest.ShowDeleted = true;
                        //calRequest.SingleEvents = true;
                        calRequest.TimeMin = minLimit;
                        calRequest.TimeMax = maxLimit;

                        logTrace.WriteLine("--- Google--->VS: IdGoogleCalendar=" + wCal.IdGoogleCalendar, "Hermes");

                        String pageToken = null;
                        do 
                        {
                            calRequest.PageToken = pageToken;
                            Events response = calRequest.Execute();
                            List<Event> Eventitems = (List<Event>)response.Items;
                            foreach (Event gooEv in Eventitems)
                            {
                                try
                                {
                                    //Controllo l'oggetto Event che non sia null
                                    if (gooEv == null) 
                                        continue;

                                    //Controllo che nell'evento di Google sia presente l'informazione del creatore
                                    if ((gooEv.Creator == null) ||
                                        (gooEv.Creator.Email == null))
                                        continue;

                                    //Controllo che l'evento abbia impostato sia l'ora di inizio che l'ora di fine (non considero gli eventi di giornata intera)
                                    if ((gooEv.Start == null) || (gooEv.End == null) ||
                                        (gooEv.Start.DateTime == null) || (gooEv.End.DateTime == null))
                                        continue;

                                    //Cerco il record nella tabella OM_GoogleCalendarsEvents
                                    OM_GoogleCalendarsEvents gCalEv = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(cmp, accountGoogle.WorkerId, gooEv.Id, wCal.IdGoogleCalendar);

                                    //Controllo che l'evento abbia impostato l'ora di inizio e l'ora di fine nello stesso giorno
                                    if ((gooEv.Start.DateTime.Value.Date != gooEv.End.DateTime.Value.Date) &&
                                        ((gooEv.Status != null) && (gooEv.Status != "cancelled")))
                                    {
                                        /*Se l'evento ha data inizio e data fine in giorni differenti l'evento in VS viene cancellato e l'evento in Google non viene sincronizzato
                                         *nel caso in cui la modifica riguardi il propietario o il cretore dell'evento, in caso contrario viene eseguito il rollback della modifica */
                                        if (gCalEv == null)
                                            continue;
                                        
                                        if ((gooEv.RecurringEventId == null) &&
                                             (gooEv.Recurrence != null))
                                        {
                                            //Rimozione della ricorrenza
                                            logTrace.WriteLine("DeleteRecurringEventVS==" + gooEv.Id, "Hermes");
                                            ret |= DeleteRecurringEventVS(cmp, service, gooEv, gCalEv, accountGoogle, gCal, wCal, eaMng, ted);
                                        }
                                        else if ((gooEv.RecurringEventId != null) &&
                                            (gooEv.Recurrence == null))
                                        {
                                            //Processo per verificare se rimuovere l'evento ricorrente o eseguire il rollback
                                            logTrace.WriteLine("RemovingRecurringEventProcess==" + gooEv.Id, "Hermes");
                                            RemovingRecurringEventProcess(cmp, service, gooEv, wCal, accountGoogle, gCal, gCalEv, eaMng, ted);
                                        }
                                        else
                                        {
                                            //Processo per verificare se rimuovere l'evento o eseguire il rollback
                                            logTrace.WriteLine("RemovingEventProcess==" + gooEv.Id, "Hermes");
                                            RemovingEventProcess(cmp, service, gooEv, true, wCal, accountGoogle, gCal, gCalEv, eaMng, ted);
                                        }

                                        continue;
                                    }

                                    if ((gCalEv == null) &&
                                        ((gooEv.Status != null) && (gooEv.Status != "cancelled")) &&
                                        (gooEv.RecurringEventId == null) &&
                                        (gooEv.Recurrence == null))
                                    {
                                        //Nuovo evento Singolo

                                        //Creo l'evento in VS
                                        logTrace.WriteLine("CreateEventVS==" + gooEv.Id, "Hermes");
                                        ret |= CreateEventVS(cmp, gooEv, gCal, accountGoogle, wCal, eaMng, ted);
                                    }
                                    else if ((gCalEv == null) &&
                                             ((gooEv.Status != null) && (gooEv.Status != "cancelled")) &&
                                             //(gooEv.RecurringEventId == null) &&
                                             (gooEv.Recurrence != null))
                                    {
                                        //TODO Nuovo evento ricorrente
                                        logTrace.WriteLine("CreateRecurringEventVS==" + gooEv.Id, "Hermes");
                                        CreateRecurringEventVS(cmp, service, gooEv, gCal, accountGoogle, wCal, eaMng, ted);
                                    }
                                    else if ((gCalEv == null) &&
                                             ((gooEv.Status != null) && (gooEv.Status != "cancelled")) &&
                                             (gooEv.RecurringEventId != null) &&
                                             (gooEv.Recurrence == null))
                                    {
                                        //Nuovo evento appartenente ad una ricorrenza
                                        //Questo caso è gestito nella creazione della ricorrenza corrispondente
                                    }
                                    else if ((gCalEv != null) &&
                                             ((gooEv.Status != null) && (gooEv.Status != "cancelled")) &&
                                             (gooEv.RecurringEventId == null) &&
                                             (gooEv.Recurrence != null))
                                    {
                                        //Update della ricorrenza
                                        logTrace.WriteLine("MatchVSGoogleRecurringEvent==" + gooEv.Id, "Hermes");
                                        ret |= MatchVSGoogleRecurringEvent(cmp, service, gooEv, gCalEv, accountGoogle, gCal, wCal, eaMng, ted);
                                    }
                                    else if ((gCalEv != null) &&
                                             ((gooEv.Status != null) && (gooEv.Status == "cancelled")) &&
                                             (gooEv.RecurringEventId == null) &&
                                             (gooEv.Recurrence != null))
                                    {
                                        //Rimozione della ricorrenza
                                        logTrace.WriteLine("DeleteRecurringEventVS==" + gooEv.Id, "Hermes");
                                        ret |= DeleteRecurringEventVS(cmp, service, gooEv, gCalEv, accountGoogle, gCal, wCal, eaMng, ted);
                                    }
                                    else if ((gCalEv != null) &&
                                             (gooEv.Status != null) && (gooEv.Status != "cancelled"))
                                    {
                                        //Evento già sincronizzato, verifico le date di modifica per aggiornare i campi 

                                        //Verifico se ci sono state modifiche e aggiorno (l'evento avrà i dati del più recente tra Commitment e Google Event)  
                                        logTrace.WriteLine("MatchVSGoogleEvents==" + gooEv.Id, "Hermes");
                                        ret |= MatchVSGoogleEvents(cmp, service, gooEv, gCal, gCalEv, accountGoogle, wCal, eaMng, ted);

                                        //Nel caso di evento ricorrenti imposto l'evento come eccezione della sequenza
                                        ret |= OM_Commitments.CheckRecurrenceException(cmp, gCalEv, ted);
                                    }
                                    else if ((gCalEv != null) &&
                                             (gooEv.Status != null) && (gooEv.Status == "cancelled") &&
                                             (gooEv.RecurringEventId != null) &&
                                             (gooEv.Recurrence == null))

                                    {
                                        //Evento eliminato (Eccezione di una sequenza ricorrente) 

                                        //Processo per valutare se eliminare il record in VS oppure eseguire il rollback dell'evento in Google
                                        logTrace.WriteLine("RemovingRecurringEventProcess==" + gooEv.Id, "Hermes");
                                        RemovingRecurringEventProcess(cmp, service, gooEv, wCal, accountGoogle, gCal, gCalEv, eaMng, ted);
                                    }
                                    else if ((gCalEv != null) &&
                                             (gooEv.Status != null) && (gooEv.Status == "cancelled"))
                                    {
                                        //Evento eliminato in Google 

                                        //Processo per valutare se eliminare il record in VS oppure eseguire il rollback dell'evento in Google
                                        logTrace.WriteLine("RemovingEventProcess==" + gooEv.Id, "Hermes");
                                        RemovingEventProcess(cmp, service, gooEv, false, wCal, accountGoogle, gCal, gCalEv, eaMng, ted);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    string a = ex.Message;
                                    if (ted != null)
                                        ted(ex);
                                    //ret = false;
                                }
                            }

                            pageToken = response.NextPageToken;
                        } while (pageToken != null);
                    }
                }
            }
            catch (Exception ex)
            {
                string a = ex.Message;
                string b = a;
                a = b;
                //ret = false;
                //txtArea.Text = ex.Message + " - " + ex.InnerException;
            }

            return ret;
        }

        //-------------------------------------------------------------------------------
        //Elimino i record di collegamento per il calendario tra VS e Google Calendar
        public static bool RemoveCalendarSynchronizationVS(TbSenderDatabaseInfo company,
            string calendarID, int workerID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            //Recupero il record della tabella OM_WorkersCalendars
            OM_WorkersCalendars wCal = OM_WorkersCalendars.GetWorkersCalendar(company, workerID, calendarID);
            if (wCal == null)
                return false;

            //Recupero la lista di eventi sincronizzati sul calendario di VS
            List<OM_GoogleCalendarsEvents> listGCE = OM_GoogleCalendarsEvents.GetGoogleCalendarsEvents(company, workerID, calendarID);

            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        // Ciclo su tutti gli eventi sincronizzati
                        foreach (OM_GoogleCalendarsEvents gce in listGCE)
                        {
                            //Elimino i record di collegamento tra VS e Google Calendar
                            RemoveSynchronizationEventVS(db, wCal.IdGoogleCalendar, gce.IdEvent, gce.CommitmentId, gce.WorkerId, excLogger);
                        }

                        //Elimino il record nella tabella OM_GoogleCalendars
                        OM_GoogleCalendars.RemoveGoogleCalendar(db, workerID, calendarID, excLogger);

                        //Scollego il record nella tabella OM_WorkersCalendars
                        OM_WorkersCalendars.RemoveSynchronizationToWorkersCalendar(db, workerID, calendarID);

                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex)
                {
                    ret = false;
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Processo di valutazione per l'eliminazione del commitment in VS o per eseguire il rollback dell'evento rimosso in Google Calendar
        public static bool RemovingEventProcess(TbSenderDatabaseInfo company, CalendarService service,
            Event gooEv, bool deleteGoogle,
            OM_WorkersCalendars wCal,
            OM_GoogleAccounts accountGoogle, OM_GoogleCalendars gCal, OM_GoogleCalendarsEvents gCalEv,
            IEasyAttachmentManager eaMng, Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = false;

            OM_CommitmentsWorkers commwor = OM_CommitmentsWorkers.GetCommitmentsWorkers(company, gCalEv.CommitmentId, gCalEv.WorkerId);

            //Recupero il corrispondenete Commitment in VS 
            OM_Commitments comm = OM_Commitments.GetCommitment(company, gCalEv.CommitmentId);
            bool isCreator = (comm.TBCreatedID == accountGoogle.WorkerId);

            if ((commwor.BoolIsOwner) || (isCreator))
            {
                //Se sono il propietario elimino l'evento o il creatore in VS

                //Elimito il corrispondente Commitment in VS
                ret |= RemoveEventVS(company, gCalEv, excLogger);
            }
            else
            {
                //Se non sono il propietario devo ripristinare l'evento originale presente in VS
                
                //Forzo l'eliminazione dell'evento in Google Calendar
                if (deleteGoogle)
                    RemoveEventGoogle(company, service, gCalEv, excLogger);

                //Elimino i record di sincronizzazione tra VS e Google Calendar
                ret |= RemoveSynchronizationEventVS(company, gCalEv.IdCalendar, gCalEv.IdEvent, gCalEv.CommitmentId, gCalEv.WorkerId, excLogger);

                //Rigenero l'evento in Google Calendar
                ret |= CreateEventGoogle(company, service, accountGoogle, gCal, comm, wCal, eaMng, excLogger);
            }
            
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Processo di valutazione per l'eliminazione del commitment ricorrente in VS o per eseguire il rollback dell'evento rimosso in Google Calendar
        public static bool RemovingRecurringEventProcess(TbSenderDatabaseInfo company, CalendarService service,
            Event gooEv,
            OM_WorkersCalendars wCal,
            OM_GoogleAccounts accountGoogle, OM_GoogleCalendars gCal, OM_GoogleCalendarsEvents gCalEv,
            IEasyAttachmentManager eaMng, Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = false;

            //Recupero il RecurrenceId
            string recurringId = gooEv.RecurringEventId;
            OM_Commitments comm = OM_Commitments.GetCommitment(company, gCalEv.CommitmentId);
            bool isCreator = (comm.TBCreatedID == accountGoogle.WorkerId);

            OM_CommitmentsWorkers commwor = OM_CommitmentsWorkers.GetCommitmentsWorkers(company, gCalEv.CommitmentId, gCalEv.WorkerId);
            if (commwor == null)
                return ret;

            //Controllo se sono il propietario del commitment o il creatore
            if ((commwor.BoolIsOwner) || (isCreator))
            {
                //Se sono il propietario elimino l'evento in VS

                //Elimito il corrispondente Commitment in VS
                ret |= RemoveEventVS(company, gCalEv, excLogger);

                //Riallineo la sequenza degli eventi ricorrenti in VS
                ret |= CheckRecurringEventVS(company, recurringId, accountGoogle, wCal, excLogger);
            }
            else
            {
                //Se non sono il propietario devo ripristinare l'evento originale presente in VS

                //Recupero l'evento padre
                OM_GoogleCalendarsEvents gCalEvMain = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(company, gCalEv.WorkerId, 0, recurringId, gCalEv.IdCalendar);

                if (gCalEvMain == null)
                    return ret;
                //Recupero il primo Commitment della sequenza non definito come eccezione
                OM_Commitments commSequence = OM_Commitments.GetNonExceptionCommitmentFromRecurrenceId(company, accountGoogle.WorkerId, wCal.SubId, gCalEv.ParentCommitmentId);

                //Elimino l'evento padre in Google Calendar
                ret |= RemoveEventGoogle(company, service, gCalEvMain, excLogger);

                //Elimino i record di sincronizzazione tra VS e Google Calendar
                ret |= RemoveSynchronizationRecurringEventVS(company, gCalEvMain, accountGoogle, wCal, excLogger);

                if (commSequence == null)
                    return ret;
                //Rigenero l'evento in Google Calendar
                ret |= CreateRecurrenceEventGoogle(company, service, true, accountGoogle, gCal, commSequence, wCal, eaMng, excLogger);
            }

            return ret;
        }

        #region Single Events
        //-------------------------------------------------------------------------------
        //Creazione dell'evento in VS
        public static bool CreateEventVS(TbSenderDatabaseInfo company, Event gooEv,
            OM_GoogleCalendars gCal, OM_GoogleAccounts accountGoogle,
            OM_WorkersCalendars wCal, 
            IEasyAttachmentManager eaMng, Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //Cerco l'evento nella tabella OM_GoogleEvents (potrebbe essere già presente nel calendario di un'altro worker)
                        OM_GoogleEvents gEv = OM_GoogleEvents.GetGoogleEvent(db, gooEv.Id);
                        if (gEv == null)
                        {
                            //Creo il record nella tabella OM_GoogleEvents
                            gEv = OM_GoogleEvents.CreateItem(db, accountGoogle.WorkerId, gooEv, wCal.IdGoogleCalendar, gCal.TimeZone);
                            db.OM_GoogleEvents.Add(gEv);
                        }

                        //Prendo il nuovo CommitmentId
                        int nextCommitmentId = ConnectionHelper.GetCommitmentsNextKey(company, 1, (ILockerClient)eaMng);
                        //Creo il record nella tabella OM_Commitments
                        OM_Commitments comm = OM_Commitments.CreateItem(nextCommitmentId, accountGoogle.WorkerId, wCal, accountGoogle, gEv);
                        db.OM_Commitments.Add(comm);

                        //Creo il record nella tabella OM_GoogleCalendarsEvents
                        OM_GoogleCalendarsEvents gCalEv = OM_GoogleCalendarsEvents.CreateItem(accountGoogle.WorkerId, comm, gooEv, wCal.IdGoogleCalendar);
                        db.OM_GoogleCalendarsEvents.Add(gCalEv);

                        //Creo il record nella tabella OM_CommitmentsWorkers
                        OM_CommitmentsWorkers commwor = OM_CommitmentsWorkers.CreateItem(nextCommitmentId, accountGoogle.WorkerId, wCal.SubId, true);
                        db.OM_CommitmentsWorkers.Add(commwor);

                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }

            return ret;
        }

        //-------------------------------------------------------------------------------
        //Aggiorno l'evento in VS
        public static bool UpdateEventVS(TbSenderDatabaseInfo company, Event gooEv,
            OM_GoogleEvents gEv, OM_GoogleCalendarsEvents gCalEv, OM_GoogleCalendars gCal, OM_GoogleAccounts accountGoogle,
            OM_Commitments comm, OM_WorkersCalendars wCal,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //Aggiorno le Facility collegate al Commitment
                        bool updateFacitity = UpdateFacility(db, gooEv, comm, accountGoogle, excLogger);

                        //Creo il record nella tabella OM_GoogleEvents
                        gEv = OM_GoogleEvents.UpdateItem(db, gCalEv.WorkerId, gooEv, gCal.TimeZone, excLogger);

                        //Creo il record nella tabella OM_Commitments
                        comm = OM_Commitments.UpdateItem(db, gCalEv.WorkerId, gCalEv.CommitmentId, wCal, gEv, excLogger);

                        //Se il calendario è cambiato aggiorno il record nella tabella OM_CommitmentsWorkers
                        bool bUpdateCommitmentsWorker = (comm.CalendarSubId != wCal.SubId);
                        if (bUpdateCommitmentsWorker)
                            OM_CommitmentsWorkers.UpdateItem(db, gCalEv.WorkerId, gCalEv.CommitmentId, wCal, gEv, excLogger);

                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex)
                {
                    ret = false;
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Elimina il Commitment in VS (attraverso TbSenderDatabaseInfo)
        public static bool RemoveEventVS(TbSenderDatabaseInfo company, OM_GoogleCalendarsEvents gCalEv,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //Elimino il commitment in VS
                        ret = RemoveEventVS(db, gCalEv, excLogger);

                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex)
                {
                    ret = false;
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Elimina il Commitment in VS (attraverso MZP_CompanyEntities)
        public static bool RemoveEventVS(MZP_CompanyEntities db, OM_GoogleCalendarsEvents gCalEv,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;

            try
            {
                string calID = gCalEv.IdCalendar;
                string evID = gCalEv.IdEvent;
                int commID = gCalEv.CommitmentId;
                int workerID = gCalEv.WorkerId;

                //Elimino i record di collegamento tra VS e Google Calendar
                RemoveSynchronizationEventVS(db, calID, evID, commID, workerID, excLogger);

                //Recupero il record della tabella OM_CommitmentsWorkers
                OM_CommitmentsWorkers commwor = OM_CommitmentsWorkers.GetCommitmentsWorkers(db, commID, workerID);
                if (commwor == null)
                    return false;

                //Recupero il record della tabella OM_Commitments
                OM_Commitments comm = OM_Commitments.GetCommitment(db, commID);
                if (comm == null)
                    return false;

                //Controllo se è il propietario o il creatore del commitment 
                bool isOwner = commwor.BoolIsOwner;
                bool isCreator = (comm.TBCreatedID == workerID);
                if ((isOwner) || (isCreator))
                {
                    //Rimovo i record nella tabella OM_FacilitiesDetails
                    OM_FacilitiesDetails.RemoveFacilitiesDetails(db, commID, excLogger);

                    //Rimovo i record nella tabella OM_CommitmentsWorkers
                    OM_CommitmentsWorkers.RemoveAllCommitmentWorker(db, commID, excLogger);

                    //Rimovo i record nella tabella OM_Commitments
                    OM_Commitments.RemoveCommitment(db, commID, excLogger);
                }
                //Rimuovo il record nella tabella OM_CommitmentsWorkers
                OM_CommitmentsWorkers.RemoveCommitmentWorker(db, commID, workerID, excLogger);
            }
            catch (Exception ex)
            {
                ret = false;
                string a = ex.Message;
                if (excLogger != null)
                    excLogger(ex);
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Elimino i record di collegamento tra VS e Google Calendar (attraverso TbSenderDatabaseInfo)
        public static bool RemoveSynchronizationEventVS(TbSenderDatabaseInfo company,
            string calendarID, string eventID, int commID, int workerID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //Elimino i record di collegamento tra VS e Google Calendar
                        RemoveSynchronizationEventVS(db, calendarID, eventID, commID, workerID, excLogger);
                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex)
                {
                    ret = false;
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Elimino i record di collegamento tra VS e Google Calendar (attraverso MZP_CompanyEntities)
        public static bool RemoveSynchronizationEventVS(MZP_CompanyEntities db,
            string calendarID, string eventID, int commID, int workerID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            try
            {
                //Elimino il record nella tabella OM_GoogleCalendarsEvents
                OM_GoogleCalendarsEvents.RemoveGoogleCalendarEvent(db, workerID, commID, calendarID, eventID, excLogger);
                //Controllo se sono presenti altri record nella tabella OM_GoogleCalendarsEvents
                OM_GoogleCalendarsEvents otherGCE = OM_GoogleCalendarsEvents.GetOtherGoogleCalendarEvent(db, eventID, calendarID, workerID, commID);
                if (otherGCE == null)
                {
                    //Elimino il record nella tabella OM_GoogleEvents
                    OM_GoogleEvents.RemoveGoogleEvent(db, eventID, excLogger);
                }
            }
            catch (Exception ex)
            {
                ret = false;
                string a = ex.Message;
                if (excLogger != null)
                    excLogger(ex);
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Creo l'evento in Google
        public static bool CreateEventGoogle(TbSenderDatabaseInfo company, CalendarService service,
            OM_GoogleAccounts accountGoogle, OM_GoogleCalendars gCal,
            OM_Commitments comm, OM_WorkersCalendars wCal, 
            IEasyAttachmentManager eaMng, Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //Lancio la creazione dell'evento in Google
                        Event gooEvNew = CreateEventGoogleService(service, gCal.TimeZone, wCal.IdGoogleCalendar, comm, excLogger);

                        //Controllo se l'evento è valido
                        if (gooEvNew != null)
                        {
                            //Creo il record nella tabella OM_GoogleEvents
                            OM_GoogleEvents gEv = OM_GoogleEvents.CreateItem(db, accountGoogle.WorkerId, gooEvNew, wCal.IdGoogleCalendar, gCal.TimeZone);
                            db.OM_GoogleEvents.Add(gEv);

                            //Controllo se sono il propietario 
                            bool bOwner = (comm.WorkerId == accountGoogle.WorkerId);

                            //Creo il record nella tabella OM_GoogleCalendarsEvents
                            OM_GoogleCalendarsEvents gCalEv = OM_GoogleCalendarsEvents.CreateItem(accountGoogle.WorkerId, comm, gooEvNew, wCal.IdGoogleCalendar, bOwner);
                            db.OM_GoogleCalendarsEvents.Add(gCalEv);

                            //Aggiorno l'ora di modifica del record nella tabella OM_Commitments
                            DateTime updateDateTime = (gooEvNew.Updated != null) ? gooEvNew.Updated.Value : DateTime.Now;
                            OM_Commitments.UpdateItemUpdatedDateTime(db, accountGoogle.WorkerId, comm.CommitmentId, updateDateTime, excLogger);

                            db.SaveChanges();
                            ts.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ret = false;
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
                return ret;
            }

        }
        
        //-------------------------------------------------------------------------------
        //Aggiorno l'evento su Google Calendar (attraverso TbSenderDatabaseInfo)
        public static bool UpdateEventGoogle(TbSenderDatabaseInfo company, CalendarService service, Event gooEv,
            OM_GoogleEvents gEv, OM_GoogleCalendars gCal, OM_GoogleCalendarsEvents gCalEv,
            OM_Commitments comm, OM_WorkersCalendars wCal,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //Aggiorno l'evento su Google Calendar
                        UpdateEventGoogle(db, service, gooEv, gEv, gCal, gCalEv, comm, wCal, excLogger);

                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex)
                {
                    ret = false;
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Aggiorno l'evento su Google (attraverso MZP_CompanyEntities)
        public static bool UpdateEventGoogle(MZP_CompanyEntities db, CalendarService service, Event gooEv,
            OM_GoogleEvents gEv, OM_GoogleCalendars gCal, OM_GoogleCalendarsEvents gCalEv,
            OM_Commitments comm, OM_WorkersCalendars wCal,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            try 
            { 
                //Aggiorno il record nella tabella OM_GoogleEvents attraverso il Commitment modificato 
                gEv = OM_GoogleEvents.UpdateItem(db, comm, gCalEv, gCal.TimeZone, excLogger);
                
                //Lancio il servizio per aggiornare l'evento su Google Calendar
                Event updatedEvent = UpdateEventGoogleService(service, gooEv, gEv, gCal, wCal.IdGoogleCalendar, excLogger);
                if ((updatedEvent != null) && (updatedEvent.Updated != null) && (updatedEvent.Updated.HasValue))
                {
                    //Aggiorno la data di modifica delle tabelle OM_GoogleEvents e OM_Commitments
                    DateTime dtUpdate = updatedEvent.Updated.Value;
                    OM_GoogleEvents.UpdateItemUpdatedDateTime(db, updatedEvent.Id, dtUpdate, excLogger);
                    comm = OM_Commitments.UpdateItemUpdatedDateTime(db, gEv.WorkerId, gCalEv.CommitmentId, dtUpdate, excLogger);
                }
            }
            catch (Exception ex)
            {
                ret = false;
                string a = ex.Message;
                if (excLogger != null)
                    excLogger(ex);
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Elimino l'evento su Google Calendar
        public static bool RemoveEventGoogle(TbSenderDatabaseInfo company, CalendarService service, OM_GoogleCalendarsEvents gCalEv,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        string calID = gCalEv.IdCalendar;
                        string evID = gCalEv.IdEvent;
                        int commID = gCalEv.CommitmentId;
                        int workerID = gCalEv.WorkerId;

                        try
                        {
                            //Istruzione per elimiare il record su Google Calendar
                            var gooRet = service.Events.Delete(calID, evID).Execute();
                        }
                        catch (Exception ex)
                        {
                            ret = false;
                            string a = ex.Message;
                            if (excLogger != null)
                                excLogger(ex);
                        }

                        //Elimino i record di collegamento tra VS e Google Calendar
                        RemoveSynchronizationEventVS(db, calID, evID, commID, workerID, excLogger);

                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex)
                {
                    ret = false;
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
                return ret;
            }
        }

        //-------------------------------------------------------------------------------
        //Impostazioni per creare l'evento in Google Calendar
        public static Event CreateEventGoogleService(CalendarService service, string gooCalTimeZone, string IdGoogleCalendar, OM_Commitments comm,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            Event ret = null;
            try
            {
                //Imposto i campi da trasferire a Google nel nuovo evento
                Event eventToSynchronize = new Event()
                {
                    Summary = comm.Subject,
                    Description = comm.Description,
                    Location = comm.Location,
                    Start = new EventDateTime() 
                    {
                        DateTime = comm.CommitmentDate.Add(comm.StartTime.TimeOfDay),
                        TimeZone = gooCalTimeZone
                    },
                    End = new EventDateTime() 
                    {
                        DateTime = comm.CommitmentDate.Add(comm.EndTime.TimeOfDay),
                        TimeZone = gooCalTimeZone
                    },
                };

                //Istruzioni per creare l'evento in Google Calendar
                ret = service.Events.Insert(eventToSynchronize, IdGoogleCalendar).Execute();
            }
            catch(Exception ex)
            {
                string a = ex.Message;
                if (excLogger != null)
                    excLogger(ex);
            }

            return ret;
        }

        //-------------------------------------------------------------------------------
        //Impostazioni per modificare l'evento in Google Calendar
        public static Event UpdateEventGoogleService(CalendarService service, Event gooEv, OM_GoogleEvents gEv, OM_GoogleCalendars gCal, string IdGoogleCalendar,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            Event thisevent = null;

            try
            {
                //Imposto i campi da trasferire a Google nell'evento modificato
                gooEv.Summary = gEv.Summary;
                gooEv.Location = gEv.Location;
                gooEv.Description = gEv.Description;
                gooEv.Start = new EventDateTime()
                {
                    DateTime = gEv.EventStart,
                    TimeZone = gCal.TimeZone
                };
                gooEv.End = new EventDateTime()
                {
                    DateTime = gEv.EventEnd,
                    TimeZone = gCal.TimeZone
                };
                gooEv.Updated = gEv.Updated;

                //Istruzioni per modificare l'evento in Google Calendar
                thisevent = service.Events.Update(gooEv, IdGoogleCalendar, gEv.Id).Execute();
            }
            catch (Exception ex)
            {
                string a = ex.Message;
                if (excLogger != null)
                    excLogger(ex);
            }

            return thisevent;
        }

        //-------------------------------------------------------------------------------
        //Confronto tra evento di Google Calendar e Commitment di VS per aggiornamento
        public static bool MatchVSGoogleEvents(TbSenderDatabaseInfo company, CalendarService service, Event gooEv,
            OM_GoogleCalendars gCal, OM_GoogleCalendarsEvents gCalEv, OM_GoogleAccounts accountGoogle,
            OM_WorkersCalendars wCal,
            IEasyAttachmentManager eaMng, Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = false;

            //Recupero il record dell'evento nella tabella OM_GoogleEvents
            OM_GoogleEvents gEv = OM_GoogleEvents.GetGoogleEvent(company, gCalEv.IdEvent);

            //Recupero il record dell'evento nella tabella OM_Commitments
            OM_Commitments comm = OM_Commitments.GetCommitment(company, gCalEv.WorkerId, wCal.SubId, gCalEv.CommitmentId);
            
            //Recupero il record dell'evento nella tabella OM_CommitmentsWorkers
            OM_CommitmentsWorkers commwor = OM_CommitmentsWorkers.GetCommitmentsWorkers(company, gCalEv.CommitmentId, gCalEv.WorkerId);

            //Controllo che tutti i record precedenti esistano
            if ((gEv == null) || (comm == null) || (gooEv.Updated == null) || (commwor == null))
                return ret;

            //Controllo se è il propietario dell'evento
            bool isOwner = commwor.BoolIsOwner;

            //Controllo se è il creatore dell'evento
            bool isCreator = (comm.TBCreatedID == accountGoogle.WorkerId);

            //Calcolo la data di modifica dell'evento del Google Calendar 
            DateTime aGoogleLastUpdate = new DateTime(
                gooEv.Updated.Value.Year,
                gooEv.Updated.Value.Month,
                gooEv.Updated.Value.Day,
                gooEv.Updated.Value.Hour,
                gooEv.Updated.Value.Minute,
                gooEv.Updated.Value.Second
                );

            //Calcolo la data di modifica dell'evento nellla tabella OM_GoogleEvents 
            DateTime aVSLastUpdate = new DateTime(
                gEv.Updated.Year,
                gEv.Updated.Month,
                gEv.Updated.Day,
                gEv.Updated.Hour,
                gEv.Updated.Minute,
                gEv.Updated.Second
                );

            //Calcolo la data di modifica dell'evento nellla tabella OM_Commitments 
            DateTime aVSCommitmentLastUpdate = new DateTime(
                comm.TBModified.Year,
                comm.TBModified.Month,
                comm.TBModified.Day,
                comm.TBModified.Hour,
                comm.TBModified.Minute,
                comm.TBModified.Second
                );

            //Controllo se gli eventi sono stati modificati in momenti diversi
            if (aGoogleLastUpdate < aVSCommitmentLastUpdate)
            {
                //Commitment più recente di Google Calendar

                //Aggiorno l'evento in Google Calendar
                UpdateEventGoogle(company, service, gooEv, gEv, gCal, gCalEv, comm, wCal, excLogger);
            }
            else if ((aGoogleLastUpdate > aVSCommitmentLastUpdate) && (aGoogleLastUpdate >= aVSLastUpdate))
            {
                //Evento di Google Calendar più recente del Commitment e del record nella tabella OM_GoogleEvents (controllo in caso di record condiviso)

                //Controllo se è possibile modificare l'evento in VS in base alle Facilities ad esso collegate
                bool okFacility = CheckFacility(company, gooEv, comm, true, excLogger);

                //Controllo se sono il propietario e la modifica delle Facilities è consentita
                if ((isOwner || isCreator) && (okFacility))
                {
                    //Aggiorno l'evento in VS
                    ret |= UpdateEventVS(company, gooEv, gEv, gCalEv, gCal, accountGoogle, comm, wCal, excLogger);
                }
                else
                {
                    //Riporto l'evento di Google Calendar alle impostazioni precedenti alla modifica attraverso i valori del Commitment
                    ret |= UpdateEventGoogle(company, service, gooEv, gEv, gCal, gCalEv, comm, wCal, excLogger);
                }
            }

            return ret;
        }

        #endregion

        #region Recurring Events
        //-------------------------------------------------------------------------------
        //Creazione dell'evento ricorrente in VS
        public static bool CreateRecurringEventVS(TbSenderDatabaseInfo company, CalendarService service, Event gooEv,
            OM_GoogleCalendars gCal, OM_GoogleAccounts accountGoogle,
            OM_WorkersCalendars wCal,
            IEasyAttachmentManager eaMng, Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;

            //Ricavo la data di inizio dell'evento
            DateTime startDateTime = gooEv.Start.DateTime.Value;
            DateTime lastEventDateTime = gooEv.Start.DateTime.Value;

            //Recupero tutti gli eventi in Google Calendar che appartengono alla sequenza
            Events instances = service.Events.Instances(gCal.Id, gooEv.Id).Execute();
            int recurrenceId = 0;

            //Recupero le informazioni di ricorrenza
            IList<String> recurrence = gooEv.Recurrence;
            string strRecurrence = OM_Commitments.GetRecurrenceString(recurrence);
            DateTime RecurrenceEndDate = OM_Commitments.GetRecurrenceEndDate(recurrence);
            int RecurrenceOccurrences = instances.Items.Count;
            OM_Commitments.CommitmentRecurrenceType RecurrenceType = OM_Commitments.GetRecurrenceType(recurrence);
            int RecurrenceEvery = OM_Commitments.GetRecurrenceEvery(recurrence);
            int WeekDays = OM_Commitments.GetRecurrenceWeekday(recurrence);
            int MonthlyWeek = OM_Commitments.GetRecurrenceMonthlyWeek(recurrence);
            OM_Commitments.CommitmentRecurrenceEnd RecurrenceEnd = OM_Commitments.GetRecurrenceEndType(recurrence);
            bool RecurrenceException = false;

            //Controllo che esista almeno un evento nella sequenza  
            if (RecurrenceOccurrences == 0)
            {
                service.Events.Delete(gCal.Id, gooEv.Id).Execute();
                return false;
            }

            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        int nextCommitmentId = ConnectionHelper.GetCommitmentsNextKey(company, RecurrenceOccurrences, (ILockerClient)eaMng);
                        int newCommId = nextCommitmentId;
                        int geNewId = 0;
                        int RecSubId = 1;

                        //Cerco il record dell'evento padre nella tabella OM_GoogleEvents (non ha corrispondenze nelle tabelle OM_Commitments e OM_CommitmentsWorkers)
                        OM_GoogleEvents gEv = OM_GoogleEvents.GetGoogleEvent(db, gooEv.Id);
                        if (gEv == null)
                        {
                            //Creo il record dell'evento padre nella tabella OM_GoogleEvents
                            gEv = OM_GoogleEvents.CreateItem(db, accountGoogle.WorkerId, gooEv, wCal.IdGoogleCalendar, gCal.TimeZone);
                            geNewId = gEv.GoogleEventId;
                            gEv.Recurrence = strRecurrence;
                            db.OM_GoogleEvents.Add(gEv);

                            //Creo il record dell'evento padre nella tabella OM_GoogleCalendarsEvents
                            OM_GoogleCalendarsEvents gCalEv = OM_GoogleCalendarsEvents.CreateItem(accountGoogle.WorkerId, 0, nextCommitmentId, gooEv, wCal.IdGoogleCalendar);
                            db.OM_GoogleCalendarsEvents.Add(gCalEv);

                            geNewId++;
                        }

                        //Ciclo per tutti gli eventi della ricorrenza
                        foreach (Event childEvent in instances.Items)
                        {
                            //Controllo che l'evento esista e non sia stato cancellato
                            if ((childEvent == null) ||
                                (childEvent.Status == null) ||
                                (childEvent.Status.ToString() == "cancelled"))
                            {
                                RecSubId++;
                                newCommId++;
                                continue;
                            }

                            //Cerco il record dell'occorrenza nella tabella OM_GoogleEvents
                            OM_GoogleEvents gEvChild = OM_GoogleEvents.GetGoogleEvent(db, childEvent.Id);
                            if (gEvChild == null)
                            {
                                //Creo il record dell'occorrenza nella tabella OM_GoogleEvents
                                gEvChild = OM_GoogleEvents.CreateItem(db, accountGoogle.WorkerId, childEvent, wCal.IdGoogleCalendar, gCal.TimeZone);
                                gEvChild.GoogleEventId = geNewId++;
                                gEvChild.Recurrence = strRecurrence;
                                db.OM_GoogleEvents.Add(gEvChild);
                            }

                            //Creo il record dell'occorrenza nella tabella OM_Commitments
                            OM_Commitments comm = OM_Commitments.CreateItem(newCommId, accountGoogle.WorkerId, wCal, accountGoogle, gEvChild);
                            //Imposto i campi della ricorrenza
                            comm.OriginalParentStartDate = startDateTime;
                            comm.BoolIsRecurring = true;
                            comm.RecurrenceId = nextCommitmentId;
                            comm.RecurrenceOldRecurrenceId = nextCommitmentId;
                            comm.RecurrenceSubId = RecSubId;
                            comm.RecurrenceEndDate = RecurrenceEndDate;
                            comm.RecurrenceOccurrences = RecurrenceOccurrences;
                            comm.EnumCommitmentRecurrenceType = RecurrenceType;
                            comm.BoolRecurrenceException = RecurrenceException;
                            comm.RecurrenceEvery = RecurrenceEvery;
                            comm.RecurrenceWeekdays = WeekDays;
                            comm.EnumCommitmentRecurrenceEnd = RecurrenceEnd;

                            bool bMonthlyDay = false;
                            bool bMonthlyWeek = false;

                            if (RecurrenceType == OM_Commitments.CommitmentRecurrenceType.Month)
                            {
                                if (MonthlyWeek > 0)
                                    bMonthlyWeek = true;

                                bMonthlyDay = !bMonthlyWeek;
                            }

                            comm.RecurrenceMonthlyWeeks = MonthlyWeek;
                            comm.BoolIsRecurrenceMonthlyDay = bMonthlyDay;
                            comm.BoolIsRecurrenceMonthlyWeek = bMonthlyWeek;
                            db.OM_Commitments.Add(comm);

                            if (lastEventDateTime < comm.CommitmentDate)
                                lastEventDateTime = comm.CommitmentDate;

                            //Creo il record dell'occorrenza nella tabella OM_GoogleCalendarsEvents
                            OM_GoogleCalendarsEvents gCalEv = OM_GoogleCalendarsEvents.CreateItem(accountGoogle.WorkerId, comm, childEvent, wCal.IdGoogleCalendar);
                            db.OM_GoogleCalendarsEvents.Add(gCalEv);

                            //Creo il record dell'occorrenza nella tabella OM_CommitmentsWorkers
                            OM_CommitmentsWorkers commwor = OM_CommitmentsWorkers.CreateItem(newCommId, accountGoogle.WorkerId, wCal.SubId, true);
                            db.OM_CommitmentsWorkers.Add(commwor);

                            RecSubId++;
                            newCommId++;
                        }

                        recurrenceId = nextCommitmentId;

                        db.SaveChanges();
                        ts.Complete();
                        ret = true;
                    }
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }

            if (recurrenceId != 0)
                ret |= OM_Commitments.CheckRecurrenceMaxCommitmentDate(company, wCal, recurrenceId, lastEventDateTime, excLogger);

            return ret;
        }

        //-------------------------------------------------------------------------------
        //Modifica dell'evento ricorrente in VS
        public static bool UpdateRecurringEventVS(TbSenderDatabaseInfo company, CalendarService service, Event gooEv, int recurrenceId, string strRecurringId,
            OM_GoogleAccounts accountGoogle, OM_GoogleCalendars gCal,
            OM_WorkersCalendars wCal,
            IEasyAttachmentManager eaMng, Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = false;

            List<OM_FacilitiesDetails> listFacilities = null;
            //Recupero il record nella tabella OM_Commitments del primo evento della sequenza che non è etichettato come Exception
            OM_Commitments commMain = OM_Commitments.GetNonExceptionCommitmentFromRecurrenceId(company, accountGoogle.WorkerId, wCal.SubId, recurrenceId);
            if (commMain != null)
            {
                //Recupero la lista di Facilities collegate al Commitment 
                listFacilities = OM_FacilitiesDetails.GetCommitmentFacilities(company, commMain.CommitmentId);
            }

            int checkCommitments = 0;
            //Recupero la sequenza di Commitments appartenenti alla ricorrenza
            List<OM_Commitments> listC = OM_Commitments.GetRecurrenceCommitments(company, accountGoogle.WorkerId, wCal.SubId, recurrenceId);
            if ((listC != null) && (listC.Count > 0))
                checkCommitments = listC.Count;
            
            //Recupero la data di inizio dell'evento di Google Calendar
            DateTime startDateTime = gooEv.Start.DateTime.Value;
            DateTime lastEventDateTime = gooEv.Start.DateTime.Value;

            //Recupero la lista di workers coinvolti nell'evento
            List<OM_CommitmentsWorkers> guestWorkers = OM_CommitmentsWorkers.GetGuestListWorkers(company, recurrenceId);
            
            //Ricavo tutti i figli del record padre
            Events instances = service.Events.Instances(wCal.IdGoogleCalendar, gooEv.Id).Execute();

            //Ricavo i campi per la ricorrenza
            IList<String> recurrence = gooEv.Recurrence;
            string strRecurrence = OM_Commitments.GetRecurrenceString(recurrence);
            DateTime RecurrenceEndDate = OM_Commitments.GetRecurrenceEndDate(recurrence);
            int RecurrenceOccurrences = instances.Items.Count;
            OM_Commitments.CommitmentRecurrenceType RecurrenceType = OM_Commitments.GetRecurrenceType(recurrence);
            int RecurrenceEvery = OM_Commitments.GetRecurrenceEvery(recurrence);
            int WeekDays = OM_Commitments.GetRecurrenceWeekday(recurrence);
            int MonthlyWeek = OM_Commitments.GetRecurrenceMonthlyWeek(recurrence);
            OM_Commitments.CommitmentRecurrenceEnd RecurrenceEnd = OM_Commitments.GetRecurrenceEndType(recurrence);
            bool RecurrenceException = false;

            //Controllo se esistono eventi appartenenti alla ricorrenza
            if (RecurrenceOccurrences == 0)
                return ret;

            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        int newEvents = 0;
                        int RecSubId = 0;

                        //Aggiorno il record dell'evento padre (non ha corrispondenze nelle tabelle OM_Commitments e OM_CommitmentsWorkers)
                        OM_GoogleEvents gEvMaster = OM_GoogleEvents.UpdateItem(db, accountGoogle.WorkerId, gooEv, gCal.TimeZone, excLogger);
                        gEvMaster.Recurrence = strRecurrence;

                        List<string> EventIdList = new List<string>();

                        //Fase di aggiornamento degli eventi di VS appartenenti alla sequenza
                        //Ciclo su tutte le istanze della ricorrenza
                        foreach (Event childEvent in instances.Items)
                        {
                            //Controllo se l'evento esiste e non è stato cancellato
                            if ((childEvent == null) ||
                                (childEvent.Status == null) ||
                                (childEvent.Status.ToString() == "cancelled")) // Se l'evento è cancellato viene gestita nelle eccezioni della ricorrenza
                                continue;

                            //Inserisco l'Id nell'array per il controllo successivo sui record eliminati 
                            EventIdList.Add(childEvent.Id);

                            //Recupero i record nelle tabelle OM_GoogleEvents e OM_GoogleCalendarsEvents e controllo la loro esistenza
                            OM_GoogleEvents gChildEv = OM_GoogleEvents.GetGoogleEvent(db, childEvent.Id);
                            OM_GoogleCalendarsEvents gChildCalEv = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(company, accountGoogle.WorkerId, childEvent.Id, wCal.IdGoogleCalendar);
                            if ((gChildEv != null) && (gChildCalEv != null))
                            {
                                //Aggiorno il record nella tabella OM_GoogleEvents
                                gChildEv = OM_GoogleEvents.UpdateItem(db, accountGoogle.WorkerId, childEvent, gCal.TimeZone, excLogger);
                                gChildEv.Recurrence = strRecurrence;

                                //Aggiorno il record nella tabella OM_Commitments
                                OM_Commitments commChild = OM_Commitments.UpdateItem(db, accountGoogle.WorkerId, gChildCalEv.CommitmentId, wCal, gChildEv, excLogger);
                                //Aggiorno i campi della ricorrenza
                                commChild.BoolIsRecurring = true;
                                commChild.OriginalParentStartDate = startDateTime;
                                commChild.RecurrenceId = recurrenceId;
                                commChild.RecurrenceOldRecurrenceId = recurrenceId;
                                //commChild.RecurrenceSubId = RecSubId; // Non modifico il RecurrenceSubId
                                commChild.RecurrenceEndDate = RecurrenceEndDate;
                                commChild.RecurrenceOccurrences = RecurrenceOccurrences;
                                commChild.EnumCommitmentRecurrenceType = RecurrenceType;
                                commChild.BoolRecurrenceException = RecurrenceException;
                                commChild.RecurrenceEvery = RecurrenceEvery;
                                commChild.RecurrenceWeekdays = WeekDays;
                                commChild.EnumCommitmentRecurrenceEnd = RecurrenceEnd;

                                bool bMonthlyDay = false;
                                bool bMonthlyWeek = false;
                                if (RecurrenceType == OM_Commitments.CommitmentRecurrenceType.Month)
                                {
                                    if (MonthlyWeek > 0)
                                        bMonthlyWeek = true;

                                    bMonthlyDay = !bMonthlyWeek;
                                }
                                commChild.RecurrenceMonthlyWeeks = MonthlyWeek;
                                commChild.BoolIsRecurrenceMonthlyDay = bMonthlyDay;
                                commChild.BoolIsRecurrenceMonthlyWeek = bMonthlyWeek;
                                
                                if (commChild.RecurrenceSubId > RecSubId)
                                    RecSubId = commChild.RecurrenceSubId;

                                if (lastEventDateTime < commChild.CommitmentDate)
                                    lastEventDateTime = commChild.CommitmentDate;

                                //Aggiorno la Facilities collegate all'evento 
                                UpdateFacility(db, childEvent, commChild, accountGoogle, excLogger);
                            }
                            else
                            {
                                //Se non trovo il record aumento il contatore dei record da aggiungere
                                newEvents++;
                            }
                        }

                        //Fase di cancellazione dei record eliminati dalla sequenza
                        //Recupero la lista di OM_GoogleCalendarsEvents collegati alla ricorrenza
                        List<OM_GoogleCalendarsEvents> RecurringEventList = OM_GoogleCalendarsEvents.GetRecurrenceGoogleCalendarsEvents(company, accountGoogle.WorkerId, wCal.IdGoogleCalendar, strRecurringId);
                        int deleted = 0;
                        //Ciclo sui record della OM_GoogleCalendarsEvents appartenenti alla ricorrenza
                        foreach (OM_GoogleCalendarsEvents gce in RecurringEventList)
                        {
                            //Controllo se il record è presente nella lista degli eventi della sequenza 
                            if ((EventIdList.Count > 0) &&
                                (EventIdList.Contains(gce.IdEvent)))
                                continue;

                            //Elimino l'evento in VS
                            RemoveEventVS(db, gce, excLogger);
                            deleted++;
                        }

                        //Fase di creazione dei nuovi eventi appartenenti alla sequenza
                        //Se sono stati rilevati nuovi eventi 
                        if (newEvents > 0)
                        {
                            //Controllo se il numero di occorrenze è uguale al numero di record eliminati
                            if (checkCommitments == deleted)
                                recurrenceId = 0;   //Annullo il CommitmentParentId per i nuovi commitment

                            //Recupero il nuovo CommitmentId
                            int nextCommitmentId = ConnectionHelper.GetCommitmentsNextKey(company, newEvents, (ILockerClient)eaMng);
                            int newCommId = nextCommitmentId;

                            //Controllo se il CommitmentParentId è nullo
                            if (recurrenceId == 0)
                                recurrenceId = nextCommitmentId; //imposto il nuovo CommitmentId che si sta creando

                            RecSubId++;
                            int geNewId = 0;
                            bool boolNewGeID = true;

                            //Ciclo per tutti gli eventi appartenenti alla sequenza
                            foreach (Event childEvent in instances.Items)
                            {
                                //Recupero i record nelle tabelle OM_GoogleEvents e OM_GoogleCalendarsEvents e controllo la loro esistenza
                                OM_GoogleEvents gChildEv = OM_GoogleEvents.GetGoogleEvent(db, childEvent.Id);
                                OM_GoogleCalendarsEvents gChildCalEv = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(company, accountGoogle.WorkerId, childEvent.Id, wCal.IdGoogleCalendar);
                                if ((gChildEv == null) && (gChildCalEv == null))
                                {

                                    //Creo il record nella tabella OM_GoogleEvents
                                    gChildEv = OM_GoogleEvents.CreateItem(db, accountGoogle.WorkerId, childEvent, wCal.IdGoogleCalendar, gCal.TimeZone);
                                    if (boolNewGeID)
                                    {
                                        geNewId = gChildEv.GoogleEventId;
                                        boolNewGeID = false;
                                    }
                                    gChildEv.GoogleEventId = geNewId++;
                                    gChildEv.Recurrence = strRecurrence;
                                    db.OM_GoogleEvents.Add(gChildEv);

                                    //Creo il record nella tabella OM_Commitments
                                    OM_Commitments commChild = OM_Commitments.CreateItem(newCommId, accountGoogle.WorkerId, wCal, accountGoogle, gChildEv);
                                    //Imposto i valori per i campi della ricorrenza
                                    commChild.BoolIsRecurring = true;
                                    commChild.OriginalParentStartDate = startDateTime;
                                    commChild.RecurrenceId = recurrenceId;
                                    commChild.RecurrenceOldRecurrenceId = recurrenceId;
                                    commChild.RecurrenceSubId = RecSubId;
                                    commChild.RecurrenceEndDate = RecurrenceEndDate;
                                    commChild.RecurrenceOccurrences = RecurrenceOccurrences;
                                    commChild.EnumCommitmentRecurrenceType = RecurrenceType;
                                    commChild.BoolRecurrenceException = RecurrenceException;
                                    commChild.RecurrenceEvery = RecurrenceEvery;
                                    commChild.RecurrenceWeekdays = WeekDays;
                                    commChild.EnumCommitmentRecurrenceEnd = RecurrenceEnd;

                                    bool bMonthlyDay = false;
                                    bool bMonthlyWeek = false;
                                    if (RecurrenceType == OM_Commitments.CommitmentRecurrenceType.Month)
                                    {
                                        if (MonthlyWeek > 0)
                                            bMonthlyWeek = true;

                                        bMonthlyDay = !bMonthlyWeek;
                                    }
                                    commChild.RecurrenceMonthlyWeeks = MonthlyWeek;
                                    commChild.BoolIsRecurrenceMonthlyDay = bMonthlyDay;
                                    commChild.BoolIsRecurrenceMonthlyWeek = bMonthlyWeek;
                                    db.OM_Commitments.Add(commChild);

                                    if (lastEventDateTime < commChild.CommitmentDate)
                                        lastEventDateTime = commChild.CommitmentDate;

                                    //Creo il record nella tabella OM_GoogleCalendarsEvents
                                    gChildCalEv = OM_GoogleCalendarsEvents.CreateItem(accountGoogle.WorkerId, commChild, childEvent, wCal.IdGoogleCalendar);
                                    db.OM_GoogleCalendarsEvents.Add(gChildCalEv);

                                    //Creo il record nella tabella OM_CommitmentsWorkers
                                    OM_CommitmentsWorkers commworChild = OM_CommitmentsWorkers.CreateItem(newCommId, accountGoogle.WorkerId, wCal.SubId, true);
                                    db.OM_CommitmentsWorkers.Add(commworChild);

                                    //Controllo se la sequenza è condivisa con altri workers
                                    if ((guestWorkers != null) && (guestWorkers.Count>0))
                                    {
                                        foreach (OM_CommitmentsWorkers cw in guestWorkers)
                                        {
                                            //Creo il record nella tabella OM_CommitmentsWorkers per i workers invitati
                                            OM_CommitmentsWorkers commworGuestChild = OM_CommitmentsWorkers.CreateItem(newCommId, cw.WorkerId, cw.CalendarSubId, false);
                                            db.OM_CommitmentsWorkers.Add(commworGuestChild); 
                                        }
                                    }

                                    //Creo i record nella tabella OM_FacilitiesDetails se sono collegate delle facilities alla sequenza
                                    if ((commMain != null) && (listFacilities != null))
                                        NewFacilities(db, childEvent, listFacilities, commMain, newCommId, accountGoogle, excLogger);

                                    RecSubId++;
                                    newCommId++;
                                }
                            }

                            //Aggiorno il ParentId della sequenza nella tabella OM_GoogleCalendarsEvents per il record padre
                            OM_GoogleCalendarsEvents gCalEvMaster = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(db, accountGoogle.WorkerId, gEvMaster.Id, wCal.IdGoogleCalendar);
                            if ((gCalEvMaster != null) && (recurrenceId != gCalEvMaster.ParentCommitmentId))
                                gCalEvMaster = OM_GoogleCalendarsEvents.UpdateGoogleCalendarEventParentId(db, accountGoogle.WorkerId, 0, gEvMaster.Id, wCal.IdGoogleCalendar, recurrenceId, excLogger);
                        }

                        db.SaveChanges();
                        ts.Complete();
                        ret = true;
                    }
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }

            //Ciclo di controllo e compattazione del campo RecurrenceSubId sui record appartenenti alla Ricorrenza
            CheckRecurringEventVS(company, strRecurringId, accountGoogle, wCal, excLogger);

            if (recurrenceId != 0)
                ret |= OM_Commitments.CheckRecurrenceMaxCommitmentDate(company, wCal, recurrenceId, lastEventDateTime, excLogger);

            return ret;
        }

        //-------------------------------------------------------------------------------
        //Eliminazione dell'evento ricorrente in VS
        public static bool DeleteRecurringEventVS(TbSenderDatabaseInfo company, CalendarService service, Event gooEv,
            OM_GoogleCalendarsEvents gCalEv, OM_GoogleAccounts accountGoogle, OM_GoogleCalendars gCal,
            OM_WorkersCalendars wCal,
            IEasyAttachmentManager eaMng, Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = false;

            //Recupero il RecurrenceId dall'evento padre di Google Calendar
            string strRecurringId = gCalEv.IdEvent;
            int recurringId = gCalEv.ParentCommitmentId;
            //Recupero il record dell'eventopadre nella tabella OM_Commitments e OM_CommitmentsWorkers
            OM_Commitments comm = OM_Commitments.GetCommitment(company, recurringId);
            OM_CommitmentsWorkers commwor = OM_CommitmentsWorkers.GetCommitmentsWorkers(company, recurringId, accountGoogle.WorkerId);
            
            //Controllo se è il propietario dell'evento
            bool isOwner = commwor.BoolIsOwner;

            //Controllo se è il creatore dell'evento
            bool isCreator = (comm.TBCreatedID == accountGoogle.WorkerId);

            if (isOwner || isCreator)
            {
                using (var db = ConnectionHelper.GetHermesDBEntities(company))
                {
                    try
                    {
                        //Apro la transazione sul db
                        using (TransactionScope ts = new TransactionScope())
                        {
                            //Recupero la lista dei record collegati alla sequenza nella tabella OM_GoogleCalendarsEvents
                            List<OM_GoogleCalendarsEvents> RecurringEventList = OM_GoogleCalendarsEvents.GetRecurrenceGoogleCalendarsEvents(company, accountGoogle.WorkerId, wCal.IdGoogleCalendar, strRecurringId);

                            //Cliclo di eliminazione dei commitment di VS appartenenti alla sequenza
                            foreach (OM_GoogleCalendarsEvents gce in RecurringEventList)
                                RemoveEventVS(db, gce, excLogger);

                            //Eliminazione dell'evento padre della ricorrenza in VS 
                            RemoveEventVS(db, gCalEv, excLogger);

                            db.SaveChanges();
                            ts.Complete();
                        }
                    }
                    catch (Exception ex)
                    {
                        string a = ex.Message;
                        if (excLogger != null)
                            excLogger(ex);
                    }
                }
            }
            else
            {
                //Recupero l'evento padre
                OM_GoogleCalendarsEvents gCalEvMain = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(company, gCalEv.WorkerId, 0, strRecurringId, gCalEv.IdCalendar);

                if (gCalEvMain == null)
                    return ret;
                //Recupero il primo Commitment della sequenza non definito come eccezione
                OM_Commitments commSequence = OM_Commitments.GetNonExceptionCommitmentFromRecurrenceId(company, accountGoogle.WorkerId, wCal.SubId, gCalEv.ParentCommitmentId);

                //Elimino l'evento padre in Google Calendar
                ret |= RemoveEventGoogle(company, service, gCalEvMain, excLogger);

                //Elimino i record di sincronizzazione tra VS e Google Calendar
                ret |= RemoveSynchronizationRecurringEventVS(company, gCalEvMain, accountGoogle, wCal, excLogger);

                if (commSequence == null)
                    return ret;
                //Rigenero l'evento in Google Calendar
                ret |= CreateRecurrenceEventGoogle(company, service, true, accountGoogle, gCal, commSequence, wCal, eaMng, excLogger);

            }

            return ret;
        }

        //-------------------------------------------------------------------------------
        //Elimino i record di collegamento tra VS e Google Calendar per la ricorrenza
        public static bool RemoveSynchronizationRecurringEventVS(TbSenderDatabaseInfo company, 
            OM_GoogleCalendarsEvents gCalEv, OM_GoogleAccounts accountGoogle,
            OM_WorkersCalendars wCal,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = false;

            //Recupero il RecurrenceId dall'evento pagre di Google Calendar
            string strRecurringId = gCalEv.IdEvent;

            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //Recupero la lista dei record collegati alla sequenza nella tabella OM_GoogleCalendarsEvents
                        List<OM_GoogleCalendarsEvents> RecurringEventList = OM_GoogleCalendarsEvents.GetRecurrenceGoogleCalendarsEvents(company, accountGoogle.WorkerId, wCal.IdGoogleCalendar, strRecurringId);

                        //Cliclo di eliminazione dei record di collegamento tra Google Calendar e VS appartenenti alla sequenza
                        foreach (OM_GoogleCalendarsEvents gce in RecurringEventList)
                            RemoveSynchronizationEventVS(db, gce.IdCalendar, gce.IdEvent, gce.CommitmentId, gce.WorkerId, excLogger);

                        //Eliminazione dei record di collegamento tra Google Calendar e VS appartenenti all'evento padre della sequenza
                        RemoveSynchronizationEventVS(db, gCalEv.IdCalendar, gCalEv.IdEvent, gCalEv.CommitmentId, gCalEv.WorkerId, excLogger);

                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Ciclo di controllo e compattazione del campo RecurrenceSubId sui record appartenenti alla Ricorrenza
        public static bool CheckRecurringEventVS(TbSenderDatabaseInfo company,
            string recurringId,
            OM_GoogleAccounts accountGoogle,
            OM_WorkersCalendars wCal,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            int RecSubId = 1;
            int checkIndex = 0;
            int recId = 0;
            int newRecurrenceId = 0;
            int MaxRecSubId = 0;
            try
            {
                //Recupero il OM_Commitments.RecurrenceId dal OM_GoogleEvents.RecurrenceId 
                recId = OM_Commitments.GetRecurrenceIDFromRecurrenceEvent(company, accountGoogle.WorkerId, wCal.IdGoogleCalendar, recurringId);
                newRecurrenceId = recId;

                //Recupero il OM_Commitments.RecurrenceSubId maggiore della sequenza 
                MaxRecSubId = OM_Commitments.GetMaxRecurrenceSubIdFromRecurrenceId(company, accountGoogle.WorkerId, wCal.SubId, recId);

                //Recupero il primo Commitment della sequenza
                OM_Commitments FirstCommitment = OM_Commitments.GetCommitment(company, accountGoogle.WorkerId, wCal.SubId, recId);
                if (FirstCommitment == null)
                {
                    //Se il primo evento non esiste ciclo su tutti i RecurrenceSubId
                    for (int i = 0; i < MaxRecSubId; i++)
                    {
                        RecSubId = i + 1;
                        FirstCommitment = OM_Commitments.GetCommitmentRecurrenceChild(company, accountGoogle.WorkerId, wCal.SubId, recId, RecSubId);
                        if (FirstCommitment != null)
                        {
                            //Ricavo il nuovo OM_Commitments.RecurrenceId
                            newRecurrenceId = FirstCommitment.CommitmentId;
                            checkIndex = RecSubId - 1;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                recId = 0;
                MaxRecSubId = 0;
                string a = ex.Message;
                if (excLogger != null)
                    excLogger(ex);
            }

            if ((MaxRecSubId == 0) || (recId == 0))
                return false;

            //Controllo se la sequenza è cambiata
            if ((checkIndex == 0) && (newRecurrenceId == recId))
                return false;

            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        bool indexFound = false;

                        //Ciclo su tutti i Commitments della sequenza
                        for (int i = 0; i < MaxRecSubId; i++)
                        {
                            RecSubId = i + 1;
                            OM_Commitments FirstCommitment = OM_Commitments.GetCommitmentRecurrenceChild(db, accountGoogle.WorkerId, wCal.SubId, recId, RecSubId);
                            if (FirstCommitment != null)
                            {
                                if (FirstCommitment.CommitmentId == newRecurrenceId)
                                    indexFound = true;

                                //Imposto su tutti i Commitments i nuovi valori per OM_Commitments.RecurrenceId e OM_Commitments.RecurrenceSubId
                                FirstCommitment.RecurrenceId = newRecurrenceId;
                                FirstCommitment.RecurrenceSubId = RecSubId - checkIndex;
                                FirstCommitment.RecurrenceOldRecurrenceId = newRecurrenceId;
                            }
                            else
                            {
                                if (indexFound)
                                    checkIndex++;
                            }
                        }

                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex)
                {
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Ciclo di controllo e modifica dei record corrispondenti a Eventi Exception della Ricorrenza in VS
        public static bool CheckRecurringEventExceptionVS(TbSenderDatabaseInfo company,
            CalendarService service,
            OM_GoogleAccounts accountGoogle, OM_GoogleCalendars gCal, OM_GoogleCalendarsEvents gCalEvMain,
            OM_WorkersCalendars wCal,
            OM_Commitments commMain,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = false;
            int recurrenceId = commMain.RecurrenceId;
            //Recupero il record dell'evento padre dalla tabella OM_GoogleEvents
            OM_GoogleEvents gEvMain = OM_GoogleEvents.GetGoogleEvent(company, gCalEvMain.IdEvent);
            if (gEvMain == null)
                return ret;
            string gRecurrenceId = gEvMain.RecurringEventId;

            //Recupero la sequenza di Commitments appartenenti alla ricorrenza
            List<OM_Commitments> listC = OM_Commitments.GetRecurrenceCommitments(company, accountGoogle.WorkerId, wCal.SubId, recurrenceId);
            foreach (OM_Commitments c in listC)
            {
                //Controllo se il commitment non è un RecurrenceException
                if (!c.BoolRecurrenceException)
                    continue;

                //Recupero il record dell'evento dalla tabella OM_GoogleCalendarsEvents
                OM_GoogleCalendarsEvents gce = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(company, accountGoogle.WorkerId, c.CommitmentId);
                if (gce == null)
                    continue;

                //Recupero il record dell'evento dalla tabella OM_GoogleEvents
                OM_GoogleEvents ge = OM_GoogleEvents.GetGoogleEvent(company, gce.IdEvent);
                if (ge == null)
                    continue;

                //Controllo se l'evento nella tabella OM_GoogleEvents non è riconosciuto com Exception
                if (!ge.BoolIsException)
                {
                    try
                    {
                        //Recuprero l'evento da Google Calendar 
                        Event gooEventException = service.Events.Get(wCal.IdGoogleCalendar, gce.IdEvent).Execute();

                        using (var db = ConnectionHelper.GetHermesDBEntities(company))
                        {
                            try
                            {
                                using (TransactionScope ts = new TransactionScope())
                                {
                                    //Aggiorno l'evento in Google Calendar
                                    UpdateEventGoogle(db, service, gooEventException, ge, gCal, gce, c, wCal, excLogger);

                                    //Imposto come Exception il record dell'evento nella tabella OM_GoogleEvents
                                    ge = OM_GoogleEvents.SetExceptionGoogleEvent(db, gce.IdEvent, true, excLogger);

                                    db.SaveChanges();
                                    ts.Complete();
                                    ret |= true;
                                }
                            }
                            catch (Exception ex)
                            {
                                if (excLogger != null)
                                    excLogger(ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ret = false;
                        string a = ex.Message;
                        if (excLogger != null)
                            excLogger(ex);
                    }
                }
            }

            if (ret)
            {
                //Riallineo la sequenza degli eventi ricorrenti in VS
                CheckRecurringEventVS(company, gRecurrenceId, accountGoogle, wCal, excLogger);
            }

            return ret;
        }

        //-------------------------------------------------------------------------------
        //Creazione di un nuovo evento ricorrente in Google Calendar
        public static bool CreateRecurrenceEventGoogle(TbSenderDatabaseInfo company, CalendarService service, bool updateGoogle,
            OM_GoogleAccounts accountGoogle, OM_GoogleCalendars gCal,
            OM_Commitments comm, OM_WorkersCalendars wCal,
            IEasyAttachmentManager eaMng, Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = false;

            //Creazione di un nuovo evento ricorrente in Google Calendar
            Event gooMain = CreateRecurerenceEventGoogleService(service, gCal.TimeZone, wCal.IdGoogleCalendar, comm, excLogger);
            if (gooMain == null)
                return ret;

            //Recupero il record nella tabella OM_GoogleEvents
            OM_GoogleEvents gEvMain = OM_GoogleEvents.GetGoogleEvent(company, gooMain.Id);

            //Recupero il record nella tabella OM_GoogleCalendarsEvents
            OM_GoogleCalendarsEvents gCalEvMain = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(company, accountGoogle.WorkerId, 0, gooMain.Id, wCal.IdGoogleCalendar);
            
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        int gceId = OM_GoogleEvents.GetMaxEventId(db, accountGoogle.WorkerId);

                        if (gEvMain == null)
                        {
                            //Creo il record nella tabella OM_GoogleEvents
                            gEvMain = OM_GoogleEvents.CreateItem(db, accountGoogle.WorkerId, gooMain, wCal.IdGoogleCalendar, gCal.TimeZone);
                            gceId = gEvMain.GoogleEventId;
                            db.OM_GoogleEvents.Add(gEvMain);
                        }

                        if (gCalEvMain == null)
                        {
                            //Recupero il record nella tabella OM_GoogleCalendarsEvents
                            gCalEvMain = OM_GoogleCalendarsEvents.CreateItem(accountGoogle.WorkerId, 0, comm.RecurrenceId, gooMain, wCal.IdGoogleCalendar);
                            db.OM_GoogleCalendarsEvents.Add(gCalEvMain);
                        }

                        //Recupero tutte le istanze della sequenza in Google Calendar
                        Events instances = service.Events.Instances(gCal.Id, gooMain.Id).Execute();

                        gceId++;
                        int recurrenceSubId = 1;
                        //Ciclo su tutte le istanze della sequenza in Google Calendar
                        foreach (Event gooChild in instances.Items)
                        {
                            //Recupero il record dell'evento nella tabella OM_Commitments
                            OM_Commitments commChild = OM_Commitments.GetCommitmentRecurrenceChild(db, accountGoogle.WorkerId, wCal.SubId, comm.RecurrenceId, recurrenceSubId);    
                            if (commChild != null)
                            {
                                //Recupero il record dell'evento nella tabella OM_GoogleEvents
                                OM_GoogleEvents gEvChild = OM_GoogleEvents.GetGoogleEvent(db, gooChild.Id);
                                if (gEvChild == null)
                                {
                                    //Creo il record dell'evento nella tabella OM_GoogleEvents
                                    gEvChild = OM_GoogleEvents.CreateItem(db, accountGoogle.WorkerId, gooChild, wCal.IdGoogleCalendar, gCal.TimeZone);
                                    gEvChild.GoogleEventId = gceId;
                                    db.OM_GoogleEvents.Add(gEvChild);
                                    gceId++;
                                }

                                //Recupero il record dell'evento nella tabella OM_GoogleCalendarsEvents
                                OM_GoogleCalendarsEvents gCalEvChild = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(db, accountGoogle.WorkerId, commChild.CommitmentId, gooChild.Id, wCal.IdGoogleCalendar);
                                if (gCalEvChild == null)
                                {
                                    //Creo il record dell'evento nella tabella OM_GoogleCalendarsEvents
                                    gCalEvChild = OM_GoogleCalendarsEvents.CreateItem(accountGoogle.WorkerId, commChild, gooChild, wCal.IdGoogleCalendar);
                                    db.OM_GoogleCalendarsEvents.Add(gCalEvChild);
                                }

                                //Se devo sovrascrivere l'evento di Google Calendar e l'evento è una RecurrenceException
                                if ((updateGoogle) && (commChild.BoolRecurrenceException))
                                {
                                    //Aggiorno l'evento nella tabella OM_GoogleEvents
                                    gEvChild = OM_GoogleEvents.UpdateItem(db, commChild, gCalEvChild, gCal.TimeZone, excLogger);
                                    //Aggiorno l'evento in Google Calendar
                                    ret |= UpdateEventGoogle(db, service, gooChild, gEvChild, gCal, gCalEvChild, commChild, wCal, excLogger);
                                }
                                else
                                {
                                    //Aggiorno il commitment di VS con le impostazioni dell'evento di Google Calendar
                                    commChild = OM_Commitments.UpdateItem(db, accountGoogle.WorkerId, commChild.CommitmentId, wCal, gEvChild, excLogger);
                                }
                            }
                            recurrenceSubId++;
                        }

                        db.SaveChanges();
                        ts.Complete();
                        ret = true;
                   }
                }
                catch (Exception ex)
                {
                    ret = false;
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Impostazioni per creare l'evento ricorrente in Google Calendar
        public static Event CreateRecurerenceEventGoogleService(CalendarService service, string gooCalTimeZone, string IdGoogleCalendar, OM_Commitments comm,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            Event ret = null;
            try
            {
                List<String> recurrenceList = new List<string>();
                string recurrence = OM_Commitments.RecurrenceString(comm);
                recurrenceList.Add(recurrence);

                //Imposto i campi da trasferire a Google nel nuovo evento ricorrente
                Event eventToSynchronize = new Event()
                {
                    Summary = comm.Subject,
                    Description = comm.Description,
                    Location = comm.Location,
                    Start = new EventDateTime()
                    {
                        DateTime = comm.OriginalParentStartDate,//.Add(comm.StartTime.TimeOfDay),
                        TimeZone = gooCalTimeZone
                    },
                    End = new EventDateTime()
                    {
                        DateTime = comm.OriginalParentStartDate.Date.Add(comm.EndTime.TimeOfDay),
                        TimeZone = gooCalTimeZone
                    },
                    Recurrence = recurrenceList
                };

                //Istruzioni per creare l'evento in Google Calendar
                ret = service.Events.Insert(eventToSynchronize, IdGoogleCalendar).Execute();
            }
            catch (Exception ex)
            {
                string a = ex.Message;
                if (excLogger != null)
                    excLogger(ex);
            }

            return ret;
        }

        //-------------------------------------------------------------------------------
        //Impostazioni per modificare l'evento ricorrente in Google Calendar
        public static bool UpdateRecurringEventGoogle(TbSenderDatabaseInfo company, CalendarService service, Event gooEv,
            OM_GoogleCalendars gCal, OM_GoogleCalendarsEvents gCalEv, OM_GoogleAccounts accountGoogle,
            OM_Commitments comm, OM_WorkersCalendars wCal,
            IEasyAttachmentManager eaMng, Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            int recurrenceId = comm.RecurrenceId;

            List<OM_FacilitiesDetails> listFacilities = null;
            List<OM_CommitmentsWorkers> guestWorkers = null;
            //Recupero il record nella tabella OM_Commitments del primo evento della sequenza che non è etichettato come Exception
            OM_Commitments commMain = OM_Commitments.GetNonExceptionCommitmentFromRecurrenceId(company, accountGoogle.WorkerId, wCal.SubId, recurrenceId);
            if (commMain != null)
            {
                //Recupero la lista di Facilities collegate al Commitment 
                listFacilities = OM_FacilitiesDetails.GetCommitmentFacilities(company, comm.CommitmentId);
                guestWorkers = OM_CommitmentsWorkers.GetGuestListWorkers(company, recurrenceId);
            }

            string strRecurringId = OM_Commitments.RecurrenceString(comm);
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    //Apro la transazione sul db
                    using (TransactionScope ts = new TransactionScope())
                    {
                        //Aggiorno il record nella tabella OM_GoogleEvents
                        OM_GoogleEvents gEvMain = OM_GoogleEvents.UpdateItem(db, comm, gCalEv, gCal.TimeZone, excLogger);
                        gEvMain.Recurrence = strRecurringId;

                        //Aggiorno la sequenza dell'evento padre di Google Calendar
                        gooEv.Recurrence.Clear();
                        gooEv.Recurrence.Add(gEvMain.Recurrence);

                        //Aggiorno l'evento in Google Calendar
                        Event updatedEvent = UpdateEventGoogleService(service, gooEv, gEvMain, gCal, wCal.IdGoogleCalendar, excLogger);
                        if ((updatedEvent != null) && (updatedEvent.Updated != null) && (updatedEvent.Updated.HasValue))
                        {
                            //Aggiorno la data di modifica dell record nella tabella OM_GoogleEvents
                            DateTime dtUpdate = updatedEvent.Updated.Value;
                            OM_GoogleEvents.UpdateItemUpdatedDateTime(db, updatedEvent.Id, dtUpdate, excLogger);

                            if ((updatedEvent.Start == null) || (gooEv.Start.DateTime == null))
                                return false;
                            DateTime startDateTime = gooEv.Start.DateTime.Value;
                            
                            //Recupero le istanze degli eventi appartenenti alla sequenza
                            Events instances = service.Events.Instances(wCal.IdGoogleCalendar, updatedEvent.Id).Execute();

                            //Ricavo i campi per la ricorrenza
                            IList<String> recurrence = updatedEvent.Recurrence;
                            string strRecurrence = OM_Commitments.GetRecurrenceString(recurrence);
                            DateTime RecurrenceEndDate = OM_Commitments.GetRecurrenceEndDate(recurrence);
                            int RecurrenceOccurrences = instances.Items.Count;
                            OM_Commitments.CommitmentRecurrenceType RecurrenceType = OM_Commitments.GetRecurrenceType(recurrence);
                            int RecurrenceEvery = OM_Commitments.GetRecurrenceEvery(recurrence);
                            int WeekDays = OM_Commitments.GetRecurrenceWeekday(recurrence);
                            int MonthlyWeek = OM_Commitments.GetRecurrenceMonthlyWeek(recurrence);
                            OM_Commitments.CommitmentRecurrenceEnd RecurrenceEnd = OM_Commitments.GetRecurrenceEndType(recurrence);
                            bool RecurrenceException = false;

                            if (RecurrenceOccurrences == 0)
                                return ret;

                            int newEvents = 0;
                            int RecSubId = 1;

                            //Aggiorno il record dell'evento padre (non ha corrispondenze nelle tabelle OM_Commitments e OM_CommitmentsWorkers)
                            gEvMain = OM_GoogleEvents.UpdateItem(db, accountGoogle.WorkerId, gooEv, gCal.TimeZone, excLogger);
                            gEvMain.Recurrence = strRecurrence;

                            List<string> EventIdList = new List<string>();

                            //Fase di aggiornamento degli eventi di VS appartenenti alla sequenza
                            //Ciclo sugli eventi appartenenti alla sequenza di Google Calendar
                            foreach (Event childEvent in instances.Items)
                            {
                                //Controllo se l'evento eviste e non è stato eliminato
                                if ((childEvent == null) ||
                                    (childEvent.Status == null) ||
                                    (childEvent.Status.ToString() == "cancelled")) // Se l'evento è cancellato viene gestita nelle eccezioni della ricorrenza
                                    continue;

                                //Inserisco l'Id nell'array per il controllo successivo sui record eliminati 
                                EventIdList.Add(childEvent.Id);

                                //Recupero i record dalle tabelle OM_GoogleEvents e OM_GoogleCalendarsEvents
                                OM_GoogleEvents gChildEv = OM_GoogleEvents.GetGoogleEvent(db, childEvent.Id);
                                OM_GoogleCalendarsEvents gChildCalEv = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(company, accountGoogle.WorkerId, childEvent.Id, wCal.IdGoogleCalendar);
                                if ((gChildEv != null) && (gChildCalEv != null))
                                {
                                    //Aggiorno il record nella tabella OM_GoogleEvents
                                    gChildEv = OM_GoogleEvents.UpdateItem(db, accountGoogle.WorkerId, childEvent, gCal.TimeZone, excLogger);
                                    gChildEv.Recurrence = strRecurrence;

                                    //Aggiorno il record nella tabella OM_Commitments
                                    OM_Commitments commChild = OM_Commitments.UpdateItem(db, accountGoogle.WorkerId, gChildCalEv.CommitmentId, wCal, gChildEv, excLogger);
                                    commChild.BoolIsRecurring = true;
                                    commChild.OriginalParentStartDate = startDateTime;
                                    commChild.RecurrenceId = recurrenceId;
                                    commChild.RecurrenceOldRecurrenceId = recurrenceId;
                                    //commChild.RecurrenceSubId = RecSubId;
                                    commChild.RecurrenceEndDate = RecurrenceEndDate;
                                    commChild.RecurrenceOccurrences = RecurrenceOccurrences;
                                    commChild.EnumCommitmentRecurrenceType = RecurrenceType;
                                    commChild.BoolRecurrenceException = RecurrenceException;
                                    commChild.RecurrenceEvery = RecurrenceEvery;
                                    commChild.RecurrenceWeekdays = WeekDays;
                                    commChild.EnumCommitmentRecurrenceEnd = RecurrenceEnd;
                                    bool bMonthlyDay = false;
                                    bool bMonthlyWeek = false;
                                    if (RecurrenceType == OM_Commitments.CommitmentRecurrenceType.Month)
                                    {
                                        if (MonthlyWeek > 0)
                                            bMonthlyWeek = true;

                                        bMonthlyDay = !bMonthlyWeek;
                                    }
                                    commChild.RecurrenceMonthlyWeeks = MonthlyWeek;
                                    commChild.BoolIsRecurrenceMonthlyDay = bMonthlyDay;
                                    commChild.BoolIsRecurrenceMonthlyWeek = bMonthlyWeek;

                                    if (commChild.RecurrenceSubId > RecSubId)
                                        RecSubId = commChild.RecurrenceSubId;
                                }
                                else
                                {
                                    //Se non trovo il record aumento il contatore dei record da aggiungere
                                    newEvents++;
                                }
                            }

                            //Fase di cancellazione dei record eliminati dalla sequenza
                            //Recupero la lista di OM_GoogleCalendarsEvents collegati alla ricorrenza
                            List<OM_GoogleCalendarsEvents> RecurringEventList = OM_GoogleCalendarsEvents.GetRecurrenceGoogleCalendarsEvents(company, accountGoogle.WorkerId, wCal.IdGoogleCalendar, strRecurringId);
                            foreach (OM_GoogleCalendarsEvents gce in RecurringEventList)
                            {
                                //Controllo se il record è presente nella lista degli eventi della sequenza 
                                if ((EventIdList.Count > 0) &&
                                    (EventIdList.Contains(gce.IdEvent)))
                                    continue;

                                //Elimino l'evento in VS
                                RemoveEventVS(db, gce, excLogger);
                            }

                            //Fase di creazione dei nuovi eventi appartenenti alla sequenza
                            //Se sono stati rilevati nuovi eventi 
                            if (newEvents > 0)
                            {
                                int nextCommitmentId = ConnectionHelper.GetCommitmentsNextKey(company, newEvents, (ILockerClient)eaMng);
                                int newCommId = nextCommitmentId;

                                RecSubId++;
                                int geNewId = 0;
                                bool boolNewGeID = true;

                                //Ciclo per tutti gli eventi appartenenti alla ricorrenza 
                                foreach (Event childEvent in instances.Items)
                                {
                                    //Recupero i record nelle tabelle OM_GoogleEvents e OM_GoogleCalendarsEvents e controllo la loro esistenza
                                    OM_GoogleEvents gChildEv = OM_GoogleEvents.GetGoogleEvent(db, childEvent.Id);
                                    OM_GoogleCalendarsEvents gChildCalEv = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(company, accountGoogle.WorkerId, childEvent.Id, wCal.IdGoogleCalendar);
                                    if ((gChildEv == null) && (gChildCalEv == null))
                                    {
                                        //Creo il record nella tabella OM_GoogleEvents
                                        gChildEv = OM_GoogleEvents.CreateItem(db, accountGoogle.WorkerId, childEvent, wCal.IdGoogleCalendar, gCal.TimeZone);
                                        if (boolNewGeID)
                                        {
                                            geNewId = gChildEv.GoogleEventId;
                                            boolNewGeID = false;
                                        }
                                        gChildEv.GoogleEventId = geNewId++;
                                        gChildEv.Recurrence = strRecurrence;
                                        db.OM_GoogleEvents.Add(gChildEv);

                                        //Creo il record nella tabella OM_Commitments
                                        OM_Commitments commChild = OM_Commitments.CreateItem(newCommId, accountGoogle.WorkerId, wCal, accountGoogle, gChildEv);
                                        commChild.BoolIsRecurring = true;
                                        commChild.OriginalParentStartDate = startDateTime;
                                        commChild.RecurrenceId = recurrenceId;
                                        commChild.RecurrenceOldRecurrenceId = recurrenceId;
                                        commChild.RecurrenceSubId = RecSubId;
                                        commChild.RecurrenceEndDate = RecurrenceEndDate;
                                        commChild.RecurrenceOccurrences = RecurrenceOccurrences;
                                        commChild.EnumCommitmentRecurrenceType = RecurrenceType;
                                        commChild.BoolRecurrenceException = RecurrenceException;
                                        commChild.RecurrenceEvery = RecurrenceEvery;
                                        commChild.RecurrenceWeekdays = WeekDays;
                                        commChild.EnumCommitmentRecurrenceEnd = RecurrenceEnd;
                                        bool bMonthlyDay = false;
                                        bool bMonthlyWeek = false;
                                        if (RecurrenceType == OM_Commitments.CommitmentRecurrenceType.Month)
                                        {
                                            if (MonthlyWeek > 0)
                                                bMonthlyWeek = true;

                                            bMonthlyDay = !bMonthlyWeek;
                                        }
                                        commChild.RecurrenceMonthlyWeeks = MonthlyWeek;
                                        commChild.BoolIsRecurrenceMonthlyDay = bMonthlyDay;
                                        commChild.BoolIsRecurrenceMonthlyWeek = bMonthlyWeek;
                                        db.OM_Commitments.Add(commChild);

                                        //Creo il record nella tabella OM_GoogleCalendarsEvents
                                        gChildCalEv = OM_GoogleCalendarsEvents.CreateItem(accountGoogle.WorkerId, commChild, childEvent, wCal.IdGoogleCalendar);
                                        db.OM_GoogleCalendarsEvents.Add(gChildCalEv);

                                        //Creo il record nella tabella OM_CommitmentsWorkers
                                        OM_CommitmentsWorkers commworChild = OM_CommitmentsWorkers.CreateItem(newCommId, accountGoogle.WorkerId, wCal.SubId, true);
                                        db.OM_CommitmentsWorkers.Add(commworChild);
                                        //Controllo se la sequenza è condivisa con altri workers
                                        if ((guestWorkers != null) && (guestWorkers.Count > 0))
                                        {
                                            foreach (OM_CommitmentsWorkers cw in guestWorkers)
                                            {
                                                //Creo il record nella tabella OM_CommitmentsWorkers per i workers invitati
                                                OM_CommitmentsWorkers commworGuestChild = OM_CommitmentsWorkers.CreateItem(newCommId, cw.WorkerId, cw.CalendarSubId, false);
                                                db.OM_CommitmentsWorkers.Add(commworGuestChild);
                                            }
                                        }

                                        //Creo i record nella tabella OM_FacilitiesDetails se sono collegate delle facilities alla sequenza
                                        if ((commMain != null) && (listFacilities != null))
                                            NewFacilities(db, childEvent, listFacilities, commMain, newCommId, accountGoogle, excLogger);
                                        
                                        RecSubId++;
                                        newCommId++;
                                    }
                                }
                            }
                        }
                        db.SaveChanges();
                        ts.Complete();
                    }
                    
                    //Ciclo di controllo e compattazione del campo RecurrenceSubId sui record appartenenti alla Ricorrenza
                    CheckRecurringEventVS(company, strRecurringId, accountGoogle, wCal, excLogger);
                }
                catch (Exception ex)
                {
                    ret = false;
                    string a = ex.Message;
                    if (excLogger != null)
                        excLogger(ex);
                }
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Confronto tra evento ricorrente di Google Calendar e Commitment di VS per aggiornamento
        public static bool MatchVSGoogleRecurringEvent(TbSenderDatabaseInfo company, CalendarService service, Event gooEv,
            OM_GoogleCalendarsEvents gCalEv, OM_GoogleAccounts accountGoogle, OM_GoogleCalendars gCal,
            OM_WorkersCalendars wCal,
            IEasyAttachmentManager eaMng, Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = false;

            //Recupero il RecurrenceId dei Commitments
            int recurrenceId = OM_Commitments.GetRecurrenceIDFromRecurrenceEvent(company, accountGoogle.WorkerId, wCal.IdGoogleCalendar, gCalEv.IdEvent);
            if (recurrenceId == 0)
                return ret;

            //Recupero il RecurrenceId dell'evento padre di Google
            string strRecurringId = gCalEv.IdEvent;

            //Recupero il record dell'evento padre nella tabella OM_GoogleEvents
            OM_GoogleEvents gEvMaster = OM_GoogleEvents.GetGoogleEvent(company, gCalEv.IdEvent);

            //Recupero il record del primo evento appartenente alla ricorrenza nella tabella OM_Commitments
            OM_Commitments commSequence = OM_Commitments.GetNonExceptionCommitmentFromRecurrenceId(company, accountGoogle.WorkerId, wCal.SubId, recurrenceId);

            //Controllo l'esistenza ei record ricercati
            if ((gEvMaster == null) || (gooEv.Updated == null) || (commSequence == null))
                return ret;   //TODO PIPPO gestire errore evento non trovato!!!

            //Recupero il record dell'evento precedente nella tabella OM_CommitmentsWorkers
            OM_CommitmentsWorkers commworSequence = OM_CommitmentsWorkers.GetCommitmentsWorkers(company, commSequence.CommitmentId, accountGoogle.WorkerId);
            if (commworSequence == null)
                return ret;

            //Controllo se è il propietario dell'evento
            bool isOwner = commworSequence.BoolIsOwner;

            //Controllo se è il creatore dell'evento
            bool isCreator = (commSequence.TBCreatedID == accountGoogle.WorkerId);

            //Calcolo la data di modifica dell'evento del Google Calendar 
            DateTime aGoogleLastUpdate = new DateTime(
                gooEv.Updated.Value.Year,
                gooEv.Updated.Value.Month,
                gooEv.Updated.Value.Day,
                gooEv.Updated.Value.Hour,
                gooEv.Updated.Value.Minute,
                gooEv.Updated.Value.Second
                );

            //Calcolo la data di modifica dell'evento nellla tabella OM_GoogleEvents 
            DateTime aVSLastUpdate = new DateTime(
                gEvMaster.Updated.Year,
                gEvMaster.Updated.Month,
                gEvMaster.Updated.Day,
                gEvMaster.Updated.Hour,
                gEvMaster.Updated.Minute,
                gEvMaster.Updated.Second
                );

            //Calcolo la data di modifica dell'evento nellla tabella OM_Commitments 
            DateTime aVSCommitmentLastUpdate = new DateTime(
                commSequence.TBModified.Year,
                commSequence.TBModified.Month,
                commSequence.TBModified.Day,
                commSequence.TBModified.Hour,
                commSequence.TBModified.Minute,
                commSequence.TBModified.Second
                );


            //Controllo se gli eventi sono stati modificati in momenti diversi
            if (aGoogleLastUpdate < aVSCommitmentLastUpdate)
            {
                //Commitment più recente di Google Calendar

                //Aggiorno l'evento ricorrente in Google Calendar
                UpdateRecurringEventGoogle(company, service, gooEv, gCal, gCalEv, accountGoogle, commSequence, wCal, eaMng, excLogger);
            }
            else if ((aGoogleLastUpdate > aVSCommitmentLastUpdate) && (aGoogleLastUpdate >= aVSLastUpdate))
            {
                //Evento di Google Calendar più recente del Commitment e del record nella tabella OM_GoogleEvents (controllo in caso di record condiviso)

                //Controllo se è possibile modificare la sequenza in VS in base alle Facilities ad esso collegate
                bool checkFacilities = CheckRecurrenceFacility(company, service, gooEv, strRecurringId, wCal, gCalEv, accountGoogle, excLogger);

                //Controllo se sono il propietario e la modifica delle Facilities è consentita
                if ((isOwner || isCreator) && (checkFacilities))
                {
                    //Aggiorno la ricorrenza in VS
                    ret |= UpdateRecurringEventVS(company, service, gooEv, recurrenceId, strRecurringId, accountGoogle, gCal, wCal, eaMng, excLogger);
                }
                else
                {
                    //Cancello e ricreo la sequenza in Google Calendar con le impostazioni precedenti alla modifica attraverso i valori dei Commitment in VS

                    //Elimino l'evento padre in Google Calendar
                    ret |= RemoveEventGoogle(company, service, gCalEv, excLogger);

                    //Elimino i record di collegamento tra Google Calendar e VS
                    ret |= RemoveSynchronizationRecurringEventVS(company, gCalEv, accountGoogle, wCal, excLogger);

                    //Creo una nuova sequenza in Google Calendar con le impostazioni dei commitments appartenenti alla ricorrenza
                    ret |= CreateRecurrenceEventGoogle(company, service, true, accountGoogle, gCal, commSequence, wCal, eaMng, excLogger);
                }

            }

            return ret;
        }

        #endregion

        #region Facilities
        //-------------------------------------------------------------------------------
        //Controllo la disponibilità delle risorse per l'evento di Google Calendar
        public static bool CheckFacility(TbSenderDatabaseInfo cmp, Event gooEvent, OM_Commitments comm, bool checkCommitment,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            try
            {
                DateTime start = gooEvent.Start.DateTime.Value;
                DateTime end = gooEvent.End.DateTime.Value;

                //Recupero le Facilities collegate al Commitment
                List<OM_FacilitiesDetails> list = OM_FacilitiesDetails.GetCommitmentFacilities(cmp, comm.CommitmentId);
                if (list == null)
                    return ret;

                foreach (OM_FacilitiesDetails fd in list)
                {
                    //Calcolo lo scarto temporale tra Facility e Commitment
                    TimeSpan startDaySpan = comm.CommitmentDate.Date - fd.StartDate.Date;
                    TimeSpan endDaySpan = fd.EndDate.Date - comm.CommitmentDate.Date;

                    TimeSpan startTimeSpan = comm.StartTime.TimeOfDay - fd.StartTime.TimeOfDay;
                    TimeSpan endTimeSpan = fd.EndTime.TimeOfDay - comm.EndTime.TimeOfDay;

                    DateTime newStart = start - (startDaySpan + startTimeSpan);
                    DateTime newEnd = end + (endDaySpan + endTimeSpan);

                    OM_Facilities facility = OM_Facilities.GetFacility(cmp, fd.FacilityId);
                    if (facility.BoolAllowOverlap)
                        continue;

                    //Recupero gli impegni della Facility nel periodo di interesse
                    List<OM_FacilitiesDetails> listDetails = OM_FacilitiesDetails.GetFacilitiesFromDateRange(cmp, facility.FacilityId, newStart, newEnd);
                    if (listDetails == null)
                        return ret;

                    foreach (OM_FacilitiesDetails facilitydetails in listDetails)
                    {
                        //Controllo se la risorsa è legata al commitment in modifica
                        if ((fd.CommitmentId == facilitydetails.CommitmentId) && (checkCommitment))
                            continue;

                        //Cotrollo se gli impegni della facility non si intrecciano con il periodo dell'evento 
                        if (((newStart.TimeOfDay <= facilitydetails.StartTime.TimeOfDay) && (facilitydetails.StartTime.TimeOfDay <= newEnd.TimeOfDay)) ||
                            ((facilitydetails.StartTime.TimeOfDay <= newStart.TimeOfDay) && (newStart.TimeOfDay <= facilitydetails.EndTime.TimeOfDay)) ||
                            ((facilitydetails.StartTime.TimeOfDay <= newEnd.TimeOfDay) && (newEnd.TimeOfDay <= facilitydetails.EndTime.TimeOfDay)) ||
                            ((newStart.TimeOfDay <= facilitydetails.EndTime.TimeOfDay) && (facilitydetails.EndTime.TimeOfDay <= newEnd.TimeOfDay)))
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                ret = false;
                string a = ex.Message;
                if (excLogger != null)
                    excLogger(ex);
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Controllo la disponibilità delle risorse della ricorrenza
        public static bool CheckRecurrenceFacility(TbSenderDatabaseInfo cmp, CalendarService service, Event gooEvent, string recurrenceId,
            OM_WorkersCalendars wCal,
            OM_GoogleCalendarsEvents gCalEv, OM_GoogleAccounts accountGoogle,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;

            //Recupero gli eventi in Google Calendar appartenenti alla ricorrenza
            Events instances = service.Events.Instances(wCal.IdGoogleCalendar, recurrenceId).Execute();

            //Ciclo su tutti gli eventi della sequenza 
            foreach (Event childEvent in instances.Items)
            {
                bool check = false;

                //Controllo se l'evento esiste e non è stato cancellato
                if ((childEvent == null) ||
                    (childEvent.Status == null) ||
                    (childEvent.Status.ToString() == "cancelled")) // Se l'evento è cancellato viene gestita nelle eccezioni della ricorrenza
                    continue;

                //Recupero i record dalle tabelle OM_GoogleEvents e OM_GoogleCalendarsEvents
                OM_GoogleEvents gChildEv = OM_GoogleEvents.GetGoogleEvent(cmp, childEvent.Id);
                OM_GoogleCalendarsEvents gChildCalEv = OM_GoogleCalendarsEvents.GetGoogleCalendarEvent(cmp, accountGoogle.WorkerId, childEvent.Id, wCal.IdGoogleCalendar);
                if ((gChildEv != null) && (gChildCalEv != null))
                {
                    //Recupero il commitment dell'evento corrispondenete
                    OM_Commitments commChild = OM_Commitments.GetCommitment(cmp, gChildCalEv.CommitmentId);
                    if (commChild != null)
                    {
                        //Setto l'evento come processato
                        check = true;
                        //Controllo la disponibilità delle facilities per l'evento modificato in Google Calendar
                        ret &= CheckFacility(cmp, childEvent, commChild, true, excLogger);
                    }
                }

                //Se non ho trovato l'evento in VS
                if (!check)
                {
                    //Recupero il primo evento appartenente alla serie non Exception
                    OM_Commitments commMain = OM_Commitments.GetNonExceptionCommitmentFromRecurrenceId(cmp, accountGoogle.WorkerId, wCal.SubId, gCalEv.ParentCommitmentId);
                    if (commMain == null)
                        return false;

                    //Controllo la disponibilità delle facilities per il nuovo evento della sequenza in Google Calendar
                    ret &= CheckFacility(cmp, childEvent, commMain, false, excLogger);
                }

                
            }

            return ret;
        }

        //-------------------------------------------------------------------------------
        //Creazione di un'impegno delle Facility collegate al Commitment
        public static bool NewFacilities(MZP_CompanyEntities db, Event gooEvent, List<OM_FacilitiesDetails> listFD, OM_Commitments commMain, int newCommitmentId, OM_GoogleAccounts accountGoogle,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            try
            {
                DateTime start = gooEvent.Start.DateTime.Value;
                DateTime end = gooEvent.End.DateTime.Value;

                //Ciclo per tutte le Facilities della lista
                foreach (OM_FacilitiesDetails fd in listFD)
                {
                    //Calcolo lo scarto temporale tra Facility e Commitment
                    TimeSpan startDaySpan = commMain.CommitmentDate.Date - fd.StartDate.Date;
                    TimeSpan startTimeSpan = commMain.StartTime.TimeOfDay - fd.StartTime.TimeOfDay;

                    TimeSpan endDaySpan = fd.EndDate.Date - commMain.CommitmentDate.Date;
                    TimeSpan endTimeSpan = fd.EndTime.TimeOfDay - commMain.EndTime.TimeOfDay;

                    DateTime newStart = start - (startDaySpan + startTimeSpan);
                    DateTime newEnd = end + (endDaySpan + endTimeSpan);

                    //Creo il record nella tabella OM_FacilitiesDetails per l'impegno delle facilities per il commitment
                    OM_FacilitiesDetails newFacility = OM_FacilitiesDetails.NewItem(db, fd, newStart, newEnd, newCommitmentId, excLogger);
                }
            }
            catch (Exception ex)
            {
                ret = false;
                string a = ex.Message;
                if (excLogger != null)
                    excLogger(ex);
            }
            return ret;
        }

        //-------------------------------------------------------------------------------
        //Aggiorno le Facility collegate al Commitment
        public static bool UpdateFacility(MZP_CompanyEntities db, Event gooEvent, OM_Commitments comm, OM_GoogleAccounts accountGoogle,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            bool ret = true;
            try
            {
                DateTime start = gooEvent.Start.DateTime.Value;
                DateTime end = gooEvent.End.DateTime.Value;

                //Recupero le Facilities collegate al Commitment
                List<OM_FacilitiesDetails> list = OM_FacilitiesDetails.GetCommitmentFacilities(db, comm.CommitmentId);
                if (list == null)
                    return ret;

                foreach (OM_FacilitiesDetails fd in list)
                {
                    //Calcolo lo scarto temporale tra Facility e Commitment
                    TimeSpan startDaySpan = comm.CommitmentDate.Date - fd.StartDate.Date;
                    TimeSpan startTimeSpan = comm.StartTime.TimeOfDay - fd.StartTime.TimeOfDay;

                    TimeSpan endDaySpan = fd.EndDate.Date - comm.CommitmentDate.Date;
                    TimeSpan endTimeSpan = fd.EndTime.TimeOfDay - comm.EndTime.TimeOfDay;

                    DateTime newStart = start - (startDaySpan + startTimeSpan);
                    DateTime newEnd = end + (endDaySpan + endTimeSpan);

                    //Aggiorno il record della tabella OM_FacilitiesDetails
                    OM_FacilitiesDetails newFacility = OM_FacilitiesDetails.UpdateItem(db, fd, newStart, newEnd, excLogger);
                }
            }
            catch (Exception ex)
            {
                ret = false;
                string a = ex.Message;
                if (excLogger != null)
                    excLogger(ex);
            }
            return ret;
        }
        #endregion
    }
}
