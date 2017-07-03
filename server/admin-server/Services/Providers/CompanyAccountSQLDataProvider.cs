using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System.Collections.Generic;

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

		// la query potrebbe estrarre piu' righe, quindi dovrebbe ritornare una lista di companies per account
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
						command.Parameters.AddWithValue("@AccountName", account.AccountName);
						command.Parameters.AddWithValue("@CompanyId", account.CompanyId);
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								account.Admin = (bool)dataReader["Admin"];
								account.ExistsOnDB = true;
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

			return account;
		}

		//---------------------------------------------------------------------
		public OperationResult Save(IAdminModel iModel)
		{
			CompanyAccount account;
			OperationResult opRes = new OperationResult();

			try
			{
				account = (CompanyAccount)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					bool existUrl = false;

					using (SqlCommand command = new SqlCommand(Consts.ExistCompanyAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountName", account.AccountName);
						command.Parameters.AddWithValue("@CompanyId", account.CompanyId);
						existUrl = (int)command.ExecuteScalar() > 0;
					}

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = existUrl ? Consts.UpdateCompanyAccount : Consts.InsertCompanyAccount;

						command.Parameters.AddWithValue("@AccountName", account.AccountName);
						command.Parameters.AddWithValue("@CompanyId", account.CompanyId);
						command.Parameters.AddWithValue("@Admin", account.Admin);

						command.ExecuteNonQuery();
					}
				}

				opRes.Result = true;
			}
			catch (Exception e)
			{
				opRes.Result = false;
				opRes.Message = String.Concat("An error occurred while saving CompanyAccount: ", e.Message);
				return opRes;
			}

			return opRes;
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
						command.Parameters.AddWithValue("@AccountName", account.AccountName);
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
