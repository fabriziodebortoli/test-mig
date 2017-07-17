using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;

namespace Microarea.AdminServer.Services.Providers
{
	//================================================================================
	public class SubscriptionDatabaseSQLDataProvider : ISubscriptionDatabaseDataProvider
    {
        string connectionString;

		//---------------------------------------------------------------------
		public SubscriptionDatabaseSQLDataProvider(string connString)
        {
            this.connectionString = connString;
        }

		//---------------------------------------------------------------------
		public DateTime MinDateTimeValue  { get { return (DateTime)SqlDateTime.MinValue; } }

		//---------------------------------------------------------------------
		public IAdminModel Load(IAdminModel iModel)
		{
			SubscriptionDatabase subDatabase;

			try
			{
				subDatabase = (SubscriptionDatabase)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Queries.SelectSubscriptionDatabase, connection))
					{
						command.Parameters.AddWithValue("@SubscriptionKey", subDatabase.SubscriptionKey);
						command.Parameters.AddWithValue("@Name", subDatabase.Name);

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								subDatabase.SubscriptionKey = dataReader["SubscriptionKey"] as string;
								subDatabase.Name = dataReader["Name"] as string;
								subDatabase.Description = dataReader["Description"] as string;
								subDatabase.DBServer = dataReader["DBServer"] as string;
								subDatabase.DBName = dataReader["DBName"] as string;
								subDatabase.DBOwner = dataReader["DBOwner"] as string;
								subDatabase.DBPassword = dataReader["DBPassword"] as string;
								subDatabase.DatabaseCulture = dataReader["DatabaseCulture"] as string;
								subDatabase.Disabled = (bool)dataReader["Disabled"];
								subDatabase.IsUnicode = (bool)dataReader["IsUnicode"];
								subDatabase.PreferredLanguage = dataReader["PreferredLanguage"] as string;
								subDatabase.ApplicationLanguage = dataReader["ApplicationLanguage"] as string;
								subDatabase.Provider = dataReader["Provider"] as string;
								subDatabase.UseDMS = (bool)dataReader["UseDMS"];
								subDatabase.DMSDBServer = dataReader["DMSDBServer"] as string;
								subDatabase.DMSDBName = dataReader["DMSDBName"] as string;
								subDatabase.DMSDBOwner = dataReader["DMSDBOwner"] as string;
								subDatabase.DMSDBPassword = dataReader["DMSDBPassword"] as string;
								subDatabase.Test = (bool)dataReader["Test"];
								subDatabase.ExistsOnDB = true;
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

			return subDatabase;
		}

		//---------------------------------------------------------------------
		public OperationResult Save(IAdminModel iModel)
        {
			SubscriptionDatabase subDatabase;
			OperationResult opRes = new OperationResult();

            try
            {
				subDatabase = (SubscriptionDatabase)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

					bool existsDb = false;

					using (SqlCommand command = new SqlCommand(Queries.ExistSubscriptionDatabase, connection))
					{
						command.Parameters.AddWithValue("@SubscriptionKey", subDatabase.SubscriptionKey);
						command.Parameters.AddWithValue("@Name", subDatabase.Name);
						existsDb = (int)command.ExecuteScalar() > 0;
					}

					using (SqlCommand command = new SqlCommand())
					{
						command.Connection = connection;
						command.CommandText = existsDb ? Queries.UpdateSubscriptionDatabase : Queries.InsertSubscriptionDatabase;

						command.Parameters.AddWithValue("@SubscriptionKey", subDatabase.SubscriptionKey);
						command.Parameters.AddWithValue("@Name", subDatabase.Name);

						command.Parameters.AddWithValue("@Description", subDatabase.Description);
						command.Parameters.AddWithValue("@DBServer", subDatabase.DBServer);
						command.Parameters.AddWithValue("@DBName", subDatabase.DBName);
						command.Parameters.AddWithValue("@DBOwner", subDatabase.DBOwner);
						command.Parameters.AddWithValue("@DBPassword", subDatabase.DBPassword);
						command.Parameters.AddWithValue("@DatabaseCulture", subDatabase.DatabaseCulture);
						command.Parameters.AddWithValue("@Disabled", subDatabase.Disabled);
						command.Parameters.AddWithValue("@IsUnicode", subDatabase.IsUnicode);
						command.Parameters.AddWithValue("@PreferredLanguage", subDatabase.PreferredLanguage);
						command.Parameters.AddWithValue("@ApplicationLanguage", subDatabase.ApplicationLanguage);
						command.Parameters.AddWithValue("@Provider", subDatabase.Provider);
						command.Parameters.AddWithValue("@UseDMS", subDatabase.UseDMS);
						command.Parameters.AddWithValue("@DMSDBServer", subDatabase.DMSDBServer);
						command.Parameters.AddWithValue("@DMSDBName", subDatabase.DMSDBName);
						command.Parameters.AddWithValue("@DMSDBOwner", subDatabase.DMSDBOwner);
						command.Parameters.AddWithValue("@DMSDBPassword", subDatabase.DMSDBPassword);
						command.Parameters.AddWithValue("@Test", subDatabase.Test);

						command.ExecuteNonQuery();
					}

					opRes.Result = true;
					opRes.Content = subDatabase;
				}
            }
            catch (Exception e)
            {
				opRes.Result = false;
				opRes.Message = String.Concat("An error occurred while saving SubscriptionDatabase: ", e.Message);
				return opRes;
			}

            return opRes;
        }

		//---------------------------------------------------------------------
		public bool Delete(IAdminModel iModel)
		{
			SubscriptionDatabase subDatabase;

			try
			{
				subDatabase = (SubscriptionDatabase)iModel;
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();
					using (SqlCommand command = new SqlCommand(Queries.DeleteSubscriptionDatabase, connection))
					{
						command.Parameters.AddWithValue("@SubscriptionKey", subDatabase.SubscriptionKey);
						command.Parameters.AddWithValue("@Name", subDatabase.Name);

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
		/// Ritorna una lista di databases dato una subscriptionKey e un name (opzionale)
		/// </summary>
		/// <param name="subscriptionKey"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public List<SubscriptionDatabase> GetDatabasesBySubscription(string subscriptionKey, string name)
		{
			if (string.IsNullOrWhiteSpace(subscriptionKey))
				return null;

			List<SubscriptionDatabase> databasesList = new List<SubscriptionDatabase>();

			string selectQuery = @"SELECT* FROM MP_SubscriptionDatabases WHERE SubscriptionKey =  @SubscriptionKey";

			if (!string.IsNullOrWhiteSpace(name))
				selectQuery += " AND Name = @Name";

			try
			{
				using (SqlConnection connection = new SqlConnection(this.connectionString))
				{
					connection.Open();

					using (SqlCommand command = new SqlCommand(selectQuery, connection))
					{
						command.Parameters.AddWithValue("@SubscriptionKey", subscriptionKey);

						if (!string.IsNullOrWhiteSpace(name))
							command.Parameters.AddWithValue("@Name", name);

						using (SqlDataReader dataReader = command.ExecuteReader())
						{
							while (dataReader.Read())
							{
								SubscriptionDatabase subDatabase = new SubscriptionDatabase();
								subDatabase.SubscriptionKey = dataReader["SubscriptionKey"] as string;
								subDatabase.Name = dataReader["Name"] as string;
								subDatabase.Description = dataReader["Description"] as string;
								subDatabase.DBServer = dataReader["DBServer"] as string;
								subDatabase.DBName = dataReader["DBName"] as string;
								subDatabase.DBOwner = dataReader["DBOwner"] as string;
								subDatabase.DBPassword = dataReader["DBPassword"] as string;
								subDatabase.UseDMS = (bool)dataReader["UseDMS"];
								subDatabase.DMSDBServer = dataReader["DMSDBServer"] as string;
								subDatabase.DMSDBName = dataReader["DMSDBName"] as string;
								subDatabase.DMSDBOwner = dataReader["DMSDBOwner"] as string;
								subDatabase.DMSDBPassword = dataReader["DMSDBPassword"] as string;
								subDatabase.Disabled = (bool)dataReader["Disabled"];
								subDatabase.IsUnicode = (bool)dataReader["IsUnicode"];
								subDatabase.Disabled = (bool)dataReader["Disabled"];
								subDatabase.DatabaseCulture = dataReader["DatabaseCulture"] as string;
								subDatabase.PreferredLanguage = dataReader["PreferredLanguage"] as string;
								subDatabase.ApplicationLanguage = dataReader["ApplicationLanguage"] as string;
								subDatabase.Provider = dataReader["Provider"] as string;
								subDatabase.Test = (bool)dataReader["Test"];
								databasesList.Add(subDatabase);
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

			return databasesList;
		}

		//---------------------------------------------------------------------
		public OperationResult Query(QueryInfo qi)
		{
			OperationResult opRes = new OperationResult();

			List<SubscriptionDatabase> databasesList = new List<SubscriptionDatabase>();

			string selectQuery = "SELECT * FROM MP_SubscriptionDatabases WHERE ";

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
								SubscriptionDatabase subDatabase = new SubscriptionDatabase();
								subDatabase.SubscriptionKey = dataReader["SubscriptionKey"] as string;
								subDatabase.Name = dataReader["Name"] as string;
								subDatabase.Description = dataReader["Description"] as string;
								subDatabase.DBServer = dataReader["DBServer"] as string;
								subDatabase.DBName = dataReader["DBName"] as string;
								subDatabase.DBOwner = dataReader["DBOwner"] as string;
								subDatabase.DBPassword = dataReader["DBPassword"] as string;
								subDatabase.Disabled = (bool)dataReader["Disabled"];
								subDatabase.DatabaseCulture = dataReader["DatabaseCulture"] as string;
								subDatabase.IsUnicode = (bool)dataReader["IsUnicode"];
								subDatabase.PreferredLanguage = dataReader["PreferredLanguage"] as string;
								subDatabase.ApplicationLanguage = dataReader["ApplicationLanguage"] as string;
								subDatabase.Provider = dataReader["Provider"] as string;
								subDatabase.UseDMS = (bool)dataReader["UseDMS"];
								subDatabase.DMSDBServer = dataReader["DMSDBServer"] as string;
								subDatabase.DMSDBName = dataReader["DMSDBName"] as string;
								subDatabase.DMSDBOwner = dataReader["DMSDBOwner"] as string;
								subDatabase.DMSDBPassword = dataReader["DMSDBPassword"] as string;
								subDatabase.Test = (bool)dataReader["Test"];
								databasesList.Add(subDatabase);
							}
						}
					}
					opRes.Result = true;
					opRes.Content = databasesList;
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
