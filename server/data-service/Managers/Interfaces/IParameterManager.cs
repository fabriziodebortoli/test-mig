using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.DataService.Managers.Interfaces
{
    public interface IParameterManager
    {
        Dictionary<string, string> GetParameters(IEnumerable<string> parameters);
    }
}
