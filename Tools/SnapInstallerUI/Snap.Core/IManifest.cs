using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Microarea.Snap.Core
{
    public interface IManifest
    {
        string Id { get; }
        int Version { get; }
        string Title { get; }
        Uri IconUrl { get; }
        string Producer { get; }
        string Product { get; }
        string ProductVersion { get; }
        string Description { get; }
        Uri ReleaseNotesUrl { get; }

        ICollection<IFile> Files { get; }
    }
}
