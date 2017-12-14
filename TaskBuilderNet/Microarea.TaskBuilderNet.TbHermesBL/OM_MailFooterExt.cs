using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_MailFooter
    {
        //-------------------------------------------------------------------------------
        public enum MailFooterType
        {
            Footer = 11730944, // Footer
            AutomaticResponse = 11730945 // Automatic Response
        }

        public static List<OM_MailFooter> GetAutomaticResponse(TbSenderDatabaseInfo cmp, int aWorkerId)
        {
            List<OM_MailFooter> list = new List<OM_MailFooter>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_MailFooter
                            where p.FooterType == 11730945
                                && p.WorkerId == aWorkerId                                    
                            select p;
                list.AddRange(items);
            }
            return list;
        }
    }

}
