using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.TbHermesBL;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_ModulesOFM
    {
        //-------------------------------------------------------------------------------
        public bool BoolIsActive
        {
            get { return this.IsActive == "1"; }
            set { this.IsActive = value ? "1" : "0"; }
        }

        //-------------------------------------------------------------------------------
        public static OM_ModulesOFM GetModulesOFM(TbSenderDatabaseInfo cmp, string module)
        {
            OM_ModulesOFM moduleOFM = null;

            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                moduleOFM = GetModulesOFM(db, module);
            }

            return moduleOFM;
        }

        //-------------------------------------------------------------------------------
        public static OM_ModulesOFM GetModulesOFM(MZP_CompanyEntities db, string module)
        {
            List<OM_ModulesOFM> list = new List<OM_ModulesOFM>();
            var items = from m in db.OM_ModulesOFM
                        where m.ModuleOFM == module
                        select m;
            list.AddRange(items);

            OM_ModulesOFM comm = null;
            if (list.Count > 0)
                comm = list[0];

            return comm;
        }
    }
}
