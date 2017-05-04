using Microarea.AdminServer.Services.Interfaces;
using System;
using System.Data.SqlClient;
using Microarea.AdminServer.Model.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microarea.AdminServer.Services
{
    //================================================================================
    public class SqlAdminDataServiceProvider : IAdminDataServiceProvider
    {
        AppOptions _settings;
        string _connectionString;

        public SqlAdminDataServiceProvider(IOptions<AppOptions> settings)
        {
            _settings = settings.Value;

            if (_settings == null)
                return;

            _connectionString = _settings.DatabaseConnection.Value;
        }
        public IUserAccount ReadLogin(string userName, string password)
        {
            throw new NotImplementedException();
        }
    }
}
