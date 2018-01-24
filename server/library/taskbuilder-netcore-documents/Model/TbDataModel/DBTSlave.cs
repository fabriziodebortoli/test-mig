using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class DBTSlave : DBTObject
    {
        //---------------------------------------------------------------------
        public DBTSlave(Type recordType)
            : 
            base(recordType)
        {
        }
    }
}
