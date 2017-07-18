using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System.Collections.Generic;

namespace Microarea.AdminServer.Services.Providers
{
	//================================================================================
	public class InstanceSQLDataProvider : IInstanceDataProvider
    {
        string connectionString;

		//---------------------------------------------------------------------
		public InstanceSQLDataProvider(string connString)
        {
            this.connectionString = connString;
        }

		//---------------------------------------------------------------------
		public DateTime MinDateTimeValue  { get { return (DateTime)SqlDateTime.MinValue; } }

		//---------------------------------------------------------------------
		public IAdminModel Load(IAdminModel iModel)
		{
			IInstance instance;
            
			try
			{
				instance = (IInstance)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Queries.SelectInstance, connection))
					{
                        command.Parameters.AddWithValue("@InstanceKey", instance.InstanceKey);

                        using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
                                instance.Description = dataReader["Description"] as string;
                                instance.Description = dataReader["Origin"] as string;
                                instance.Description = dataReader["Tags"] as string;
                                instance.UnderMaintenance = (bool)dataReader["UnderMaintenance"];
                                instance.Disabled = (bool)dataReader["Disabled"];
								instance.ExistsOnDB = true;
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

			return instance;
		}

		//---------------------------------------------------------------------
		public List<IServerURL> LoadURLs(string instanceKey)
		{
			List<IServerURL> serverURLs = new List<IServerURL>();
            try
			{
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					using (SqlCommand command = new SqlCommand(Queries.SelectURlsInstance, connection))
					{
						command.Parameters.AddWithValue("@InstanceKey", instanceKey);

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
                                IServerURL serverUrl = new ServerURL();
								serverUrl.InstanceKey = dataReader["InstanceKey"] as string;
								serverUrl.URLType = (URLType)dataReader["URLType"];
                                serverUrl.URL = dataReader["URL"] as string;
                                serverUrl.ExistsOnDB = true;
                                serverURLs.Add(serverUrl);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return new List<IServerURL>();
			}

			return serverURLs;
		}

		//---------------------------------------------------------------------
		public OperationResult Save(IAdminModel iModel)
        {
			Instance instance;
			OperationResult opRes = new OperationResult();
            try
            {
				instance = (Instance)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

					bool existInstance = false;

					using (SqlCommand command = new SqlCommand(Queries.ExistInstance, connection))
					{
						command.Parameters.AddWithValue("@InstanceKey", instance.InstanceKey);
						existInstance = (int)command.ExecuteScalar() > 0;
					}

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = existInstance ? Queries.UpdateInstance : Queries.InsertInstance;
						command.Parameters.AddWithValue("@Description", instance.Description);
						command.Parameters.AddWithValue("@Disabled", instance.Disabled);
                        command.Parameters.AddWithValue("@Origin", instance.Origin);
                        command.Parameters.AddWithValue("@Tags", instance.Tags);
                        command.Parameters.AddWithValue("@UnderMaintenance", instance.UnderMaintenance);
                        command.Parameters.AddWithValue("@InstanceKey", instance.InstanceKey);
                     
						command.ExecuteNonQuery();
					}

					opRes.Result = true;
					opRes.Content = instance;
                }
            }
            catch (Exception e)
            {
                opRes.Result = false;
                opRes.Message = String.Concat("An error occurred while saving Instance; ", e.Message);
                return opRes;
            }
            return opRes;
        }

		//---------------------------------------------------------------------
		public bool Delete(IAdminModel iModel)
		{
			Instance instance;
			try
			{
				instance = (Instance)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Queries.DeleteInstance, connection))
					{
						command.Parameters.AddWithValue("@InstanceKey", instance.InstanceKey);
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

		/// <summary>
		/// Ritorna una lista con tutte le istanze di una specifica subscription
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public List<IInstance> GetInstancesBySubscription(string subscriptionKey)
		{
			List<IInstance> instanceList = new List<IInstance>();

			string selectQuery = @"SELECT * FROM dbo.MP_Instances INNER JOIN
								MP_SubscriptionInstances ON MP_Instances.InstanceKey = MP_SubscriptionInstances.InstanceKey
								WHERE MP_SubscriptionInstances.SubscriptionKey = @SubscriptionKey";

			try
			{
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(selectQuery, connection))
					{
						command.Parameters.AddWithValue("@SubscriptionKey", subscriptionKey);

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								Instance instance = new Instance();
								instance.InstanceKey = dataReader["InstanceKey"] as string;
								instance.Origin = dataReader["Origin"] as string;
								instance.Tags = dataReader["Tags"] as string;
								instance.Description = dataReader["Description"] as string;
								instance.Disabled = (bool)dataReader["Disabled"];
								instance.UnderMaintenance = (bool)dataReader["UnderMaintenance"];
								instanceList.Add(instance);
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

			return instanceList;
		}

		/// <summary>
		/// Ritorna una lista con tutte le istanze di uno specifico account
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public List<IInstance> GetInstancesByAccount(string accountName)
		{
			List<IInstance> instanceList = new List<IInstance>();

			string selectQuery = @"SELECT DISTINCT MP_Instances.* FROM MP_Instances 
								INNER JOIN MP_SubscriptionInstances ON MP_Instances.InstanceKey = MP_SubscriptionInstances.InstanceKey 
								INNER JOIN MP_SubscriptionAccounts ON MP_SubscriptionAccounts.SubscriptionKey = MP_SubscriptionInstances.SubscriptionKey
								WHERE MP_SubscriptionAccounts.AccountName = @AccountName";

			try
			{
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(selectQuery, connection))
					{
						command.Parameters.AddWithValue("@AccountName", accountName);

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								Instance instance = new Instance();
								instance.InstanceKey = dataReader["InstanceKey"] as string;
								instance.Origin = dataReader["Origin"] as string;
								instance.Tags = dataReader["Tags"] as string;
								instance.Description = dataReader["Description"] as string;
								instance.Disabled = (bool)dataReader["Disabled"];
								instance.UnderMaintenance = (bool)dataReader["UnderMaintenance"];
								instanceList.Add(instance);
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

			return instanceList;
		}

		/// <summary>
		/// Ritorna una lista con tutte le istanze
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public List<IInstance> GetInstances()
        {
            List<IInstance> instanceList = new List<IInstance>();

            string selectQuery = "SELECT * FROM MP_Instances";

            try
            {
                using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(selectQuery, connection))
                    {
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                IInstance instance = new Instance();
                                instance.InstanceKey = dataReader["InstanceKey"] as string;
                                instance.Origin = dataReader["Origin"] as string;
                                instance.Tags = dataReader["Tags"] as string;
                                instance.Description = dataReader["Description"] as string;
                                instance.Disabled = (bool)dataReader["Disabled"];
                                instance.UnderMaintenance = (bool)dataReader["UnderMaintenance"];
                                instanceList.Add(instance);
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

            return instanceList;
        }

        //---------------------------------------------------------------------
        public OperationResult Query(QueryInfo qi)
		{
			OperationResult opRes = new OperationResult();
			List<IInstance> list = new List<IInstance>();
			string selectQuery = "SELECT * FROM MP_Instances WHERE ";
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
								IInstance instance = new Instance();
                                instance.Origin = dataReader["Origin"] as string; 
                                instance.Tags = dataReader["Tags"] as string;
                                instance.Description= dataReader["Description"] as string;
                                instance.Disabled = (bool)dataReader["Disabled"] ;
                                instance.UnderMaintenance=(bool)dataReader["UnderMaintenance"];
                                instance.InstanceKey = dataReader["InstanceKey"] as string;
                                list.Add(instance);
							}
						}
					}
					opRes.Result = true;
					opRes.Content = list;
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
