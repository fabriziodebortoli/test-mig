using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    class SystemInfoService : ISystemInfoService
    {
        readonly string operatingSystem;
        readonly string netFxVersion;
        readonly object access = new object();

        public string OperatingSystem => this.operatingSystem;

        public string NetFxVersion => this.netFxVersion;

        public SystemInfoService()
        {
            lock (access)
            {
                try
                {
                    this.operatingSystem = Environment.OSVersion.VersionString;
                }
                catch (InvalidOperationException)
                { }

                this.netFxVersion = Environment.Version.ToString(4);

            }
        }
    }
}
