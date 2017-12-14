using Microarea.Snap.IO;
using System;
using System.IO;

namespace Microarea.Snap.Core
{
    internal class SnapPackage : IPackage
    {
        public virtual IFile PackageFile
        {
            get;
            internal set;
        }

        public virtual IManifest Manifest
        {
            get;
            private set;
        }

        public virtual void Init(IFile packageFile, IManifest manifest)
        {
            if (packageFile == null)
            {
                throw new ArgumentNullException("packageFile");
            }
            if (packageFile.FullName == null)
            {
                throw new ArgumentNullException("packageFile");
            }
            if (packageFile.FullName.Trim().Length == 0)
            {
                throw new ArgumentException("'packageFile.FullName' is empty", "packageFile");
            }
            if (!packageFile.Exists)
            {
                throw new ArgumentException("'packageFile.FullName' file does not exist", "packageFile");
            }

            this.PackageFile = packageFile;            

            if (manifest == null)
            {
                throw new ArgumentNullException("manifest");
            }

            this.Manifest = manifest;
        }
    }
}
