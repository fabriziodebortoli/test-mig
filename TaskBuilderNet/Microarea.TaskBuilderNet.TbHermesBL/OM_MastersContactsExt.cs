using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
        //FABIO
    //-------------------------------------------------------------------------------
    public enum MastersContactType
    {
        undef       = 47382528,
        telef       = 47382529,
        fax         = 47382530,
        mobile      = 47382531,
        mail        = 47382532,
        skype       = 47382533,
        internet    = 47382534 
    }

    public partial class OM_MastersContacts
    {
        //-------------------------------------------------------------------------------
        public MastersContactType EnumContactType
        {
            get { return Enum.IsDefined(typeof(MastersContactType), this.ContactType) ? (MastersContactType)this.ContactType : (MastersContactType)0; }
            set { this.ContactType = (Int32)value; }
        }

    }


}
