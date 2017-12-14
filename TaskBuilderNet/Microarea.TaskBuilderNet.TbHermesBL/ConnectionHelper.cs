using System;
using System.Data;
using System.Diagnostics;  // trace
using System.Data.EntityClient;
using System.Data.SqlClient;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
	// TODO copincolla identico a omologa in PostaLite, sposta in core dove è già definita TbSenderDatabaseInfo,
	//      e TbSenderDatabaseInfo rinomina in modo neutro! cambia solo databaseModelName, da parametrizzare
	static class ConnectionHelper
	{
		const string databaseModelName = "Mailbox";// "PostaLiteIntegrationModel"; // TODO leggi da property interna, specializzata in classe derivata
		//-------------------------------------------------------------------------------------
		private static string CreateEntityConnectionString(TbSenderDatabaseInfo infos) //CreateConnectionString
		{
			return CreateEntityConnectionString(infos, databaseModelName);
		}
		private static string CreateEntityConnectionString(TbSenderDatabaseInfo infos, string databaseModelName)
		{
			string sqlConnectionString = CreateConnectionString(infos);
            return CreateEntityConnectionString(databaseModelName, sqlConnectionString);
		}

		public static string CreateConnectionString(TbSenderDatabaseInfo infos)
		{
			// Initialize the connection string builder for the
			// underlying provider.
			SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();

			// Set the properties for the data source.
			sqlBuilder.DataSource = infos.ServerName;
			sqlBuilder.InitialCatalog = infos.DatabaseName;
			sqlBuilder.IntegratedSecurity = infos.WinAuthentication;
			sqlBuilder.UserID = infos.Username;
			sqlBuilder.Password = infos.Password;

			// Build the SqlConnection connection string.
			string sqlConnectionString = sqlBuilder.ToString();
			return sqlConnectionString;
		}

        private static string CreateEntityConnectionString(string databaseModelName, string sqlConnectionString)
        {
 			//connectionString="metadata=res://*/PostaLiteIntegrationModel.csdl|res://*/PostaLiteIntegrationModel.ssdl|res://*/PostaLiteIntegrationModel.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=(local);initial catalog=Az37_prova;integrated security=True;multipleactiveresultsets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient" />

			// Specify the provider name, server and database.
			string providerName = "System.Data.SqlClient";

           // Initialize the EntityConnectionStringBuilder.
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

            //Set the provider name.
            entityBuilder.Provider = providerName;

            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = sqlConnectionString;

            // Set the Metadata location.
            entityBuilder.Metadata = string.Format(@"res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl", databaseModelName);

            return entityBuilder.ToString();
        }

		//-------------------------------------------------------------------------------------
		internal static MZP_CompanyEntities GetHermesDBEntities(TbSenderDatabaseInfo dbInfo)
		{
			return GetEntitiesDB(dbInfo);
		}

        internal static MZP_CompanyEntities GetEntitiesDB(TbSenderDatabaseInfo dbInfo) // sposta in tb.core
		//	where T : System.Data.Objects.ObjectContext//, new()
		{
			if (dbInfo == null)
				return null;
			string connectionString = CreateEntityConnectionString(dbInfo);
			//return new T(connectionString); // runtime error
			// workaround più semplice:
            return (MZP_CompanyEntities)Activator.CreateInstance(typeof(MZP_CompanyEntities), new object[] { connectionString });
			// altri workaround:
			// http://stackoverflow.com/questions/840261/passing-arguments-to-c-sharp-generic-new-of-templated-type
		}

		//-------------------------------------------------------------------------------------
		internal static MZPcmpDMSEntities GetDmsEntities(DmsDatabaseInfo dmsDB)
		{
			string dbModelName = "EasyAttachment";
            string entConn = CreateEntityConnectionString(dbModelName, dmsDB.DMSConnectionString);
			return (MZPcmpDMSEntities)Activator.CreateInstance(typeof(MZPcmpDMSEntities), new object[] { entConn });
		}

		static public int GetNextKey(TbSenderDatabaseInfo dbInfo, int reserveItems, ILockerClient lockerClient)//string tableName, string idName) // TODO rivedere parametri, lo identifichiamo tramite 'Entity'
		{
			string entity = "Entity.OFM.Mail.Messages.MessageId";
			string lockKey = entity;
			string tableName = "TB_AutoincrementEntities";

			//PathFinder pathFinder = BasePathFinder.BasePathFinderInstance.LockManagerUrl;
			string companyDBName = dbInfo.DatabaseName;
			//string authToken = lockUser.AuthenticationToken;
			//string userName = lockUser.UserName;
			//string tableName = "TB_AutoincrementEntities";
			//string lockAddr = "TbHermes";

			//LockManager lockManager = null;

			try
			{
				//lockManager = new LockManager(BasePathFinder.BasePathFinderInstance.LockManagerUrl);
				//if (false == lockManager.LockRecord(companyDBName, authToken, userName, tableName, lockKey, lockAddr, lockAddr))
				if (false == lockerClient.LockRecord(companyDBName, tableName, lockKey))
				{
                    // e' un problema oppure e' la prima volta, registro e provo a continuare
                    Trace.WriteLine("ERR: unable to lock table TB_AutoincrementEntities, entity=" + entity, "Hermes");
				}

				string connectionString = CreateConnectionString(dbInfo);
				int nextKey = GetNextKey(connectionString, reserveItems, entity);

				return nextKey;
			}
			finally
			{
				lockerClient.UnlockRecord(companyDBName, tableName, lockKey);
				//lockManager.UnlockRecord(companyDBName, authToken, tableName, lockKey, lockAddr);
			}

			/*
			// string tableName, string idName) // TODO rivedere parametri, lo identifichiamo tramite 'Entity'
			// tableName: "OM_OfficeMailMessages", idName: "OfficeMailMessageID"); // TODO remove magic strings
			
			// Metodo temporaneo.
			// A regime andrà usato un servizio condiviso per la generazione e prenotazione degli ID
			string connectionString = CreateConnectionString(dbInfo);
			string commandText = string.Format(CultureInfo.InvariantCulture, "SELECT MAX([{0}]) FROM [{1}];", idName, tableName);
			using (IDbConnection connection = new SqlConnection(connectionString))
			using (IDbCommand command = new SqlCommand(commandText, (SqlConnection)connection))
			{
				connection.Open();
				object obj = command.ExecuteScalar();
				int nextKey = -1;
				if (false == object.ReferenceEquals(obj, DBNull.Value))
					nextKey = (int)obj;
				return ++nextKey;
			}
			*/
		}
        static public int GetMailAddressBookNextKey(TbSenderDatabaseInfo dbInfo, int reserveItems, ILockerClient lockerClient)//string tableName, string idName) // TODO rivedere parametri, lo identifichiamo tramite 'Entity'
        {
            string entity = "Entity.OFM.Mail.MailAddressBook.MailAddressBookId";
            string lockKey = entity;
            string tableName = "TB_AutoincrementEntities";

            //PathFinder pathFinder = BasePathFinder.BasePathFinderInstance.LockManagerUrl;
            string companyDBName = dbInfo.DatabaseName;
            //string authToken = lockUser.AuthenticationToken;
            //string userName = lockUser.UserName;
            //string tableName = "TB_AutoincrementEntities";
            //string lockAddr = "TbHermes";

            //LockManager lockManager = null;

            try
            {
                //lockManager = new LockManager(BasePathFinder.BasePathFinderInstance.LockManagerUrl);
                //if (false == lockManager.LockRecord(companyDBName, authToken, userName, tableName, lockKey, lockAddr, lockAddr))
                if (false == lockerClient.LockRecord(companyDBName, tableName, lockKey))
                {
                    // e' un problema oppure e' la prima volta, registro e provo a continuare
                    Trace.WriteLine("ERR: unable to lock table TB_AutoincrementEntities, entity=" + entity, "Hermes");
                }

                string connectionString = CreateConnectionString(dbInfo);
                int nextKey = GetNextKey(connectionString, reserveItems, entity);

                return nextKey;
            }
            finally
            {
                lockerClient.UnlockRecord(companyDBName, tableName, lockKey);
                //lockManager.UnlockRecord(companyDBName, authToken, tableName, lockKey, lockAddr);
            }

            /*
            // string tableName, string idName) // TODO rivedere parametri, lo identifichiamo tramite 'Entity'
            // tableName: "OM_OfficeMailMessages", idName: "OfficeMailMessageID"); // TODO remove magic strings
			
            // Metodo temporaneo.
            // A regime andrà usato un servizio condiviso per la generazione e prenotazione degli ID
            string connectionString = CreateConnectionString(dbInfo);
            string commandText = string.Format(CultureInfo.InvariantCulture, "SELECT MAX([{0}]) FROM [{1}];", idName, tableName);
            using (IDbConnection connection = new SqlConnection(connectionString))
            using (IDbCommand command = new SqlCommand(commandText, (SqlConnection)connection))
            {
                connection.Open();
                object obj = command.ExecuteScalar();
                int nextKey = -1;
                if (false == object.ReferenceEquals(obj, DBNull.Value))
                    nextKey = (int)obj;
                return ++nextKey;
            }
            */
        }

        static public int GetCommitmentsNextKey(TbSenderDatabaseInfo dbInfo, int reserveItems, ILockerClient lockerClient)
        {
            string entity = "Entity.OFM.Office.Commitments.CommitmentId";
            string lockKey = entity;
            string tableName = "TB_AutoincrementEntities";

            string companyDBName = dbInfo.DatabaseName;

            try
            {
                if (false == lockerClient.LockRecord(companyDBName, tableName, lockKey))
                {
                    // e' un problema oppure e' la prima volta, registro e provo a continuare
                    Trace.WriteLine("ERR: unable to lock table TB_AutoincrementEntities, entity=" + entity, "Hermes");
                }

                string connectionString = CreateConnectionString(dbInfo);
                int nextKey = GetNextKey(connectionString, reserveItems, entity);

                return nextKey;
            }
            finally
            {
                lockerClient.UnlockRecord(companyDBName, tableName, lockKey);
            }
         }

		static private int GetNextKey(string connectionString, int reserveItems, string entity)
		{
			int lastNr = 0;
            bool bFound = false;
			//string entity = "OFM.MailBox.Messages.MessageId";

			using (TBConnection connection = new TBConnection(connectionString, DBMSType.SQLSERVER)) // TODO - permettere switch (come sapere qual è?)
			{
				string selQuery = "SELECT LastNumber FROM TB_AutoincrementEntities WHERE Entity = @entity";
				using (TBCommand command = new TBCommand(selQuery, connection))
				{
					command.Parameters.Add("@entity", entity);
					connection.Open();
					using (IDataReader dataReader = command.ExecuteReader())
					{
						if (dataReader.Read())
						{
                            bFound = true;
							lastNr = (int)dataReader["LastNumber"];
						}
					}
				}

				string insQuery = "INSERT INTO TB_AutoincrementEntities (LastNumber, Entity) VALUES (@lastNumber, @entity);";
				string updQuery = "UPDATE TB_AutoincrementEntities SET LastNumber = @lastNumber WHERE Entity = @entity";

				//string wrtQuery = (lastNr > 0) ? updQuery : insQuery;
                string wrtQuery = (bFound) ? updQuery : insQuery;

				int nextLastNr = lastNr + reserveItems;

				using (TBCommand command = new TBCommand(wrtQuery, connection))
				{
					command.Parameters.Add("@entity", entity);
					command.Parameters.Add("@lastNumber", nextLastNr);
					//connection.Open();
					command.ExecuteNonQuery();
				}
			}

			return lastNr + 1;
		}
	}
}
