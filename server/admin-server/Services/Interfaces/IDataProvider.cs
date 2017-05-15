using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services
{
    public interface IDataProvider
    {
        bool Save(IAdminModel iModel, string connString);
        bool Update(IAdminModel iModel, string connString);
        bool Delete(int accountId, string connString);
    }
}
