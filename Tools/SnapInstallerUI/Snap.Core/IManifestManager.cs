using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Core
{
    public interface IManifestManager
    {
        IManifest CreateManifest(IFolder toExplore, IFile outputFile, IFile inputFile, string build, string productVersion);
    }
}
