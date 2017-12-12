using LightInject;
using Microarea.Snap.Core;
using Microarea.Snap.IO;
using Microarea.Snap.Services;
using System;

namespace Microarea.Snap.Installer
{
    internal sealed class IocFactory : Snap.Core.IocFactory
    {
        PerContainerLifetime singletonInstallerService = new PerContainerLifetime();
        PerContainerLifetime singletonSnapPackagesRegistry = new PerContainerLifetime();
        PerContainerLifetime systemInfoService = new PerContainerLifetime();

        public IocFactory()
        {
            Init();
        }

        protected override void Dispose(bool managed)
        {
            base.Dispose(managed);
            if (managed)
            {
                if (singletonInstallerService != null)
                {
                    singletonInstallerService.Dispose();
                    singletonInstallerService = null;
                }
                if (singletonSnapPackagesRegistry != null)
                {
                    singletonSnapPackagesRegistry.Dispose();
                    singletonSnapPackagesRegistry = null;
                }
                if (systemInfoService != null)
                {
                    systemInfoService.Dispose();
                    systemInfoService = null;
                }
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "It has to know all these classes since it is the ioc container factory")]
        protected override void Init()
        {
            base.Init();

            Container.Register<IPackageManager, SnapPackageManager>();
            Container.Register<IManifestManager, SnapManifestManager>();
            Container.Register<IPackageLoader, SnapPackageLoader>();
            Container.Register<IInstallerService, InstallerService>(singletonInstallerService);
            Container.Register<IPackagesRegistry, SnapPackagesRegistry>(singletonSnapPackagesRegistry);
            Container.Register<IClickOnceService, ClickOnceService>();
            Container.Register<IProductCompatibilityService, ProductCompatibilityService>();
            Container.Register<IInstallationVersionService, InstallationVersionService>();
            Container.Register<IRegistryService, RegistryService>();
            Container.Register<IPathProviderService, ExecutionPathProviderService>();
            Container.Register<IFileSystemService, FileSystemService>();
            Container.Register<ISnapInstallerVersionService, SnapInstallerVersionService>();
            Container.Register<ISystemInfoService, SystemInfoService>(systemInfoService);

            Container.Register<IInversionOfControlFactory>((factory) => this);

            Container.Register<IProductFolderService, ProductFolderService>();

            Container.Register(s => ITransactionManagerFactoryMethod());
        }

        private ITransactionManager ITransactionManagerFactoryMethod()
        {
            var settings = this.GetInstance<ISettings>();
            if (settings.UseTransactions)
            {
                return new TransactionManager();
            }
            return new NoTransactionManager();
        }
    }
}
