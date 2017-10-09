using Microarea.AdminServer.Controllers.Helpers;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Libraries.DatabaseManager;
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
			OperationResult opRes = new OperationResult();

			if (string.IsNullOrWhiteSpace(subDatabase.InstanceKey))
			{
				opRes.Result = false;
				opRes.Message = Strings.InstanceKeyEmpty;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (String.IsNullOrEmpty(subDatabase.SubscriptionKey))
			{
				opRes.Result = false;
				opRes.Message = Strings.SubscriptionKeyEmpty;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			if (String.IsNullOrEmpty(subDatabase.Name))
			{
				opRes.Result = false;
				opRes.Message = Strings.DatabaseNameEmpty;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			string authHeader = HttpContext.Request.Headers["Authorization"];

			// check AuthorizationHeader first

			opRes = SecurityManager.ValidateAuthorization(
				authHeader, settings.SecretsKeys.TokenHashingKey, RolesStrings.Admin, subDatabase.SubscriptionKey, RoleLevelsStrings.Subscription);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 401, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			try
			{
				if (subDatabase != null)
				{
					opRes = subDatabase.Save(burgerData);
					opRes.Message = Strings.OperationOK;
				}
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
		public IActionResult QuickCreate(string instanceKey, string subscriptionKey)
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

			string dbName = instanceKey + "_" + subscriptionKey + "_Master_DB";
			string dmsDbName = dbName + "_DMS";

			// creazione contenitore db su Azure
			AzureCreateDBParameters param = new AzureCreateDBParameters();
			param.DatabaseName = dbName;
			param.MaxSize = AzureMaxSize.GB1;

			// to create database I need to connect to master

			// I create ERP database

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
			opRes.Result = dTask.CreateAzureDatabase(param);
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// I create DMS database

			param.DatabaseName = dmsDbName;
			opRes.Result = dTask.CreateAzureDatabase(param);
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

			// creo la struttura leggendo i metedati da filesystem

			Diagnostic dbTesterDiagnostic = new Diagnostic("DatabaseController");

			PathFinder pf = new PathFinder("USR-DELBENEMIC", "Development", "WebMago", "sa");//dati reperibili dal db, attualmente il nome server però non lo abbiamo 
			pf.Edition = "Professional";

			// creazione tabelle per il database aziendale
			DatabaseManager dbManager = new DatabaseManager
				(
				pf,
				dbTesterDiagnostic,
				(BrandLoader)InstallationData.BrandLoader,
				new ContextInfo.SystemDBConnectionInfo(), // da togliere
				DBNetworkType.Large,
				"IT"
				);

			subDatabase.Provider = "SQLServer";
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

			opRes.Result = dbManager.ConnectAndCheckDBStructure(subDatabase);
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			dbManager.ImportDefaultData = false;
			dbManager.ImportSampleData = false;
			Debug.WriteLine("Start database creation: " + DateTime.Now.ToString("hh:mm:ss.fff"));
			opRes.Result = dbManager.DatabaseManagement(false) && !dbManager.ErrorInRunSqlScript; // passo il parametro cosi' salvo il log
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			Debug.WriteLine("End database creation: " + DateTime.Now.ToString("hh:mm:ss.fff"));

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
		public IActionResult TestConnection(string subscriptionKey, [FromBody] DatabaseCredentials dbCredentials)
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

			// if databaseName is empty I use master
			bool isAzureDB = (dbCredentials.Provider == "SQLAzure");

			string connectionString =
				string.Format
				(
				(!isAzureDB) ? NameSolverDatabaseStrings.SQLConnection : NameSolverDatabaseStrings.SQLAzureConnection,
				dbCredentials.Server,
				string.IsNullOrWhiteSpace(dbCredentials.Database) ? DatabaseLayerConsts.MasterDatabase : dbCredentials.Database,
				dbCredentials.Login,
				dbCredentials.Password
				);

			DatabaseTask dTask = new DatabaseTask(isAzureDB);
			dTask.CurrentStringConnection = connectionString;

			opRes.Result = dTask.TryToConnect();
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

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
		public IActionResult ExistDatabase(string subscriptionKey, string dbName, [FromBody] DatabaseCredentials dbCredentials)
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

			// I use master database to load all dbs
			bool isAzureDB = (dbCredentials.Provider == "SQLAzure");

			string connectionString =
				string.Format
				(
				(!isAzureDB) ? NameSolverDatabaseStrings.SQLConnection : NameSolverDatabaseStrings.SQLAzureConnection,
				dbCredentials.Server,
				DatabaseLayerConsts.MasterDatabase,
				dbCredentials.Login,
				dbCredentials.Password
				);

			DatabaseTask dTask = new DatabaseTask(isAzureDB);
			dTask.CurrentStringConnection = connectionString;

			opRes.Result = dTask.ExistDataBase(dbName);
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		[HttpPost("/api/database/update")]
		//---------------------------------------------------------------------
		public IActionResult Update([FromBody] FullSubcriptionDatabase subDatabase)
		{
			// @@TODO: in Angular devo effettuare un controllo preventivo e farmi passare anche le credenziali di amministrazione
			// vedere se e' corretto riempire una classe esterna con le informazioni delle DatabaseCredentials e SubscriptionDatabase
			// i nomi dei server devono essere tutti uguali!

			OperationResult opRes = new OperationResult();

			if (subDatabase == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// I set subscription UnderMaintenance = true

			opRes = SetSubscriptionDBUnderMaintenance(subDatabase.Database);

			if (!opRes.Result)
			{
				opRes.Message = Strings.OperationKO;
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// eseguo i controlli preventivi sui database ERP+DMS (unicode, collation, etc.)

			opRes = CheckDatabases(subDatabase);

			if (!opRes.Result)
			{
				//@TODO lascio il flag UnderMaintenance a true?
				opRes.Message = Strings.OperationKO;
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// check login per erp db
			opRes = CheckLogin(subDatabase);

			if (opRes.Result)
				opRes = CheckLogin(subDatabase, true);

            if (!opRes.Result)//@TODO se fallisce la checklogin interrompo perchè potrebbe non aver creato  nulla, per esempio se password non rispettano le policy
            {
                //@TODO lascio il flag UnderMaintenance a true?
                opRes.Message = Strings.OperationKO;
                return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
            }

            // se tutto ok allora devo aggiornare i campi nella SubscriptionDatabase
            // e poi eseguire il check del database 
            // se e' necessario un aggiornamento devo chiedere prima conferma all'utente

            // I set subscription UnderMaintenance = false

            opRes = SetSubscriptionDBUnderMaintenance(subDatabase.Database, false);

			if (!opRes.Result)
			{
				opRes.Message = Strings.OperationKO;
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		//---------------------------------------------------------------------
		private OperationResult CheckDatabases(FullSubcriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			// I use master database to load information
			bool isAzureDB = (subDatabase.AdminCredentials.Provider == "SQLAzure");

			string connectionString =
				string.Format
				(
				(!isAzureDB) ? NameSolverDatabaseStrings.SQLConnection : NameSolverDatabaseStrings.SQLAzureConnection,
				subDatabase.AdminCredentials.Server,
				DatabaseLayerConsts.MasterDatabase,
				subDatabase.AdminCredentials.Login,
				subDatabase.AdminCredentials.Password
				);

			// provo a connetermi al master con le credenziali di amministrazione
			DatabaseTask dTask = new DatabaseTask(isAzureDB) { CurrentStringConnection = connectionString };

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
				// creazione contenitore db su Azure
				AzureCreateDBParameters param = new AzureCreateDBParameters();
				param.DatabaseName = subDatabase.Database.DBName;
				param.MaxSize = AzureMaxSize.GB1;

				// I create ERP database

				opRes.Result = dTask.CreateAzureDatabase(param);
				opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

				if (!opRes.Result)
					return opRes;
			}

			if (!existDMSDb && !string.IsNullOrWhiteSpace(subDatabase.Database.DMSDBName))
			{
				// creazione contenitore db su Azure
				AzureCreateDBParameters param = new AzureCreateDBParameters();
				param.DatabaseName = subDatabase.Database.DMSDBName;
				param.MaxSize = AzureMaxSize.GB1;

				// I create DMS database

				opRes.Result = dTask.CreateAzureDatabase(param);
				opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson(true);

				if (!opRes.Result)
					return opRes;
			}

			DBInfo erpDBInfo = LoadDBMarkInfo(connectionString, subDatabase.Database.DBName);
			DBInfo dmsDBInfo = LoadDBMarkInfo(connectionString, subDatabase.Database.DMSDBName);

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
					opRes.Message = "Flag unicode non compatibili";
					return opRes;
				}

				if (string.Compare(erpDBInfo.Collation, dmsDBInfo.Collation, StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					opRes.Result = false;
					opRes.Message = "Collation non compatibili";
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
		private OperationResult CheckLogin(FullSubcriptionDatabase subDatabase, bool isDMS = false)
		{
			OperationResult opRes = new OperationResult();

            string serverName = isDMS ? subDatabase.Database.DMSDBServer : subDatabase.Database.DBServer;
			string dbName = isDMS ?     subDatabase.Database.DMSDBName     :subDatabase.Database.DBName  ;
			string dbowner = isDMS ?    subDatabase.Database.DMSDBOwner    :subDatabase.Database.DBOwner;
			string password = isDMS ?   subDatabase.Database.DMSDBPassword :subDatabase.Database.DBPassword ;

            if (string.IsNullOrWhiteSpace(serverName) || string.IsNullOrWhiteSpace(dbName) || string.IsNullOrWhiteSpace(dbowner))
            {
                opRes.Result = false;
                opRes.Message = Strings.EmptyCredentials;
                return opRes;
            }
                // I use master database to load information
                bool isAzureDB = (subDatabase.AdminCredentials.Provider == "SQLAzure");

			string connectionString =
				string.Format
				(
				(!isAzureDB) ? NameSolverDatabaseStrings.SQLConnection : NameSolverDatabaseStrings.SQLAzureConnection,
				subDatabase.AdminCredentials.Server,
				DatabaseLayerConsts.MasterDatabase,
				subDatabase.AdminCredentials.Login,
				subDatabase.AdminCredentials.Password
				);

			// provo a connetermi al master con le credenziali di amministrazione
			DatabaseTask dTask = new DatabaseTask(isAzureDB) { CurrentStringConnection = connectionString };
			
			// check esistenza login
			bool existLogin = dTask.ExistLogin(dbowner);

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
				dTask.CurrentStringConnection = string.Format((!isAzureDB) ? NameSolverDatabaseStrings.SQLConnection : NameSolverDatabaseStrings.SQLAzureConnection, serverName, dbName, dbowner, password);
				opRes.Result = dTask.TryToConnect();
				if (!opRes.Result)
					opRes.Message = dTask.Diagnostic.ToJson(true);
			}

			return opRes;
		}

		//---------------------------------------------------------------------
		private DBInfo LoadDBMarkInfo(string connectionString, string dbName)
		{
			// devo cambiare il nome del database senza utilizzare la ChangeDatabase, non supportata da Azure
			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = dbName	};

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
			catch(Exception e)
			{
				info.Error = e.Message;
			}

			return info;
		}

		//---------------------------------------------------------------------
		private OperationResult SetSubscriptionDBUnderMaintenance(SubscriptionDatabase subDatabase, bool set = true)
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
	}

	//================================================================================
	public class FullSubcriptionDatabase
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