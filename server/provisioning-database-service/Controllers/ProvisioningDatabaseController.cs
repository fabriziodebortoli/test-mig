using Microarea.Common.DiagnosticManager;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using Microarea.ProvisioningDatabase.Controllers.Helpers;
using Microarea.ProvisioningDatabase.Infrastructure;
using Microarea.ProvisioningDatabase.Infrastructure.Model;
using Microarea.ProvisioningDatabase.Libraries;
using Microarea.ProvisioningDatabase.Libraries.DatabaseManager;
using Microarea.ProvisioningDatabase.Libraries.DataManagerEngine;
using Microarea.ProvisioningDatabase.Properties;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.ProvisioningDatabase.Controllers
{
	[Route("provisioning-database-service")]
	
	/// <summary>
	/// Controller with APIs against database  
	/// </summary>
	//============================================================================
	public class ProvisioningDatabaseController : Controller
	{
		private IJsonHelper jsonHelper;

		//---------------------------------------------------------------------
		public ProvisioningDatabaseController(/*IOptions<AppOptions> settings, */IJsonHelper jsonHelper)
		{
			this.jsonHelper = jsonHelper;
			//this.settings = settings.Value;
		}

		[HttpGet]
		[Route("api")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiHome()
		{
			jsonHelper.AddJsonCouple<string>("message", "Welcome to Microarea Provisioning Database Service API");
			return new ContentResult { Content = jsonHelper.WriteFromKeysAndClear(), ContentType = "application/json" };
		}

		[HttpGet]
		[Route("api/status")]
		//-----------------------------------------------------------------------------	
		public IActionResult ApiStatus()
		{
			OperationResult opRes = new OperationResult();
			opRes.Result = true;
			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		// @@TODO: procedura di creazione automatica su Cloud da rivedere!!!
		// mi deve essere passata la stringa di connessione dal provisioning

		/// <summary>
		/// Creazione automatica:
		/// - contenitore db
		/// - login SQL + associazione login al database
		/// - struttura tabelle ERP
		/// - insert riga in MP_SubscriptionDatabases
		/// </summary>
		/// <param name="instanceKey"></param>
		/// <param name="subscriptionKey"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		/*		[HttpPost("/api/database/quickcreate/{instanceKey}/{subscriptionKey}")]
				public IActionResult ApiQuickCreate(string instanceKey, string subscriptionKey)
				{
					OperationResult opRes = new OperationResult();

					//************************************************************************************
					// aggiungere controlli di esistenza database - login - connessioni al server, etc.
					//************************************************************************************

					// creo il record nella tabella con il flag UnderMaintenance a true

					SubscriptionDatabase subDatabase = new SubscriptionDatabase();
					subDatabase.InstanceKey = instanceKey;
					subDatabase.SubscriptionKey = subscriptionKey;
					subDatabase.Name = instanceKey + "_" + subscriptionKey + "_Master";
					subDatabase.UnderMaintenance = true;

					try
					{
						opRes = subDatabase.Save(burgerData);
						opRes.Message = Strings.OperationOK;
					}
					catch (Exception exc)
					{
						opRes.Result = false;
						opRes.Message = "010 DatabaseController.QuickCreate" + exc.Message;
						return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					if (!opRes.Result)
					{
						opRes.Message = Strings.OperationKO;
						return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					// imposto i nomi dei database
					string dbName = instanceKey + "_" + subscriptionKey + "_Master_DB";
					string dmsDbName = dbName + "_DMS";

					// impostazione parametri creazione contenitore db su Azure
					// queste impostazioni dovranno essere definite a livello di subscription
					AzureCreateDBParameters param = new AzureCreateDBParameters();
					param.DatabaseName = dbName;
					param.MaxSize = AzureMaxSize.GB1;

					// impostazione parametri creazione contenitore db su SqlServer
					SQLCreateDBParameters sqlParam = new SQLCreateDBParameters();
					sqlParam.DatabaseName = dbName;

					// to create database I need to connect to master

					DatabaseTask dTask = new DatabaseTask(true);
					dTask.CurrentStringConnection =
						string.Format
						(
						NameSolverDatabaseStrings.SQLConnection,
						settings.DatabaseInfo.DBServer,
						DatabaseLayerConsts.MasterDatabase,
						settings.DatabaseInfo.DBUser,
						settings.DatabaseInfo.DBPassword
						);

					// I create ERP database
					opRes.Result = //dTask.CreateSQLDatabase(sqlParam); 
						dTask.CreateAzureDatabase(param);
					opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

					if (!opRes.Result)
					{
						jsonHelper.AddPlainObject<OperationResult>(opRes);
						return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					// I create DMS database

					param.DatabaseName = dmsDbName;
					sqlParam.DatabaseName = dmsDbName;

					opRes.Result = //dTask.CreateSQLDatabase(sqlParam); 
					 dTask.CreateAzureDatabase(param);
					opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

					if (!opRes.Result)
					{
						jsonHelper.AddPlainObject<OperationResult>(opRes);
						return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					// creo la login dbowner

					string loginName = instanceKey + "_" + subscriptionKey + "_Admin";
					string password = SecurityManager.GetRandomPassword();

					opRes.Result = dTask.CreateLogin(loginName, password);
					opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

					if (!opRes.Result)
					{
						jsonHelper.AddPlainObject<OperationResult>(opRes);
						return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					// associo la login appena creata al database di ERP con il ruolo di db_owner

					opRes.Result = dTask.CreateUser(loginName, dbName);
					opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

					if (!opRes.Result)
					{
						jsonHelper.AddPlainObject<OperationResult>(opRes);
						return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					// associo la login appena creata al database DMS con il ruolo di db_owner
					opRes.Result = dTask.CreateUser(loginName, dmsDbName);
					opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

					if (!opRes.Result)
					{
						jsonHelper.AddPlainObject<OperationResult>(opRes);
						return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					// il Provider deve essere definito a livello di subscription
					subDatabase.Provider = "SQLAzure";

					subDatabase.DBName = dbName;
					subDatabase.DBServer = settings.DatabaseInfo.DBServer;
					subDatabase.DBOwner = loginName;
					subDatabase.DBPassword = password;

					subDatabase.UseDMS = true;
					subDatabase.DMSDBName = dmsDbName;
					subDatabase.DMSDBServer = settings.DatabaseInfo.DBServer;
					subDatabase.DMSDBOwner = loginName;
					subDatabase.DMSDBPassword = password;

					Debug.WriteLine("-------- DB Name: " + dbName);
					Debug.WriteLine("-------- DMS DB Name: " + dmsDbName);
					Debug.WriteLine("-------- Login Name: " + loginName);
					Debug.WriteLine("-------- Password: " + password);

					// creo la struttura leggendo i metadati da filesystem

					DatabaseManager dbManager = APIDatabaseHelper.CreateDatabaseManager();
					opRes.Result = dbManager.ConnectAndCheckDBStructure(subDatabase);
					opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
					if (!opRes.Result)
					{
						jsonHelper.AddPlainObject<OperationResult>(opRes);
						return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					dbManager.ImportDefaultData = true;
					dbManager.ImportSampleData = false;
					opRes.Result = dbManager.DatabaseManagement(false) && !dbManager.ErrorInRunSqlScript; // passo il parametro cosi' salvo il log
					opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();

					if (!opRes.Result)
					{
						jsonHelper.AddPlainObject<OperationResult>(opRes);
						return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					try
					{
						// ho terminato l'elaborazione, aggiorno il record nella tabella 
						// con i dati del database e rimetto il flag UnderMaintenance a false
						subDatabase.UnderMaintenance = false;
						opRes = subDatabase.Save(burgerData);
						opRes.Message = Strings.OperationOK;
					}
					catch (Exception exc)
					{
						opRes.Result = false;
						opRes.Message = "020 DatabaseController.QuickCreate" + exc.Message;
						return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					if (!opRes.Result)
					{
						opRes.Message = Strings.OperationKO;
						return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
					}

					jsonHelper.AddPlainObject<OperationResult>(opRes);
					return new ContentResult { StatusCode = 201, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
				}
				*/

		/// <summary>
		/// Try to open connection with credentials in the body
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/testconnection/{subscriptionKey}")]
		public IActionResult ApiTestConnection(string subscriptionKey, [FromBody] DatabaseCredentials dbCredentials)
		{
			OperationResult opRes = new OperationResult();

			if (dbCredentials == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// if databaseName is empty I use master
			bool isAzureDB = (dbCredentials.Provider == "SQLAzure");

			string connectionString =
				string.Format
				(
				isAzureDB ? NameSolverDatabaseStrings.SQLAzureConnection : NameSolverDatabaseStrings.SQLConnection,
				dbCredentials.Server,
				string.IsNullOrWhiteSpace(dbCredentials.Database) ? DatabaseLayerConsts.MasterDatabase : dbCredentials.Database,
				dbCredentials.Login,
				dbCredentials.Password
				);

			DatabaseTask dTask = new DatabaseTask(isAzureDB);
			dTask.CurrentStringConnection = connectionString;

			opRes.Result = dTask.TryToConnect();
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

			// se sono riuscita a connettermi allora
			// vado a controllare che se l'edizione di SQL sia compatibile con il provider prescelto
			if (opRes.Result)
			{
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					connection.Open();
					SQLServerEdition sqlEdition = TBCheckDatabase.GetSQLServerEdition(connection);
					if (
						(isAzureDB && sqlEdition != SQLServerEdition.SqlAzureV12) ||
						(!isAzureDB && sqlEdition == SQLServerEdition.SqlAzureV12)
						)
					{
						opRes.Result = false;
						opRes.Message = "The provider and the edition of SQL Server you are chosen are not compatible. Please choose another one.";
					}
				}
			}

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Try to open connection with administrative credentials in the body and check if dbName already exists
		/// </summary>
		/// <param name="subscriptionKey"></param>
		/// <param name="dbName"></param>
		/// <param name="dbCredentials"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/exist/{subscriptionKey}/{dbName}")]
		public IActionResult ApiExistDatabase(string subscriptionKey, string dbName, [FromBody] DatabaseCredentials dbCredentials)
		{
			OperationResult opRes = new OperationResult();

			if (dbCredentials == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// I use master database to load all dbs
			bool isAzureDB = (dbCredentials.Provider == "SQLAzure");

			string connectionString =
				string.Format
				(
				isAzureDB ? NameSolverDatabaseStrings.SQLAzureConnection : NameSolverDatabaseStrings.SQLConnection,
				dbCredentials.Server,
				DatabaseLayerConsts.MasterDatabase,
				dbCredentials.Login,
				dbCredentials.Password
				);

			DatabaseTask dTask = new DatabaseTask(isAzureDB);
			dTask.CurrentStringConnection = connectionString;

			opRes.Result = dTask.ExistDataBase(dbName);

			// controllo se nel diagnostico c'e' un errore e imposto il result a false
			if (dTask.Diagnostic.Error)
			{
				opRes.Result = false;
				opRes.Message = dTask.Diagnostic.ToJson(true);
			}
			else
				opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		//---------------------------------------------------------------------
		[HttpPost("api/database/check/{subscriptionKey}")]
		public IActionResult ApiCheck(string subscriptionKey, [FromBody] ExtendedSubscriptionDatabase extSubDatabase)
		{
			OperationResult opRes = new OperationResult();

			if (extSubDatabase.AdminCredentials == null || extSubDatabase.Database == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			opRes = APIDatabaseHelper.PrecheckSubscriptionDB(extSubDatabase);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Update subscription database
		/// </summary>
		/// <param name="subscriptionKey"></param>
		/// <param name="extSubDatabase"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/update/{subscriptionKey}")]
		public IActionResult ApiUpdate(string subscriptionKey, [FromBody] ExtendedSubscriptionDatabase extSubDatabase)
		{
			OperationResult opRes = new OperationResult();

			if (extSubDatabase.AdminCredentials == null || extSubDatabase.Database == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// eseguo i controlli preventivi sui database ERP+DMS (unicode, collation, etc.)

			opRes = APIDatabaseHelper.CheckDatabases(extSubDatabase);

			if (!opRes.Result)
			{
				opRes.Message = Strings.OperationKO;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// check login per erp db
			opRes = APIDatabaseHelper.CheckLogin(extSubDatabase);

			if (opRes.Result)
				opRes = APIDatabaseHelper.CheckLogin(extSubDatabase, true);

            if (!opRes.Result)//@TODO se fallisce la checklogin interrompo perchè potrebbe non aver creato  nulla, per esempio se password non rispettano le policy
            {
				opRes.Message = Strings.OperationKO;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
            }

			// se tutto ok allora devo aggiornare i campi nella SubscriptionDatabase
			// e poi eseguire il check del database 
			// se e' necessario un aggiornamento devo chiedere prima conferma all'utente
			/*DatabaseManager dbManager = APIDatabaseHelper.CreateDatabaseManager();
			opRes.Result = dbManager.ConnectAndCheckDBStructure(extSubDatabase.Database);
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			dbManager.ImportDefaultData = true;
			dbManager.ImportSampleData = false;
			opRes.Result = dbManager.DatabaseManagement(false) && !dbManager.ErrorInRunSqlScript; // passo il parametro cosi' salvo il log
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();

			if (!opRes.Result)
			{
				//re-imposto il flag UnderMaintenance a false
				opRes = APIDatabaseHelper.SetSubscriptionDBUnderMaintenance(extSubDatabase.Database, burgerData, false);
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}*/

			if (!opRes.Result)
			{
				opRes.Message = Strings.OperationKO;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Import default data in SubscriptionDatabase
		/// </summary>
		/// <param name="subscriptionKey"></param>
		/// <param name="iso"></param>
		/// <param name="configuration"></param>
		/// <param name="importDataContent"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/import/default/{subscriptionKey}/{iso}/{configuration}")]
		public IActionResult ApiImportDefaultData(string subscriptionKey, string iso, string configuration, [FromBody] ImportDataBodyContent importDataContent)
		{
			OperationResult opRes = new OperationResult();

			if (importDataContent == null || importDataContent.Database == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			DatabaseManager dbManager = APIDatabaseHelper.CreateDatabaseManager();
			opRes.Result = dbManager.ConnectAndCheckDBStructure(importDataContent.Database , false); // il param 2 effettua il controllo solo sul db di ERP
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (dbManager.StatusDB == DatabaseStatus.EMPTY)
			{
				opRes.Result = false;
				opRes.Message = "ERP database does not contain any table, unable to proceed!";
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (dbManager.ContextInfo.MakeSubscriptionDatabaseConnection(importDataContent.Database))
			{
				BaseImportExportManager importExportManager = new BaseImportExportManager(dbManager.ContextInfo, (BrandLoader)InstallationData.BrandLoader);
				importExportManager.SetDefaultDataConfiguration(configuration);
				importExportManager.ImportDefaultDataForSubscription(importDataContent.ImportParameters);
			}

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Import sample data in SubscriptionDatabase
		/// </summary>
		/// <param name="subscriptionKey"></param>
		/// <param name="iso"></param>
		/// <param name="configuration"></param>
		/// <param name="importDataContent"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/import/sample/{subscriptionKey}/{iso}/{configuration}")]
		public IActionResult ApiImportSampleData(string subscriptionKey, string iso, string configuration, [FromBody] ImportDataBodyContent importDataContent)
		{
			OperationResult opRes = new OperationResult();

			if (importDataContent == null || importDataContent.Database == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			DatabaseManager dbManager = APIDatabaseHelper.CreateDatabaseManager();
			opRes.Result = dbManager.ConnectAndCheckDBStructure(importDataContent.Database, false); // il param 2 effettua il controllo solo sul db di ERP
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (dbManager.StatusDB == DatabaseStatus.EMPTY)
			{
				opRes.Result = false;
				opRes.Message = "ERP database does not contain any table, unable to proceed!";
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (dbManager.ContextInfo.MakeSubscriptionDatabaseConnection(importDataContent.Database))
			{
				BaseImportExportManager importExportManager = new BaseImportExportManager(dbManager.ContextInfo, (BrandLoader)InstallationData.BrandLoader);
				importExportManager.SetSampleDataConfiguration(configuration, iso);
				importExportManager.ImportSampleDataForSubscription(importDataContent.ImportParameters);
			}

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Delete only objects (tables, views, stored procedures, triggers) in ERP database (no DMS db is involved!)
		/// </summary>
		/// <param name="subscriptionKey"></param>
		/// <param name="subDatabase"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/deleteobjects/{subscriptionKey}")]
		public IActionResult ApiDeleteDatabaseObjects(string subscriptionKey, [FromBody]SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			if (subDatabase == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			bool isAzureDB = (subDatabase.Provider == "SQLAzure");
			string connectionString =
				string.Format
				(
				isAzureDB ? NameSolverDatabaseStrings.SQLAzureConnection : NameSolverDatabaseStrings.SQLConnection,
				subDatabase.DBServer,
				subDatabase.DBName,
				subDatabase.DBOwner,
				subDatabase.DBPassword
				);

			DatabaseTask dTask = new DatabaseTask(isAzureDB) { CurrentStringConnection = connectionString };
			opRes.Result = dTask.DeleteDatabaseObjects();
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Delete the SubscriptionDatabase row and, eventually, the database containers
		/// </summary>
		/// <param name="subscriptionKey"></param>
		/// <param name="deleteContent"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/delete/{subscriptionKey}")]
		public IActionResult ApiDeleteDatabase(string subscriptionKey, [FromBody]DeleteDatabaseBodyContent deleteContent)
		{
			OperationResult opRes = new OperationResult();

			if (deleteContent == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// se ho scelto di eliminare almeno uno dei contenitori dei database richiamo l'apposito metodo
			if (deleteContent.DeleteParameters.DeleteDMSDatabase || deleteContent.DeleteParameters.DeleteERPDatabase)
				/*opRes =*/ APIDatabaseHelper.DeleteDatabase(deleteContent);
			
			// anche se il result e' false devo procedere ma tenere traccia da qualche parte dell'errore (nell'area notifiche)
			/*if (!opRes.Result)
			{
				opRes.Message = Strings.OperationKO;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}*/


			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Esegue il check della struttura di un SubscriptionDatabase (per il db di ERP ed il db del DMS)
		/// </summary>		
		/// <param name="subscriptionKey"></param>
		/// <param name="subDatabase"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/checkstructure/{subscriptionKey}")]
		public IActionResult ApiCheckDatabaseStructure(string subscriptionKey, [FromBody] SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			if (subDatabase == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			DatabaseManager dbManager = APIDatabaseHelper.CreateDatabaseManager();
			opRes.Result = dbManager.ConnectAndCheckDBStructure(subDatabase);
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			opRes.Content = APIDatabaseHelper.GetMessagesList(dbManager.DBManagerDiagnostic);

			// TODO: dall'attivazione della Subscription devo sapere se provengo da una vecchia versione
			// forse questo controllo non sara' piu' necessario, dipende cosa verra' deciso a livello commerciale
			/*if ((dbManager.StatusDB & DatabaseStatus.PRE_40) == DatabaseStatus.PRE_40)
			{
				if (!this.canMigrate)
				{
				}
			}*/

			if (opRes.Result)
			{
				if (
					((dbManager.StatusDB == DatabaseStatus.UNRECOVERABLE || dbManager.StatusDB == DatabaseStatus.NOT_EMPTY) &&
					!dbManager.ContextInfo.HasSlaves)
					||
					(dbManager.StatusDB == DatabaseStatus.UNRECOVERABLE || dbManager.StatusDB == DatabaseStatus.NOT_EMPTY) &&
					(dbManager.DmsStructureInfo.DmsCheckDbStructInfo.DBStatus == DatabaseStatus.UNRECOVERABLE ||
					dbManager.DmsStructureInfo.DmsCheckDbStructInfo.DBStatus == DatabaseStatus.NOT_EMPTY)
					)
				{
					// significa che non e' possibile procedere con l'aggiornamento perche':
					// - i database sono gia' aggiornati
					// - i database sono privi della TB_DBMark e pertanto sono in uno stato non recuperabile
					opRes.Code = (int)AppReturnCodes.InternalError;
				}
				else
					opRes.Code = (int)AppReturnCodes.OK;
			}
			else // anche se la connessione non e' andata a buon fine ritorno il codice -1 cosi da inibire l'upgrade
				opRes.Code = (int)AppReturnCodes.InternalError;

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Esegue l'upgrade di un SubscriptionDatabase (per il db di ERP ed il db del DMS)
		/// Se specificata una configurazione importo anche i relativi dati di default
		/// </summary>
		/// <param name="subscriptionKey"></param>
		/// <param name="configuration"></param>
		/// <param name="subDatabase"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/upgradestructure/{subscriptionKey}/{configuration?}")]
		public IActionResult ApiUpgradeDatabaseStructure(string subscriptionKey, string configuration, [FromBody] SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			if (subDatabase == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			DatabaseManager dbManager = APIDatabaseHelper.CreateDatabaseManager();
			opRes.Result = dbManager.ConnectAndCheckDBStructure(subDatabase);
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// set Configuration for default data
			dbManager.ImportSampleData = false;
			dbManager.ImportDefaultData = false;
			if (!string.IsNullOrWhiteSpace(configuration))
			{
				dbManager.ImportDefaultData = true;
				dbManager.ImpExpManager.SetDefaultDataConfiguration(configuration);
			}
			
			opRes.Result = dbManager.DatabaseManagement(false) && !dbManager.ErrorInRunSqlScript; // passo il parametro cosi' salvo il log
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			opRes.Content = APIDatabaseHelper.GetMessagesList(dbManager.DBManagerDiagnostic);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Returns a list of configurations for default/sample data, separated by iso and INTL
		/// </summary>
		/// <param name="subscriptionKey">to check AuthorizationHeader</param>
		/// <param name="configType">admitted values: default / sample</param>
		/// <param name="iso">country code of subscription</param>
		/// <returns>"Content": {
		///     "IT": [
		///         "Basic",
		///         "Manufacturing-Advanced",
		///         "Manufacturing-Basic",
		///         "Manufacturing-WMS-Adv-Additional-Data",
		///         "Manufacturing-WMS-Basic-Additional-Data",
		///         "WMS-Advanced",
		///         "WMS-Basic"
		///       ],
		///       "INTL": [
		///         "Basic",
		///         "Manufacturing-Advanced",
		///         "Manufacturing-Basic",
		///         "Manufacturing-WMS-Adv-Additional-Data-EN",
		///         "Manufacturing-WMS-Basic-Additional-Data-EN",
		///         "WMS-Advanced",
		///         "WMS-Basic"
		///        ]
		///	}
		///</returns>
		//---------------------------------------------------------------------
		[HttpGet("api/database/configurations/{subscriptionKey}/{configType}/{iso}")]
		[Produces("application/json")]
		public IActionResult ApiGetConfigurations(string subscriptionKey, string configType, string iso)
		{
			OperationResult opRes = new OperationResult();

			// if configType is empty or not equal to default or sample I return
			if (string.IsNullOrWhiteSpace(configType) || 
				(string.Compare(configType, NameSolverStrings.Default, StringComparison.InvariantCultureIgnoreCase) != 0 &&
				string.Compare(configType, NameSolverStrings.Sample, StringComparison.InvariantCultureIgnoreCase) != 0)
				)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			PathFinder pathFinder = new PathFinder("USR-DELBENEMIC", "Development", "WebMago", "sa") { Edition = "Professional" };
			opRes.Content = APIDatabaseHelper.GetConfigurationList(pathFinder, configType, iso);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}
	}
}