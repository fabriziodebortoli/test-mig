using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer
{
    //================================================================================
    public class AppOptions
    {
        public DatabaseConnection DatabaseConnection { get; set; }
    }

    //================================================================================
    public class DatabaseConnection
    {
        public string Value { get; set; }
    }
}
