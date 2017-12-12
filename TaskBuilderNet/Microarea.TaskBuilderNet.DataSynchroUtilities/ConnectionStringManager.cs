using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities
{
    public class ParserConnectionString
    {
        public string GetConnectionStringInfo(string connectionString, string node)
        {
            string info = string.Empty;
            // Estrazione della companyName dalla ConnectionString: "Data Source=USR-BONZIANNA1;Initial Catalog='M4G0_1_x';User ID='IT';Password='';Connect Timeout=30;Pooling=false;"
            if (connectionString.Contains(node))
            {
                string[] result = connectionString.Split(new char[] { ';' });
                foreach (string str in result)
                {
                    if (!str.StartsWith(node))
                        continue;

                    int nStart = str.IndexOf('\'') + 1;
                    int nEnd = str.LastIndexOf('\'');
                    int nLength = nEnd - nStart;
                    info = str.Substring(nStart, nLength);
                    break;
                }
            }
            return info;
        }
    }      

    public static class StringExtensions
    {
        public static string GetConnectionStringInfo(string connectionString, string node)
        {
            if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(node))
                return string.Empty;

            string info = string.Empty;
            // Estrazione delle info data una ConnectionString: "Data Source=USR-xxx;Initial Catalog='xxx';User ID='xxx';Password='';Connect Timeout=30;Pooling=false;"
            if (connectionString.Contains(node))
            {
                string[] result = connectionString.Split(new char[] { ';' });
                foreach (string str in result)
                {
                    if (!str.StartsWith(node))
                        continue;
                                 
                    int nStart = str.IndexOf('=') + 1;
                    int nLength = str.Length - nStart;
                    string strFinal = str.Substring(nStart, nLength);
                    info = strFinal.Trim('\'');
                    break;
                }
            }
            return info;
        }

        public static string GetCompany(this string connectionString)
        {
            return GetConnectionStringInfo(connectionString, "Initial Catalog");
        }

        public static string GetUserID(this string connectionString)
        {
            return GetConnectionStringInfo(connectionString, "User ID");
        }

        public static string GetPassword(this string connectionString)
        {
            return GetConnectionStringInfo(connectionString, "Password");
        }
        
        public static string GetDataSource(this string connectionString)
        {
            return GetConnectionStringInfo(connectionString, "Data Source");            
        }
    }

    public class ConnectionStringManager
    {
        public string CompanyConnectionString { get; private set; }
        public string DMSConnectionString { get; private set; }

        public ConnectionStringManager(string authenticationToken)
        {
            CompanyConnectionString = ConnectionStrings.GetConnectionString(authenticationToken);
            DMSConnectionString = ConnectionStrings.GetDMSConnectionString(authenticationToken);
        }
    }
}
