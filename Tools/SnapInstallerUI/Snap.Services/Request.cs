using Microarea.Snap.Core;
using Microarea.Snap.IO;

namespace Microarea.Snap.Services
{
    public class Request
    {
        public RequestType RequestType { get; set; }
        public IPackage Package { get; set; }
        public IPackage InstalledPackage { get; set; }
        public IFolder RootFolder { get; set; }
    }

}