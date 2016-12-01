using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using System.Data.SqlClient;
using System.Data.Common;


namespace TaskBuilderNetCore.Data
{
    public class DBConnection 
    {    
        private string connectionString { get; set; }
        private Provider.DBType dbType { get; set; }

        private DbConnection connection;

        // constructor 
        public DBConnection (Provider.DBType dbType, string connectionString)
        {
            this.dbType = dbType;
            this.connectionString = connectionString;

            switch(dbType)
            {
                case Provider.DBType.POSTGRE:
                    {
                        connection = new NpgsqlConnection(new NpgsqlConnectionStringBuilder(connectionString));
                        break;
                    }
                case Provider.DBType.SQLSERVER:
                    {                
                        connection = new SqlConnection(connectionString);
                        break;
                    }
              
            }
        }



    }
}
