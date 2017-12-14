using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microarea.TaskBuilderNet.Interfaces;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Core.Generic;

namespace NotificationService
{
	internal class NotificationSqlConnection
	{
		private SqlConnection SysDBConnection = new SqlConnection();
		internal bool SqlConnectionOpened { get { return ((SysDBConnection.State & ConnectionState.Open) == ConnectionState.Open); } }

		private string connectionString = InstallationData.ServerConnectionInfo.SysDBConnectionString;

		private bool OpenSysDBConnection()
		{
			if(SqlConnectionOpened)
				return true;

			if(String.IsNullOrWhiteSpace(connectionString))
			{
				NotificationCore.WriteMessageInEventLog("Connection String is null or WhiteSpace", System.Diagnostics.EventLogEntryType.Error, true);
				return false;
			}

			try
			{
				SysDBConnection.ConnectionString = connectionString;
				SysDBConnection.Open();
				//diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Warning, "E' stata aperta una connessione al sysdb");
				NotificationCore.WriteMessageInEventLog("It was opened a connection to SysDb", System.Diagnostics.EventLogEntryType.Information, true);
				return true;
			}
			catch(SqlException exception)
			{
				CloseSysDBConnection();
				NotificationCore.WriteMessageInEventLog(exception.Message + Environment.NewLine + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, true);
				Debug.WriteLine(string.Format("NotificationSqlConnection, OpenSysDBConnection - Error: {0} Number: {1}", exception.Message, exception.Number.ToString()));
				return false;
			}
		}

		private void CloseSysDBConnection()
		{
			try
			{
				if(SqlConnectionOpened)
				{
					SysDBConnection.Close();
					SysDBConnection.Dispose();
				}

				SysDBConnection.ConnectionString = string.Empty;
				NotificationCore.WriteMessageInEventLog("It was close a connection to SysDb", System.Diagnostics.EventLogEntryType.Information, true);
			}
			catch(SqlException exception)
			{
				NotificationCore.WriteMessageInEventLog(exception.Message + Environment.NewLine + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, true);
				Debug.WriteLine(string.Format("NotificationSqlConnection, CloseSysDBConnection - Error: {0} Number: {1}", exception.Message, exception.Number.ToString()));
			}
		}

		/// <summary>
		/// Inserisce la notifica sul database di sistema e restituisce l'identificatore dell'entità
		/// </summary>
		/// <param name="notify"></param>
		/// <returns></returns>
		internal int InsertGenericNotifications(GenericNotify notify)
		{
			if(!OpenSysDBConnection())
				throw new Exception("Impossible to open a connection to SysDb");
			//Prendo lo LoginId dell'utente
			string query = "INSERT INTO [dbo].[MSD_Notifications] ([FromCompanyId], [FromWorkerId], [ToCompanyId], [ToWorkerId], [Title], [Description], [NotificationType], [FromUserName]) " +
							" OUTPUT INSERTED.NotificationId " + 
							"VALUES (@FromCompanyId, @FromWorkerId,@ToCompanyId,@ToWorkerId,@Title,@Description,@NotificationType, @FromUserName)";

			SqlCommand aSqlCommand = new SqlCommand();

			try
			{
				aSqlCommand.CommandText = query;
				aSqlCommand.Connection = SysDBConnection;
				aSqlCommand.Parameters.AddWithValue("@FromCompanyId", Convert.ToInt16(notify.FromCompanyId));
				aSqlCommand.Parameters.AddWithValue("@FromWorkerId", Convert.ToInt16(notify.FromWorkerId));
				aSqlCommand.Parameters.AddWithValue("@ToCompanyId", Convert.ToInt16(notify.ToCompanyId));
				aSqlCommand.Parameters.AddWithValue("@ToWorkerId", Convert.ToInt16(notify.ToWorkerId));
				aSqlCommand.Parameters.AddWithValue("@Title", notify.Title.LeftWithSuspensionPoints(50));
				aSqlCommand.Parameters.AddWithValue("@Description", notify.Description);
				aSqlCommand.Parameters.AddWithValue("@NotificationType", notify.NotificationType);
				aSqlCommand.Parameters.AddWithValue("@FromUserName", notify.FromUserName.LeftWithSuspensionPoints(50));

				int newId = (int)(decimal)aSqlCommand.ExecuteScalar();

				return newId;
			}
			catch(Exception err)
			{
				NotificationCore.WriteMessageInEventLog(err.Message + Environment.NewLine + err.StackTrace, System.Diagnostics.EventLogEntryType.Error, true);
				throw new Exception("Error while inserting a new notify in the database");
			}
			finally
			{
				if(aSqlCommand != null)
					aSqlCommand.Dispose();
			}
		}

		internal bool SetNotificationAsRead(int NotificationId)
		{
			if(!OpenSysDBConnection())
				return false;
			//Prendo lo LoginId dell'utente
			string query = "UPDATE [dbo].[MSD_Notifications] SET [ReadDate] = (getdate()) WHERE NotificationId = @NotificationId";

			SqlCommand aSqlCommand = new SqlCommand();

			try
			{
				aSqlCommand.CommandText = query;
				aSqlCommand.Connection = SysDBConnection;
				aSqlCommand.Parameters.AddWithValue("@NotificationId", NotificationId);

				int rowAffected = aSqlCommand.ExecuteNonQuery();

				if(rowAffected != 1)
					return false;

				return true;
			}
			catch(Exception err)
			{
				NotificationCore.WriteMessageInEventLog(err.Message + Environment.NewLine + err.StackTrace, System.Diagnostics.EventLogEntryType.Error, true);
				return false;
			}
			finally
			{
				if(aSqlCommand != null)
					aSqlCommand.Dispose();
			}
		}

		internal GenericNotify[] GetAllNotifications(int ToCompanyId, int ToWorkerId, bool includeProcessed)
		{
			if(!OpenSysDBConnection())
				return null;
			

			//Prendo lo LoginId dell'utente
			string query =  " SELECT [Date], [FromCompanyId], [FromWorkerId], [ToCompanyId], [ToWorkerId], [Title], [Description], [ReadDate], [NotificationId], [FromUserName] " +
							" FROM [dbo].[MSD_Notifications]" +
							" WHERE ToCompanyId = @ToCompanyId and ToWorkerId = @ToWorkerId" + (includeProcessed? "" : " and ReadDate IS NULL");

			SqlCommand aSqlCommand = new SqlCommand();
			var retrievedNofications = new List<GenericNotify>();
			SqlDataReader reader = null;
			try
			{
				aSqlCommand.CommandText = query;
				aSqlCommand.Connection = SysDBConnection;
				aSqlCommand.Parameters.AddWithValue("@ToCompanyId", ToCompanyId);
				aSqlCommand.Parameters.AddWithValue("@ToWorkerId", ToWorkerId);

				reader = aSqlCommand.ExecuteReader();
					
				if(reader.HasRows)
				{
					while(reader.Read())
					{
						var FromCompanyId= (int)reader["FromCompanyId"];
						var FromWorkerId=(int)reader["FromWorkerId"];
						var ToCompanyId2=(int)reader["ToCompanyId"];
						var ToWorkerId2 = (int)reader["ToWorkerId"];		
						var Date = reader["Date"] == DBNull.Value ? DateTime.Now : (DateTime)reader["Date"];
						var Description= reader["Description"].ToString();
						var ReadDate= reader["ReadDate"]==DBNull.Value?DateTime.MinValue:(DateTime)reader["ReadDate"];
						var Title=reader["Title"].ToString();
						var NotificationId = (int)(decimal)reader["NotificationId"];
						var FromUserName=reader["FromUserName"].ToString();
						//var NotificationType= (NotificationType)reader["NotificationType"];
						
						retrievedNofications.Add(new GenericNotify 
						{
							Date = Date,
							Description = Description,
							FromCompanyId = FromCompanyId,
							FromWorkerId =FromWorkerId,
							FromUserName = FromUserName,
							NotificationType = NotificationType.Generic,
							ReadDate = ReadDate,
							Title = Title,
							ToCompanyId = ToCompanyId2,
							ToWorkerId = ToWorkerId2,
							NotificationId=NotificationId, 
							StoredOnDb = true
						});
					}
				}
				
			}
			catch(Exception err)
			{
				NotificationCore.WriteMessageInEventLog(err.Message + Environment.NewLine + err.StackTrace, System.Diagnostics.EventLogEntryType.Error, true);
			}
			finally
			{
				if(aSqlCommand != null)
					aSqlCommand.Dispose();

				if(reader != null && reader.IsClosed == false)
					reader.Close();
			}
			return retrievedNofications.Count==0? null: retrievedNofications.ToArray();
		}

		/// <summary>
		/// Inserisce la notifica sul database di sistema e restituisce l'identificatore dell'entità
		/// </summary>
		/// <param name="notify"></param>
		/// <returns></returns>
		internal string GetBrainBusinessServiceUrl()
		{
			//code
			string code = "BrainBusinessServiceUrl";

			string returnedUrl =string.Empty;

			if(!OpenSysDBConnection())
				throw new Exception("Impossible to open a connection to SysDb");
			//Prendo lo LoginId dell'utente
			string query = " SELECT [Adv] " +
							" FROM [dbo].[MSD_ADVSecurity]" +
							" WHERE Code = @Code ";

			SqlCommand aSqlCommand = new SqlCommand();

			try
			{
				aSqlCommand.CommandText = query;
				aSqlCommand.Connection = SysDBConnection;

				aSqlCommand.Parameters.AddWithValue("@Code", code);

				returnedUrl = (string)aSqlCommand.ExecuteScalar();
				
				if(returnedUrl == null)
					throw new Exception();
			}
			catch(Exception err)
			{
				NotificationCore.WriteMessageInEventLog("Problem in the recovery of the address of the service of Business Brain\n " + err.Message, EventLogEntryType.Information, true);
				throw new Exception("Error retrieving the address of the service business");
			}
			finally
			{
				if(aSqlCommand != null)
					aSqlCommand.Dispose();
			}

			return returnedUrl;
		}

		/// <summary>
		/// Inserisce la notifica sul database di sistema e restituisce l'identificatore dell'entità
		/// </summary>
		/// <param name="notify"></param>
		/// <returns></returns>
		internal bool UpdateOrInsertBrainBusinessServiceUrl(string BBServiceUrl)
		{
			//code
			string code = "BrainBusinessServiceUrl";

			bool error = false;

			if(!OpenSysDBConnection())
				throw new Exception("Impossible to open a connection to SysDb");
			//Prendo lo LoginId dell'utente

			string query = "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE; " +
							"BEGIN TRANSACTION; " +
							"UPDATE [dbo].[MSD_ADVSecurity] " +
							"SET Adv = @Adv " +
							"WHERE Code = @Code " +
							"IF @@ROWCOUNT = 0 BEGIN " +
							"INSERT INTO [dbo].[MSD_ADVSecurity] ([Code], [Adv]) VALUES " +
							"(@Code, @Adv) " +
							"END COMMIT TRANSACTION;";

			SqlCommand aSqlCommand = new SqlCommand();

			try
			{
				aSqlCommand.CommandText = query;
				aSqlCommand.Connection = SysDBConnection;

				aSqlCommand.Parameters.AddWithValue("@Code", code.Left(256));
				aSqlCommand.Parameters.AddWithValue("@Adv", BBServiceUrl.Left(2048));

				aSqlCommand.ExecuteNonQuery();
			}
			catch(Exception err)
			{
				NotificationCore.WriteMessageInEventLog("Error while inserting the url of the service of Business Brain\n" + err.Message + Environment.NewLine + err.StackTrace, System.Diagnostics.EventLogEntryType.Error, true);
				error = true;
			}
			finally
			{
				if(aSqlCommand != null)
					aSqlCommand.Dispose();
			}

			return !error;
		}
	}
}