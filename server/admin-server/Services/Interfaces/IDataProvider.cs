using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Services
{
    public interface IDataProvider
    {
        bool Save(IAdminModel iModel, string connString);
        bool Update(IAdminModel iModel, string connString);
    }
}
