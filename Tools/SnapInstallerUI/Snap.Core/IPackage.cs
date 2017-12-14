using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Core
{
    public interface IPackage
    {
        IFile PackageFile { get; }

        IManifest Manifest { get; }
    }
}
