using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model.Interfaces
{
    public interface IAdminModel
    {
        void SetDataProvider(IDataProvider dataProvider);
        bool Save();
        void Load();
    }
}
