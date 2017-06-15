using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services.Providers
{
	//================================================================================
	public class CompanySQLDataProvider : IDataProvider
    {
        string connectionString;

		//---------------------------------------------------------------------
		public CompanySQLDataProvider(string connString)
        {
            this.connectionString = connString;
        }

		//---------------------------------------------------------------------
		public DateTime MinDateTimeValue  { get { return (DateTime)SqlDateTime.MinValue; } }

		//---------------------------------------------------------------------
		public IAdminModel Load(IAdminModel iModel)
		{
			Company company;

			try
			{
				company = (Company)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.SelectCompanyByName, connection))
					{
						command.Parameters.AddWithValue("@Name", company.Name);
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								company.Description = dataReader["Description"] as string;
								company.Provider = dataReader["Provider"] as string;
								company.Disabled = (bool)dataReader["Disabled"];
								company.IsUnicode = (bool)dataReader["IsUnicode"];
								company.ExistsOnDB = true;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}

			return company;
		}

		//---------------------------------------------------------------------
		public bool Save(IAdminModel iModel)
        {
			Company company;

            try
            {
				company = (Company)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

					bool existCompany = false;

					using (SqlCommand command = new SqlCommand(Consts.ExistCompany, connection))
					{
						command.Parameters.AddWithValue("@CompanyId", company.CompanyId);
						existCompany = (int)command.ExecuteScalar() > 0;
					}

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = existCompany ? Consts.UpdateCompany : Consts.InsertCompany;
						
						command.Parameters.AddWithValue("@Name", company.Name);
						command.Parameters.AddWithValue("@Description", company.Description);
						command.Parameters.AddWithValue("@CompanyDBServer", (company.CompanyDatabaseInfo != null) ? company.CompanyDatabaseInfo.Server : string.Empty);
						command.Parameters.AddWithValue("@CompanyDBName", (company.CompanyDatabaseInfo != null) ? company.CompanyDatabaseInfo.Database : string.Empty);
						command.Parameters.AddWithValue("@CompanyDBOwner", (company.CompanyDatabaseInfo != null) ? company.CompanyDatabaseInfo.DBOwner : string.Empty);
						command.Parameters.AddWithValue("@CompanyDBPassword", (company.CompanyDatabaseInfo != null) ? company.CompanyDatabaseInfo.Password : string.Empty);
						command.Parameters.AddWithValue("@DatabaseCulture", company.DatabaseCulture);
						command.Parameters.AddWithValue("@Disabled", company.Disabled);
						command.Parameters.AddWithValue("@IsUnicode", company.IsUnicode);
						command.Parameters.AddWithValue("@PreferredLanguage", company.PreferredLanguage);
						command.Parameters.AddWithValue("@ApplicationLanguage", company.ApplicationLanguage);
						command.Parameters.AddWithValue("@Provider", company.Provider);
						command.Parameters.AddWithValue("@SubscriptionId", company.SubscriptionId);
						command.Parameters.AddWithValue("@UseDMS", company.UseDMS);
						command.Parameters.AddWithValue("@DMSDBServer", (company.DMSDatabaseInfo != null) ? company.DMSDatabaseInfo.Server : string.Empty);
						command.Parameters.AddWithValue("@DMSDBName", (company.DMSDatabaseInfo != null) ? company.DMSDatabaseInfo.Database : string.Empty);
						command.Parameters.AddWithValue("@DMSDBOwner", (company.DMSDatabaseInfo != null) ? company.DMSDatabaseInfo.DBOwner : string.Empty);
						command.Parameters.AddWithValue("@DMSDBPassword", (company.DMSDatabaseInfo != null) ? company.DMSDatabaseInfo.Password : string.Empty);

						if (existCompany)
							command.Parameters.AddWithValue("@CompanyId", company.CompanyId);

						command.ExecuteNonQuery();
					}
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

		//---------------------------------------------------------------------
		public bool Delete(IAdminModel iModel)
		{
			Company company;

			try
			{
				company = (Company)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.DeleteCompany, connection))
					{
						command.Parameters.AddWithValue("@CompanyId", company.CompanyId);
						command.ExecuteNonQuery();
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}

			return true;
		}
	}
}
