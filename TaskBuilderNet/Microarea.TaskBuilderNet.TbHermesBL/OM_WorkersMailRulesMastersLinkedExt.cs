using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_WorkersMailRulesMastersLinked
    {
        //-------------------------------------------------------------------------------
        public static List<OM_WorkersMailRulesMastersLinked> GetWorkersMailRulesMastersLinkedByWorkersMailRulesId(TbSenderDatabaseInfo cmp, int intWorkersMailRulesId)
        {
            List<OM_WorkersMailRulesMastersLinked> list = new List<OM_WorkersMailRulesMastersLinked>();
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                var items = from p in db.OM_WorkersMailRulesMastersLinked
                            where p.WorkersMailRulesId == intWorkersMailRulesId
                            select p;
                list.AddRange(items);
            }
            return list;
        }

    }
}
