using System.Collections.Generic;

namespace Microarea.DataService.Managers.Interfaces
{
    public interface IParameterManager
    {
        Dictionary<string, object> GetParameters(IEnumerable<string> parameters, string connectionString);
        void UpdateCache(IEnumerable<string> parameters, string connectionString);
    }
}
