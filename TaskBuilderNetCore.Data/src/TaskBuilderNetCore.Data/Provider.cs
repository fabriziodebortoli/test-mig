using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;

namespace TaskBuilderNetCore.Data
{
    public class Provider
    {
        public enum DBType { SQLSERVER, POSTGRE };
    }


    public class DBException : DbException
    {
        public DBException() : base(){ }
        public DBException(string message) : base(message) { }
        public DBException(string message, Exception innerException) : base( message, innerException) { }

    }
}
