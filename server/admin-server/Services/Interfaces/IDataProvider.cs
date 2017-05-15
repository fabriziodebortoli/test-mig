using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services
{
    public interface IDataProvider
    {
        bool Load();
        bool Save(IAdminModel iModel);
        bool Update(IAdminModel iModel);
        bool Delete(string userName);
    }
}
