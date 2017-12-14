using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_Workers
    {
        //-------------------------------------------------------------------------------
        public static string GetUserName(TbSenderDatabaseInfo dbi, int wID)
        {
            string userName;
            using (var db = ConnectionHelper.GetHermesDBEntities(dbi))
            {
                var items = from p in db.OM_Workers
                            where p.WorkerId == wID
                            select p;
                userName = items.FirstOrDefault().WorkerLogin;
            }
            return userName;
        }
    }
}
