﻿using Microarea.AdminServer.Library;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;

namespace Microarea.AdminServer.Services.Providers
{
	//================================================================================
	public class SubscriptionSQLDataProvider : ISubscriptionDataProvider
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
					using (SqlCommand command = new SqlCommand(Consts.SelectSubscription, connection))
					{
						command.Parameters.AddWithValue("@SubscriptionKey", subscription.SubscriptionKey);
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								subscription.ActivationToken = new ActivationToken (dataReader["ActivationToken"] as string);
								subscription.Description = dataReader["Description"] as string;
								subscription.PreferredLanguage = dataReader["PreferredLanguage"] as string;
								subscription.ApplicationLanguage = dataReader["ApplicationLanguage"] as string;
								subscription.MinDBSizeToWarn = (int)dataReader["MinDBSizeToWarn"];
								subscription.InstanceKey = dataReader["InstanceKey"] as string;
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
						command.Parameters.AddWithValue("@PreferredLanguage", subscription.PreferredLanguage);
						command.Parameters.AddWithValue("@ApplicationLanguage", subscription.ApplicationLanguage);
						command.Parameters.AddWithValue("@MinDBSizeToWarn", subscription.MinDBSizeToWarn);
						command.Parameters.AddWithValue("@InstanceKey", subscription.InstanceKey);
						command.Parameters.AddWithValue("@SubscriptionKey", subscription.SubscriptionKey);

						command.ExecuteNonQuery();
					}

					opRes.Result = true;
					opRes.Content = subscription;
				}
            }
            catch (Exception e)
            {
				opRes.Result = false;
				opRes.Message = String.Concat("An error occurred while saving Subscription: ", e.Message);
				return opRes;
            }

            return opRes;
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
		public List<Subscription> GetSubscriptions(string instanceKey)
		{
			List<Subscription> subsList = new List<Subscription>();

			string selectQuery = "SELECT * FROM MP_Subscriptions";
			if (!string.IsNullOrWhiteSpace(instanceKey))
				selectQuery += " WHERE InstanceKey = @InstanceKey";

			try
			{
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					using (SqlCommand command = new SqlCommand(selectQuery, connection))
					{
						if (!string.IsNullOrWhiteSpace(instanceKey))
							command.Parameters.AddWithValue("@InstanceKey", instanceKey);

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								Subscription subs = new Subscription();
								subs.SubscriptionKey = dataReader["SubscriptionKey"] as string;
								subs.InstanceKey = dataReader["InstanceKey"] as string;
								subs.Description = dataReader["Description"] as string;
								subs.PreferredLanguage = dataReader["PreferredLanguage"] as string;
								subs.ApplicationLanguage = dataReader["ApplicationLanguage"] as string;
								subs.MinDBSizeToWarn = (int)dataReader["MinDBSizeToWarn"];
								subsList.Add(subs);
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

			return subsList;
		}

		//---------------------------------------------------------------------
		public List<Subscription> GetSubscriptionsByAccount(string accountName, string instanceKey)
		{
			List<Subscription> subsList = new List<Subscription>();

			string selectQuery = @"SELECT * FROM MP_Subscriptions INNER JOIN MP_SubscriptionAccounts ON 
				MP_SubscriptionAccounts.SubscriptionKey = MP_Subscriptions.SubscriptionKey WHERE
				MP_SubscriptionAccounts.AccountName = @AccountName AND MP_Subscriptions.InstanceKey = @InstanceKey";

			try
			{
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					using (SqlCommand command = new SqlCommand(selectQuery, connection))
					{
						command.Parameters.AddWithValue("@AccountName", accountName);
						command.Parameters.AddWithValue("@InstanceKey", instanceKey);

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								Subscription subs = new Subscription();
								subs.SubscriptionKey = dataReader["SubscriptionKey"] as string;
								subs.InstanceKey = dataReader["InstanceKey"] as string;
								subs.Description = dataReader["Description"] as string;
								subs.PreferredLanguage = dataReader["PreferredLanguage"] as string;
								subs.ApplicationLanguage = dataReader["ApplicationLanguage"] as string;
								subs.MinDBSizeToWarn = (int)dataReader["MinDBSizeToWarn"];
								subsList.Add(subs);
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

			return subsList;
		}

		//---------------------------------------------------------------------
		public OperationResult Query(QueryInfo qi)
		{
			OperationResult opRes = new OperationResult();

			List<Subscription> subscriptionList = new List<Subscription>();

			string selectQuery = "SELECT * FROM MP_Subscriptions WHERE ";

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
								Subscription sub = new Subscription();
								sub.SubscriptionKey = dataReader["SubscriptionKey"] as string;
								sub.Description = dataReader["Description"] as string;
								sub.ActivationToken = new ActivationToken(dataReader["ActivationToken"] as string);
								sub.PreferredLanguage = dataReader["PreferredLanguage"] as string;
								sub.ApplicationLanguage = dataReader["ApplicationLanguage"] as string;
								sub.MinDBSizeToWarn = (int)dataReader["MinDBSizeToWarn"];
								sub.InstanceKey = dataReader["InstanceKey"] as string;
								subscriptionList.Add(sub);
							}
						}
					}
					opRes.Result = true;
					opRes.Content = subscriptionList;
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
