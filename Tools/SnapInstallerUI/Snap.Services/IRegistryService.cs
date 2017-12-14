namespace Microarea.Snap.Services
{
    public interface IRegistryService
    {
        string RetrieveProductInstallationPath();
        string[] RetrieveInstalledDictionaries();
    }
}
