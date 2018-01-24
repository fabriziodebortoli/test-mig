using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.Documents.Model
{
    //====================================================================================    
    public class DBTSlaveBuffered : DBTSlave
    {
        //---------------------------------------------------------------------
        public DBTSlaveBuffered(Type recordType)
            : 
            base(recordType)
        {
        }
    }
}
