﻿using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;

using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;

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
						command.Parameters.AddWithValue("@InstanceId", serverUrl.InstanceId);
						command.Parameters.AddWithValue("@URLType", serverUrl.URLType);
						
						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
								serverUrl.URL = dataReader["URL"] as string;
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
		public bool Save(IAdminModel iModel)
		{
			ServerURL serverUrl;

			try
			{
				serverUrl = (ServerURL)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					bool existUrl = false;

					using (SqlCommand command = new SqlCommand(Consts.ExistServerURL, connection))
					{
						command.Parameters.AddWithValue("@InstanceId", serverUrl.InstanceId);
						command.Parameters.AddWithValue("@URLType", serverUrl.URLType);
						existUrl = (int)command.ExecuteScalar() > 0;
					}

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = existUrl ? Consts.UpdateServerURL : Consts.InsertServerURL;

						command.Parameters.AddWithValue("@InstanceId", serverUrl.InstanceId);
						command.Parameters.AddWithValue("@URLType", serverUrl.URLType);
						command.Parameters.AddWithValue("@URL", serverUrl.URL);
						
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
			ServerURL serverUrl;

			try
			{
				serverUrl = (ServerURL)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Consts.DeleteServerURL, connection))
					{
						command.Parameters.AddWithValue("@InstanceId", serverUrl.InstanceId);
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
	}
}
