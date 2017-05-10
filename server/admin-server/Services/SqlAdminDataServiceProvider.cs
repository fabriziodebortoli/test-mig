using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services.Interfaces;

namespace Microarea.AdminServer.Services
{
    //================================================================================
    public class SqlAdminDataServiceProvider : IAdminDataServiceProvider
    {
        AppOptions _settings;
        string _connectionString;

		//-----------------------------------------------------------------------------	
		public SqlAdminDataServiceProvider(IOptions<AppOptions> settings)
        {
            _settings = settings.Value;

            if (_settings == null)
                return;

            _connectionString = _settings.DatabaseInfo.ConnectionString;
        }

		//-----------------------------------------------------------------------------	
		public IAccount ReadLogin(string userName, string password)
        {
			IAccount account = null;
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				connection.Open();

				using (SqlCommand command = new SqlCommand("SELECT * FROM MP_Accounts WHERE Name = '{0}' AND Password = '{1}'", connection))
				{
					using (SqlDataReader reader = command.ExecuteReader())
					{
						if (reader.HasRows)
						{
							account = new Account();
							while (!reader.Read())
							{
							}
						}
					}
				}
			}

			return account;
		}
	}
}
