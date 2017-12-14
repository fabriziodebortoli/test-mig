using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_WorkersMailRulesFilters
    {
        //-------------------------------------------------------------------------------
        public enum MailRuleOperator
        {
            Contain = 11403264, // Contain
            Equal = 11403265,   // Equal
            Begin = 11403266,   // Begin
            Finish = 11403267   // Finish
        }

        //-------------------------------------------------------------------------------
        public enum MailRuleField
        {
            All = 11337728,         // All
            Sender = 11337729,      // Sender
            Recipients  = 11337730, // Recipients
            Subject = 11337731,     // Subject
            MailText = 11337732        // Text
        }

        //-------------------------------------------------------------------------------
        public static List<OM_WorkersMailRulesFilters> GetWorkerMailRulesFilterByWorkersMailRulesId(TbSenderDatabaseInfo cmp, int intWorkerMailRulesId)
        {
            List<OM_WorkersMailRulesFilters> list = new List<OM_WorkersMailRulesFilters>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_WorkersMailRulesFilters
                            where p.WorkersMailRulesId == intWorkerMailRulesId
                            select p;
                list.AddRange(items);
            }
            return list;
        }

        //-------------------------------------------------------------------------------
        public static bool useRule(TbSenderDatabaseInfo cmp, int intWorkersMailRulesId, MailHolder msg, bool bAnd)
        {
            bool bToUse = false;

            List<OM_WorkersMailRulesFilters> listFilters = GetWorkerMailRulesFilterByWorkersMailRulesId(cmp, intWorkersMailRulesId);
            if ((listFilters != null) && (listFilters.Count > 0))
            { 
                foreach (var filter in listFilters)
                {
                    MailRuleField mrf = (MailRuleField) Enum.Parse(typeof(MailRuleField), filter.FilterField.Value.ToString());  
                    string strToFind = filter.FilterText;
                    string strSource = "";
                    MailRuleOperator mro = (MailRuleOperator) Enum.Parse(typeof(MailRuleOperator), filter.FilterOperator.Value.ToString());  
                    
                    bool bToUseFilter = false;

                    switch (mrf)
                    {
                        case MailRuleField.All:
                            {
                                strSource = msg.Mail.Text;
                                bToUseFilter |= searchTextInField(strToFind, strSource, mro);

                                strSource = msg.Mail.GetTextFromHtml();
                                bToUseFilter |= searchTextInField(strToFind, strSource, mro);

                                strSource = OM_MailMessages.GetMailAddressSender(msg);
                                bToUseFilter |= searchTextInField(strToFind, strSource, mro);

                                List<string> listRecipients = OM_MailMessages.GetMailAddressesRecipients(msg);
                                if ((listRecipients != null) && (listRecipients.Count > 0))
                                {
                                    foreach (var s in listRecipients)
                                    {
                                        bToUseFilter |= searchTextInField(strToFind, s, mro);
                                    }
                                }

                                strSource = msg.Mail.Subject;
                                bToUseFilter |= searchTextInField(strToFind, strSource, mro);
                            }
                            break;

                        case MailRuleField.MailText:
                            {
                                strSource = msg.Mail.Text;
                                bToUseFilter |= searchTextInField(strToFind, strSource, mro);

                                strSource = msg.Mail.GetTextFromHtml();
                                bToUseFilter |= searchTextInField(strToFind, strSource, mro);
                            }
                            break;

                        case MailRuleField.Recipients:
                            {
                                List<string> listRecipients = OM_MailMessages.GetMailAddressesRecipients(msg);
                                if ((listRecipients != null) && (listRecipients.Count > 0))
                                {
                                    foreach (var s in listRecipients)
                                    {
                                        bToUseFilter |= searchTextInField(strToFind, s, mro);
                                    }
                                }
                            }
                            break;
                        case MailRuleField.Sender:
                            {
                                strSource = OM_MailMessages.GetMailAddressSender(msg);
                                bToUseFilter |= searchTextInField(strToFind, strSource, mro);
                            }
                            break;

                        case MailRuleField.Subject:
                            {
                                strSource = msg.Mail.Subject;
                                bToUseFilter |= searchTextInField(strToFind, strSource, mro);
                            }
                            break;
                    }

                    if ((bAnd) && (!bToUseFilter))
                        return false;

                    if ((!bAnd) && (bToUseFilter))
                        return true;
                    
                    bToUse |= bToUseFilter;
                }

            }

            return bToUse;
        }

        //-------------------------------------------------------------------------------
        public static bool searchTextInField(string strSearch, string strSource, MailRuleOperator mro)
        {
            bool bFound = false;

            if ((strSearch == null)||(strSource==null)||(strSearch.Length==0)||(strSource.Length==0)||(strSearch.Length>strSource.Length))
                return false;

            switch (mro)
            { 
                case MailRuleOperator.Contain:
                     string sSource = strSource;
                     string sSearch = strSearch;
                     bFound = sSource.ToLower().Contains(sSearch.ToLower());
                     break;
                case MailRuleOperator.Equal:
                     bFound = strSource.Equals(strSearch, StringComparison.OrdinalIgnoreCase);
                     break;
                case MailRuleOperator.Begin:
                     bFound = strSource.Left(strSearch.Length).Equals(strSearch, StringComparison.OrdinalIgnoreCase);
                     break;
                case MailRuleOperator.Finish:
                     bFound = strSource.Right(strSearch.Length).Equals(strSearch, StringComparison.OrdinalIgnoreCase);
                     break;
            }


            return bFound;
        }

    }
}
