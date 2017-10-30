﻿using Microarea.AdminServer.Libraries.DatabaseManager;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using Microarea.Common.DiagnosticManager;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.AdminServer.Controllers.Helpers.APIQuery
{
	/// <summary>
	/// 
	/// </summary>
	//============================================================================
	public class APIDatabaseHelper
    {
		//---------------------------------------------------------------------
		public static OperationResult CheckDatabases(ExtendedSubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			// I use master database to load information
			bool isAzureDB = (subDatabase.AdminCredentials.Provider == "SQLAzure");

			string masterConnectionString =
				string.Format
				(
				isAzureDB ? NameSolverDatabaseStrings.SQLAzureConnection : NameSolverDatabaseStrings.SQLConnection,
				subDatabase.AdminCredentials.Server,
				DatabaseLayerConsts.MasterDatabase,
				subDatabase.AdminCredentials.Login,
				subDatabase.AdminCredentials.Password
				);

			// provo a connetermi al master con le credenziali di amministrazione
			DatabaseTask dTask = new DatabaseTask(isAzureDB) { CurrentStringConnection = masterConnectionString };

			// operazione forse superflua se il TestConnection viene fatto a monte dall'interfaccia angular
			opRes.Result = dTask.TryToConnect();
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

			if (!opRes.Result)
				return opRes;

			// controllo se i database esistono
			bool existERPDb = dTask.ExistDataBase(subDatabase.Database.DBName);
			bool existDMSDb = dTask.ExistDataBase(subDatabase.Database.DMSDBName);

			if (!existERPDb && !string.IsNullOrWhiteSpace(subDatabase.Database.DBName))
			{
				// devo controllare che il provider indicato sia compatibile con la versione del server specificato, 
				// altrimenti la creazione del db potrebbe non funzionare per errata sintassi
				if (isAzureDB)
				{
					// creazione contenitore db su Azure
					AzureCreateDBParameters param = new AzureCreateDBParameters();
					param.DatabaseName = subDatabase.Database.DBName;
					param.MaxSize = AzureMaxSize.GB1;

					// I create ERP database
					opRes.Result = dTask.CreateAzureDatabase(param);
				}
				else
				{
					// impostazione parametri creazione contenitore db su SqlServer
					SQLCreateDBParameters sqlParam = new SQLCreateDBParameters();
					sqlParam.DatabaseName = subDatabase.Database.DBName;

					// I create ERP database
					opRes.Result = dTask.CreateSQLDatabase(sqlParam);
				}

				opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

				if (!opRes.Result)
					return opRes;
			}

			if (!existDMSDb && !string.IsNullOrWhiteSpace(subDatabase.Database.DMSDBName))
			{
				// devo controllare che il provider indicato sia compatibile con la versione del server specificato, 
				// altrimenti la creazione del db potrebbe non funzionare per errata sintassi
				if (isAzureDB)
				{
					// creazione contenitore db su Azure
					AzureCreateDBParameters param = new AzureCreateDBParameters();
					param.DatabaseName = subDatabase.Database.DMSDBName;
					param.MaxSize = AzureMaxSize.GB1;

					// I create DMS database
					opRes.Result = dTask.CreateAzureDatabase(param);
				}
				else
				{
					// impostazione parametri creazione contenitore db su SqlServer
					SQLCreateDBParameters sqlParam = new SQLCreateDBParameters();
					sqlParam.DatabaseName = subDatabase.Database.DBName;

					// I create DMS database
					opRes.Result = dTask.CreateSQLDatabase(sqlParam);
				}

				opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

				if (!opRes.Result)
					return opRes;
			}

			DBInfo erpDBInfo = LoadDBMarkInfo(masterConnectionString, subDatabase.Database.DBName);
			DBInfo dmsDBInfo = LoadDBMarkInfo(masterConnectionString, subDatabase.Database.DMSDBName);

			if (erpDBInfo.HasError || dmsDBInfo.HasError)
			{
				opRes.Result = false;
				opRes.Message = erpDBInfo.Error + "\r\n" + dmsDBInfo.Error;
				return opRes;
			}

			if (erpDBInfo.ExistDBMark && dmsDBInfo.ExistDBMark)//questi bool sono rimasti vecchi , se non esistevano dopo creazione db rimangono a false, è corretto?
			{
				if (erpDBInfo.UseUnicode != dmsDBInfo.UseUnicode)
				{
					opRes.Result = false;
					opRes.Message = DatabaseManagerStrings.ErrorUnicodeValuesNotCompatible;
					return opRes;
				}

				if (string.Compare(erpDBInfo.Collation, dmsDBInfo.Collation, StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					opRes.Result = false;
					opRes.Message = string.Format(DatabaseManagerStrings.ErrorCollationNotCompatible, erpDBInfo.Name, erpDBInfo.Collation, dmsDBInfo.Name, dmsDBInfo.Collation);
					return opRes;
				}
				else
				{
					//@@TODO: verifica compatibilita' collation con ISOSTATO attivazione
					// se non va bene return false
				}
			}

			else
			{
				//@@TODO ALTER DATABASE ALTER COLLATE per ogni database empty con collate != Latin1_General_CI_AS
				// (valutare se basta il check esistenza TB_DBMark o entrare nel merito di tutte le tabelle)
				// in AC controllava tutte le tabelle 
			}

			subDatabase.Database.IsUnicode = erpDBInfo.UseUnicode;
			subDatabase.Database.DatabaseCulture = "1040"; //todo : lcid della collation
			opRes.Result = true;

			return opRes;
		}

		//---------------------------------------------------------------------
		public static OperationResult CheckLogin(ExtendedSubscriptionDatabase subDatabase, bool isDMS = false)
		{
			OperationResult opRes = new OperationResult();

			string serverName = isDMS ? subDatabase.Database.DMSDBServer : subDatabase.Database.DBServer;
			string dbName = isDMS ? subDatabase.Database.DMSDBName : subDatabase.Database.DBName;
			string dbowner = isDMS ? subDatabase.Database.DMSDBOwner : subDatabase.Database.DBOwner;
			string password = isDMS ? subDatabase.Database.DMSDBPassword : subDatabase.Database.DBPassword;

			if (string.IsNullOrWhiteSpace(serverName) || string.IsNullOrWhiteSpace(dbName) || string.IsNullOrWhiteSpace(dbowner))
			{
				opRes.Result = false;
				opRes.Message = Strings.EmptyCredentials;
				return opRes;
			}

			// I use master database to load information
			bool isAzureDB = (subDatabase.AdminCredentials.Provider == "SQLAzure");

			string masterConnectionString =
				string.Format
				(
				isAzureDB ? NameSolverDatabaseStrings.SQLAzureConnection : NameSolverDatabaseStrings.SQLConnection,
				subDatabase.AdminCredentials.Server,
				DatabaseLayerConsts.MasterDatabase,
				subDatabase.AdminCredentials.Login,
				subDatabase.AdminCredentials.Password
				);

			// provo a connetermi al master con le credenziali di amministrazione
			DatabaseTask dTask = new DatabaseTask(isAzureDB) { CurrentStringConnection = masterConnectionString };

			// check esistenza login
			bool existLogin = dTask.ExistLogin(dbowner);

			// se nel diagnostico c'e' un errore ritorno subito
			if (dTask.Diagnostic.Error)
			{
				opRes.Result = false;
				opRes.Message = dTask.Diagnostic.ToJson(true);
				return opRes;
			}

			// se la login non esiste la creo, poi creo l'utente sul database con ruolo dbo
			if (!existLogin)
			{
				opRes.Result = dTask.CreateLogin(dbowner, password);

				if (opRes.Result)
					opRes.Result = dTask.CreateUser(dbowner, dbName);

				if (!opRes.Result)
					opRes.Message = dTask.Diagnostic.ToJson(true);

				return opRes;
			}

			// la login esiste
			// quindi vado solo a creare utente + ruolo
			opRes.Result = dTask.CreateUser(dbowner, dbName);

			if (opRes.Result)
			{
				// provo a connettermi al database con le nuove credenziali
				dTask.CurrentStringConnection = string.Format(isAzureDB ? NameSolverDatabaseStrings.SQLAzureConnection : NameSolverDatabaseStrings.SQLConnection, serverName, dbName, dbowner, password);
				opRes.Result = dTask.TryToConnect();
				if (!opRes.Result)
					opRes.Message = dTask.Diagnostic.ToJson(true);
			}

			return opRes;
		}

		//---------------------------------------------------------------------
		public static DBInfo LoadDBMarkInfo(string connectionString, string dbName)
		{
			// devo cambiare il nome del database senza utilizzare la ChangeDatabase, non supportata da Azure
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = dbName };

			DBInfo info = new DBInfo { Name = dbName };
			if (string.IsNullOrWhiteSpace(dbName))
				return info;

			try
			{
				using (TBConnection myConnection = new TBConnection(builder.ConnectionString, DBMSType.SQLSERVER))
				{
					myConnection.Open();

					TBDatabaseSchema tbSchema = new TBDatabaseSchema(myConnection);
					// se esiste la tabella TB_DBMark procedo a leggere le informazioni sulla colonna Status
					if (tbSchema.ExistTable(DatabaseLayerConsts.TB_DBMark))
					{
						info.ExistDBMark = true;
						info.UseUnicode = TBCheckDatabase.IsUnicodeDataType(myConnection);
						info.Collation = TBCheckDatabase.GetColumnCollation(myConnection);
					}
					else
						info.ExistDBMark = false;
				}
			}
			catch (Exception e)
			{
				info.Error = e.Message;
			}

			return info;
		}

		//---------------------------------------------------------------------
		public static OperationResult SetSubscriptionDBUnderMaintenance(SubscriptionDatabase subDatabase, BurgerData burgerData, bool set = true)
		{
			OperationResult opRes = new OperationResult();

			try
			{
				subDatabase.UnderMaintenance = set;
				opRes = subDatabase.Save(burgerData);
				opRes.Message = Strings.OperationOK;
			}
			catch (Exception exc)
			{
				opRes.Result = false;
				opRes.Message = "DatabaseController.SetUnderMaintenance" + exc.Message;
			}

			return opRes;
		}

		//---------------------------------------------------------------------
		public static OperationResult PrecheckSubscriptionDB(ExtendedSubscriptionDatabase extSubDatabase)
		{
			// result globale dell'operazione, imposto un numero di codice per capire se ci sono operazioni da eseguire
			OperationResult opRes = new OperationResult();

			List<OperationResult> msgList = new List<OperationResult>();

			// check preventivi sui dati inseriti
			if (extSubDatabase.Database.DBServer != extSubDatabase.Database.DMSDBServer)
			{
				msgList.Add(new OperationResult() { Message = "Both databases must be in the same server!" });
			}

			if (extSubDatabase.Database.DBName == extSubDatabase.Database.DMSDBName)
			{
				msgList.Add(new OperationResult() { Message = "The databases names must be different!" });
			}

			if (extSubDatabase.Database.DBOwner == extSubDatabase.Database.DMSDBOwner)
				if (extSubDatabase.Database.DBPassword != extSubDatabase.Database.DMSDBPassword)
				{
					msgList.Add(new OperationResult() { Message = "Passwords different for same users!" });
				}

			if (msgList.Count > 0)
			{
				opRes.Code = -1;
				opRes.Content = msgList;
				return opRes;
			}
			//

			// I use master database to load all dbs
			bool isAzureDB = (extSubDatabase.AdminCredentials.Provider == "SQLAzure");

			string masterConnectionString =
				string.Format
				(
				isAzureDB ? NameSolverDatabaseStrings.SQLAzureConnection : NameSolverDatabaseStrings.SQLConnection,
				extSubDatabase.AdminCredentials.Server,
				DatabaseLayerConsts.MasterDatabase,
				extSubDatabase.AdminCredentials.Login,
				extSubDatabase.AdminCredentials.Password
				);

			DatabaseTask dTask = new DatabaseTask(isAzureDB) { CurrentStringConnection = masterConnectionString };

			// check databases existence
			bool existERPDb = dTask.ExistDataBase(extSubDatabase.Database.DBName);

			if (dTask.Diagnostic.Error)
			{
				msgList.Add(new OperationResult() { Message = dTask.Diagnostic.GetErrorsStrings() });
				opRes.Result = false;
				opRes.Content = msgList;
				opRes.Code = -1;
				// faccio return perche' si e' verificata un'eccezione sul server e, visto che il server del DMS e' uguale, non procedo ulteriormente
				return opRes;
			}

			if (!existERPDb)
				msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningDBNotExists, extSubDatabase.Database.DBName, extSubDatabase.Database.DBServer) });

			bool existDMSDb = dTask.ExistDataBase(extSubDatabase.Database.DMSDBName);
			if (dTask.Diagnostic.Error)
			{
				msgList.Add(new OperationResult() { Message = dTask.Diagnostic.GetErrorsStrings() });
				opRes.Result = false;
				opRes.Content = msgList;
				opRes.Code = -1;
				// faccio return perche' si e' verificata un'eccezione sul server e, visto che il server del DMS e' uguale, non procedo ulteriormente
				return opRes;
			}

			if (!existDMSDb)
				msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningDBNotExists, extSubDatabase.Database.DBName, extSubDatabase.Database.DBServer) });
			//

			// check informazioni database (Unicode - Collation)
			if (existERPDb && existDMSDb)
			{
				DBInfo erpDBInfo = LoadDBMarkInfo(masterConnectionString, extSubDatabase.Database.DBName);
				DBInfo dmsDBInfo = LoadDBMarkInfo(masterConnectionString, extSubDatabase.Database.DMSDBName);

				if (erpDBInfo.HasError || dmsDBInfo.HasError)
				{
					opRes.Result = false;
					msgList.Add(new OperationResult() { Message = erpDBInfo.Error + "\r\n" + dmsDBInfo.Error });
					opRes.Content = msgList;
					opRes.Code = -1;
					return opRes;
				}

				if (erpDBInfo.ExistDBMark && dmsDBInfo.ExistDBMark)
				{
					if (erpDBInfo.UseUnicode != dmsDBInfo.UseUnicode)
					{
						opRes.Result = false;
						msgList.Add(new OperationResult() { Message = DatabaseManagerStrings.ErrorUnicodeValuesNotCompatible });
						opRes.Content = msgList;
						opRes.Code = -1;
						return opRes;
					}

					if (string.Compare(erpDBInfo.Collation, dmsDBInfo.Collation, StringComparison.InvariantCultureIgnoreCase) != 0)
					{
						opRes.Result = false;
						msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.ErrorCollationNotCompatible, erpDBInfo.Name, erpDBInfo.Collation, dmsDBInfo.Name, dmsDBInfo.Collation) });
						opRes.Content = msgList;
						opRes.Code = -1;
						return opRes;
					}
					else
					{
						//@@TODO: verifica compatibilita' collation con ISOSTATO attivazione
						// se non va bene return false
					}
				}
			}
			//

			// check esistenza login
			bool existLogin = dTask.ExistLogin(extSubDatabase.Database.DBOwner);

			// se nel diagnostico c'e' un errore ritorno subito
			if (dTask.Diagnostic.Error)
			{
				opRes.Result = false;
				opRes.Message = dTask.Diagnostic.ToJson(true);
				opRes.Code = -1;
				return opRes;
			}

			if (!existLogin)
				msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningLoginNotExists, extSubDatabase.Database.DBOwner, extSubDatabase.Database.DBServer) });
			//

			// check validita' login e password per il db di ERP
			// provo a connettermi con la login specificata per il db di ERP al master
			dTask.CurrentStringConnection =
				string.Format
				(
				isAzureDB ? NameSolverDatabaseStrings.SQLAzureConnection : NameSolverDatabaseStrings.SQLConnection,
				extSubDatabase.Database.DBServer,
				DatabaseLayerConsts.MasterDatabase,
				extSubDatabase.Database.DBOwner,
				extSubDatabase.Database.DBPassword
				);

			bool result = dTask.TryToConnect(out int errorNr);
			if (dTask.Diagnostic.Error)
			{
				// se errorNr == 916 si tratta di mancanza di privilegi per la connessione 
				// ma la coppia utente/password e' corretta (altrimenti il nr di errore ritornato sarebbe 18456)
				if (errorNr == 916)
					opRes.Result = true;
				else
				{
					if (errorNr == 18456)
						msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.ErrorIncorrectPassword, extSubDatabase.Database.DBOwner) });

					opRes.Result = false;
					opRes.Message = dTask.Diagnostic.ToJson(true);
					opRes.Code = -1;
					return opRes;
				}
			}
			//

			// check validita' login e password per il db del DMS
			// provo a connettermi con la login specificata per il db del DMS al master
			dTask.CurrentStringConnection =
				string.Format
				(
				isAzureDB ? NameSolverDatabaseStrings.SQLAzureConnection : NameSolverDatabaseStrings.SQLConnection,
				extSubDatabase.Database.DMSDBServer,
				DatabaseLayerConsts.MasterDatabase,
				extSubDatabase.Database.DMSDBOwner,
				extSubDatabase.Database.DMSDBPassword
				);

			result = dTask.TryToConnect(out errorNr);
			if (dTask.Diagnostic.Error)
			{
				// se errorNr == 916 si tratta di mancanza di privilegi per la connessione 
				// ma la coppia utente/password e' corretta (altrimenti il nr di errore ritornato sarebbe 18456)
				if (errorNr == 916)
					opRes.Result = true;
				else
				{
					if (errorNr == 18456)
						msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.ErrorIncorrectPassword, extSubDatabase.Database.DMSDBOwner) });

					opRes.Result = false;
					opRes.Message = dTask.Diagnostic.ToJson(true);
					opRes.Code = -1;
					return opRes;
				}
			}
			//

			// check preventivo struttura dei database, se esistono
			if (existERPDb && existDMSDb)
			{
				DatabaseManager dbManager = CreateDatabaseManager();
				opRes.Result = dbManager.ConnectAndCheckDBStructure(extSubDatabase.Database);

				IDiagnosticItems items = dbManager.DBManagerDiagnostic.AllMessages();
				if (items != null)
				{
					foreach (IDiagnosticItem item in items)
						if (!string.IsNullOrWhiteSpace(item.FullExplain))
							msgList.Add(new OperationResult() { Message = item.FullExplain });
				}

				if (((dbManager.StatusDB == DatabaseStatus.UNRECOVERABLE || dbManager.StatusDB == DatabaseStatus.NOT_EMPTY) &&
					!dbManager.ContextInfo.HasSlaves)
					||
					(dbManager.StatusDB == DatabaseStatus.UNRECOVERABLE || dbManager.StatusDB == DatabaseStatus.NOT_EMPTY) &&
					(dbManager.DmsStructureInfo.DmsCheckDbStructInfo.DBStatus == DatabaseStatus.UNRECOVERABLE ||
					dbManager.DmsStructureInfo.DmsCheckDbStructInfo.DBStatus == DatabaseStatus.NOT_EMPTY))
				{
					opRes.Code = -1;
				}
			}
			//

			opRes.Content = msgList;
			return opRes;
		}

		/// <summary>
		/// Crea un oggetto di tipo DatabaseManager per richiamare i controlli sui database
		/// </summary>
		//---------------------------------------------------------------------
		public static DatabaseManager CreateDatabaseManager()
		{
			PathFinder pf = new PathFinder("USR-DELBENEMIC", "Development", "WebMago", "sa") { Edition = "Professional" };
			return new DatabaseManager(pf, new Diagnostic("DatabaseController"), (BrandLoader)InstallationData.BrandLoader, DBNetworkType.Large, "IT");
		}
	}
}
