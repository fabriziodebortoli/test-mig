using Microarea.Snap.Core;
using Microarea.Snap.IO;

namespace Microarea.Snap.Services
{
    public interface IPackageManager
    {
        void Install(IPackage package, IFolder rootFolder);
        void Uninstall(IPackage package, IFolder rootFolder);
        void Update(IPackage oldPackage, IPackage newPackage, IFolder rootFolder);
        IPackage CreatePackage(IFile manifestFile, IFolder rootFolder, IFile outputFile);
    }
}
