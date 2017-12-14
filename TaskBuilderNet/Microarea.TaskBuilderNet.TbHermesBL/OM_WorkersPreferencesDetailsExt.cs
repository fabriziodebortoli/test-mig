using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_WorkersPreferencesDetails
    {
        public static bool DateInAutomaticResponseRange(TbSenderDatabaseInfo cmp, int aWorkerId)
        {
            bool bReturn = false;
            List<OM_WorkersPreferencesDetails> list = new List<OM_WorkersPreferencesDetails>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_WorkersPreferencesDetails
                            where p.WorkerId == aWorkerId
                            select p;
                list.AddRange(items);
            }
            if ((list != null) && (list.Count > 0))
            {
                if ((list.ElementAt(0).MailAutomaticResponseStart.HasValue) &&
                    (list.ElementAt(0).MailAutomaticResponseEnd.HasValue))
                {

                    if ((list.ElementAt(0).MailAutomaticResponseStart.Value.Date <= DateTime.Now.Date) &&
                        (list.ElementAt(0).MailAutomaticResponseEnd.Value.Date >= DateTime.Now.Date))
                        bReturn = true;
                }
            }

            return bReturn;
        }

    }
}
