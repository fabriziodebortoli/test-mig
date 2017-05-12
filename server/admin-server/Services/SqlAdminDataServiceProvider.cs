using System;
using Microsoft.Extensions.Options;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Services.Interfaces;

using System.Data.SqlClient;

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

			try
			{
				using (SqlConnection connection = new SqlConnection(_connectionString))
				{
					connection.Open();

					using (SqlCommand command = new SqlCommand(string.Format("SELECT * FROM MP_Accounts WHERE Name = '{0}' AND Password = '{1}'", userName, password), connection))
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
			}
			catch (SqlException e)
			{
				Console.WriteLine(e.Message);
				throw (e);
			}

			return account;
		}

	}
}
