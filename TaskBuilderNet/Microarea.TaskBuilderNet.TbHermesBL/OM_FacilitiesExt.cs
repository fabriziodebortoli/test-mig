using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.TbHermesBL;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_Facilities
    {

        //-------------------------------------------------------------------------------
        public bool BoolAllowOverlap
        {
            get { return this.AllowOverlap == "1"; }
            set { this.AllowOverlap = value ? "1" : "0"; }
        }
        
        //-------------------------------------------------------------------------------
        public static OM_Facilities GetFacility(TbSenderDatabaseInfo cmp, int facilityId)
        {
            OM_Facilities facility = null;
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                facility = GetFacility(db, facilityId);
            }

            return facility;
        }

        //-------------------------------------------------------------------------------
        public static OM_Facilities GetFacility(MZP_CompanyEntities db, int facilityId)
        {
            List<OM_Facilities> list = new List<OM_Facilities>();
            var items = from f in db.OM_Facilities
                        where f.FacilityId == facilityId
                        select f;
            list.AddRange(items);

            OM_Facilities facility = null;
            if (list.Count > 0)
                facility = list[0];

            return facility;
        }

    }
}
