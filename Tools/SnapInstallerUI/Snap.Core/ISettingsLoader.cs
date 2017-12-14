namespace Microarea.Snap.Core
{
    public interface ISettingsLoader
    {
        Settings Load();
        void Save(ISettings settings);
    }
}