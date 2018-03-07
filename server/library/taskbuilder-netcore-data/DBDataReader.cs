using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Npgsql;
using System.Data.SqlClient;
using System.Data.Common;
using System.Collections;
using System.Threading;


namespace TaskBuilderNetCore.Data
{
    public class DBDataReader : DbDataReader, IDisposable
    {
        private DbDataReader dt;

        public DBDataReader(DbDataReader dt)
        {
            this.dt = dt;
        }

        public override object this[string name]
        {
            get
            {
                return dt[name];
            }
        }

        public override object this[int ordinal]
        {
            get
            {
                return dt[ordinal];
            }
        }

        public override int Depth
        {
            get
            {
                return dt.Depth;
            }
        }

        public override int FieldCount
        {
            get
            {
                return dt.FieldCount;
            }
        }

        public override bool HasRows
        {
            get
            {
                return dt.HasRows;
            }
        }

        public override bool IsClosed
        {
            get
            {
                return dt.IsClosed;
            }
        }

        public override int RecordsAffected
        {
            get
            {
                return dt.RecordsAffected;
            }
        }

        public override bool GetBoolean(int ordinal)
        {
            return dt.GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return dt.GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return dt.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override char GetChar(int ordinal)
        {
            return dt.GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return dt.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return dt.GetDataTypeName(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return dt.GetDateTime(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return dt.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return dt.GetDouble(ordinal);
        }

        public override IEnumerator GetEnumerator()
        {
            return dt.GetEnumerator();
        }

        public override Type GetFieldType(int ordinal)
        {
            return dt.GetFieldType(ordinal);
        }
        public override T GetFieldValue<T>(int i)
        {
            return dt.GetFieldValue<T>(i);
        }

        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            return dt.GetFieldValueAsync<T>(ordinal, cancellationToken);
        }

        public override float GetFloat(int ordinal)
        {
            return dt.GetFloat(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return dt.GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            return dt.GetInt16(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return dt.GetInt32(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return dt.GetInt64(ordinal);
        }

        public override string GetName(int ordinal)
        {
            return dt.GetName(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            return dt.GetOrdinal(name);
        }

        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            return dt.GetProviderSpecificFieldType(ordinal);
        }

        public override object GetProviderSpecificValue(int ordinal)
        {
            return dt.GetProviderSpecificValue(ordinal);
        }

        public override int GetProviderSpecificValues(object[] values)
        {
            return dt.GetProviderSpecificValues(values);
        }
        public override string GetString(int ordinal)
        {
            return dt.GetString(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            return dt.GetValue(ordinal);
        }

        public override int GetValues(object[] values)
        {
            return dt.GetValues(values);
        }

        public override bool IsDBNull(int ordinal)
        {
            return dt.IsDBNull(ordinal);
        }

        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        {
            return dt.IsDBNullAsync(ordinal, cancellationToken);
        }

        public override bool NextResult()
        {
            return dt.NextResult();
        }

        public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            return dt.NextResultAsync(cancellationToken);
        }

        public override bool Read()
        {
            return dt.Read();
        }


        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            return dt.ReadAsync(cancellationToken);
        }

        public new void Dispose()
        {
            dt.Dispose();
            dt = null;
        }
    }
}
