using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services.Providers
{
	//================================================================================
	public class SubscriptionAccountSQLDataProvider : IDataProvider
	{
		string connectionString;

		//---------------------------------------------------------------------
		public DateTime MinDateTimeValue { get { return (DateTime)SqlDateTime.MinValue; } }

		//---------------------------------------------------------------------
		public SubscriptionAccountSQLDataProvider(string connString)
		{
			this.connectionString = connString;
		}

		// carica le subscription di uno specifico AccountName
		//---------------------------------------------------------------------
		public void Load(IAdminModel iModel)
		{
			SubscriptionAccount iSubscription;
			bool found = false;

			try
			{
				iSubscription = (SubscriptionAccount)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.SelectSubscriptionAccountBySubscriptionId, connection))
					{
						command.Parameters.AddWithValue("@AccountName", iSubscription.AccountName);
						
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								iSubscription.SubscriptionId = (int)dataReader["SubscriptionId"];
								found = true;
							}
								
						}
					}
				}
			}
			catch (Exception e)
			{
				iSubscription = new SubscriptionAccount();
				return;
			}

			if (!found)
			{
				iSubscription = new SubscriptionAccount();
				return;
			}
		}

		// si occupa solo dell'insert, se il record esiste gia' torno false
		//---------------------------------------------------------------------
		public bool Save(IAdminModel iModel)
		{
			SubscriptionAccount isubscripion;

			try
			{
				isubscripion = (SubscriptionAccount)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					bool existSubscription = false;

					using (SqlCommand command = new SqlCommand(Consts.ExistSubscriptionAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountName", isubscripion.AccountName);
						command.Parameters.AddWithValue("@SubscriptionId", isubscripion.SubscriptionId);
						existSubscription = (int)command.ExecuteScalar() > 0;
					}

					if (existSubscription)
						return false;

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = Consts.InsertInstanceAccount;

						command.Parameters.AddWithValue("@AccountName", isubscripion.AccountName);
						command.Parameters.AddWithValue("@SubscriptionId", isubscripion.SubscriptionId);

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
			SubscriptionAccount isubscription;

			try
			{
				isubscription = (SubscriptionAccount)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.DeleteSubscriptionAccount, connection))
					{
						command.Parameters.AddWithValue("@AccountName", isubscription.AccountName);
						command.Parameters.AddWithValue("@SubscriptionId", isubscription.SubscriptionId);
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
