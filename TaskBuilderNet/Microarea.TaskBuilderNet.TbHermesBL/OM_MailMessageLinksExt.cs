using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_MailMessageLinks
    {
        //-------------------------------------------------------------------------------
        static public OM_MailMessageLinks CreateDBMailMessageLinks()
        {
            OM_MailMessageLinks item = new OM_MailMessageLinks();
            DateTime now = DateTime.Now;
            item.TBCreated = now;
            item.TBModified = now;
            return item;
        }

    		
    }

}
