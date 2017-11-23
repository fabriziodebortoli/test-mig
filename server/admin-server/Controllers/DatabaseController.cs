using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Controllers.Helpers.All;
using Microarea.AdminServer.Controllers.Helpers.APIQuery;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Libraries.DatabaseManager;
using Microarea.AdminServer.Libraries.DataManagerEngine;
using Microarea.AdminServer.Model;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Properties;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Services.BurgerData;
using Microarea.Common.DiagnosticManager;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.AdminServer.Controllers
{
	/// <summary>
	/// Controller with APIs against database  
	/// </summary>
	//============================================================================
	public class DatabaseController : Controller
	{
		private IHostingEnvironment environment;
		private IJsonHelper jsonHelper;
		private AppOptions settings;
		private BurgerData burgerData;

		//---------------------------------------------------------------------
		public DatabaseController(IHostingEnvironment env, IOptions<AppOptions> settings, IJsonHelper jsonHelper)
		{
			environment = env;
			this.jsonHelper = jsonHelper;
			this.settings = settings.Value;

			burgerData = new BurgerData(this.settings.DatabaseInfo.ConnectionString);
		}

		/// <summary>
		/// Insert/update subscription database
		/// </summary>
		//-----------------------------------------------------------------------------	
		[HttpPost("/api/databases")]
		public IActionResult ApiDatabases([FromBody] SubscriptionDatabase subDatabase)
		{
			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			OperationResult opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subDatabase.SubscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			try
			{
				if (subDatabase != null)
					opRes = subDatabase.Save(burgerData);
				else
				{
					opRes.Result = false;
					opRes.Message = Strings.NoValidInput;
					opRes.Code = (int)AppReturnCodes.InvalidData;
				}
			}
			catch (Exception exc)
			{
				opRes.Result = false;
				opRes.Message = "010 DatabaseController.ApiDatabases" + exc.Message;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			opRes.Message = (opRes.Result) ? Strings.OperationOK : Strings.OperationKO;
			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Returns the databases list of a instanceKey+subscriptionKey
		/// You can also specify a database name (it is the third pk segment)
		/// </summary>
		/// <param name="instanceKey"></param>
		/// <param name="subscriptionKey"></param>
		/// <param name="dbName"></param>
		/// <returns></returns>
		//-----------------------------------------------------------------------------	
		[HttpGet("/api/databases/{instanceKey}/{subscriptionKey}/{dbName?}")]
		[Produces("application/json")]
		public IActionResult ApiGetDatabases(string instanceKey, string subscriptionKey, string dbName)
		{
			OperationResult opRes = new OperationResult();

			if (string.IsNullOrWhiteSpace(instanceKey))
			{
				opRes.Result = false;
				opRes.Message = Strings.InstanceKeyEmpty;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (string.IsNullOrWhiteSpace(subscriptionKey))
			{
				opRes.Result = false;
				opRes.Message = Strings.SubscriptionKeyEmpty;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			List<ISubscriptionDatabase> databasesList = new List<ISubscriptionDatabase>();

			try
			{
				databasesList = string.IsNullOrWhiteSpace(dbName)
					? this.burgerData.GetList<SubscriptionDatabase, ISubscriptionDatabase>(string.Format(Queries.SelectDatabases, instanceKey, subscriptionKey), ModelTables.SubscriptionDatabases)
					: this.burgerData.GetList<SubscriptionDatabase, ISubscriptionDatabase>(string.Format(Queries.SelectDatabaseByName, instanceKey, subscriptionKey, dbName), ModelTables.SubscriptionDatabases);
			}
			catch (Exception exc)
			{
				opRes.Result = false;
				opRes.Message = "010 DatabaseController.ApiGetDatabases" + exc.Message;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 501, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (databasesList.Count == 0)
			{
				opRes.Code = (int)AppReturnCodes.NoSubscriptionDatabasesAvailable;
				opRes.Message = Strings.NoSubscriptionDatabasesAvailable;
			}

			opRes.Result = true;
			opRes.Content = databasesList;
			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

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
		[HttpPost("/api/database/quickcreate/{instanceKey}/{subscriptionKey}")]
		public IActionResult ApiQuickCreate(string instanceKey, string subscriptionKey)
		{
			OperationResult opRes = new OperationResult();

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

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
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Try to open connection with credentials in the body
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("/api/database/testconnection/{subscriptionKey}")]
		public IActionResult ApiTestConnection(string subscriptionKey, [FromBody] DatabaseCredentials dbCredentials)
		{
			OperationResult opRes = new OperationResult();

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

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

			// controllo che se l'edizione di SQL sia compatibile con il provider prescelto
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
		[HttpPost("/api/database/exist/{subscriptionKey}/{dbName}")]
		public IActionResult ApiExistDatabase(string subscriptionKey, string dbName, [FromBody] DatabaseCredentials dbCredentials)
		{
			OperationResult opRes = new OperationResult();

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

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

		[HttpPost("/api/database/check/{subscriptionKey}")]
		//---------------------------------------------------------------------
		public IActionResult ApiCheck(string subscriptionKey, [FromBody] ExtendedSubscriptionDatabase extSubDatabase)
		{
			OperationResult opRes = new OperationResult();

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}
			 
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

		[HttpPost("/api/database/update/{subscriptionKey}")]
		//---------------------------------------------------------------------
		public IActionResult ApiUpdate(string subscriptionKey, [FromBody] ExtendedSubscriptionDatabase extSubDatabase)
		{
			// @@TODO: in Angular devo effettuare un controllo preventivo e farmi passare anche le credenziali di amministrazione
			// vedere se e' corretto riempire una classe esterna con le informazioni delle DatabaseCredentials e SubscriptionDatabase
			// i nomi dei server devono essere tutti uguali!

			OperationResult opRes = new OperationResult();

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (extSubDatabase.AdminCredentials == null || extSubDatabase.Database == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// devo controllare il flag UnderMaintenance: se a true devo fare return e segnalare che e' gia' in aggiornamento

			// I set subscription UnderMaintenance = true
			opRes = APIDatabaseHelper.SetSubscriptionDBUnderMaintenance(extSubDatabase.Database, burgerData);

			if (!opRes.Result)
			{
				opRes.Message = Strings.OperationKO;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// eseguo i controlli preventivi sui database ERP+DMS (unicode, collation, etc.)

			opRes = APIDatabaseHelper.CheckDatabases(extSubDatabase);

			if (!opRes.Result)
			{
				//re-imposto il flag UnderMaintenance a false
				if (!APIDatabaseHelper.SetSubscriptionDBUnderMaintenance(extSubDatabase.Database, burgerData, false).Result)
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
				//re-imposto il flag UnderMaintenance a false
				opRes = APIDatabaseHelper.SetSubscriptionDBUnderMaintenance(extSubDatabase.Database, burgerData, false);
				opRes.Message = Strings.OperationKO;
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

			// I set subscription UnderMaintenance = false

			opRes = APIDatabaseHelper.SetSubscriptionDBUnderMaintenance(extSubDatabase.Database, burgerData, false);

			if (!opRes.Result)
			{
				opRes.Message = Strings.OperationKO;
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		[HttpPost("/api/database/import/default/{subscriptionKey}/{iso}/{configuration}")]
		//---------------------------------------------------------------------
		public IActionResult ApiImportDefaultData(string subscriptionKey, string iso, string configuration, [FromBody] SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (subDatabase == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			DatabaseManager dbManager = APIDatabaseHelper.CreateDatabaseManager();
			opRes.Result = dbManager.ConnectAndCheckDBStructure(subDatabase, false); // il param 2 effettua il controllo solo sul db di ERP
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

			if (dbManager.ContextInfo.MakeCompanyConnection(subDatabase))
			{
				BaseImportExportManager importExportManager = new BaseImportExportManager(dbManager.ContextInfo, (BrandLoader)InstallationData.BrandLoader);
				importExportManager.SetDefaultDataConfiguration(configuration);
				importExportManager.ImportDefaultDataSilentMode(true);
			}

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		[HttpPost("/api/database/import/sample/{subscriptionKey}/{iso}/{configuration}")]
		//---------------------------------------------------------------------
		public IActionResult ApiImportSampleData(string subscriptionKey, string iso, string configuration, [FromBody] SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (subDatabase == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			DatabaseManager dbManager = APIDatabaseHelper.CreateDatabaseManager();
			opRes.Result = dbManager.ConnectAndCheckDBStructure(subDatabase, false); // il param 2 effettua il controllo solo sul db di ERP
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

			if (dbManager.ContextInfo.MakeCompanyConnection(subDatabase))
			{
				BaseImportExportManager importExportManager = new BaseImportExportManager(dbManager.ContextInfo, (BrandLoader)InstallationData.BrandLoader);
				importExportManager.SetSampleDataConfiguration(configuration, iso);
				importExportManager.ImportSampleDataSilentMode(true);
			}

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		[HttpPost("/api/database/deleteobjects/{subscriptionKey}")]
		//---------------------------------------------------------------------
		public IActionResult ApiDeleteObjects(string subscriptionKey, [FromBody]SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (subDatabase == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// I use master database to load all dbs
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

		[HttpPost("/api/database/checkstructure/{subscriptionKey}")]
		//---------------------------------------------------------------------
		public IActionResult ApiCheckDatabaseStructure(string subscriptionKey, [FromBody] SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (subDatabase == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// devo controllare il flag UnderMaintenance: se a true devo fare return e segnalare che e' gia' in aggiornamento

			// I set subscription UnderMaintenance = true
			opRes = APIDatabaseHelper.SetSubscriptionDBUnderMaintenance(subDatabase, burgerData);

			if (!opRes.Result)
			{
				opRes.Message = Strings.OperationKO;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			DatabaseManager dbManager = APIDatabaseHelper.CreateDatabaseManager();
			opRes.Result = dbManager.ConnectAndCheckDBStructure(subDatabase);
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			opRes.Content = GetMessagesList(dbManager.DBManagerDiagnostic);

			// TODO: dall'attivazione della Subscription devo sapere se 
			/*if ((dbManager.StatusDB & DatabaseStatus.PRE_40) == DatabaseStatus.PRE_40)
			{
				if (!this.canMigrate)
				{
				}
			}*/

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
				opRes.Code = -1;
			}

			//re-imposto il flag UnderMaintenance a false
			APIDatabaseHelper.SetSubscriptionDBUnderMaintenance(subDatabase, burgerData, false);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		//---------------------------------------------------------------------
		private List<OperationResult> GetMessagesList(Diagnostic diagnostic)
		{
			List<OperationResult> messagesList = new List<OperationResult>();

			IDiagnosticItems items = diagnostic.AllMessages();
			if (items == null)
				return messagesList;

			foreach (IDiagnosticItem item in items)
				if (!string.IsNullOrEmpty(item.FullExplain))
					messagesList.Add(new OperationResult() { Message = item.FullExplain });

			return messagesList;
		}

		[HttpPost("/api/database/upgradestructure/{subscriptionKey}/{configuration?}")]
		//---------------------------------------------------------------------
		public IActionResult ApiUpgradeDatabaseStructure(string subscriptionKey, string configuration, [FromBody] SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (subDatabase == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// devo controllare il flag UnderMaintenance: se a true devo fare return e segnalare che e' gia' in aggiornamento

			// I set subscription UnderMaintenance = true
			opRes = APIDatabaseHelper.SetSubscriptionDBUnderMaintenance(subDatabase, burgerData);

			if (!opRes.Result)
			{
				opRes.Message = Strings.OperationKO;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
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
			opRes.Content = GetMessagesList(dbManager.DBManagerDiagnostic);

			//re-imposto il flag UnderMaintenance a false
			APIDatabaseHelper.SetSubscriptionDBUnderMaintenance(subDatabase, burgerData, false);

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
		[HttpGet("/api/database/configurations/{subscriptionKey}/{configType}/{iso}")]
		[Produces("application/json")]
		//---------------------------------------------------------------------
		public IActionResult ApiGetConfigurations(string subscriptionKey, string configType, string iso)
		{
			OperationResult opRes = new OperationResult();

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

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

	//================================================================================
	public class ExtendedSubscriptionDatabase
	{
		public DatabaseCredentials AdminCredentials;
		public SubscriptionDatabase Database;
	}

	//================================================================================
	public class DBInfo
	{
		public string Name = string.Empty;
		public bool ExistDBMark = false;
		public bool UseUnicode = false;
		public string Collation = string.Empty;
		public string Error = string.Empty;

		public bool HasError { get { return !string.IsNullOrWhiteSpace(Error); } }
	}
}