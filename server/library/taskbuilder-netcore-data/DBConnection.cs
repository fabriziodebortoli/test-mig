using System;
using Npgsql;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data;
using System.Threading;
using System.Threading.Tasks;


namespace TaskBuilderNetCore.Data
{
    public class DBConnection :DbConnection, IDisposable
    {
            
        public Provider.DBType dbType { get; private set; }

        private DbConnection connection;

        public DbConnection GetConnectionObject
        {  
            get
            {  
                return connection;
            }
        }

        public override string ConnectionString
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

        public override string Database
        {
            get
            {
                return connection.Database;
            }
        }

        public override string DataSource
        {
            get
            {
                return connection.DataSource;
            }
        }

        public override string ServerVersion
        {

            get
            {
                return connection.ServerVersion;

            }
        }

        public override ConnectionState State
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
                //case Provider.DBType.POSTGRE:
                //    {
                //        connection = new NpgsqlConnection(new NpgsqlConnectionStringBuilder(connectionString));
                //        break;
                //    }
                case Provider.DBType.SQLSERVER:
                    {
                        connection = new SqlConnection(connectionString);
                        break;
                    }
                default:
                    throw new DBException(DBExceptionStrings.DatabaseNotSuported);
            }
        }


        public new DBTransaction BeginTransaction()
        {
            return new DBTransaction(connection.BeginTransaction(),dbType);
        }

        public new DBTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return new DBTransaction(connection.BeginTransaction(isolationLevel), dbType);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return connection.BeginTransaction(isolationLevel);
        }
                                                                           
        protected override DbCommand CreateDbCommand()
        {
            return connection.CreateCommand();
        }

        protected new DbCommand CreateCommand()
        {
            return CreateDbCommand();
        }

        public override void ChangeDatabase(string databaseName)
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

        public override void Close()
        {
            try
            {
                //TODO RSWEB
                //if (connection.State == ConnectionState.Open)  != closed
                //    connection.Close();
            }
            catch (SqlException se)
            {; }
            catch (Exception e)
            {; }

        }


        public override void Open()
        {
            connection.Open();
        }

        public override Task OpenAsync(CancellationToken token)
        {
            return connection.OpenAsync(token);
        }

        public new void Dispose()
        {
            //connection.Dispose();
        }
    }
}
