using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace provisioning_server.Model.Interfaces
{
    interface IInstance
    {
        string Id { get; }
        string Name { get; }
        bool Disabled { get; }
    }
}
