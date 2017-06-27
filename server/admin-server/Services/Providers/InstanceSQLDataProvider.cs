using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System.Collections.Generic;

namespace Microarea.AdminServer.Services.Providers
{
	//================================================================================
	public class InstanceSQLDataProvider : IDataProvider, IInstanceDataProvider
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
			Instance instance;
            
			try
			{
				instance = (Instance)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.SelectInstance, connection))
					{
					
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
                                instance.InstanceKey = dataReader["InstanceKey"] as string;
                                instance.Description = dataReader["Description"] as string;
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
		public List<ServerURL> LoadURLs(string instanceKey)
		{
			List<ServerURL> serverURLs = new List<ServerURL>();

			try
			{
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.SelectURlsInstance, connection))
					{
						command.Parameters.AddWithValue("@InstanceKey", instanceKey);
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							ServerURL serverUrl = new ServerURL();

							while (dataReader.Read())
							{
								serverUrl.InstanceKey = dataReader["InstanceKey"] as string;
								serverUrl.URLType = (URLType)dataReader["URLType"];
								serverUrl.URL = dataReader["URL"] as string;

								serverURLs.Add(serverUrl);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return new List<ServerURL>();
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

					using (SqlCommand command = new SqlCommand(Consts.ExistInstance, connection))
					{
						command.Parameters.AddWithValue("@InstanceKey", instance.InstanceKey);
						existInstance = (int)command.ExecuteScalar() > 0;
					}

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = existInstance ? Consts.UpdateInstance : Consts.InsertInstance;
						command.Parameters.AddWithValue("@Description", instance.Description);
						command.Parameters.AddWithValue("@Disabled", instance.Disabled);

						if (existInstance)
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
				opRes.Message = String.Concat("An error occurred while saving Instance ", e.Message);
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
					using (SqlCommand command = new SqlCommand(Consts.DeleteInstance, connection))
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
	}
}
