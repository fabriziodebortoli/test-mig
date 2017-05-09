using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class DBTMaster : DBTObject
    {
        //---------------------------------------------------------------------
        public DBTMaster(Type recordType)
            : 
            base(recordType)
        {
        }
    }
}
