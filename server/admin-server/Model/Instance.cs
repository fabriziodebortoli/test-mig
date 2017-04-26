using System;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class Instance : IInstance
    {
        string id;
        string instanceName;
        bool disabled;
        public string Id { get { return this.Id; } }
        public string InstanceName { get { throw new NotImplementedException(); } }
        public bool Disabled { get { return this.Disabled; } }
    }
}
