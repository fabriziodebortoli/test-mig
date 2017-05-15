using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services
{
    public interface IDataProvider
    {
        bool Save(IAdminModel iModel);
        bool Update(IAdminModel iModel);
        bool Delete(int accountId);
    }
}
