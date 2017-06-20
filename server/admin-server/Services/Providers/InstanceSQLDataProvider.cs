using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services.Providers
{
	//================================================================================
	public class InstanceSQLDataProvider : IDataProvider
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
						command.Parameters.AddWithValue("@InstanceKey", instance.InstanceKey);
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								instance.Description = dataReader["Description"] as string;
								instance.Customer = dataReader["Customer"] as string;
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
		public bool Save(IAdminModel iModel)
        {
			Instance instance;

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
						command.Parameters.AddWithValue("@Customer", instance.Customer);
						command.Parameters.AddWithValue("@Disabled", instance.Disabled);

						if (existInstance)
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
