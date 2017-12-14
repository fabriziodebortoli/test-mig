using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microarea.Snap.Core;
using System.Globalization;
using Microarea.Snap.Services.Properties;

namespace Microarea.Snap.Services
{
    class ProductCompatibilityService : IProductCompatibilityService, ILogger
    {
        IInstallationVersionService installationVersionService;

        public ProductCompatibilityService(IInstallationVersionService installationVersionService)
        {
            this.installationVersionService = installationVersionService;
        }
        public void EnsureProductCompatibility(IPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }
            var manifestProduct = package.Manifest.Product.Replace(".", string.Empty);
            var product = installationVersionService.ProductName.Replace(".", string.Empty);
            if (string.Compare(manifestProduct, product, StringComparison.OrdinalIgnoreCase) != 0)
            {
                var exc = new PackageException(string.Format(CultureInfo.InvariantCulture, "Package {0} is built for {1} but {2} is installed instead", package.Manifest.Id, manifestProduct, product));
                this.LogError(Resources.WrongProduct, exc);
                throw exc;
            }

            if (string.CompareOrdinal(package.Manifest.ProductVersion, installationVersionService.Version) != 0)
            {
                var exc = new PackageException(string.Format(CultureInfo.InvariantCulture, "Package {0} is for {1} {2} but the installed {3} version is {4}", package.Manifest.Id, manifestProduct, package.Manifest.ProductVersion, manifestProduct, installationVersionService.Version));
                this.LogError(Resources.WrongProductVersion, exc);
                throw exc;
            }
        }
    }
}
