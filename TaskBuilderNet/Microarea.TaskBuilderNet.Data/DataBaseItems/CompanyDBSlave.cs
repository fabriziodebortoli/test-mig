using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseItems
{
	/// <summary>
	/// CompanyDBSlave
	/// Classe che gestisce i record della tabella MSD_CompanyDBSlaves
	/// </summary>
	//================================================================================
	public class CompanyDBSlave : DataBaseItem
	{
		//---------------------------------------------------------------------
		public CompanyDBSlave()
		{ }

		/// <summary>
		/// CompanyDBSlave
		/// </summary>
		//---------------------------------------------------------------------
		public CompanyDBSlave(string connectionString)
		{
			ConnectionString = connectionString;
		}

		#region Add - Inserimento di un nuovo slave agganciato ad un'azienda
		/// <summary>
		/// Add
		/// Inserisce una nuova azienda nella tabella MSD_CompanyDBSlaves
		/// </summary>
		//---------------------------------------------------------------------
		public bool Add
			(
			string companyId, 
			string signature,
			string server,
			string database,
			string slaveDBOwner
			)
		{
			bool result = false;
			
			SqlCommand myCommand = new SqlCommand();
			myCommand.Connection = CurrentSqlConnection;
			
			try
			{
				string strQuery =
				@"INSERT INTO MSD_CompanyDBSlaves
				(CompanyId, Signature, ServerName, DatabaseName, SlaveDBOwner)
				VALUES 
				(@CompanyId, @Signature, @ServerName, @DatabaseName, @SlaveDBOwner)";

				myCommand.CommandText = strQuery;
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(companyId));
				myCommand.Parameters.AddWithValue("@Signature", signature);
				myCommand.Parameters.AddWithValue("@ServerName", server);
				myCommand.Parameters.AddWithValue("@DatabaseName", database);
				myCommand.Parameters.AddWithValue("@SlaveDBOwner", Int32.Parse(slaveDBOwner));
				myCommand.ExecuteNonQuery();
				result = true;
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDBSlave.Add");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.SlaveAdd, signature), extendedInfo);
			}
			return result;
		}
		#endregion

		#region Modify - Modifica uno slave agganciato ad un'azienda
		/// <summary>
		/// Modify
		/// Modifica i dati di uno slave agganciato ad un'azienda
		/// </summary>
		//---------------------------------------------------------------------
		public bool Modify
			(
			string slaveId,
			string companyId,
			string server,
			string database,
			string slaveDBOwner
			)
		{
			bool result = true;

			SqlCommand myCommand = new SqlCommand();
			myCommand.Connection = CurrentSqlConnection;

			try
			{
				string strQuery = @"UPDATE MSD_CompanyDBSlaves
									SET ServerName = @ServerName, DatabaseName = @DatabaseName, SlaveDBOwner = @SlaveDBOwner
									WHERE SlaveId = @SlaveId AND CompanyId = @CompanyId";

				myCommand.CommandText = strQuery;
				myCommand.Parameters.AddWithValue("@SlaveId", Int32.Parse(slaveId));
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(companyId));
				myCommand.Parameters.AddWithValue("@ServerName", server);
				myCommand.Parameters.AddWithValue("@DatabaseName", database);
				myCommand.Parameters.AddWithValue("@SlaveDBOwner", Int32.Parse(slaveDBOwner));
				myCommand.ExecuteNonQuery();
				result = true;
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDBSlave.Modify");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, string.Format(DatabaseItemsStrings.CompanyDBSlaveModify, slaveId), extendedInfo);
			}
			return result;
		}
		#endregion

		#region Delete - Cancellazione di uno slave e relative logins
		/// <summary>
		/// Delete
		/// Cancella dalla tabella MSD_CompanyDBSlaves lo slave specificato
		/// Inoltre elimina anche tutte le logins associate a quello Slave (nella tabella MSD_SlaveLogins)
		/// Viene utilizzata la storedProcedure MSD_DeleteSlave
		/// </summary>
		/// <param name="slaveId">identificativo dello slave da cancellare</param>
		/// <param name="companyId">identificativo dell'azienda da cancellare</param>
		//---------------------------------------------------------------------
		public bool Delete(string slaveId, string companyId)
		{
			bool result = false;

			try
			{
				SqlCommand myCommand = new SqlCommand();
				myCommand.Connection = CurrentSqlConnection;
				myCommand.CommandText = "MSD_DeleteSlave";
				myCommand.CommandType = CommandType.StoredProcedure;

				myCommand.Parameters.AddWithValue("@par_slaveid", Int32.Parse(slaveId));
				myCommand.Parameters.AddWithValue("@par_companyid", Int32.Parse(companyId));

				myCommand.ExecuteNonQuery();
				result = true;
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDBSlave.Delete");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyDBSlaveDelete, extendedInfo);
			}

			return result;
		}
		#endregion

		///<summary>
		/// SelectSlaveForCompanyIdAndSignature
		/// Ritorna il record dello slave dato il CompanyId e la signature dell'applicazione
		///</summary>
		//---------------------------------------------------------------------
		public bool SelectSlaveForCompanyIdAndSignature(string companyId, string signature, out CompanyDBSlaveItem slave)
		{
			bool result = false;
			slave = null;

			if (string.IsNullOrWhiteSpace(companyId) || string.IsNullOrWhiteSpace(signature))
				return result;

			SqlDataReader myDataReader = null;

			try
			{
				string selectQuery = @"SELECT * FROM MSD_CompanyDBSlaves 
										WHERE CompanyId = @CompanyId AND Signature=@Signature";

				SqlCommand myCommand = new SqlCommand(selectQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(companyId));
				myCommand.Parameters.AddWithValue("@Signature", signature);
				myDataReader = myCommand.ExecuteReader();

				while (myDataReader.Read())
					slave = ReadSlave(myDataReader);

				result = true;
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDBSlave.SelectSlaveForCompanyIdAndSignature");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyDBSlaveRead, extendedInfo);
				result = false;
			}
			finally
			{
				if (myDataReader != null && !myDataReader.IsClosed)
				{
					myDataReader.Close();
					myDataReader.Dispose();
				}
			}

			return result;
		}

		///<summary>
		/// SelectSlaveForCompanyId
		/// Ritorna il record dello slave dato il CompanyId
		///</summary>
		//---------------------------------------------------------------------
		public bool SelectSlaveForCompanyId(string companyId, out CompanyDBSlaveItem slave)
		{
			bool result = false;
			slave = null;

			if (string.IsNullOrWhiteSpace(companyId))
				return result;

			SqlDataReader myDataReader = null;

			try
			{
				string selectQuery = "SELECT * FROM MSD_CompanyDBSlaves WHERE CompanyId = @CompanyId";

				SqlCommand myCommand = new SqlCommand(selectQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@CompanyId", Int32.Parse(companyId));
				myDataReader = myCommand.ExecuteReader();

				while (myDataReader.Read())
					slave = ReadSlave(myDataReader);

				result = true;
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDBSlave.SelectSlaveForCompanyId");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyDBSlaveRead, extendedInfo);
				result = false;
			}
			finally
			{
				if (myDataReader != null && !myDataReader.IsClosed)
				{
					myDataReader.Close();
					myDataReader.Dispose();
				}
			}

			return result;
		}

		/// <summary>
		/// SelectSlavesOnSameServer
		/// Ritorna una lista di slave che hanno il database sul medesimo server
		/// </summary>
		//---------------------------------------------------------------------
		public bool SelectSlavesOnSameServer(out List<CompanyDBSlaveItem> slavesOnSameServer, string slaveServer)
		{
			slavesOnSameServer = new List<CompanyDBSlaveItem>();
			
			bool result = false;

			SqlDataReader myDataReader = null;

			string selectQuery = "SELECT * FROM MSD_CompanyDBSlaves WHERE MSD_CompanyDBSlaves.ServerName = @serverName";

			try
			{
				SqlCommand myCommand = new SqlCommand(selectQuery, CurrentSqlConnection);
				myCommand.Parameters.AddWithValue("@serverName", slaveServer);
				myDataReader = myCommand.ExecuteReader();

				if (myDataReader == null || !myDataReader.HasRows)
					return result;

				while (myDataReader.Read())
					slavesOnSameServer.Add(ReadSlave(myDataReader));

				result = true;
			}
			catch (SqlException e)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, e.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, e.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, e.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, e.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, e.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDBSlave.SelectSlavesOnSameServer");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyDBSlaveRead, extendedInfo);
				return result;
			}
			finally
			{
				if (myDataReader != null && !myDataReader.IsClosed)
				{
					myDataReader.Close();
					myDataReader.Dispose();
				}
			}

			return result;
		}

		//---------------------------------------------------------------------
		private CompanyDBSlaveItem ReadSlave(SqlDataReader myDataReader)
		{
			CompanyDBSlaveItem slaveItem = new CompanyDBSlaveItem();
	
			if (myDataReader == null) 
				return slaveItem;

			try
			{
				slaveItem.SlaveId = myDataReader["SlaveId"].ToString();
				slaveItem.CompanyId = myDataReader["CompanyId"].ToString();
				slaveItem.Signature = myDataReader["Signature"].ToString();
				slaveItem.ServerName = myDataReader["ServerName"].ToString();
				slaveItem.DatabaseName = myDataReader["DatabaseName"].ToString();
				slaveItem.SlaveDBOwner = myDataReader["SlaveDBOwner"].ToString();
			}
			catch (SqlException sqlException)
			{
				ExtendedInfo extendedInfo = new ExtendedInfo();
				extendedInfo.Add(DatabaseLayerStrings.Description, sqlException.Message);
				extendedInfo.Add(DatabaseLayerStrings.Procedure, sqlException.Procedure);
				extendedInfo.Add(DatabaseLayerStrings.Server, sqlException.Server);
				extendedInfo.Add(DatabaseLayerStrings.Number, sqlException.Number);
				extendedInfo.Add(DatabaseLayerStrings.LineNumber, sqlException.LineNumber);
				extendedInfo.Add(DatabaseLayerStrings.Function, "CompanyDBSlave.ReadSlave");
				extendedInfo.Add(DatabaseLayerStrings.DefinedInto, "TaskBuilderNet.Data.DatabaseItems");
				Diagnostic.Set(DiagnosticType.Error, DatabaseItemsStrings.CompanyDBSlaveRead, extendedInfo);
			}

			return slaveItem;
		}
	}

	/// <summary>
	/// CompanyDBSlaveItem
	/// Classe che gestisce le informazioni di un database secondario [slave]
	/// </summary>
	//=========================================================================
	public class CompanyDBSlaveItem
	{
		private string slaveId = string.Empty;
		private string companyId = string.Empty;
		private string signature = string.Empty;
		private string serverName = string.Empty;
		private string databaseName = string.Empty;
		private string slaveDBOwner = string.Empty;

		//---------------------------------------------------------------------
		public string SlaveId { get { return slaveId; } set { slaveId = value; } }
		public string CompanyId { get { return companyId; }	set { companyId = value; } }
		public string Signature { get { return signature; } set { signature = value; } }
		public string ServerName { get { return serverName; } set { serverName = value; } }
		public string DatabaseName { get { return databaseName; } set { databaseName = value; } }
		public string SlaveDBOwner { get { return slaveDBOwner; } set { slaveDBOwner = value; } }

		//---------------------------------------------------------------------
		public CompanyDBSlaveItem() { }
	}
}
