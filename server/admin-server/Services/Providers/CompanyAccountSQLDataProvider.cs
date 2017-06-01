using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services.Providers
{
	//================================================================================
	public class CompanyAccountSQLDataProvider : IDataProvider
	{
		string connectionString;

		//---------------------------------------------------------------------
		public DateTime MinDateTimeValue { get { return (DateTime)SqlDateTime.MinValue; } }

		//---------------------------------------------------------------------
		public CompanyAccountSQLDataProvider(string connString)
		{
			this.connectionString = connString;
		}

		//---------------------------------------------------------------------
		public IAdminModel Load(IAdminModel iModel)
		{
			CompanyAccount account;

			try
			{
				account = (CompanyAccount)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.SelectCompanyAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountId", account.AccountId);
						command.Parameters.AddWithValue("@CompanyId", account.CompanyId);
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
								account.Admin = (bool)dataReader["Admin"];
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}

			return account;
		}

		//---------------------------------------------------------------------
		public bool Save(IAdminModel iModel)
		{
			CompanyAccount account;

			try
			{
				account = (CompanyAccount)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					bool existUrl = false;

					using (SqlCommand command = new SqlCommand(Consts.ExistCompanyAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountId", account.AccountId);
						command.Parameters.AddWithValue("@CompanyId", account.CompanyId);
						existUrl = (int)command.ExecuteScalar() > 0;
					}

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = existUrl ? Consts.UpdateCompanyAccount : Consts.InsertCompanyAccount;

						command.Parameters.AddWithValue("@AccountId", account.AccountId);
						command.Parameters.AddWithValue("@CompanyId", account.CompanyId);
						command.Parameters.AddWithValue("@Admin", account.Admin);

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
			CompanyAccount account;

			try
			{
				account = (CompanyAccount)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.DeleteCompanyAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountId", account.AccountId);
						command.Parameters.AddWithValue("@CompanyId", account.CompanyId);
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
