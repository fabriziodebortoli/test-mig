using System;
using System.Collections.Generic;
using System.Linq;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.TbHermesBL.Config;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public enum MailServerType
    {
        Pop3 = 35782657,
        Imap = 35782658
    }

	public partial class OM_WorkerAccounts : IOM_MailAccount
	{
        //-------------------------------------------------------------------------------
        public int MailAccountID
        {
            get { return WorkerId; }
            set { WorkerId = value; }
        }

        //-------------------------------------------------------------------------------
        public int MailAccountSubID
        {
            get { return AccountSubID; }
            set { AccountSubID = value; }
        }

        //-------------------------------------------------------------------------------
        // FABIO: questa proprieta' diversifica gli oggetti di tipo cliente(master) o addetto(worker) nel caso si voglia creare una OM_MasterAccounts
        public bool IsWorkerAccount
        {
            get { return true; }  //TODO-FABIO soluzione migliorabile con la typeof ma forse ottima per le prestazioni
        }

        //-------------------------------------------------------------------------------
		public MailServerType EnumAccountType
		{
            get { return Enum.IsDefined(typeof(MailServerType), this.AccountType) ? (MailServerType)this.AccountType : (MailServerType)0; }
			set { this.AccountType = (Int32)value; }
		}
        //-------------------------------------------------------------------------------
		public bool BoolIsEnabled
		{
			get { return this.IsEnabled == "1"; }
			set { this.IsEnabled = value ? "1" : "0"; }
		}
		//-------------------------------------------------------------------------------
		public bool BoolReceiveSsl
		{
			get { return this.ReceiveSsl == "1"; }
			set { this.ReceiveSsl = value ? "1" : "0"; }
		}
		//-------------------------------------------------------------------------------
		public bool BoolSmtpSsl
		{
			get { return this.SmtpSsl == "1"; }
			set { this.SmtpSsl = value ? "1" : "0"; }
		}
        //-------------------------------------------------------------------------------
		public bool BoolIsImportOutbox
        {
            get { return this.IsImportOutbox == "1"; }
            set { this.IsImportOutbox = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public bool BoolIsOwner
        {
            get { return this.IsOwner == "1"; }
            set { this.IsOwner = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public bool BoolIsResponseConfirm
        {
            get { return this.IsResponseConfirm == "1"; }
            set { this.IsResponseConfirm = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public bool BoolIsAutomaticMasterAssociation
        {
            get { return this.IsAutomaticMasterAssociation == "1"; }
            set { this.IsAutomaticMasterAssociation = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public int IntDayBeforeSearch
        {
            get { return DayBeforeSearch; }
            set { DayBeforeSearch = value; }
        }
        //-------------------------------------------------------------------------------
        public int intMaxMailRecipients 
        {
            get { return MaxMailRecipients; }
            set { MaxMailRecipients = value; }
        }
        //-------------------------------------------------------------------------------
        public int IntDayBeforeFromToday
        {
            get { return DayBeforeFromToday; }
            set { DayBeforeFromToday = value; }
        }
        //-------------------------------------------------------------------------------
        public string StrOutboxName
        {
            get { return OutboxName; }
            set { OutboxName = value; }
        }
        //-------------------------------------------------------------------------------
        public bool BoolIsPrivate
        {
            get { return this.IsPrivate == "1"; }
            set { this.IsPrivate = value ? "1" : "0"; }
        }
        //-------------------------------------------------------------------------------
        public bool BoolIsDefault
        {
            get { return this.IsDefault == "1"; }
            set { this.IsDefault = value ? "1" : "0"; }
        }
        public bool BoolIsAutomaticResponse
        {
            get { return this.IsAutomaticResponse == "1"; }
            set { this.IsAutomaticResponse = value ? "1" : "0"; }
        }
        public bool BoolAuthAccess
        {
            get { return this.AuthAccess == "1"; }
            set { this.AuthAccess = value ? "1" : "0"; }
        }

		//-------------------------------------------------------------------------------
		public static List<OM_WorkerAccounts> GetWorkerAccounts(TbSenderDatabaseInfo cmp)
		{
			List<OM_WorkerAccounts> list = new List<OM_WorkerAccounts>();
			using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
			{
				var items = (from p in db.OM_WorkerAccounts
							where p.IsEnabled == "1"
							select p).OrderBy(x => x.WorkerId);
				list.AddRange(items);
			}
			return list;
		}

        //-------------------------------------------------------------------------------
        public static bool IsAutomaticResponseActive(TbSenderDatabaseInfo cmp, OM_WorkerAccounts wrk)
        {
            bool bReturn = false;
            if (wrk.BoolIsAutomaticResponse)
                bReturn = OM_WorkersPreferencesDetails.DateInAutomaticResponseRange(cmp, wrk.WorkerId);
            return bReturn;
        }


    }
}
