using Microarea.Snap.IO;

namespace Microarea.Snap.Services
{
    public interface IFileSystemService
    {
        string CalculateProductInstallationPath();
        IFile InstallationVersionFile { get; }
    }
}