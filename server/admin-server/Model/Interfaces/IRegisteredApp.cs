using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Model.Interfaces
{
    public interface IRegisteredApp
    {
        string AppId { get; set; }
        string Name { get; set; }
        string Description { get; set; } // received from PAI
        string URL { get; set; }
        string SecurityValue { get; set; }
    }
}
