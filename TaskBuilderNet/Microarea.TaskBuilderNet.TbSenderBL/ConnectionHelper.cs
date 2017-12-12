using System.Data.EntityClient;
using System.Data.SqlClient;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.TbSenderBL
{
	static class ConnectionHelper
	{
		//-------------------------------------------------------------------------------------
		private static string CreateConnectionString(TbSenderDatabaseInfo infos)
		{
			//connectionString="metadata=res://*/PostaLiteIntegrationModel.csdl|res://*/PostaLiteIntegrationModel.ssdl|res://*/PostaLiteIntegrationModel.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=(local);initial catalog=Az37_prova;integrated security=True;multipleactiveresultsets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient" />

			// Specify the provider name, server and database.
			string providerName = "System.Data.SqlClient";
			string serverName = infos.ServerName; // ".";
			string databaseModelName = "PostaLiteIntegrationModel";

			// Initialize the connection string builder for the
			// underlying provider.
			SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();

			// Set the properties for the data source.
			sqlBuilder.DataSource = infos.ServerName;
			sqlBuilder.InitialCatalog = infos.DatabaseName;
			sqlBuilder.IntegratedSecurity = infos.WinAuthentication;
			sqlBuilder.UserID = infos.Username;
			sqlBuilder.Password = infos.Password;

			// Build the SqlConnection connection string.
			string providerString = sqlBuilder.ToString();

			// Initialize the EntityConnectionStringBuilder.
			EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

			//Set the provider name.
			entityBuilder.Provider = providerName;

			// Set the provider-specific connection string.
			entityBuilder.ProviderConnectionString = providerString;

			// Set the Metadata location.
			entityBuilder.Metadata = string.Format(@"res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl", databaseModelName);
		
			return entityBuilder.ToString();
		}

		//-------------------------------------------------------------------------------------
		internal static PostaLiteIntegrationEntities GetPostaLiteIntegrationEntities(string company)
		{
			TbSenderDatabaseInfo dbInfo = LoginManagerConnector.GetCompaniesInfo(company);
			if (dbInfo == null)
				return null;
			string connectionString = CreateConnectionString(dbInfo);
			return new PostaLiteIntegrationEntities(connectionString);
		}
	}
}
