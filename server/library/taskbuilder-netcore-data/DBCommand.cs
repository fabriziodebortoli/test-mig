using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

using Npgsql;
using System.Data.SqlClient;
using System.Threading;

namespace TaskBuilderNetCore.Data
{
    public class DBCommand : DbCommand, IDisposable
    {                                                                                    
        private Provider.DBType dbType { get; set; }
        private DbCommand command { get; set; }

        DbDataReader reader = null;

        public DBCommand(string query, DBConnection connection)
        {
            command = connection.CreateCommand();
            command.CommandText = query;
            DbConnection = connection;
            dbType = connection.dbType;
        }

        public DBCommand(string query, DBConnection connection, DbTransaction transaction)
        {
            command = connection.CreateCommand();
            command.CommandText = query;
            DbConnection = connection;
            dbType = connection.dbType;
            command.Transaction = transaction;
        }

        public override string CommandText { get { return command.CommandText; } set { command.CommandText = value; } }

        public override int CommandTimeout { get { return command.CommandTimeout; } set { command.CommandTimeout = value; } }

        public override CommandType CommandType { get { return command.CommandType; } set { command.CommandType = value; } }

        public override bool DesignTimeVisible { get { return command.DesignTimeVisible; } set { command.DesignTimeVisible = value; } }

        public override UpdateRowSource UpdatedRowSource { get { return command.UpdatedRowSource; } set { command.UpdatedRowSource = value; } }

        protected override DbConnection DbConnection {get; set;}
                                                                       
        protected override DbParameterCollection DbParameterCollection { get { return command.Parameters; } }
        protected new DbParameterCollection Parameters { get { return command.Parameters; } }

        protected override DbTransaction DbTransaction { get; set; }

        public override void Cancel()
        {
            command.Cancel();
        }
     
        public new void Dispose()
        {
            if (reader != null) reader.Dispose();
            reader = null;

            command.Dispose();
            command = null;
        }

        public override int ExecuteNonQuery()
        {
            return command.ExecuteNonQuery();
        }

        public override Task<int> ExecuteNonQueryAsync(CancellationToken token)
        {
            return command.ExecuteNonQueryAsync(token);
        }

        public override object ExecuteScalar()
        {
            return command.ExecuteScalar();
        }

        public override Task<object> ExecuteScalarAsync(CancellationToken token)
        {
            return command.ExecuteScalarAsync(token);
        }

        public new DBDataReader ExecuteReader()
        {
            try
            {
                reader = command.ExecuteReader();
                return new DBDataReader(reader);
            }
            catch (SqlException e)
            {
                throw new DBException("ExecuteReader fails", e);
            }
        }

        public new DBDataReader ExecuteReader(CommandBehavior behavior)
        {
            return (DBDataReader)ExecuteDbDataReader(behavior);       
        }

        public override void Prepare()
        {
            command.Prepare();
        }

        protected override DbParameter CreateDbParameter()
        {
            return command.CreateParameter();
        }

        public new DbParameter CreateParameter()
        {
            return command.CreateParameter();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return new DBDataReader(command.ExecuteReader(behavior));
        }
    }
}
