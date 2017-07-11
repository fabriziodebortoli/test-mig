using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services.Providers
{
	//================================================================================
	public class CompanySQLDataProvider : ICompanyDataProvider
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
								company.CompanyId = (int)dataReader["CompanyId"];
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
					opRes.Content = company;
				}
            }
            catch (Exception e)
            {
				opRes.Result = false;
				opRes.Message = String.Concat("An error occurred while saving Company: ", e.Message);
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

		/// <summary>
		/// Ritorna una lista di Company dato un accountName ed una subscriptionKey
		/// </summary>
		/// <param name="accountName"></param>
		/// <param name="subscriptionKey"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public List<Company> GetCompanies(string accountName, string subscriptionKey)
		{
			if (string.IsNullOrWhiteSpace(accountName))
				return null;

			List<Company> companiesList = new List<Company>();

			string selectQuery = @"SELECT* FROM MP_Companies INNER JOIN MP_SubscriptionAccounts ON MP_SubscriptionAccounts.SubscriptionKey = MP_Companies.SubscriptionKey
									WHERE MP_SubscriptionAccounts.AccountName =  @AccountName";

			if (!string.IsNullOrWhiteSpace(subscriptionKey))
				selectQuery += " AND MP_Companies.SubscriptionKey = @SubscriptionKey";

			try
			{
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					using (SqlCommand command = new SqlCommand(selectQuery, connection))
					{
						command.Parameters.AddWithValue("@AccountName", accountName);

						if (!string.IsNullOrWhiteSpace(subscriptionKey))
							command.Parameters.AddWithValue("@SubscriptionKey", subscriptionKey);

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								Company company = new Company();
								company.CompanyId = (int)dataReader["CompanyId"];
								company.Name = dataReader["Name"] as string;
								company.Description = dataReader["Description"] as string;
								company.SubscriptionKey = dataReader["SubscriptionKey"] as string;
								company.CompanyDBServer = dataReader["CompanyDBServer"] as string;
								company.CompanyDBName = dataReader["CompanyDBName"] as string;
								company.CompanyDBOwner = dataReader["CompanyDBOwner"] as string;
								company.CompanyDBPassword = dataReader["CompanyDBPassword"] as string;
								company.UseDMS = (bool)dataReader["UseDMS"];
								company.DMSDBServer = dataReader["DMSDBServer"] as string;
								company.DMSDBName = dataReader["DMSDBName"] as string;
								company.DMSDBOwner = dataReader["DMSDBOwner"] as string;
								company.DMSDBPassword = dataReader["DMSDBPassword"] as string;
								company.Disabled = (bool)dataReader["Disabled"];
								company.IsUnicode = (bool)dataReader["IsUnicode"];
								company.Disabled = (bool)dataReader["Disabled"];
								company.DatabaseCulture = dataReader["DatabaseCulture"] as string;
								company.PreferredLanguage = dataReader["PreferredLanguage"] as string;
								company.ApplicationLanguage = dataReader["ApplicationLanguage"] as string;
								company.Provider = dataReader["Provider"] as string;
								companiesList.Add(company);
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

			return companiesList;
		}

		//---------------------------------------------------------------------
		public OperationResult Query(QueryInfo qi)
		{
			OperationResult opRes = new OperationResult();

			List<Company> companiesList = new List<Company>();

			string selectQuery = "SELECT * FROM MP_Companies WHERE ";

			try
			{
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;

						foreach (QueryField field in qi.Fields)
						{
							string paramName = string.Format("@{0}", field.Name);
							selectQuery += string.Format("{0} = {1} AND ", paramName, field.Name);

							command.Parameters.AddWithValue(paramName, field.Value);
						}

						selectQuery = selectQuery.Substring(0, selectQuery.Length - 5);
						command.CommandText = selectQuery;

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								Company company = new Company();
								company.CompanyId = (int)dataReader["CompanyId"];
								company.Name = dataReader["Name"] as string;
								company.Description = dataReader["Description"] as string;
								company.CompanyDBServer = dataReader["CompanyDBServer"] as string;
								company.CompanyDBName = dataReader["CompanyDBName"] as string;
								company.CompanyDBOwner = dataReader["CompanyDBOwner"] as string;
								company.CompanyDBPassword = dataReader["CompanyDBPassword"] as string;
								company.Disabled = (bool)dataReader["Disabled"];
								company.DatabaseCulture = dataReader["DatabaseCulture"] as string;
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
								companiesList.Add(company);
							}
						}
					}
					opRes.Result = true;
					opRes.Content = companiesList;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				opRes.Result = false;
				return opRes;
			}

			return opRes;
		}

		/// <summary>
		/// load all companies for a specific subscription
		/// </summary>
		/// <param name="subscriptionKey"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public List<Company> GetCompaniesBySubscription(string subscriptionKey)
		{
			List<Company> companiesList = new List<Company>();

			string selectQuery = "SELECT * FROM MP_Companies WHERE MP_Companies.SubscriptionKey = @SubscriptionKey";

			try
			{
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					using (SqlCommand command = new SqlCommand(selectQuery, connection))
					{
						command.Parameters.AddWithValue("@SubscriptionKey", subscriptionKey);

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								Company company = new Company();
								company.CompanyId = (int)dataReader["CompanyId"];
								company.Name = dataReader["Name"] as string;
								company.Description = dataReader["Description"] as string;
								company.SubscriptionKey = dataReader["SubscriptionKey"] as string;
								company.CompanyDBServer = dataReader["CompanyDBServer"] as string;
								company.CompanyDBName = dataReader["CompanyDBName"] as string;
								company.CompanyDBOwner = dataReader["CompanyDBOwner"] as string;
								company.CompanyDBPassword = dataReader["CompanyDBPassword"] as string;
								company.UseDMS = (bool)dataReader["UseDMS"];
								company.DMSDBServer = dataReader["DMSDBServer"] as string;
								company.DMSDBName = dataReader["DMSDBName"] as string;
								company.DMSDBOwner = dataReader["DMSDBOwner"] as string;
								company.DMSDBPassword = dataReader["DMSDBPassword"] as string;
								company.Disabled = (bool)dataReader["Disabled"];
								company.IsUnicode = (bool)dataReader["IsUnicode"];
								company.Disabled = (bool)dataReader["Disabled"];
								company.DatabaseCulture = dataReader["DatabaseCulture"] as string;
								company.PreferredLanguage = dataReader["PreferredLanguage"] as string;
								company.ApplicationLanguage = dataReader["ApplicationLanguage"] as string;
								company.Provider = dataReader["Provider"] as string;
								companiesList.Add(company);
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

			return companiesList;
		}
	}
}
