using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.TbHermesBL.Config;
using Microarea.TaskBuilderNet.TbHermesBL;
using System.IO;
using System.Diagnostics;
using Limilabs.Client.IMAP;
using Limilabs.Client.POP3;
using Limilabs.Client.SMTP;
using Limilabs.Mail;
using Limilabs.Mail.Headers;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    static class MailFunctions
    {
        public static int itemsToDownload; // dovrebbe essere thread safe perché l'accesso è semaforizzato

        public static string MailProcess(TbSenderDatabaseInfo cmp, IEasyAttachmentManager eaMng, HermesEngine.TreatExceptionDelegate ted, MailLogListener logTrace, LoginManagerConnector lmc)
        {
            string  sReturn = "_MAIL";

            // FABIO: popoliamo la lista di tutti gli account, per ora si basa su un'unica tabella
            // in futuro potrebbe essere diverso con una tabella dedicata solo agli account dei clienti
            List<OM_WorkerAccounts> allAccounts = new List<OM_WorkerAccounts>();
            allAccounts.AddRange(OM_WorkerAccounts.GetWorkerAccounts(cmp));


            ILockerClient lockerClient = (ILockerClient)eaMng;

            //------------------------------------------------------------------------------------------
            // scarica i messaggi dagli account
            foreach (OM_WorkerAccounts account in allAccounts)
            {
                Boolean bSendAlert = false;
                Boolean bReceivedAlert = false;

                if (account.BoolIsOwner)
                {
                    ////////////////////////////////////
                    // RICEZIONE POSTA IN ARRIVO

                    try
                    {
                        logTrace.WriteLine("- receive from=" + account.ReceiveServerName + "; user=" + account.ReceiveServerUserName, "Hermes");
                        if (string.IsNullOrEmpty(account.ReceiveServerName))
                            continue;

                        // li pesco tutti assieme in una volta sola, così riduco drasticamente il numero di query
                        List<OM_MailMessagePop3> allPop3Seen = null;
                        //if (account.EnumAccountType == MailServerType.Pop3)
                        //{
                        string pop3Host = account.ReceiveServerName;
                        string hostUser = account.ReceiveServerUserName;
                        allPop3Seen = OM_MailMessagePop3.GetAlreadyDownloadedByAccount(cmp, pop3Host, hostUser);
                        DateTime dateSearch = OM_MailMessagePop3.GetReceivedMailMessageLastDate(cmp, pop3Host, hostUser);

                        //}

                        IEnumerable<MailHolder> mailList = DownloadMessages(account, allPop3Seen, dateSearch, true);
                        logTrace.WriteLine("-- count from server=" + mailList.Count(), "Hermes");


                        int nextKey = -1;
                        // la transazione sulla singola mail
                        // (e cascata di relazioni, sennò la transazione non servirebbe proprio),
                        // invece di metterla su tutto, così se una fallisce le altre almeno possono 
                        // essere salvate
                        foreach (MailHolder mailItem in mailList)
                        {
                            try
                            {
                                if (nextKey == -1) // così la faccio dopo la valutazione dell'enumerator
                                    nextKey = ConnectionHelper.GetNextKey(cmp, itemsToDownload, lockerClient);

                                logTrace.WriteLine("--- save message=" + mailItem.Mail.MessageID, "Hermes");
                                bool bSaved = OM_MailMessages.SaveNewMessage(cmp, mailItem, nextKey++, false, ted, logTrace, eaMng);
                                if (bSaved)
                                {
                                    bReceivedAlert = true;

                                    if ((OM_WorkerAccounts.IsAutomaticResponseActive(cmp, account)) && (mailItem.Mail.From.Count > 0))
                                        AutomaticResponse(cmp, eaMng, ted, account, mailItem.Mail.Subject, mailItem.Mail.From[0].Address, mailItem.Mail.From[0].Name);

                                    // GESTIONE BALLOONS
                                    string sUser = OM_Workers.GetUserName(cmp, account.MailAccountID);

                                    lmc.SendBalloonToUser(mailItem.Mail.Subject, sUser);  // come stringa da stampare uso l'oggetto del messaggio
                                }
                            }
                            catch (Exception ex) // così non blocco l'account successivo
                            {
                                ted(ex);
                            }
                        }
                    }
                    catch (Exception ex) // così non blocco l'account successivo
                    {
                        ted(ex);
                    }

                    ////////////////////////////////////
                    // RICEZIONE POSTA INVIATA      (IMAP)
                    if (account.BoolIsImportOutbox && (account.EnumAccountType == MailServerType.Imap))
                    {
                        try
                        {
                            logTrace.WriteLine("- receive outbox (imap only)", "Hermes");

                            // li pesco tutti assieme in una volta sola, così riduco drasticamente il numero di query
                            List<OM_MailMessagePop3> allPop3Sent = null;
                            //if (account.EnumAccountType == MailServerType.Pop3)
                            //{
                            string pop3Host = account.ReceiveServerName;
                            string hostUser = account.ReceiveServerUserName;
                            allPop3Sent = OM_MailMessagePop3.GetAlreadySentByAccount(cmp, pop3Host, hostUser);
                            DateTime dateSearch = OM_MailMessagePop3.GetSentMailMessageLastDate(cmp, pop3Host, hostUser);

                            IEnumerable<MailHolder> mailList = DownloadMessages(account, allPop3Sent, dateSearch, false);

                            logTrace.WriteLine("-- count from server outbox" + mailList.Count(), "Hermes");
                            int nextKey = -1;
                            // la transazione sulla singola mail
                            // (e cascata di relazioni, sennò la transazione non servirebbe proprio),
                            // invece di metterla su tutto, così se una fallisce le altre almeno possono 
                            // essere salvate
                            foreach (MailHolder mailItem in mailList)
                            {
                                try
                                {
                                    string headerID = mailItem.Mail.Document.Root.Headers["x-mailmessageid"];
                                    if (headerID.IsNullOrEmpty())
                                    {
                                        if (nextKey == -1) // così la faccio dopo la valutazione dell'enumerator
                                            nextKey = ConnectionHelper.GetNextKey(cmp, itemsToDownload, lockerClient);

                                        logTrace.WriteLine("--- save message=" + mailItem.Mail.MessageID, "Hermes");
                                        OM_MailMessages.SaveNewMessage(cmp, mailItem, nextKey++, true, ted, logTrace, eaMng);
                                        bSendAlert = true;
                                    }
                                    else
                                    {
                                        Int32 MailMessageId = Convert.ToInt32(headerID);
                                        // scrittura del OM_MailMessagePop3.codice email arrivata corretta dal server di posta

                                        logTrace.WriteLine("--- update message=" + mailItem.Mail.MessageID, "Hermes");
                                        OM_MailMessages.UpdateMailPop3MessageId(cmp, MailMessageId, mailItem, ted);
                                        bSendAlert = true;
                                    }
                                }
                                catch (Exception ex) // così non blocco l'account successivo
                                {
                                    ted(ex);
                                }
                            }
                        }
                        catch (Exception ex) // così non blocco l'account successivo
                        {
                            ted(ex);
                        }
                    }
                }
                ////////////////////////////////////
                // INVIO POSTA IN USCITA
                try
                {
                    logTrace.WriteLine("- send to=" + account.SendServerName + "; user=" + account.SendServerUserName, "Hermes");

                    if (string.IsNullOrEmpty(account.SendServerName))
                    {
                        if ((bSendAlert) || (bReceivedAlert))
                            OM_WorkersAlerts.UpdateMailWorkersAlerts(cmp, account.WorkerId, bSendAlert, bReceivedAlert, ted);
                        continue;
                    }

                   List<OM_MailMessages> msgToSend = OM_MailMessages.GetMessagesToSendByWorkerAccount(cmp, account.MailAccountID, account.MailAccountSubID);
                    logTrace.WriteLine("-- count from db=" + msgToSend.Count, "Hermes");
                   if (msgToSend.Count == 0)
                    {
                        if ((bSendAlert) || (bReceivedAlert))
                            OM_WorkersAlerts.UpdateMailWorkersAlerts(cmp, account.WorkerId, bSendAlert, bReceivedAlert, ted);
                        continue;
                    }

                    // TODO se campi SMTP non definiti nemmeno in console, loggare messaggio d'errore

                    using (Smtp smtp = new Smtp()) // Dispose pattern
                    {

                        smtp.Connect(account.SendServerName, account.SendServerPort, account.BoolSmtpSsl);
                        if (!account.SendServerUserName.IsNullOrEmpty())
                        {
                            smtp.UseBestLogin(
                                account.SendServerUserName,
                                account.SendServerPassword);
                        }


                        foreach (OM_MailMessages msg in msgToSend)
                        {
                            // reperire account opportuni in base a campi FKs
                            // se non vi sono impostati server smtp, usare main
                            // se non è popolato nemmeno quello, skippare
                            try
                            {
                                List<OM_MailMessageAddresses> addresses = msg.FetchAddresses(cmp); // fa una query unica

                                OM_MailMessageAddresses addFrom = OM_MailMessageAddresses.FilterByType(addresses, OM_MailMessages.MailAddressType.F).FirstOrDefault();
                                if (addFrom == null)
                                    continue; //TODO cosa??

                                List<MailBox> toList
                                    = OM_MailMessageAddresses.ToMailboxes(OM_MailMessageAddresses.FilterByType(addresses, OM_MailMessages.MailAddressType.T));
                                List<MailBox> ccList
                                    = OM_MailMessageAddresses.ToMailboxes(OM_MailMessageAddresses.FilterByType(addresses, OM_MailMessages.MailAddressType.C));
                                List<MailBox> bccList
                                    = OM_MailMessageAddresses.ToMailboxes(OM_MailMessageAddresses.FilterByType(addresses, OM_MailMessages.MailAddressType.B));

                                if (toList.Count == 0 && ccList.Count == 0 && bccList.Count == 0)
                                    continue; //TODO cosa??
                                
                                OM_MailMessageAddresses addReplyTo = OM_MailMessageAddresses.FilterByType(addresses, OM_MailMessages.MailAddressType.R).FirstOrDefault();

                                MailBuilder builder = new MailBuilder();
                                builder.Html = msg.FetchHtmlText(cmp);
                                if (builder.Html == null && msg.PlainText != null)
                                    builder.Text = msg.PlainText;
                                builder.Subject = msg.MessageSubject;

                                if (msg.BoolResponseConfirm)
                                    builder.RequestReadReceipt();

                                builder.From.Add(new MailBox(addFrom.MailAddress, addFrom.MailLongName));
                                if (addReplyTo != null)
                                    builder.ReplyTo.Add(new MailBox(addReplyTo.MailAddress, addReplyTo.MailLongName));

                                foreach (MailAddress mailAdd in toList)
                                    builder.To.Add(mailAdd);
                                foreach (MailAddress mailAdd in ccList)
                                    builder.Cc.Add(mailAdd);
                                foreach (MailAddress mailAdd in bccList)
                                    builder.Bcc.Add(mailAdd);

                                // TODO aggiungere reperimento attachments via EasyAttachment
                                List<OM_MailMessageAttachments> attachments = msg.FetchAttachments(cmp);
                                if (attachments != null && attachments.Count != 0)
                                {
                                    foreach (OM_MailMessageAttachments attRec in attachments)
                                    {
                                        byte[] attData = eaMng.GetAttachmentData(attRec);
                                        if (attData == null)
                                            continue; // TODO log?

                                        if (attRec.AttachmentTag.IsNullOrEmpty())
                                        {
                                            Limilabs.Mail.MIME.MimeData mime = builder.AddAttachment(attData);
                                            mime.FileName = attRec.AttachmentFileName;
                                        }
                                        else
                                        {
                                            Limilabs.Mail.MIME.MimeData mime = builder.AddVisual(attData);
                                            mime.FileName = attRec.AttachmentFileName;
                                            string aCidTag = "";
                                            aCidTag = attRec.AttachmentTag.Replace("cid:", "");
                                            mime.ContentId = aCidTag;
                                        }
                                    }
                                }

                                // riferimento MessageID utile per la Pop3
                                builder.AddCustomHeader("x-mailmessageid", msg.MailMessageID.ToString());

                                // campo inReplyTo
                                if ((msg.ParentMessageID.Value > 0) && (OM_MailMessagePop3.GetMailMessagePop3FromMessageID(cmp, msg.ParentMessageID.Value).Count > 0))
                                    builder.InReplyTo = OM_MailMessagePop3.GetMailMessagePop3FromMessageID(cmp, msg.ParentMessageID.Value).FirstOrDefault().Codice;

                                IMail email = builder.Create();

                                //IMail email = Mail
                                //.Html(msg.HTMLText)
                                //.Subject(msg.MessageSubject)
                                //.From(new MailBox(msg.ReplyTo, msg.FROM))
                                //.To(listTo) // (msg.TO, msg.TO))
                                //.Create();

                                string tableName = "OM_MailMessages";
                                string msgLockKey = msg.MailMessageID.ToString(CultureInfo.InvariantCulture);
                                try
                                {
                                    if (lockerClient.LockRecord(cmp.Company, tableName, msgLockKey))
                                    {
                                        smtp.SendMessage(email);

                                        // qui devo segnare sul record che è stato inviato, ora e risultato
                                        //msg.BoolIsSent = true;
                                        //msg.SentDateTime = DateTime.Now;
                                        //msg.BoolIsQueued = false;
                                        // il OM_MailMessagePop3.codice e' impostato sul salvaggio delle email in arrivo, perche' e' quello giusto

                                        // save
                                        msg.UpdateSentMessage(cmp, ted);
                                        bSendAlert = true;
                                    }
                                }
                                finally
                                {
                                    lockerClient.UnlockRecord(cmp.Company, tableName, msgLockKey);
                                }
                            }
                            catch (Exception ex)
                            {
                                ted(ex);
                                sReturn += ex.Message + "\r\n";
                            };
                        };
                        smtp.Close(); // dovrebbe già farlo Dispose, ma per sicurezza lo lascio (non sia mai siano non-standard)
                    }

                    // NdF: Userei una coda asincrona per spedire: non serve attendere di aver scaricato prima di spedire (miglioria successiva)
                }
                catch (Exception ex)
                {
                    ted(ex);
                };

                if ((bSendAlert) || (bReceivedAlert))
                    OM_WorkersAlerts.UpdateMailWorkersAlerts(cmp, account.WorkerId, bSendAlert, bReceivedAlert, ted);
                
            } // for each account
            sReturn += "_OK";

            return sReturn;
        }


        private static IOM_MailAccount GetMainAccount(MailServerSettings serverSettings)
        {
            // carica valori letti da file di configurazione prodotto da console
            OM_WorkerAccounts mainAccount = new OM_WorkerAccounts();

            mainAccount.EnumAccountType = serverSettings.AccountType;// "Pop3";
            mainAccount.ReceiveServerName = serverSettings.ReceiveServerName; // "srv-exc7.microarea.it";
            mainAccount.ReceiveServerUserName = serverSettings.ReceiveServerUserName; // "sasso";
            mainAccount.ReceiveServerPassword = serverSettings.ReceiveServerPassword; // "********";
            mainAccount.MailAddress = serverSettings.EMailAddress; // "federico.sasso@microarea.it";
            mainAccount.SendServerName = serverSettings.SmtpServer; // "srv-exc7.microarea.it";
            mainAccount.SendServerPort = serverSettings.SmtpPort;
            mainAccount.ReceiveServerPort = serverSettings.Port;
            mainAccount.BoolReceiveSsl = serverSettings.ReceiveSsl;
            mainAccount.BoolSmtpSsl = serverSettings.SmtpSsl;
            mainAccount.WorkerId = -1;
            mainAccount.AccountSubID = -1; // already is
            mainAccount.SendServerUserName = serverSettings.ReceiveServerUserName; // TODO specialize?
            mainAccount.SendServerPassword = serverSettings.ReceiveServerPassword; // TODO specialize?
            return mainAccount;
        }

        private static IEnumerable<MailHolder> DownloadMessages(IOM_MailAccount account, List<OM_MailMessagePop3> allPop3Seen, DateTime dateLastMail, bool bInBox)
        {
            string srv = account.ReceiveServerName;
            string usr = account.ReceiveServerUserName;
            string pwd = account.ReceiveServerPassword;
            int port = account.ReceiveServerPort;
            if (port == 0)
                port = MailServerSettings.DefaultPop3Port; // 110 default for POP3
            // POP3 - port 110
            // IMAP - port 143
            // SMTP - port 25
            // HTTP - port 80
            // Secure SMTP (SSMTP) - port 465
            // Secure IMAP (IMAP4-SSL) - port 585
            // IMAP4 over SSL (IMAPS) - port 993
            // Secure POP3 (SSL-POP) - port 995
            bool useSSL = account.BoolReceiveSsl;
            //if (port == MailServerSettings.DefaultPop3SecurePort) //  995
            //    useSSL = true;

            switch (account.EnumAccountType)
            {
                case MailServerType.Pop3:

                    // TODO POP3 riscarica ogni volta, occorre tenere traccia dei valori già scaricati in modo efficiente!
                    using (Pop3 pop3 = new Pop3())
                    {
                        pop3.Connect(srv, port, useSSL);
                        pop3.Login(usr, pwd);	// You can also use: LoginAPOP, LoginPLAIN, LoginCRAM, LoginDIGEST methods,
                        // or use UseBestLogin method if you want Mail.dll to choose for you.

                        List<string> uidList = pop3.GetAll(); // Get unique-ids of all messages.

                        // ancora bucato: non viene eseguito ancora e il count rimane a zero! TODO Spostare/Procrastinare
                        itemsToDownload = uidList.Count; // serve per prenotare un intervallo di chiavi primarie (l'enumerator tornato dal metodo non ha un count)

                        foreach (string uid in uidList)
                        {
                            if (allPop3Seen != null && allPop3Seen.Count != 0
                                && allPop3Seen.Find(x => x.Codice == uid) != null)
                            {
                                // già scaricato
                                itemsToDownload--;
                                continue;
                            }

                            string eml = pop3.GetMessageByUID(uid);
                            IMail eMail = new MailBuilder()
                                .CreateFromEml(eml);

                            // fabio: trucco per filtrare email inviate che arrivano nell'inbox con pop3
                            bool bPop3IsForInbox = true;
                            if (eMail.From.FirstOrDefault().Address.ToLower().IndexOf(usr.ToLower()) >= 0)
                                bPop3IsForInbox = false;

                            OM_MailMessagePop3 pop3Uid = OM_MailMessagePop3.CreateItem(srv, usr, uid, eMail.Date.Value, bPop3IsForInbox);

                            yield return new MailHolder() { Mail = eMail, Account = account, Pop3Uid = pop3Uid };
                        }
                        pop3.Close(); // non lo fa già il Dispose? Nel dubbio abbiano fatto male la library, lo lascio come nei samples di Limilabs
                    }
                    break;

                case MailServerType.Imap:
                    using (Imap imap = new Imap())
                    {
                        imap.Connect(srv, port, useSSL);
                        imap.Login(usr, pwd);	// You can also use: LoginPLAIN, LoginCRAM, LoginDIGEST, LoginOAUTH methods,
                        // or use UseBestLogin method if you want Mail.dll to choose for you.

                        // email in arrivo o inviate?
                        if (bInBox)
                            imap.SelectInbox(); // You can select other folders, e.g. Sent folder: imap.Select("Sent");
                        else
                            // trovo la cartella delle email spedite
                            imap.Select(getSentItemsFolder(account.StrOutboxName, imap));


                        DateTime dateSearch = DateTime.Now.AddDays(-1 * account.IntDayBeforeFromToday);

                        if (dateSearch < dateLastMail)
                        {
                            dateSearch = dateLastMail.AddDays(-1 * account.IntDayBeforeSearch);
                        }
                        var query = Expression.And(
                                    Expression.Since(dateSearch)
                                    );

                        List<long> uids = imap.Search(query);

                        itemsToDownload = uids.Count; // serve per prenotare un intervallo di chiavi primarie (l'enumerator tornato dal metodo non ha un count)

                        foreach (long uid in uids)
                        {
                            OM_MailMessagePop3 pop3Uid = null;
                            IMail eMail2 = null;
                            IMail eMail = null;

                            try
                            {
                                string eml2 = imap.PeekHeadersByUID(uid);

                                eMail2 = new MailBuilder().CreateFromEml(eml2);

                                if (eMail2.MessageID.IsNullOrEmpty())
                                {
                                    // nullo?
                                    itemsToDownload--;
                                    continue;
                                }

                                if (allPop3Seen != null && allPop3Seen.Count != 0
                                    && allPop3Seen.Find(x => x.Codice == eMail2.MessageID) != null)
                                {
                                    // già scaricato
                                    itemsToDownload--;
                                    continue;
                                }

                                string eml = imap.PeekMessageByUID(uid);

                                eMail = new MailBuilder().CreateFromEml(eml);

                                pop3Uid = OM_MailMessagePop3.CreateItem(srv, usr, eMail.MessageID.ToString(), eMail.Date, bInBox);
                            }
                            catch (Exception e) 
                            {
                                String error = e.ToString();
                                itemsToDownload--;
                                continue;
                            }

                            yield return new MailHolder() { Mail = eMail, Account = account, Pop3Uid = pop3Uid };

                            //yield return new MailHolder() { Mail = eMail, Account = account };
                        }
                        imap.Close(); // non lo fa già il Dispose? Nel dubbio abbiano fatto male la library, lo lascio come nei samples di Limilabs
                    }
                    break;

                default:
                    //throw new NotImplementedException();
                    break;
            }
        }


        //---------------------------------------------------------------------
        public static FolderInfo getSentItemsFolder(string aStrOutboxName, Imap aImap)
        {
            string CustomOutboxFolder = aStrOutboxName;
            bool canUseCommonFolders = false;
            if (CustomOutboxFolder.IsNullOrEmpty() || CustomOutboxFolder.IsNullOrWhiteSpace())
            {
                // l'utente non ha scelto un nome specifico
                // per cui prima mi chiedo se posso usare lo standard imap
                List<ImapExtension> extensions = aImap.SupportedExtensions();
                canUseCommonFolders = extensions.Contains(ImapExtension.XList)
                                               || extensions.Contains(ImapExtension.SpecialUse);
                // poi imposto un semplice default per sicurezza
                CustomOutboxFolder = "Sent";

            }
            if (!canUseCommonFolders)
            {
                // cerco la cartella scelta dall'utente o quella di default
                List<FolderInfo> fiList = aImap.GetFolders(); // You can select other folders, e.g. Sent folder: imap.Select("Sent");
                foreach (FolderInfo fid in fiList)
                {
                    if (fid.Name.ToLower().IndexOf(CustomOutboxFolder.ToLower()) >= 0)
                        return fid;
                }
            }

            // posso usare la standardizzazione imap oppure non posso ma nel for sopra non ho trovato niente
            CommonFolders folders = new CommonFolders(aImap.GetFolders());
            return folders.Sent;
        }


        //---------------------------------------------------------------------
        public static bool AutomaticResponse(TbSenderDatabaseInfo cmp, IEasyAttachmentManager eaMng, HermesEngine.TreatExceptionDelegate ted, OM_WorkerAccounts account, string aStrSubject, string aStrAddress, string aStrLongName)
        {
            //SendMessageResult result;
            ////////////////////////////////////
            // RISPOSTA AUTOMANTICA
            try
            {
                List<OM_MailFooter> ResponseMail = OM_MailFooter.GetAutomaticResponse(cmp, account.WorkerId);
                if (ResponseMail != null && ResponseMail.Count != 0)
                {
                    // TODO se campi SMTP non definiti nemmeno in console, loggare messaggio d'errore
                    using (Smtp smtp = new Smtp()) // Dispose pattern
                    {
                        OM_MailFooter respRec  = ResponseMail.ElementAt(0);
                    
                        smtp.Connect(account.SendServerName, account.SendServerPort, account.BoolSmtpSsl);
                        smtp.UseBestLogin(
                            account.SendServerUserName,
                            account.SendServerPassword);

                        MailBuilder builder = new MailBuilder();
                        builder.Html = respRec.HTMLText;
                        builder.Subject = "Re: " + aStrSubject + " - Risposta di cortesia";

                        // TODO aggiungere reperimento attachments via EasyAttachment
                        if (respRec.MailFooterEasyAttachmentID.Value!=0)
                        {

                            byte[] attData = eaMng.GetAttachmentData(respRec.MailFooterEasyAttachmentID);
                            if (attData != null)
                            {
                                Limilabs.Mail.MIME.MimeData mime = builder.AddVisual(attData);
                                mime.FileName = respRec.AttachmentFileName;
                                string aCidTag = "";
                                aCidTag = respRec.AttachmentTag.Replace("cid:", "");
                                mime.ContentId = aCidTag;
                            }
                            
                        }

                        //if (msg.BoolResponseConfirm)
                        //    builder.RequestReadReceipt();

                        builder.From.Add(new MailBox(account.MailAddress, account.AccountName));
                        builder.To.Add(new MailBox(aStrAddress, aStrLongName));

                        IMail email = builder.Create();
                        smtp.SendMessage(email);

                        smtp.Close(); // dovrebbe già farlo Dispose, ma per sicurezza lo lascio (non sia mai siano non-standard)
                    }
                }
            }
            catch (Exception ex)
            {
                ted(ex);
            };

            //return (result.Status == SendMessageStatus.Success);
            return true;
        }

    }
}
