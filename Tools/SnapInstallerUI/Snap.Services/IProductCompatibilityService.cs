namespace Microarea.Snap.Services
{
    public interface IProductCompatibilityService
    {
        void EnsureProductCompatibility(Core.IPackage package);
    }
}
