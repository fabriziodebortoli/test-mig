using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microarea.DataService.Managers.Interfaces;
using MoreLinq;
using static SQLHelper;

namespace Microarea.DataService.Managers
{
    public class ParameterManager : IParameterManager
    {
        private Dictionary<string, Dictionary<string, object>> _parameters = new Dictionary<string, Dictionary<string, object>>();

        /// <summary>
        /// Data una lista di parametri ritorna un dizionario parametro-valore
        /// </summary>
        public Dictionary<string, object> GetParameters(string table, string connectionString)
        {
            var result = new Dictionary<string, object>();
            var connection = new SqlConnection(connectionString);
            using (var reader = ExecuteReader(connection, System.Data.CommandType.Text,
                 $"select * from MA_{table}", null))
            {
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result[reader.GetName(i)] = reader.GetValue(i);
                    }
                }

                return result;
            }
        }
    }
}
