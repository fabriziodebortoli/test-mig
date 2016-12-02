using System;
using Npgsql;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data;
using System.Threading;
using System.Threading.Tasks;


namespace TaskBuilderNetCore.Data
{
    public class DBConnection : IDisposable
    {
      
        
        private Provider.DBType dbType { get; set; }

        private DbConnection connection;

        public DbConnection GetConnectionObject
        {  
            get
            {  
                return connection;
            }
        }

        public string ConnectionString
        {
            get
            {
                return connection.ConnectionString;
            }

            set
            {
                connection.ConnectionString = value;
            }
        }

        public string Database
        {
            get
            {
                return connection.Database;
            }
        }

        public string DataSource
        {
            get
            {
                return connection.DataSource;
            }
        }

        public string ServerVersion
        {

            get
            {
                return connection.ServerVersion;

            }
        }

        public ConnectionState State
        {
            get
            {
                return connection.State;
            }
        }

        // constructor 
        public DBConnection(Provider.DBType dbType, string connectionString)
        {
            this.dbType = dbType;
           
            switch (dbType)
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
                default:
                    throw new DBException(DBExceptionStrings.DatabaseNotSuported);
            }
        }


        public DBTransaction BeginTransaction()
        {
            return new DBTransaction(connection.BeginTransaction(), dbType);
        }

        public DBTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return new DBTransaction (connection.BeginTransaction(isolationLevel), dbType);
        }

        public void ChangeDatabase(string databaseName)
        {
            connection.ChangeDatabase(databaseName);
        }
        public static void ClearPool(DbConnection connection)
        {
            if (connection is NpgsqlConnection)
                NpgsqlConnection.ClearPool((NpgsqlConnection)connection);

            else if (connection is SqlConnection)
                SqlConnection.ClearPool((SqlConnection)connection);

            else
                throw new DBException(DBExceptionStrings.DatabaseNotSuported);
        }

        public void Close()
        {
            connection.Close();
        }

        public DbCommand CreateCommand()
        {
            return connection.CreateCommand();
        }

        public void Dispose()
        {
            if (connection.State != ConnectionState.Open)
                connection.Close();
        }

        public void Open()
        {
            connection.Open();
        }

        public Task OpenAsync(CancellationToken token)
        {
            return connection.OpenAsync(token);
        }   

    }
}
