using Microarea.AdminServer.Controllers.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microarea.AdminServer.Libraries.DatabaseManager;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microarea.AdminServer.Services;
using Microarea.AdminServer.Properties;
using Microarea.Common.NameSolver;
using Microarea.Common.DiagnosticManager;
using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces;
using Microarea.AdminServer.Model;
using System.Diagnostics;
using System;
using Microarea.AdminServer.Libraries;
using Microarea.AdminServer.Services.BurgerData;

namespace Microarea.AdminServer.Controllers
{
	// Controller with APIs against database 
	// create container
	// create structure with tables

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

		// createdatabase/ dbname + subdatabase
		// createlogin / subdatabase (name e pw generata in automatico)
		// upgradestructure / subdatabase

		//---------------------------------------------------------------------
		[HttpPost("/api/database/quickcreate/{instanceKey}/{subscriptionKey}")]
		public IActionResult QuickCreate(string instanceKey, string subscriptionKey)
		{
			OperationResult opRes = new OperationResult();

			string dbName = subscriptionKey + "-ERP-DB";
			if (string.IsNullOrWhiteSpace(dbName))
			{
				opRes.Result = false;
				opRes.Message = Strings.DatabaseNameEmpty;
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

			//************************************************************************************
			// aggiungere controlli di esistenza database - login - connessioni al server, etc.
			//************************************************************************************

			// creo il record nella tabella con il flag UnderMaintenance a true

			SubscriptionDatabase subDatabase = new SubscriptionDatabase();
			subDatabase.InstanceKey = instanceKey;
			subDatabase.SubscriptionKey = subscriptionKey;
			subDatabase.Name = subscriptionKey + "-ERP";
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

			// creazione contenitore db su Azure
			AzureCreateDBParameters param = new AzureCreateDBParameters();
			param.DatabaseName = dbName;
			param.MaxSize = AzureMaxSize.GB1;

			DatabaseTask dTask = new DatabaseTask();
			dTask.CurrentStringConnection = string.Format(NameSolverDatabaseStrings.SQLConnection, settings.DatabaseInfo.DBServer, DatabaseLayerConsts.MasterDatabase, settings.DatabaseInfo.DBUser, settings.DatabaseInfo.DBPassword);
			opRes.Result = dTask.CreateAzureDatabase(param);
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson();

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// creo la login dbowner
			string loginName = dbName + "Admin";
			string password = SecurityManager.GetRandomPassword();
			opRes.Result = dTask.CreateLogin(loginName, password);
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson();

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// associo la login appena creata al database con il ruolo di db_owner
			opRes.Result = dTask.CreateUser(loginName, dbName);
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson();

			if (!opRes.Result)
			{
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			// creo la struttura leggendo i metedati da filesystem

			Diagnostic dbTesterDiagnostic = new Diagnostic("DatabaseController");

			PathFinder pf = new PathFinder("USR-DELBENEMIC", "Development", "WebMago", "sa");
			pf.Edition = "Professional";

			// creazione tabelle per il database aziendale
			DatabaseManager dbManager = new DatabaseManager
				(
				pf,
				dbTesterDiagnostic,
				(BrandLoader)InstallationData.BrandLoader,
				new ContextInfo.SystemDBConnectionInfo(), // da togliere
				DBNetworkType.Large,
				"IT",
				false // no ask credential
				);

			subDatabase.DBName = dbName;
			subDatabase.DBServer = settings.DatabaseInfo.DBServer;
			subDatabase.DBOwner = loginName;
			subDatabase.DBPassword = password;
			subDatabase.Provider = "SQL";

			Debug.WriteLine("-------- DB Name: " + dbName);
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
				opRes.Message = "040 AdminController.ApiDatabases" + exc.Message;
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
		/// Create database container and structure in manual mode
		/// da completare
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("/api/database/create/{dbname}")]
		public IActionResult CreateDatabase(string dbname, [FromBody] SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			if (string.IsNullOrWhiteSpace(dbname))
			{
				opRes.Result = false;
				opRes.Message = Strings.DatabaseNameEmpty;
				jsonHelper.AddPlainObject<OperationResult>(opRes);
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			AzureCreateDBParameters param = new AzureCreateDBParameters();
			param.DatabaseName = dbname;
			param.MaxSize = AzureMaxSize.GB1;

			DatabaseTask dTask = new DatabaseTask();
			dTask.CurrentStringConnection = settings.DatabaseInfo.ConnectionString;
			opRes.Result = dTask.CreateAzureDatabase(param);
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.ToJson();
			jsonHelper.AddPlainObject<OperationResult>(opRes);

			if (!opRes.Result)
				return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };

			Diagnostic dbTesterDiagnostic = new Diagnostic("DbTester");

			PathFinder pf = new PathFinder("USR-DELBENEMIC", "Development", "WebMago", "sa");
			pf.Edition = "Professional";

			// creazione tabelle per il database aziendale
			DatabaseManager dbManager = new DatabaseManager
				(
				pf,
				dbTesterDiagnostic,
				(BrandLoader)InstallationData.BrandLoader,
				new ContextInfo.SystemDBConnectionInfo(), // da togliere
				DBNetworkType.Large,
				"IT",
				false // no ask credential
				);

			if (dbManager.ConnectAndCheckDBStructure(subDatabase))
			{
				dbManager.ImportDefaultData = true;
				dbManager.ImportSampleData = false;
				Debug.WriteLine("Start database creation: " + DateTime.Now.ToString("hh:mm:ss.fff"));
				opRes.Result = dbManager.DatabaseManagement(false) && !dbManager.ErrorInRunSqlScript; // passo il parametro cosi' salvo il log
				Debug.WriteLine("End database creation: " + DateTime.Now.ToString("hh:mm:ss.fff"));
			}

			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}
	}
}