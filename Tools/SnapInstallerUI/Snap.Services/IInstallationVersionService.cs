namespace Microarea.Snap.Services
{
    public interface IInstallationVersionService
    {
        string ProductName { get; }
        string Version { get; }

        void InvalidateMenuCache();
    }
}