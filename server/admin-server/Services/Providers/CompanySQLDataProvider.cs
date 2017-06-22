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
					using (SqlCommand command = new SqlCommand(Consts.SelectCompany, connection))
					{
						command.Parameters.AddWithValue("@Name", company.Name);
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								company.Description = dataReader["Description"] as string;
								company.CompanyDBServer = dataReader["CompanyDBServer"] as string;
								company.CompanyDBName = dataReader["CompanyDBName"] as string;
								company.CompanyDBOwner = dataReader["CompanyDBOwner"] as string;
								company.CompanyDBPassword = dataReader["CompanyDBPassword"] as string;
								company.DatabaseCulture = dataReader["DatabaseCulture"] as string;
								company.Disabled = (bool)dataReader["Disabled"];
								company.IsUnicode = (bool)dataReader["IsUnicode"];
								company.PreferredLanguage = dataReader["PreferredLanguage"] as string;
								company.ApplicationLanguage = dataReader["ApplicationLanguage"] as string;
								company.Provider = dataReader["Provider"] as string;
								company.SubscriptionKey = dataReader["SubscriptionKey"] as string;
								company.UseDMS = (bool)dataReader["UseDMS"];
								company.DMSDBServer = dataReader["DMSDBServer"] as string;
								company.DMSDBName = dataReader["DMSDBName"] as string;
								company.DMSDBOwner = dataReader["DMSDBOwner"] as string;
								company.DMSDBPassword = dataReader["DMSDBPassword"] as string;
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
		public OperationResult Save(IAdminModel iModel)
        {
			Company company;
			OperationResult opRes = new OperationResult();

            try
            {
				company = (Company)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

					using (SqlCommand command = new SqlCommand(Consts.ExistCompany, connection))
					{
						command.Parameters.AddWithValue("@Name", company.Name);
						using (SqlDataReader reader = command.ExecuteReader())
							while (reader.Read())
								company.CompanyId = (int)reader["CompanyId"];
					}

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = (company.CompanyId != -1) ? Consts.UpdateCompany : Consts.InsertCompany;
						
						command.Parameters.AddWithValue("@Name", company.Name);
						command.Parameters.AddWithValue("@Description", company.Description);
						command.Parameters.AddWithValue("@CompanyDBServer", company.CompanyDBServer);
						command.Parameters.AddWithValue("@CompanyDBName", company.CompanyDBName);
						command.Parameters.AddWithValue("@CompanyDBOwner", company.CompanyDBOwner);
						command.Parameters.AddWithValue("@CompanyDBPassword", company.CompanyDBPassword);
						command.Parameters.AddWithValue("@DatabaseCulture", company.DatabaseCulture);
						command.Parameters.AddWithValue("@Disabled", company.Disabled);
						command.Parameters.AddWithValue("@IsUnicode", company.IsUnicode);
						command.Parameters.AddWithValue("@PreferredLanguage", company.PreferredLanguage);
						command.Parameters.AddWithValue("@ApplicationLanguage", company.ApplicationLanguage);
						command.Parameters.AddWithValue("@Provider", company.Provider);
						command.Parameters.AddWithValue("@SubscriptionKey", company.SubscriptionKey);
						command.Parameters.AddWithValue("@UseDMS", company.UseDMS);
						command.Parameters.AddWithValue("@DMSDBServer", company.DMSDBServer);
						command.Parameters.AddWithValue("@DMSDBName", company.DMSDBName);
						command.Parameters.AddWithValue("@DMSDBOwner", company.DMSDBOwner);
						command.Parameters.AddWithValue("@DMSDBPassword", company.DMSDBPassword);

						if (company.CompanyId != -1)
							command.Parameters.AddWithValue("@CompanyId", company.CompanyId);

						command.ExecuteNonQuery();
					}

					opRes.Result = true;
                }
            }
            catch (Exception e)
            {
				opRes.Result = false;
				opRes.Message = String.Concat("An error occurred while saving Compan: ", e.Message);
				return opRes;
			}

            return opRes;
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
