using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services.Providers
{
	//================================================================================
	public class InstanceAccountSQLDataProvider : IDataProvider
	{
		string connectionString;

		//---------------------------------------------------------------------
		public DateTime MinDateTimeValue { get { return (DateTime)SqlDateTime.MinValue; } }

		//---------------------------------------------------------------------
		public InstanceAccountSQLDataProvider(string connString)
		{
			this.connectionString = connString;
		}

		// carica le istanze di uno specifico AccountName
		// la query potrebbe estrarre piu' righe, quindi dovrebbe ritornare una lista di istanze per account
		//---------------------------------------------------------------------
		public IAdminModel Load(IAdminModel iModel)
		{
			InstanceAccount iAccount;

			try
			{
				iAccount = (InstanceAccount)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					using (SqlCommand command = new SqlCommand(Consts.SelectInstanceAccountByAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountName", iAccount.AccountName);
						
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								iAccount.InstanceKey = dataReader["InstanceKey"] as string;
								iAccount.ExistsOnDB = true;
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

			return iAccount;
		}

		// si occupa solo dell'insert, se il record esiste gia' torno false
		//---------------------------------------------------------------------
		public bool Save(IAdminModel iModel)
		{
			InstanceAccount iaccount;

			try
			{
				iaccount = (InstanceAccount)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					bool existInstance = false;

					using (SqlCommand command = new SqlCommand(Consts.ExistInstanceAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountName", iaccount.AccountName);
						command.Parameters.AddWithValue("@InstanceKey", iaccount.InstanceKey);
						existInstance = (int)command.ExecuteScalar() > 0;
					}

					if (existInstance)
						return false;

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = Consts.InsertInstanceAccount;

						command.Parameters.AddWithValue("@AccountName", iaccount.AccountName);
						command.Parameters.AddWithValue("@InstanceKey", iaccount.InstanceKey);

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
			InstanceAccount iaccount;

			try
			{
				iaccount = (InstanceAccount)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.DeleteInstanceAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountName", iaccount.AccountName);
						command.Parameters.AddWithValue("@InstanceKey", iaccount.InstanceKey);
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
