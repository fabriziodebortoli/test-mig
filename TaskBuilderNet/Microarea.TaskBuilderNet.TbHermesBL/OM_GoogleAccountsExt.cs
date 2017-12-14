using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.TbHermesBL;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_GoogleAccounts
    {
        public string PrimaryAddress = "";

        //-------------------------------------------------------------------------------
        public static List<OM_GoogleAccounts> GetGoogleAccounts(TbSenderDatabaseInfo cmp)
        {
            List<OM_GoogleAccounts> list = new List<OM_GoogleAccounts>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = (from p in db.OM_GoogleAccounts
                             where ((p.WorkerId != 0)                       &&
                                    (p.ClientSecretPath != "")   &&
                                    (p.WorkerLogPath != "")      &&
                                    (p.FileDataStorePath != "")) 
                             select p).OrderBy(x => x.WorkerId);
                list.AddRange(items);
            }
            return list;
        }


    }
}
