using Microarea.AdminServer.Model.Interfaces;
using System;

namespace Microarea.AdminServer.Services
{
    public interface IDataProvider
    {
        IAdminModel Load(IAdminModel iModel);
        bool Save(IAdminModel iModel);
        bool Update(IAdminModel iModel);
        bool Delete(string userName);

        // database-dependent values
        DateTime MinDateTimeValue { get; }
    }
}
