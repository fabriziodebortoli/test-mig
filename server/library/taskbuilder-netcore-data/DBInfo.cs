﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TaskBuilderNetCore.Data
{
    public class DBInfo
    {
       public static bool ExistTable(string connectionString, string tableName)
        {
            using (DBConnection conn = new DBConnection(Provider.DBType.SQLSERVER, connectionString))
            {
                conn.Open();
                string query = string.Format("SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE  TABLE_NAME = '{0}'", tableName);
                using (DBCommand command = new DBCommand(query, conn))
                {
                    using (DBDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            return true;
                    }
                }
            }
                return false;
        }

        bool ExistColumn(string connectionString, string tableName, string columnName)
        {
            using (DBConnection conn = new DBConnection(Provider.DBType.SQLSERVER, connectionString))
            {
                conn.Open();
                string query = string.Format("SELECT name FROM sys.columns WHERE Name = N'{0}' AND Object_ID = Object_ID(N'{1}')", columnName, tableName);
                using (DBCommand command = new DBCommand(query, conn))
                {
                    using (DBDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            return true;
                    }
                }
            }
            return false;
        }

        string GetColumnType(string connectionString, string tableName, string columnName)
        {
            using (DBConnection conn = new DBConnection(Provider.DBType.SQLSERVER, connectionString))
            {
                conn.Open();
                string query = string.Format("SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{0}' AND COLUMN_NAME = '{1}'", 
                    tableName, columnName);
                using (DBCommand command = new DBCommand(query, conn))
                {
                    using (DBDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            return ObjectHelper.GetDotNetType(reader.GetString(0), Provider.DBType.SQLSERVER);
                            
                    }
                }
            }
            return string.Empty;
        }

        string GetColumnCollation(string connectionString, string tableName, string columnName)
        {
            using (DBConnection conn = new DBConnection(Provider.DBType.SQLSERVER, connectionString))
            {
                conn.Open();
                string query = string.Format("SELECT c.collation_name FROM SYS.COLUMNS c JOIN SYS.TABLES t ON t.object_id = c.object_id WHERE t.name = '{0}' and c.name='{1{'",
                    tableName, columnName);
                using (DBCommand command = new DBCommand(query, conn))
                {
                    using (DBDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            return reader.GetString(0);

                    }
                }
            }
            return string.Empty;
        }

        string GetNativeConvert(string connectionString, object value, bool useUnicode, Provider.DBType dbType)
        {
            string strSqlServerDateTs = "{{ts '{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}'}}";
            switch (value.GetType().Name)
            {
                case "Boolean":
                    {
                        return ObjectHelper.CastToDBBool((bool)value);
                    }
                case "Byte":
                case "Int16":
                case "Int32":
                case "Int64":
                case "Decimal":
                case "Single":
                case "Double":
                    {
                        return value.ToString();
                    }
                case "DataEnum":
                    {

                        //return ObjectHelper.CastToDBData(value).ToString();
                        break;
                    }
                case "String":
                    {
                        string str = value.ToString().Replace("'", "''");



                        return useUnicode ? String.Format("N'{0}'", str) : String.Format("'{0}'", str);
                    }
                case "Guid": // @@Anastasia: non so 
                    {
                        return String.Format("'{0}'", value.ToString());
                    }
                case "DateTime":
                    {
                        //Timestamp format: {ts '2001-01-15 00:00:00'}
                        DateTime dt = (DateTime)value;
                        string sDateConvert = string.Empty;

                        return String.Format(strSqlServerDateTs, dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    }
                default:
                    Debug.Fail("Illegal data type");
                    break;
            }
            return String.Empty;
        }

    }
}
