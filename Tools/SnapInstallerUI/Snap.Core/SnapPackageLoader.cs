using ICSharpCode.SharpZipLib.Zip;
using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Core
{
    internal class SnapPackageLoader : IPackageLoader
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "ZipFile doesn't hold the stream so it doesn't dispose it, see ZipFile.IsStreamOwner for details")]
        public IPackage LoadPackage(IFile packageFile)
        {
            if (packageFile == null)
            {
                throw new ArgumentNullException("packageFile");
            }
            using (var inputStream = packageFile.OpenRead())
            using (var zipFile = new ZipFile(inputStream))
            {
                ZipEntry manifestZipEntry = null;
                foreach (ZipEntry zipEntry in zipFile)
                {
                    if (string.Compare(Path.GetExtension(zipEntry.Name), "." + Constants.ManifestExtension, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        manifestZipEntry = zipEntry;
                        break;
                    }
                }

                if (manifestZipEntry == null)
                {
                    throw new PackageException("Unable to find the manifest file in the given package " + packageFile.FullName);
                }

                using (var manifestStream = zipFile.GetInputStream(manifestZipEntry))
                {
                    var manifest = new SnapManifest();
                    manifest.Init(manifestStream);

                    var package = new SnapPackage();
                    package.Init(packageFile, manifest);

                    return package;
                }
            }
        }
    }
}
