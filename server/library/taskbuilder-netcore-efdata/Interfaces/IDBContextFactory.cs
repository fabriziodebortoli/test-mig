using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.Data.EntityFramework.Interfaces
{
    //====================================================================================    
    public interface IDbContextFactory
    {
        object CreateContext();
    }
}
