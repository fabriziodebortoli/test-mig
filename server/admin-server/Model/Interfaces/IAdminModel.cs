using Microarea.AdminServer.Services;

namespace Microarea.AdminServer.Model.Interfaces
{
    public interface IAdminModel
    {
        IDataProvider DataProvider { get; set; }
        bool Save(string connectionString);
    }
}
