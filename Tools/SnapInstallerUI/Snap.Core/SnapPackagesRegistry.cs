using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Core
{
    internal class SnapPackagesRegistry : IPackagesRegistry
    {
        readonly object lockTicket = new object();
 
        ISettings settings;
        List<IPackage> installedPackages = new List<IPackage>();
        IPackageLoader packageLoader;

        public int PackagesCount
        {
            get
            {
                lock (lockTicket)
                {
                    return this.installedPackages.Count;
                }
            }
        }

        public IEnumerable<IPackage> InstalledPackages
        {
            get
            {
                return installedPackages;
            }
        }

        public IPackage this[string packageId]
        {
            get
            {
                lock (lockTicket)
                {
                    return
                    this.installedPackages
                    .Where(ip => string.Compare(packageId, ip.Manifest.Id, StringComparison.OrdinalIgnoreCase) == 0)
                    .FirstOrDefault();
                }
            }
        }

        public SnapPackagesRegistry(ISettings settings, IPackageLoader packageLoader)
            : this (settings, packageLoader, IocFactory.Instance.GetInstance<IFolder, string>(settings.SnapPackagesRegistryFolder))
        {
            
        }
        //Test purpose
        internal SnapPackagesRegistry(ISettings settings, IPackageLoader packageLoader, IFolder snapPackagesRegistryFolder)
        {
            this.packageLoader = packageLoader;
            this.settings = settings;

            this.installedPackages.Clear();

            foreach (var packageFile in snapPackagesRegistryFolder.GetFiles(string.Concat("*.", Constants.PackageExtension)))
            {
                var package = this.packageLoader.LoadPackage(packageFile);

                this.AddInternal(package);
            }
        }

        public virtual bool IsInstalled(string packageId)
        {
            lock (lockTicket)
            {
                return this[packageId] != null;
            }
        }

        public virtual void Remove(string packageId)
        {
            lock (lockTicket)
            {
                var package = this[packageId];
                if (package != null)
                {
                    this.installedPackages.Remove(package);
                    package.PackageFile.Delete();
                }
            }
        }
        public virtual void Add(IPackage package)
        {
            lock (lockTicket)
            {
                if (package == null)
                {
                    throw new ArgumentNullException("package");
                }
                AddInternal(package);

                var newPath = IocFactory.Instance.GetInstance<IFile, string>(Path.Combine(this.settings.SnapPackagesRegistryFolder, package.PackageFile.Name));
                package.PackageFile.CopyTo(newPath);
                (package as SnapPackage).PackageFile = newPath;
            }
        }

        private void AddInternal(IPackage package)
        {
            lock (lockTicket)
            {
                var alreadyInstalledPackage = this[package.Manifest.Id];
                if (alreadyInstalledPackage != null)
                {
                    throw new RegistryException(package.Manifest.Id + " is already present");
                }
                this.installedPackages.Add(package);
            }
        }
    }
}
