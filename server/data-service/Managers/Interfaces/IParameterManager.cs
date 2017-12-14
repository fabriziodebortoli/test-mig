using System.Collections.Generic;

namespace Microarea.DataService.Managers.Interfaces
{
    public interface IParameterManager
    {
        Dictionary<string, object> GetParameters(string table, string connectionString);
    }
}
