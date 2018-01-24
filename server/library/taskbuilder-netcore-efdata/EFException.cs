using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.Data.EntityFramework
{
    //====================================================================================    
    public class EFException : Exception
    {
        //-----------------------------------------------------------------------------------------------------
        public EFException(string message, Exception innerEx)
            : base(message, innerEx)
        {
        }
    }
}
