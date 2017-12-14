using System;
using System.Collections.Generic;
using System.Linq;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
	public partial class OM_MailMessagePop3
	{
		//-------------------------------------------------------------------------------
		public bool BoolIsDeleted
		{
			get { return this.IsDeleted == "1"; }
			set { this.IsDeleted = value ? "1" : "0"; }
		}

        //-------------------------------------------------------------------------------
        public bool BoolIsInBox
        {
            get { return this.IsInBox == "1"; }
            set { this.IsInBox = value ? "1" : "0"; }
        }

		//-------------------------------------------------------------------------------
		public static List<OM_MailMessagePop3> GetAlreadyDownloadedByAccount
			(
			TbSenderDatabaseInfo cmp,
			//string uid,
			string pop3Host,
			string hostUser
			)
		{
			List<OM_MailMessagePop3> list = new List<OM_MailMessagePop3>();
			using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
			{
				var items = from p in db.OM_MailMessagePop3 
							where 
								//p.IsDeleted == "0" && 
								p.Host == pop3Host && p.Host_user == hostUser //&& p.codice == uid
							select p;
				list.AddRange(items);
			}
			return list;
		}

        //-------------------------------------------------------------------------------
        public static List<OM_MailMessagePop3> GetMailMessagePop3FromMessageID
            (
            TbSenderDatabaseInfo cmp,
            long messageID
            )
        {
            List<OM_MailMessagePop3> list = new List<OM_MailMessagePop3>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_MailMessagePop3
                            where
                                p.MailMessageID == messageID 
                            select p;
                if (!items.Count().Equals(0))
                    list.AddRange(items);
            }

            return list;
        }

        //-------------------------------------------------------------------------------
        public static List<OM_MailMessagePop3> GetMailMessagePop3FromCodice
            (
            TbSenderDatabaseInfo cmp,
            MailHolder mh
            )
        {
            List<OM_MailMessagePop3> list = new List<OM_MailMessagePop3>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_MailMessagePop3
                            where
                                (p.Codice == mh.Mail.InReplyTo) && 
                                (p.Host == mh.Account.ReceiveServerName) &&
                                (p.Host_user == mh.Account.ReceiveServerUserName)
                            select p;
                if (items != null)
                    list.AddRange(items);
            }

            return list;
        }

        //PIPPO
        //-------------------------------------------------------------------------------
        public static DateTime GetReceivedMailMessageLastDate
            (
            TbSenderDatabaseInfo cmp,
            //string uid,
            string pop3Host,
            string hostUser
            )
        {
            List<OM_MailMessagePop3> list = new List<OM_MailMessagePop3>();
            DateTime LastMessageDate = new DateTime(1799,12,31);

            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_MailMessagePop3
                            where
                                p.IsInBox == "1" && 
                                p.Codice != "" &&
                                p.Host == pop3Host && p.Host_user == hostUser //&& p.codice == uid
                             select p;
                if (items.Max(t=>t.MessageDateTime).HasValue)
                    LastMessageDate = items.Max(t => t.MessageDateTime).Value;
                
            }
            return LastMessageDate;
        }

        //PIPPO
        //-------------------------------------------------------------------------------
        public static DateTime GetSentMailMessageLastDate
            (
            TbSenderDatabaseInfo cmp,
            //string uid,
            string pop3Host,
            string hostUser
            )
        {
            List<OM_MailMessagePop3> list = new List<OM_MailMessagePop3>();
            DateTime LastMessageDate = new DateTime(1799, 12, 31);

            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_MailMessagePop3
                             where
                                 p.IsInBox == "0" &&
                                 p.Codice != "" &&
                                 p.Host == pop3Host && p.Host_user == hostUser //&& p.codice == uid
                            select p;
                if (items.Max(t => t.MessageDateTime).HasValue)
                    LastMessageDate = items.Max(t => t.MessageDateTime).Value;
            }
            return LastMessageDate;
        }


        //PIPPO
        //-------------------------------------------------------------------------------
        public static List<OM_MailMessagePop3> GetAlreadySentByAccount
            (
            TbSenderDatabaseInfo cmp,
            //string uid,
            string pop3Host,
            string hostUser
            )
        {
            List<OM_MailMessagePop3> list = new List<OM_MailMessagePop3>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_MailMessagePop3
                            where
                                //p.IsDeleted == "0" &&
                                p.IsInBox == "0" &&
                                p.Codice != "" &&
                                p.Host == pop3Host && p.Host_user == hostUser //&& p.codice == uid
                            select p;
                list.AddRange(items);
            }
            return list;
        }

		//-------------------------------------------------------------------------------
        public static OM_MailMessagePop3 CreateItem(string srv, string usr, string uid, DateTime? dateMessageDateTime, bool InBox)
		{
			OM_MailMessagePop3 pop3Uid = new OM_MailMessagePop3();
			pop3Uid.Codice = uid;
			pop3Uid.Host = srv;
			pop3Uid.Host_user = usr;
			pop3Uid.BoolIsDeleted = false;
            pop3Uid.BoolIsInBox = InBox;
            pop3Uid.MessageDateTime = (dateMessageDateTime.HasValue) ? dateMessageDateTime : DateTime.Now; 
			DateTime now = DateTime.Now;
			pop3Uid.TBCreated = now;
			pop3Uid.TBModified = now;
			return pop3Uid;
		}
	}
}
