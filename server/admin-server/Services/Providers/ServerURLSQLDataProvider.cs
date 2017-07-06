using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using System.Collections.Generic;

namespace Microarea.AdminServer.Services.Providers
{
	//================================================================================
	public class ServerURLSQLDataProvider : IDataProvider
	{
		string connectionString;

		//---------------------------------------------------------------------
		public DateTime MinDateTimeValue { get { return (DateTime)SqlDateTime.MinValue; } }

		//---------------------------------------------------------------------
		public ServerURLSQLDataProvider(string connString)
		{
			this.connectionString = connString;
		}

		//---------------------------------------------------------------------
		public IAdminModel Load(IAdminModel iModel)
		{
			ServerURL serverUrl;

			try
			{
				serverUrl = (ServerURL)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.SelectServerURL, connection))
					{
						command.Parameters.AddWithValue("@InstanceKey", serverUrl.InstanceKey);
						command.Parameters.AddWithValue("@URLType", serverUrl.URLType);
						
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								serverUrl.URL = dataReader["URL"] as string;
								serverUrl.ExistsOnDB = true;
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

			return serverUrl;
		}

		//---------------------------------------------------------------------
		public OperationResult Save(IAdminModel iModel)
		{
			ServerURL serverUrl;
			OperationResult opRes = new OperationResult();

			try
			{
				serverUrl = (ServerURL)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					bool existUrl = false;

					using (SqlCommand command = new SqlCommand(Consts.ExistServerURL, connection))
					{
						command.Parameters.AddWithValue("@InstanceKey", serverUrl.InstanceKey);
						command.Parameters.AddWithValue("@URLType", serverUrl.URLType);
						existUrl = (int)command.ExecuteScalar() > 0;
					}

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = existUrl ? Consts.UpdateServerURL : Consts.InsertServerURL;

						command.Parameters.AddWithValue("@InstanceKey", serverUrl.InstanceKey);
						command.Parameters.AddWithValue("@URLType", serverUrl.URLType);
						command.Parameters.AddWithValue("@URL", serverUrl.URL);
						
						command.ExecuteNonQuery();
					}

					opRes.Result = true;
				}
			}
			catch (Exception e)
			{
				opRes.Result = false;
				opRes.Message = String.Concat("An error occurred while saving URLs: ", e.Message);
				return opRes;
			}

			return opRes;
		}

		//---------------------------------------------------------------------
		public bool Delete(IAdminModel iModel)
		{
			ServerURL serverUrl;

			try
			{
				serverUrl = (ServerURL)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.DeleteServerURL, connection))
					{
						command.Parameters.AddWithValue("@InstanceKey", serverUrl.InstanceKey);
						command.Parameters.AddWithValue("@URLType", serverUrl.URLType);
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

			List<ServerURL> urlList = new List<ServerURL>();

			string selectQuery = "SELECT * FROM MP_ServerURLs WHERE ";

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
								ServerURL url = new ServerURL();
								url.InstanceKey = dataReader["InstanceKey"] as string;
								url.URLType = (URLType)dataReader["URLType"];
								url.URL = dataReader["URL"] as string;
								urlList.Add(url);
							}
						}
					}
					opRes.Result = true;
					opRes.Content = urlList;
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
