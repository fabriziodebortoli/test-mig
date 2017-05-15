using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services
{
    public interface IDataProvider
    {
        IAdminModel Load(IAdminModel iModel);
        bool Save(IAdminModel iModel);
        bool Update(IAdminModel iModel);
        bool Delete(string userName);
    }
}
