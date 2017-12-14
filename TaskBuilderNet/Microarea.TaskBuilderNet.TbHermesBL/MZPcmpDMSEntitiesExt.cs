using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class MZPcmpDMSEntities : DbContext
    {
        public MZPcmpDMSEntities(string connectionString)
            : base(connectionString)
        {
        }
    }
}
