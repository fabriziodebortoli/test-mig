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
		// la query potrebbe estrarre piu' righe, quindi dovrebbe ritornare una lista di account per subscription
		//---------------------------------------------------------------------
		public IAdminModel Load(IAdminModel iModel)
		{
			SubscriptionAccount iSubscription;

			try
			{
				iSubscription = (SubscriptionAccount)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.SelectSubscriptionAccountBySubscriptionKey, connection))
					{
						command.Parameters.AddWithValue("@AccountName", iSubscription.AccountName);
						
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								iSubscription.SubscriptionKey = dataReader["SubscriptionKey"] as string;
								iSubscription.ExistsOnDB = true;
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

			return iSubscription;
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
						command.Parameters.AddWithValue("@SubscriptionKey", isubscripion.SubscriptionKey);
						existSubscription = (int)command.ExecuteScalar() > 0;
					}

					if (existSubscription)
						return false;

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = Consts.InsertInstanceAccount;

						command.Parameters.AddWithValue("@AccountName", isubscripion.AccountName);
						command.Parameters.AddWithValue("@SubscriptionKey", isubscripion.SubscriptionKey);

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
						command.Parameters.AddWithValue("@SubscriptionKey", isubscription.SubscriptionKey);
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
