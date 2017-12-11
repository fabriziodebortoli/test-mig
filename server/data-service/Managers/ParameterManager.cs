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
        public Dictionary<string, object> GetParameters(IEnumerable<string> parameters, string connectionString)
        {
            return parameters
                            .Distinct()
                            .ToDictionary(x => x, x => GetParam(x, connectionString));
        }

        /// <summary>
        /// Aggiorna la cache delle tabelle che contengono i parametri specificati
        /// </summary>
        public void UpdateCache(IEnumerable<string> parameters, string connectionString)
        {
            var toUpdate = parameters
                   .Where(x => x.Split('.').Length == 2)
                   .Select(x => x.Split('.')[0])
                   .Distinct();

            toUpdate.ForEach(x => UpdateCachedParamTable(x, connectionString));
        }

        /// <summary>
        /// Dato un parametro ne ritorna il valore (se cachato), altrimenti ottiene i dati da db e ci riprova
        /// </summary>
        private object GetParam(string p, string connectionString)
        {
            var _p = p.Split('.');

            if (_p.Length != 2) return "unknow param";

            var table = _p[0];
            var column = _p[1];

            if (_parameters.TryGetValue(table, out Dictionary<string, object> paramTable))
            {
                if (paramTable.TryGetValue(column, out object value))
                {
                    return value;
                }
                return "unknow param";
            }

            UpdateCachedParamTable(table, connectionString);

            return GetParam(p, connectionString);
        }

        /// <summary>
        /// Aggiorna la cache dei parametri per la tabella specificata
        /// </summary>
        private void UpdateCachedParamTable(string table, string connectionString)
        {
            var connection = new SqlConnection(connectionString);
            using (var reader = ExecuteReader(connection, System.Data.CommandType.Text,
                 $"select * from MA_{table}", null))
            {
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        _parameters[table][reader.GetName(i)] = reader.GetValue(i);
                    }
                }
            }
        }
    }
}
