using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System.Collections.Generic;

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

		//---------------------------------------------------------------------
		public OperationResult Query(QueryInfo qi)
		{
			OperationResult opRes = new OperationResult();

			List<SubscriptionSlots> slotsList = new List<SubscriptionSlots>();

			string selectQuery = "SELECT * FROM MP_SubscriptionSlots WHERE ";

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
								SubscriptionSlots slot = new SubscriptionSlots();
								slot.SubscriptionKey = dataReader["SubscriptionKey"] as string;
								slot.Value = dataReader["SlotsXml"] as string;
								slotsList.Add(slot);
							}
						}
					}
					opRes.Result = true;
					opRes.Content = slotsList;
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
	}
}
