using System;
using System.Data.Common;

namespace TaskBuilderNetCore.Data
{
    public class Provider
    {
        public enum DBType { SQLSERVER, POSTGRE, MYSQL };
    }


    public class DBException : DbException
    {
        public DBException() : base(){ }
        public DBException(string message) : base(message) { }
        public DBException(string message, Exception innerException) : base( message, innerException) { }

    }


    public class DBExceptionStrings
    {
        public static string DatabaseNotSuported = "Database not supported";

    }
}
