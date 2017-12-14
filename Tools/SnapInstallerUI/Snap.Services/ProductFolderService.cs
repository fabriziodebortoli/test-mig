using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microarea.Snap.IO;
using Microarea.Snap.Core;
using System.Globalization;

namespace Microarea.Snap.Services
{
    public class ProductFolderService : IProductFolderService, ILogger
    {
        readonly ISettings settings;
        bool isProductFolderSet;
        IInversionOfControlFactory iocFactory;
        ISettingsLoader settingsLoader;
        IFileSystemService fileSystemService;
        IInstallationVersionService installationVersionService;

        public bool IsProductFolderSet
        {
            get
            {
                return isProductFolderSet;
            }
        }

        public string ProductInstanceFolder
        {
            get
            {
                return settings.ProductInstanceFolder;
            }

            set
            {
                if (value == null)
                {
                    throw new ProductFolderException(Services.Properties.Resources.SentProductFolderCannotBeNull);
                }
                if (value.Trim().Length == 0)
                {
                    throw new ProductFolderException(Services.Properties.Resources.SentProductFolderCannotBeEmpty);
                }
                if (!System.IO.Directory.Exists(value))
                {
                    throw new ProductFolderException(string.Format(CultureInfo.InvariantCulture, Services.Properties.Resources.SentProductFolderDoesNotExist, value));
                }
                settings.ProductInstanceFolder = value;
                settings.SnapPackagesRegistryFolder = CalculatePackageRegistryFolder().FullName;

                this.settingsLoader.Save(settings);
                EnsureProductFolder();
            }
        }

        public ProductFolderService(
            ISettings settings,
            IInversionOfControlFactory factory,
            ISettingsLoader settingsLoader,
            IFileSystemService fileSystemService,
            IInstallationVersionService installationVersionService
            )
        {
            this.settings = settings;
            this.iocFactory = factory;
            this.settingsLoader = settingsLoader;
            this.fileSystemService = fileSystemService;
            this.installationVersionService = installationVersionService;
        }

        public void EnsureProductFolder()
        {
            if (settings.ProductInstanceFolder == null || string.IsNullOrWhiteSpace(settings.ProductInstanceFolder))
            {
                this.LogInfo(Services.Properties.Resources.ProductInstallationPathNotSet);

                var installationPath = fileSystemService.CalculateProductInstallationPath();

                if (string.IsNullOrWhiteSpace(installationPath))
                {
                    this.LogInfo(Services.Properties.Resources.CannotReadInstallationPathFromRegistry);
                    return;
                }

                settings.ProductInstanceFolder = installationPath;

                this.LogInfo(string.Format(CultureInfo.InvariantCulture, Services.Properties.Resources.ProductInstallationPathSet, installationPath));
            }

            IFolder packagesRegistryFolder = CalculatePackageRegistryFolder();

            if (!packagesRegistryFolder.Exists)
            {
                this.LogInfo(Services.Properties.Resources.PackageRegistryFolderDoesNotExist);
                packagesRegistryFolder.Create();
                this.LogInfo(Services.Properties.Resources.PackageRegistrySuccessfullyCreated);
            }

            if (settings.SnapPackagesRegistryFolder == null || string.IsNullOrWhiteSpace(settings.SnapPackagesRegistryFolder))
            {
                settings.SnapPackagesRegistryFolder = packagesRegistryFolder.FullName;
            }

            isProductFolderSet = true;
        }

        internal IFolder CalculatePackageRegistryFolder()
        {
            var dirInfo = new System.IO.DirectoryInfo(settings.ProductInstanceFolder);
            var instanceName = dirInfo.Name;

            var packagesRegistryFolder = System.IO.Path.Combine(Settings.ConfigRootFolderPath, string.Concat(instanceName, "-", installationVersionService.ProductName, "-", installationVersionService.Version));

            return iocFactory.GetInstance<IFolder, string>(packagesRegistryFolder);
        }
    }
}
