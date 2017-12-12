using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_WorkersMailRulesWorkerShared
    {
        //-------------------------------------------------------------------------------
        public static List<OM_WorkersMailRulesWorkerShared> GetWorkersMailRulesWorkerSharedByWorkersMailRulesId(TbSenderDatabaseInfo cmp, int intWorkersMailRulesId)
        {
            List<OM_WorkersMailRulesWorkerShared> list = new List<OM_WorkersMailRulesWorkerShared>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_WorkersMailRulesWorkerShared
                            where p.WorkersMailRulesId == intWorkersMailRulesId
                            select p;
                list.AddRange(items);
            }
            return list;
        }
    }
}
