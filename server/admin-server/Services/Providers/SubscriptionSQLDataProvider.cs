using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services.Providers
{
	//================================================================================
	public class SubscriptionSQLDataProvider : IDataProvider
    {
        string connectionString;

		//---------------------------------------------------------------------
		public SubscriptionSQLDataProvider(string connString)
        {
            this.connectionString = connString;
        }

		//---------------------------------------------------------------------
		public DateTime MinDateTimeValue  { get { return (DateTime)SqlDateTime.MinValue; } }

		//---------------------------------------------------------------------
		public IAdminModel Load(IAdminModel iModel)
		{
			Subscription subscription;

			try
			{
				subscription = (Subscription)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.SelectSubscriptionByName, connection))
					{
						command.Parameters.AddWithValue("@Name", subscription.Name);
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								subscription.ActivationKey = dataReader["ActivationKey"] as string;
								subscription.PurchaseId = dataReader["PurchaseId"] as string;
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

			return subscription;
		}

		//---------------------------------------------------------------------
		public bool Save(IAdminModel iModel)
        {
			Subscription subscription;

            try
            {
				subscription = (Subscription)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

					bool existSubscription = false;

					using (SqlCommand command = new SqlCommand(Consts.ExistSubscription, connection))
					{
						command.Parameters.AddWithValue("@SubscriptionId", subscription.SubscriptionId);
						existSubscription = (int)command.ExecuteScalar() > 0;
					}

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = existSubscription ? Consts.UpdateSubscription : Consts.InsertSubscription;
						
						command.Parameters.AddWithValue("@Name", subscription.Name);
						command.Parameters.AddWithValue("@ActivationKey", subscription.ActivationKey);
						command.Parameters.AddWithValue("@PurchaseId", subscription.PurchaseId);
						command.Parameters.AddWithValue("@InstanceId", subscription.InstanceId);

						if (existSubscription)
							command.Parameters.AddWithValue("@SubscriptionId", subscription.SubscriptionId);

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
			Subscription subscription;

			try
			{
				subscription = (Subscription)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.DeleteSubscription, connection))
					{
						command.Parameters.AddWithValue("@SubscriptionId", subscription.SubscriptionId);
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
