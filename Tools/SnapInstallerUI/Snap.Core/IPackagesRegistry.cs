using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Core
{
    public interface IPackagesRegistry
    {
        IPackage this[string packageId] { get; }
        int PackagesCount { get; }

        IEnumerable<IPackage> InstalledPackages { get; }

        bool IsInstalled(string packageId);
        void Remove(string packageId);
        void Add(IPackage package);
    }
}
