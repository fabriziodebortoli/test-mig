using System;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_MailMessageWorkers
    {
        //-------------------------------------------------------------------------------
        static public OM_MailMessageWorkers CreateDBMailMessageWorker()
        {
            OM_MailMessageWorkers item = new OM_MailMessageWorkers();
            DateTime now = DateTime.Now;
            item.TBCreated = now;
            item.TBModified = now;
            return item;
        }

        //-------------------------------------------------------------------------------
        public bool BoolIsOwner
        {
            get { return this.IsOwner == "1"; }
            set { this.IsOwner = value ? "1" : "0"; }
        }
        public bool BoolIsRead
        {
            get { return this.IsRead == "1"; }
            set { this.IsRead = value ? "1" : "0"; }
        }
        public bool BoolIsDeleted
        {
            get { return this.IsDeleted == "1"; }
            set { this.IsDeleted = value ? "1" : "0"; }
        }
        public bool BoolIsNew
        {
            get { return this.IsNew == "1"; }
            set { this.IsNew = value ? "1" : "0"; }
        }
        public bool BoolIsArchived
        {
            get { return this.IsArchived == "1"; }
            set { this.IsArchived = value ? "1" : "0"; }
        }
    }
}
