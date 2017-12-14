using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_WorkersMailRules
    {
        //-------------------------------------------------------------------------------
        public bool BoolIsEnabled
        {
            get { return this.IsEnabled == "1"; }
            set { this.IsEnabled = value ? "1" : "0"; }
        }
        public bool BoolIsFromOutbox
        {
            get { return this.IsFromOutbox == "1"; }
            set { this.IsFromOutbox = value ? "1" : "0"; }
        }
        public bool BoolIsNotAutoLinked
        {
            get { return this.IsNotAutoLinked == "1"; }
            set { this.IsNotAutoLinked = value ? "1" : "0"; }
        }
        public bool BoolIsToDelete
        {
            get { return this.IsToDelete == "1"; }
            set { this.IsToDelete = value ? "1" : "0"; }
        }

        //-------------------------------------------------------------------------------
        public enum MailRuleCondition
        {
            ConditionOr = 11448800, // OR
            ConditionAnd = 11448801 // AND
        }
        //-------------------------------------------------------------------------------
        public enum MailRulePriority
        {
            None = 11534336,    // None
            Normal = 11534337,  // Normal
            Low = 11534338,     // Low
            High = 11534339     // High
        }
        public static MailRulePriority GetMailRulePriorityFromFieldValue(int? value)
        {
            return (MailRulePriority)Enum.Parse(typeof(MailRulePriority), value.ToString());
        }
        //-------------------------------------------------------------------------------
        public enum MailRulePrivacy
        {
            None = 11599872,    // None
            Private = 11599873, // Private
            Public = 11599874   // Public
        }
        public static MailRulePrivacy GetMailRulePrivacyFromFieldValue(int? value)
        {
            return (MailRulePrivacy)Enum.Parse(typeof(MailRulePrivacy), value.ToString());
        }
        //-------------------------------------------------------------------------------
        public enum MailRuleRead
        {
            None = 11665408,    // None
            Read = 11665409,    // Read
            Unread = 11665408   // Unread
        }
        public static MailRuleRead GetMailRuleReadFromFieldValue(int? value)
        {
            return (MailRuleRead)Enum.Parse(typeof(MailRuleRead), value.ToString());
        }
        

        //-------------------------------------------------------------------------------
        public static OM_MailMessages.MailMessagePriority GetMailMessagePriorityFromRulePriority(MailRulePriority rp)
        {
            OM_MailMessages.MailMessagePriority mp = OM_MailMessages.MailMessagePriority.Normal;

            switch (rp)
            {
                case MailRulePriority.None:
                case MailRulePriority.Normal:
                    mp = OM_MailMessages.MailMessagePriority.Normal;
                    break;
                case MailRulePriority.High:
                    mp = OM_MailMessages.MailMessagePriority.High;
                    break;
                case MailRulePriority.Low:
                    mp = OM_MailMessages.MailMessagePriority.Low;
                    break;
            }
            
            return mp;
        }


        //-------------------------------------------------------------------------------
        public static List<OM_WorkersMailRules> GetWorkerMailRulesByWorker(TbSenderDatabaseInfo cmp, int intWorkerId, int intAccountSubId, bool bIsInbox)
        {
            string strIsFromOutbox = "0";
            if (!bIsInbox)
                strIsFromOutbox = "1";

            List<OM_WorkersMailRules> list = new List<OM_WorkersMailRules>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_WorkersMailRules 
                            where  (p.IsEnabled == "1") 
                                && (p.WorkerId == intWorkerId)
                                && ((p.AccountSubId == 0) || (p.AccountSubId == intAccountSubId))
                                && (p.IsFromOutbox == strIsFromOutbox)
                            orderby p.RuleOrder ascending
                            select p;
                list.AddRange(items);
            }
            return list;
        }       
    }
}
