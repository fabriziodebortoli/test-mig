using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services.Providers
{
    //================================================================================
    public class SubscriptionSlotSQLDataProvider : IDataProvider
    {
        string connectionString;

        //---------------------------------------------------------------------
        public SubscriptionSlotSQLDataProvider(string connString)
        {
            this.connectionString = connString;
        }

        //---------------------------------------------------------------------
        public DateTime MinDateTimeValue { get { return (DateTime)SqlDateTime.MinValue; } }

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
                    using (SqlCommand command = new SqlCommand(Consts.SelectSubscription, connection))
                    {
                        command.Parameters.AddWithValue("@SubscriptionKey", subscription.SubscriptionKey);
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                subscription.ActivationToken = new Library.ActivationToken(dataReader["ActivationToken"] as string);
								subscription.ExistsOnDB = true;
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
        public OperationResult Save(IAdminModel iModel)
        {
            Subscription subscription;
			OperationResult opRes = new OperationResult();

            try
            {
                subscription = (Subscription)iModel;
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    bool existSubscription = false;

                    using (SqlCommand command = new SqlCommand(Consts.ExistSubscription, connection))
                    {
                        command.Parameters.AddWithValue("@SubscriptionKey", subscription.SubscriptionKey);
                        existSubscription = (int)command.ExecuteScalar() > 0;
                    }

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = existSubscription ? Consts.UpdateSubscription : Consts.InsertSubscription;

                        command.Parameters.AddWithValue("@Description", subscription.Description);
                        command.Parameters.AddWithValue("@ActivationToken", subscription.ActivationToken.ToString());
                        command.Parameters.AddWithValue("@InstanceKey", subscription.InstanceKey);

                        if (existSubscription)
                            command.Parameters.AddWithValue("@SubscriptionKey", subscription.SubscriptionKey);

                        command.ExecuteNonQuery();
                    }

					opRes.Result = true;
                }
            }
            catch (Exception e)
            {
				opRes.Result = false;
				opRes.Message = String.Concat("An error occurred while saving SubscriptionSlot: ", e.Message);
				return opRes;
            }

            return opRes;
        }

        //---------------------------------------------------------------------
        public bool Delete(IAdminModel iModel)
        {
            SubscriptionSlots subscription;

            try
            {
                subscription = (SubscriptionSlots)iModel;
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(Consts.DeleteSubscriptionSlot, connection))
                    {
                        command.Parameters.AddWithValue("@SubscriptionKey", subscription.SubscriptionKey);
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
