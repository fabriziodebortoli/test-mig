using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Data
{
    public class DBTransaction :IDisposable
    {
        private DbTransaction transaction;
        private Provider.DBType dbType { get; set; }

        public DBTransaction(DbTransaction transaction, Provider.DBType dbType)
        {
            this.transaction = transaction;
            this.dbType = dbType;
        }

        public void Commit()
        {
            transaction.Commit();
        }

        public void Rollback()
        {
            transaction.Rollback();
        }

        public void Rollback(string name)
        {
            switch (dbType)
            {
                case Provider.DBType.POSTGRE:
                    {
                        ((NpgsqlTransaction)transaction).Rollback(name);
                        break;
                    }
                case Provider.DBType.SQLSERVER:
                    {
                        ((SqlTransaction)transaction).Rollback(name);
                        break;
                    }
                default:
                    throw new DBException(DBExceptionStrings.DatabaseNotSuported);
            }
        }

        public void Save( string savePointName )
        {
            switch (dbType)
            {
                case Provider.DBType.POSTGRE:
                    {
                        ((NpgsqlTransaction)transaction).Save(savePointName);
                        break;
                    }
                case Provider.DBType.SQLSERVER:
                    {
                        ((SqlTransaction)transaction).Save(savePointName);
                        break;
                    }
                default:
                    throw new DBException(DBExceptionStrings.DatabaseNotSuported);
            }
        }

        public void Dispose()
        {
            transaction.Dispose();
        }


    }
}
