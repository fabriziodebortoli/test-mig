using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System.Collections.Generic;

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
			ISubscriptionAccount iSubscription;

			try
			{
				iSubscription = (ISubscriptionAccount)iModel;
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
		public OperationResult Save(IAdminModel iModel)
		{
            ISubscriptionAccount isubscripion;
			OperationResult opRes = new OperationResult();

			try
			{
				isubscripion = (ISubscriptionAccount)iModel;
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
					{
						opRes.Result = false;
						opRes.Message = "SubscriptionAccount already exists";
						return opRes;
					}

					opRes.Result = true;
				}
			}
			catch (Exception e)
			{
				opRes.Result = false;
				opRes.Message = String.Concat("An error occurred while saving InstanceAccount: ", e.Message);
				return opRes;
			}

			return opRes;
		}

		//---------------------------------------------------------------------
		public bool Delete(IAdminModel iModel)
		{
            ISubscriptionAccount isubscription;

			try
			{
				isubscription = (ISubscriptionAccount)iModel;
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

		//---------------------------------------------------------------------
		public OperationResult Query(QueryInfo qi)
		{
			OperationResult opRes = new OperationResult();

			List<ISubscriptionAccount> subscriptionsList = new List<ISubscriptionAccount>();

			string selectQuery = "SELECT * FROM MP_SubscriptionAccounts WHERE ";

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
                                ISubscriptionAccount companyAccount = new SubscriptionAccount();
								companyAccount.AccountName = dataReader["AccountName"] as string;
								companyAccount.SubscriptionKey = dataReader["SubscriptionKey"] as string;
								subscriptionsList.Add(companyAccount);
							}
						}
					}
					opRes.Result = true;
					opRes.Content = subscriptionsList;
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
