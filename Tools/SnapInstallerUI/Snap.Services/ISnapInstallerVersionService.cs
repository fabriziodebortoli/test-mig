using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    public interface ISnapInstallerVersionService
    {
        string SnapInstallerVersion { get; }
        string SnapInstallerProductName { get; }
        string SnapInstallerCopyright { get; }
        string SnapInstallerCompanyName { get; }
    }
}
