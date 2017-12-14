using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Transactions;
using Limilabs.Mail;
using Limilabs.Mail.Headers;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
	public partial class OM_MailMessages
	{
		//-------------------------------------------------------------------------------
		public bool BoolSystemMessage
		{
			get { return this.SystemMessage == "1"; }
			set { this.SystemMessage = value ? "1" : "0"; }
		}
		public bool BoolIsDraft
		{
			get { return this.IsDraft == "1"; }
			set { this.IsDraft = value ? "1" : "0"; }
		}
		public bool BoolIsSent
		{
			get { return this.IsSent == "1"; }
			set { this.IsSent = value ? "1" : "0"; }
		}
		public bool BoolIsQueued
		{
			get { return this.IsQueued == "1"; }
			set { this.IsQueued = value ? "1" : "0"; }
		}
		public bool BoolResponseConfirm
		{
			get { return this.ResponseConfirm == "1"; }
			set { this.ResponseConfirm = value ? "1" : "0"; }
		}
        public bool BoolIsToResponse
        {
            get { return this.IsToResponse == "1"; }
            set { this.IsToResponse = value ? "1" : "0"; }
        }
		public bool BoolIsInbound
		{
			get { return this.IsInbound == "1"; }
			set { this.IsInbound = value ? "1" : "0"; }
		}
        public bool BoolIsPrivate
        {
            get { return this.IsPrivate == "1"; }
            set { this.IsPrivate = value ? "1" : "0"; }
        }
        public bool BoolIsReply
        {
            get { return this.IsReply == "1"; }
            set { this.IsReply = value ? "1" : "0"; }
        }
        public bool BoolIsForward
        {
            get { return this.IsForward == "1"; }
            set { this.IsForward = value ? "1" : "0"; }
        }

        //-------------------------------------------------------------------------------
        public enum MailAddressType
        {
            T = 11206656, // To
            C = 11206657, // CC
            B = 11206658, // BCC
            R = 11206659, // ReplyTo
            F = 11206660 // From
        }

        //-------------------------------------------------------------------------------
        public enum MailMessagePriority
        {
            Normal = 11272192, // Normal
            Low = 11272193,     // Low
            High = 11272194    // High
        }

        //-------------------------------------------------------------------------------
		public static List<OM_MailMessages> GetMessagesToSend(TbSenderDatabaseInfo cmp)
		{
			List<OM_MailMessages> list = new List<OM_MailMessages>();
			DateTime dbNullDate = new DateTime(1799, 12, 31);
			DateTime now = DateTime.Now;
			using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
			{
				var items = from p in db.OM_MailMessages
							where p.IsSent == "0" // campo che mi permette di non dipendere dai tb datetime
								//&& p.SentDateTime != null 
								//&& p.SentDateTime != dbNullDate
								&&  ((p.AccountWorkerId != null && p.AccountWorkerSubId != null) 
                                    // FABIO: eventualmente qua aggiungere in OR i campi MasterID
                                    )
								&& p.IsDraft == "0"
								&& p.IsInbound == "0"
								&& (p.IsQueued == "0"
									|| (p.IsQueued == "1" && p.RequestQueueDateTime <= now)) //mail schedulata per una certa data
							select p;
				list.AddRange(items);
			}
			return list;
		}

        //-------------------------------------------------------------------------------
        public static List<OM_MailMessages> GetMessagesToSendByWorkerAccount(TbSenderDatabaseInfo cmp, int wID, int wSubID)
        {
            List<OM_MailMessages> list = new List<OM_MailMessages>();
            DateTime dbNullDate = new DateTime(1799, 12, 31);
            DateTime now = DateTime.Now;
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_MailMessages
                            where p.IsSent == "0" // campo che mi permette di non dipendere dai tb datetime
                                //&& p.SentDateTime != null 
                                //&& p.SentDateTime != dbNullDate
                                &&  ((p.AccountWorkerId == wID && p.AccountWorkerSubId == wSubID)
                                    // FABIO: eventualmente qua aggiungere il controllo di IsWorker
                                    )
                                && p.IsDraft == "0"
                                && p.IsInbound == "0"
                                && (p.IsQueued == "0"
                                    || (p.IsQueued == "1" && p.RequestQueueDateTime <= now)) //mail schedulata per una certa data
                            select p;
                list.AddRange(items);
            }
            return list;
        }

        //-------------------------------------------------------------------------------
		static public OM_MailMessages CreateDBMessage(IMail mail, OM_MailMessagePop3 pop3Msg)
		{
			OM_MailMessages msg = new OM_MailMessages();

			msg.MessageSubject = (mail.Subject != null) ? mail.Subject : "";
			msg.PlainText = GetPlainText(mail);
            msg.SentDateTime = mail.Date.HasValue ? mail.Date.Value : pop3Msg.MessageDateTime;
			msg.MessageDateTime = mail.Date.HasValue? mail.Date.Value : pop3Msg.MessageDateTime; // per le outbound varia tra creazione e spedizione
			msg.BoolResponseConfirm = false;
			msg.BoolSystemMessage = false;     // TODO 
			msg.BoolIsDraft = false;
            msg.BoolIsSent = !pop3Msg.BoolIsInBox; // allineato a data, serve per non dipendere da una data tb
			msg.BoolIsQueued = false; // significa schedulata (solo per outbound)
            msg.BoolIsInbound = pop3Msg.BoolIsInBox;
            msg.BoolIsReply = false; // mail.IsReply;
            msg.BoolIsForward = false; // mail.IsForward;
            msg.ParentMessageID = 0;
            msg.RequestQueueDateTime = new DateTime(1799,12,31);
            msg.SendError = "";

			DateTime now = DateTime.Now;
			msg.TBCreated = now;
			msg.TBModified = now;

			return msg;
		}

		//-------------------------------------------------------------------------------
		public static bool SaveNewMessage(TbSenderDatabaseInfo company, MailHolder mailItem, int msgID, bool outbox,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger, MailLogListener logTrace, 
			IEasyAttachmentManager eaMng)
		{
            bool ret = false;
			using (var db = ConnectionHelper.GetHermesDBEntities(company))
			{
                try
                {
                    List<DMS_DocumentToArchive> d2aList = null; // TEMP

                    using (TransactionScope ts = new TransactionScope())
                    {
                        bool bAutomaticAssociation = mailItem.Account.BoolIsAutomaticMasterAssociation;

                        OM_MailMessages msg = OM_MailMessages.CreateDBMessage(mailItem.Mail, mailItem.Pop3Uid);
                        db.OM_MailMessages.Add(msg);
                        //db.SaveChanges(); // forzo l'assegnazione di un id al nuovo msg // commentato: non uno più campo autoincrement
                        msg.MailMessageID = msgID;

                        // RULES ------------------------------------------------------------------------
                        logTrace.WriteLine("---- rules applied", "Hermes");

                        int intRuleMailFolderID = 0;
                        bool bIsPrivate = mailItem.Account.BoolIsPrivate;
                        bool bIsRead = !mailItem.Pop3Uid.BoolIsInBox;
                        bool bIsToDelete = false;
                        MailMessagePriority mailPriority = MailMessagePriority.Normal;
                        List<OM_WorkersMailRulesWorkerShared> listWorkers = null;
                        List<OM_WorkersMailRulesMastersLinked> listLinks = null;
                        List<OM_MailMessageLinks> listMessageLinks = new List<OM_MailMessageLinks>();

                        List<OM_WorkersMailRules> listRules = OM_WorkersMailRules.GetWorkerMailRulesByWorker(company, mailItem.Account.MailAccountID, mailItem.Account.MailAccountSubID, mailItem.Pop3Uid.BoolIsInBox);
                        if ((listRules != null) && listRules.Count > 0)
                        {
                            foreach (var rule in listRules)
                            {
                                OM_WorkersMailRules.MailRuleCondition mrc = (OM_WorkersMailRules.MailRuleCondition)Enum.Parse(typeof(OM_WorkersMailRules.MailRuleCondition), rule.Condition.Value.ToString());

                                bool cndAnd = mrc.Equals(OM_WorkersMailRules.MailRuleCondition.ConditionAnd);

                                bool toDo = OM_WorkersMailRulesFilters.useRule(company, rule.WorkersMailRulesId, mailItem, cndAnd);

                                if (toDo)
                                {
                                    //Imposto la cartella di destinazione
                                    if (rule.MailFolderID >= 0)
                                        intRuleMailFolderID = rule.MailFolderID;

                                    //Imposto la priorità della mail
                                    OM_WorkersMailRules.MailRulePriority rulepriority = OM_WorkersMailRules.GetMailRulePriorityFromFieldValue(rule.RulePriority);
                                    if (!rulepriority.Equals(OM_WorkersMailRules.MailRulePriority.None))
                                        mailPriority = OM_WorkersMailRules.GetMailMessagePriorityFromRulePriority(rulepriority);

                                    //Imposto la privacy della mail
                                    OM_WorkersMailRules.MailRulePrivacy ruleprivacy = OM_WorkersMailRules.GetMailRulePrivacyFromFieldValue(rule.RulePrivacy);
                                    if (!ruleprivacy.Equals(OM_WorkersMailRules.MailRulePrivacy.None))
                                        bIsPrivate = ruleprivacy.Equals(OM_WorkersMailRules.MailRulePrivacy.Private);

                                    OM_WorkersMailRules.MailRuleRead ruleread = OM_WorkersMailRules.GetMailRuleReadFromFieldValue(rule.RuleRead);
                                    if (!ruleread.Equals(OM_WorkersMailRules.MailRuleRead.None))
                                        bIsRead = ruleread.Equals(OM_WorkersMailRules.MailRuleRead.Read);

                                    listWorkers = OM_WorkersMailRulesWorkerShared.GetWorkersMailRulesWorkerSharedByWorkersMailRulesId(company, rule.WorkersMailRulesId);

                                    listLinks = OM_WorkersMailRulesMastersLinked.GetWorkersMailRulesMastersLinkedByWorkersMailRulesId(company, rule.WorkersMailRulesId);

                                    if (rule.BoolIsNotAutoLinked)
                                        bAutomaticAssociation = false;

                                    if (rule.BoolIsToDelete)
                                        bIsToDelete = true;
                                }
                            }
                        }

                        // RULES ------------------------------------------------------------------------
                        logTrace.WriteLine("---- start save message id=" + msgID, "Hermes");


                        // PIPPO: Imposto il messaggio Privato in base alle impostazioni dell'account
                        msg.BoolIsPrivate = bIsPrivate;
                        msg.MailPriority = (int)mailPriority;

                        if (mailItem.Account.IsWorkerAccount)
                        {
                            msg.AccountWorkerId = mailItem.Account.MailAccountID;
                            msg.AccountWorkerSubId = mailItem.Account.MailAccountSubID;
                        }

                        {// Collego il messaggio al worker per la visualizzazione della mail nella posta in arrivo
                            int messageWorker = 0;
                            OM_MailMessageWorkers workerRec = TbHermesBL.OM_MailMessageWorkers.CreateDBMailMessageWorker();// new OM_MailMessageWorkers();
                            workerRec.MailMessageID = msg.MailMessageID;
                            workerRec.MailMessageWorkerID = ++messageWorker;
                            workerRec.WorkerId = mailItem.Account.MailAccountID;
                            workerRec.MailFolderID = intRuleMailFolderID;
                            workerRec.BoolIsArchived = false;
                            workerRec.BoolIsRead = bIsRead;
                            workerRec.BoolIsOwner = true;
                            workerRec.BoolIsDeleted = bIsToDelete;
                            workerRec.BoolIsNew = !bIsRead;
                            logTrace.WriteLine("---- worker id=" + workerRec.WorkerId, "Hermes");
                            db.OM_MailMessageWorkers.Add(workerRec);

                            if ((listWorkers != null) && (listWorkers.Count > 0))
                            {
                                foreach (var wk in listWorkers)
                                {
                                    OM_MailMessageWorkers workerRecShared = TbHermesBL.OM_MailMessageWorkers.CreateDBMailMessageWorker();// new OM_MailMessageWorkers();
                                    workerRecShared.MailMessageID = msg.MailMessageID;
                                    workerRecShared.MailMessageWorkerID = ++messageWorker;
                                    workerRecShared.WorkerId = wk.WorkerSharedId;
                                    workerRecShared.MailFolderID = 0;
                                    workerRecShared.BoolIsArchived = false;
                                    workerRecShared.BoolIsRead = false;
                                    workerRecShared.BoolIsOwner = false;
                                    workerRecShared.BoolIsDeleted = false;
                                    workerRecShared.BoolIsNew = true;
                                    logTrace.WriteLine("---- worker id=" + workerRecShared.WorkerId, "Hermes");
                                    db.OM_MailMessageWorkers.Add(workerRecShared);
                                }
                            }

                        }

                        int count = 1;
                        // collego il messaggio al cliente (master) se ne ho trovato uno
                        //TbHermesBL.OM_MailMessageLinks.
                        if (bAutomaticAssociation)
                        {
                            CreateDBMailMessageLinks(listMessageLinks, mailItem.Mail.To, msg.MailMessageID, mailItem.Account.MailAccountID, ref count, db);
                            CreateDBMailMessageLinks(listMessageLinks, mailItem.Mail.From, msg.MailMessageID, mailItem.Account.MailAccountID, ref count, db);
                            CreateDBMailMessageLinks(listMessageLinks, mailItem.Mail.Cc, msg.MailMessageID, mailItem.Account.MailAccountID, ref count, db);
                            CreateDBMailMessageLinks(listMessageLinks, mailItem.Mail.Bcc, msg.MailMessageID, mailItem.Account.MailAccountID, ref count, db);
                            CreateDBMailMessageLinks(listMessageLinks, mailItem.Mail.ReplyTo, msg.MailMessageID, mailItem.Account.MailAccountID, ref count, db);
                        }

                        if ((listLinks != null) && (listLinks.Count > 0))
                        {
                            CreateDBMailMessageRuleLinks(listMessageLinks, listLinks, msg.MailMessageID, mailItem.Account.MailAccountID, ref count, db);
                        }

                        // se il messaggio corrente e' una risposta/inoltro ha il campo inReplyTo compilato
                        // per questo devo convertire il dato in ParentMessageId
                        if ((outbox)&&(!mailItem.Mail.InReplyTo.IsNullOrEmpty()) && (OM_MailMessagePop3.GetMailMessagePop3FromCodice(company, mailItem).Count > 0))
                        {
                            int aMailParentID = OM_MailMessagePop3.GetMailMessagePop3FromCodice(company, mailItem).FirstOrDefault().MailMessageID;

                            if (aMailParentID > 0)
                            {
                                msg.ParentMessageID = aMailParentID;
                                msg.BoolIsReply = mailItem.Mail.IsReply;
                                msg.BoolIsForward = mailItem.Mail.IsForward;
                            }
                        }

                        List<MailBox> listReadReceipt = mailItem.Mail.GetReadReceiptAddresses();
                        msg.BoolResponseConfirm = (listReadReceipt.Count != 0);
                        msg.BoolIsToResponse = msg.BoolResponseConfirm && (mailItem.Pop3Uid.BoolIsInBox);

                        string htmlText = "";

                        if (false == string.IsNullOrEmpty(mailItem.Mail.Html))
                        {
                            htmlText = mailItem.Mail.Html;
                        }
                        else
                        {
                            if (false == string.IsNullOrEmpty(mailItem.Mail.GetBodyAsHtml()))
                                htmlText = mailItem.Mail.GetBodyAsHtml();
                        }                    

                        if (false == string.IsNullOrEmpty(htmlText))
                        {
                            OM_MailMessageHTML htmlRec = new OM_MailMessageHTML();
                            htmlRec.HTMLText = htmlText;
                            htmlRec.MailMessageID = msg.MailMessageID;
                            DateTime now = DateTime.Now;
                            htmlRec.TBCreated = now;
                            htmlRec.TBModified = now;

                            logTrace.WriteLine("---- htmltext chars=" + htmlRec.HTMLText.Length.ToString(), "Hermes");
                            db.OM_MailMessageHTML.Add(htmlRec);
                        }

                        count = 1;
                        AddMailBoxAddresses(mailItem.Mail.From, msg.MailMessageID, ref count, MailAddressType.F, db);
                        AddMailBoxAddresses(mailItem.Mail.To, msg.MailMessageID, ref count, MailAddressType.T, db);
                        AddMailBoxAddresses(mailItem.Mail.Cc, msg.MailMessageID, ref count, MailAddressType.C, db);
                        AddMailBoxAddresses(mailItem.Mail.Bcc, msg.MailMessageID, ref count, MailAddressType.B, db);
                        AddMailBoxAddresses(mailItem.Mail.ReplyTo, msg.MailMessageID, ref count, MailAddressType.R, db);

                        if (mailItem.Pop3Uid != null)
                        {
                            OM_MailMessagePop3 pop3Uid = mailItem.Pop3Uid;
                            pop3Uid.MailMessageID = msg.MailMessageID;

                            logTrace.WriteLine("---- save to pop3 id=" + pop3Uid.MailMessageID, "Hermes");
                            db.OM_MailMessagePop3.Add(pop3Uid);
                        }

                        if ((mailItem.Mail.Attachments != null && mailItem.Mail.Attachments.Count != 0))
                        {
                            // salva in tabella di appoggio di Easy Attachment
                            logTrace.WriteLine("---- save to EA count=" + mailItem.Mail.Attachments.Count(), "Hermes");
                            d2aList = eaMng.StoreEMailAttachments(mailItem.Mail, msg.MailMessageID);
                        }

                        logTrace.WriteLine("---- write db", "Hermes");
                        db.SaveChanges();
                        ts.Complete();
                        ret = true;
                    }

                    // perché EA possa lavorare, occorre che la transazione sia stata già committata (quindi non posso :( transazionare la parte seguente con la precedente)

                    if (mailItem.Mail.Attachments != null &&
                        mailItem.Mail.Attachments.Count != 0 &&
                        d2aList != null)
                    {
                        using (TransactionScope ts = new TransactionScope())
                        {
                            logTrace.WriteLine("---- save attacchments", "Hermes");

                            List<int?> idsList = eaMng.AttachThem(mailItem.Mail, msgID, d2aList);
                            // TODO controllo d'errore. al momento sollevo un'eccezione generica
                            // TODO maschera oggetti db, ritorna solo gli IDs

                            int i = 0;
                            foreach (var att in mailItem.Mail.Attachments)
                            {
                                Debug.WriteLine(att.SafeFileName);// att.FileName;
                                OM_MailMessageAttachments dbAtt = Microarea.TaskBuilderNet.TbHermesBL.OM_MailMessageAttachments.CreateDBAttachment(); // namespace completo per disambiguare da property
                                dbAtt.AttachmentFileName = att.SafeFileName;
                                dbAtt.MailMessageID = msgID;
                                string aContentId = "";
                                if (!att.ContentId.IsNullOrEmpty())
                                {
                                    aContentId = att.ContentId.Replace("cid:", "");
                                    dbAtt.AttachmentTag = aContentId;
                                }
                                dbAtt.AttachmentTag = aContentId;
                                dbAtt.MailEasyAttachmentID = idsList[i++]; // valore di ritorno da EasyAttachment
                                dbAtt.MailMessageAttachmentID = i;

                                logTrace.WriteLine("---- att. id=" + dbAtt.MailEasyAttachmentID, "Hermes");
                                db.OM_MailMessageAttachments.Add(dbAtt);
                            }

                            logTrace.WriteLine("---- write attacchments to db ", "Hermes");
                            db.SaveChanges();
                            ts.Complete();
                        }
                    }
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex) // così non blocco l'account successivo
                {
                    if (excLogger != null)
                        excLogger(ex);
                }
                catch (Exception ex) // così non blocco l'account successivo
                {
                    if (excLogger != null)
                        excLogger(ex);
                    //throw; // TODO commentare una volta che trace/log/etc.. è a posto
                }

                logTrace.WriteLine("---- end save message", "Hermes");
			}

            return ret;
		}

        //-------------------------------------------------------------------------------
        public void UpdateSentMessage(TbSenderDatabaseInfo company,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        var query = from msg in db.OM_MailMessages
                                    where msg.MailMessageID == this.MailMessageID
                                    select msg;

                        // Execute the query, and change the column values 
                        // you want to change. 
                        foreach (OM_MailMessages currMSG in query)
                        {
                            currMSG.BoolIsSent = true;
                            currMSG.SentDateTime = DateTime.Now;
                            currMSG.TBModified = DateTime.Now;
                            currMSG.BoolIsQueued = false;
                        }
                        // sembrerebbe non fare nulla, ma fa l'attach nell'object graph
                        ////db.OM_MailMessages.Attach(db.OM_MailMessages.Single(p => p.MailMessageID == this.MailMessageID)); 
                        // ora scrivo
                        //db.OM_MailMessages.ApplyCurrentValues(this);
                        ////db.OM_MailMessages.First();

                        ////db.Entry(db.OM_MailMessages).CurrentValues.SetValues(this);
                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex) // così non blocco l'account successivo
                {
                    if (excLogger != null)
                        excLogger(ex);
                    //throw; // TODO commentare una volta che trace/log/etc.. è a posto
                }
            }
        }

        //-------------------------------------------------------------------------------
        public static void UpdateMailPop3MessageId(TbSenderDatabaseInfo company, Int32 msgID, MailHolder mailItem,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            using (var db = ConnectionHelper.GetHermesDBEntities(company))
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        OM_MailMessagePop3 record = OM_MailMessagePop3.CreateItem(
                            mailItem.Account.ReceiveServerName,
                            mailItem.Account.ReceiveServerUserName,
                            mailItem.Mail.MessageID,    // codice 
                            mailItem.Mail.Date.Value,
                            false);
                        record.MailMessageID = msgID;
                        db.OM_MailMessagePop3.Add(record);
                        db.SaveChanges();
                        ts.Complete();
                    }
                }
                catch (Exception ex) // così non blocco l'account successivo
                {
                    if (excLogger != null)
                        excLogger(ex);
                    //throw; // TODO commentare una volta che trace/log/etc.. è a posto
                }
            }
        }

		//-------------------------------------------------------------------------------
		private static void AddMailBoxAddresses(IList<Limilabs.Mail.Headers.MailBox> iList, int mailMessageID, ref int count, MailAddressType addressType, MZP_CompanyEntities db)
		{
			foreach (Limilabs.Mail.Headers.MailBox mb in iList)
			{
				OM_MailMessageAddresses addRec = new OM_MailMessageAddresses();
				addRec.MailAddress = (mb.Address == null) ? "" : mb.Address; // può effettivamente accadere
                string longName = (mb.Name == null) ? "" : mb.Name;
                if (longName.Length>255)
                    longName = longName.Left(255);
                addRec.MailLongName = longName; // TODO permetterei il campo di essere NULL
				addRec.MailMessageID = mailMessageID;
				addRec.MailMessageAddressID = count++;
				addRec.MailAddressType = (Int32)addressType;
                db.OM_MailMessageAddresses.Add(addRec);

				DateTime now = DateTime.Now;
				addRec.TBCreated = now;
				addRec.TBModified = now;
			}
		}
		private static void AddMailBoxAddresses(IList<Limilabs.Mail.Headers.MailAddress> iList, int mailMessageID, ref int count, MailAddressType addressType, MZP_CompanyEntities db)
		{
			foreach (Limilabs.Mail.Headers.MailAddress ma in iList)
			{
				IList<Limilabs.Mail.Headers.MailBox> list = ma.GetMailboxes();
				AddMailBoxAddresses(list, mailMessageID, ref count, addressType, db);
			}
		}


		//-------------------------------------------------------------------------------
		private static string GetMailBoxesString(IList<Limilabs.Mail.Headers.MailBox> iList)
		{
			return string.Join("; ", iList); // avranno overload-ato il ToString()?
		}

		private static string GetMailAddressesString(IList<Limilabs.Mail.Headers.MailAddress> iList)
		{
            return JoinAddresses(iList);
			//return string.Join("; ", iList); // avranno overload-ato il ToString()?
			//StringBuilder sb = new StringBuilder();
			//foreach (Limilabs.Mail.Headers.MailAddress addr in iList)
			//{
			//    //addr.ToString();
			//}
			//throw new NotImplementedException();
		}

        private static string JoinAddresses(IList<MailBox> mailboxes)
        {
            return string.Join(",",
                new List<MailBox>(mailboxes).ConvertAll(m => string.Format("{0} <{1}>", m.Name, m.Address))
                .ToArray());
        }

        private static string JoinAddresses(IList<MailAddress> addresses)
        {
            StringBuilder builder = new StringBuilder();

            foreach (MailAddress address in addresses)
            {
                if (address is MailGroup)
                {
                    MailGroup group = (MailGroup)address;
                    builder.AppendFormat("{0}: {1};, ", group.Name, JoinAddresses(group.Addresses));
                }
                if (address is MailBox)
                {
                    MailBox mailbox = (MailBox)address;
                    builder.AppendFormat("{0} <{1}>, ", mailbox.Name, mailbox.Address);
                }
            }
            return builder.ToString();
        }

		//-------------------------------------------------------------------------------
		public List<OM_MailMessageAddresses> FetchAddresses(TbSenderDatabaseInfo cmp)
		{
			using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
			{
				var list = (from h in db.OM_MailMessageAddresses
							where h.MailMessageID == this.MailMessageID
							select h)
							.ToList();
				return list;
			}
		}

		//-------------------------------------------------------------------------------
		public List<OM_MailMessageAttachments> FetchAttachments(TbSenderDatabaseInfo cmp)
		{
			using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
			{
				var list = (from h in db.OM_MailMessageAttachments
							where h.MailMessageID == this.MailMessageID
							select h)
							.ToList();
				return list;
			}
		}

		//public List<OM_MailMessagesAddresses> FetchTo(TbSenderDatabaseInfo cmp)
		//{
		//    string addrType = MailAddressType.T.ToString();
		//    using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
		//    {
		//        var list = (from h in db.OM_MailMessagesAddresses
		//                    where h.MailMessageID == this.MailMessageID
		//                    && h.MailAddressType == addrType
		//                    select h)
		//                    .ToList();
		//        return list;
		//    }
		//}

		//-------------------------------------------------------------------------------
		public string FetchHtmlText(TbSenderDatabaseInfo cmp)
		{
			using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
			{
                OM_MailMessageHTML item = (from h in db.OM_MailMessageHTML
												 where h.MailMessageID == this.MailMessageID
												 select h)
												 .FirstOrDefault();
				if (item != null)
					return item.HTMLText;
			}
			return null;
		}

		//-------------------------------------------------------------------------------
		private static string GetPlainText(IMail mail)
		{
            string aPlainText = "";

            if (!string.IsNullOrEmpty(mail.Text))
            {
                aPlainText = mail.Text;
            }
            else if (!string.IsNullOrEmpty(mail.GetTextFromHtml()))
            {
                aPlainText = mail.GetTextFromHtml();
            }
            else if (!string.IsNullOrEmpty(mail.GetBodyAsText()))
            {
                aPlainText = mail.GetBodyAsText();
            }
            
            return aPlainText;
		}

		//-------------------------------------------------------------------------------
        // crea tanti record di mailLink per quanti ne riesce a trovare tra i clienti nella lista di indirizzi iList 
        private static void CreateDBMailMessageLinks(IList<Limilabs.Mail.Headers.MailBox> iList, int mailMessageID, int workerID, ref int count, MZP_CompanyEntities db)
        {
            foreach (Limilabs.Mail.Headers.MailBox mb in iList)
            {
                List<OM_MastersContacts> contactList = (from p in db.OM_MastersContacts
                                                        where
                                                            p.Contact == mb.Address &&
                                                            p.ContactType == (int)MastersContactType.mail
                                                        select p)
                                                        .ToList();

                if (contactList.Count() == 1)
                {
                    // ho trovato un cliente valido
                    OM_MailMessageLinks addRec = TbHermesBL.OM_MailMessageLinks.CreateDBMailMessageLinks();
                    addRec.MailMessageLinkID = count++;
                    addRec.MailMessageID = mailMessageID;
                    addRec.MasterCode = contactList.First().MasterCode;
                    addRec.MasterLine = contactList.First().Line;
                    addRec.WorkerId = workerID;
                    db.OM_MailMessageLinks.Add(addRec);
                }
            }
        }

        //-------------------------------------------------------------------------------
        // crea tanti record di mailLink per quanti ne riesce a trovare tra i clienti nella lista di indirizzi iList 
        private static void CreateDBMailMessageLinks(List<OM_MailMessageLinks> listLinks, IList<Limilabs.Mail.Headers.MailBox> iList, int mailMessageID, int workerID, ref int count, MZP_CompanyEntities db)
        {
            foreach (Limilabs.Mail.Headers.MailBox mb in iList)
            {
                List<OM_MastersContacts> contactList = (from p in db.OM_MastersContacts
                                                        where
                                                            p.Contact == mb.Address &&
                                                            p.ContactType == (int)MastersContactType.mail
                                                        select p)
                                                        .ToList();

                if (contactList.Count() == 1)
                {
                    OM_MailMessageLinks ml = listLinks.Find(x => x.MasterCode.Equals(contactList.First().MasterCode)
                                             && x.OfficeFileId == 0);
                    if (ml == null)
                    {
                        // ho trovato un cliente valido
                        OM_MailMessageLinks addRec = TbHermesBL.OM_MailMessageLinks.CreateDBMailMessageLinks();
                        addRec.MailMessageLinkID = count++;
                        addRec.MailMessageID = mailMessageID;
                        addRec.MasterCode = contactList.First().MasterCode;
                        addRec.MasterLine = contactList.First().Line;
                        addRec.OfficeFileId = 0;
                        addRec.WorkerId = workerID;

                        listLinks.Add(addRec);
                        db.OM_MailMessageLinks.Add(addRec);
                    }
                }
            }
        }

        private static void CreateDBMailMessageLinks(List<OM_MailMessageLinks> listLinks, IList<Limilabs.Mail.Headers.MailAddress> iList, int mailMessageID, int workerID, ref int count, MZP_CompanyEntities db)
        {
            foreach (Limilabs.Mail.Headers.MailAddress ma in iList)
            {
                IList<Limilabs.Mail.Headers.MailBox> list = ma.GetMailboxes();
                CreateDBMailMessageLinks(listLinks, list, mailMessageID, workerID, ref count, db);
            }
        }

        //-------------------------------------------------------------------------------
        private static void CreateDBMailMessageRuleLinks(List<OM_MailMessageLinks> listLinks, List<OM_WorkersMailRulesMastersLinked> listRuleLinks, int mailMessageID, int workerID, ref int count, MZP_CompanyEntities db)
        {
            foreach (var rl in listRuleLinks)
            {
                OM_MailMessageLinks ml = listLinks.Find(x => x.MasterCode.Equals(rl.MasterCode)
                                         && x.OfficeFileId == rl.OfficeFileId);
                if (ml == null)
                {
                    // ho trovato un cliente valido
                    OM_MailMessageLinks addRec = TbHermesBL.OM_MailMessageLinks.CreateDBMailMessageLinks();
                    addRec.MailMessageLinkID = count++;
                    addRec.MailMessageID = mailMessageID;
                    addRec.MasterCode = rl.MasterCode;
                    addRec.MasterLine = 0;
                    addRec.OfficeFileId = rl.OfficeFileId;
                    addRec.WorkerId = workerID;

                    listLinks.Add(addRec);
                    db.OM_MailMessageLinks.Add(addRec);
                }
            }
        }

        //-------------------------------------------------------------------------------
        public static string GetMailAddressSender(MailHolder msg)
        {
            foreach (Limilabs.Mail.Headers.MailBox mb in msg.Mail.From)
            {
                return mb.Address;
            }

            return "";
        }

        //-------------------------------------------------------------------------------
        public static List<string> GetMailAddressesRecipients(MailHolder msg)
        {
            List<string> list = new List<string>();
   			foreach (Limilabs.Mail.Headers.MailBox mb in msg.Mail.To)
			{
                list.Add(mb.Address);
            }
            foreach (Limilabs.Mail.Headers.MailBox mb in msg.Mail.Cc)
            {
                list.Add(mb.Address);
            }
            foreach (Limilabs.Mail.Headers.MailBox mb in msg.Mail.Bcc)
            {
                list.Add(mb.Address);
            }
            foreach (Limilabs.Mail.Headers.MailBox mb in msg.Mail.ReplyTo)
            {
                list.Add(mb.Address);
            }

            return list;
        }

    }
}
