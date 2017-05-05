using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer
{
    //================================================================================
    public class AppOptions
    {
        public DatabaseConnectionString DatabaseConnection { get; set; }
    }

    //================================================================================
    public class DatabaseConnectionString
    {
        public string Value { get; set; }
    }
}
