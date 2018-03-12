using Microarea.Common.NameSolver;
using Microarea.ProvisioningDatabase.Controllers.Helpers;
using Microarea.ProvisioningDatabase.Infrastructure;
using Microarea.ProvisioningDatabase.Infrastructure.Model;
using Microarea.ProvisioningDatabase.Libraries.DatabaseManager;
using Microarea.ProvisioningDatabase.Properties;
using Microsoft.AspNetCore.Mvc;
using System;
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
		public ProvisioningDatabaseController(IJsonHelper jsonHelper)
		{
			this.jsonHelper = jsonHelper;
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
			opRes.Message = Strings.DatabaseServiceStatusValid;
			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Creazione automatica:
		/// - contenitore db
		/// - login SQL + associazione login al database
		/// - struttura tabelle ERP
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/quickcreate")]
		public IActionResult ApiQuickCreate([FromBody] ExtendedSubscriptionDatabase extSubDatabase)
		{
			OperationResult opRes = new OperationResult();

			if (extSubDatabase.AdminCredentials == null || !extSubDatabase.AdminCredentials.Validate() || extSubDatabase.Database == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			opRes = APIDatabaseHelper.QuickCreateSubscriptionDatabase(extSubDatabase);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Try to open connection with credentials in the body
		/// </summary>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/testconnection")]
		public IActionResult ApiTestConnection(string checkCode, [FromBody] DatabaseCredentials dbCredentials)
		{
			OperationResult opRes = new OperationResult();

			if (dbCredentials == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			opRes = APIDatabaseHelper.TestConnection(dbCredentials);
			
			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Try to open connection with administrative credentials in the body and check if dbName already exists
		/// </summary>
		/// <param name="dbName"></param>
		/// <param name="dbCredentials"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/exist/{dbName}")]
		public IActionResult ApiExistDatabase(string checkCode, string dbName, [FromBody] DatabaseCredentials dbCredentials)
		{
			OperationResult opRes = new OperationResult();

			if (dbCredentials == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			opRes = APIDatabaseHelper.ExistDatabase(dbName, dbCredentials);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		//---------------------------------------------------------------------
		[HttpPost("api/database/check")]
		public IActionResult ApiCheck(string checkCode, [FromBody] ExtendedSubscriptionDatabase extSubDatabase)
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
		/// <param name="extSubDatabase"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/update")]
		public IActionResult ApiUpdate([FromBody] ExtendedSubscriptionDatabase extSubDatabase)
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

            if (!opRes.Result)//@TODO se fallisce la checklogin interrompo perchè potrebbe non aver creato nulla, per esempio se password non rispettano le policy
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
		/// <param name="iso"></param>
		/// <param name="configuration"></param>
		/// <param name="importDataContent"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/import/default/{iso}/{configuration}")]
		public IActionResult ApiImportDefaultData(string checkCode, string iso, string configuration, [FromBody] ImportDataBodyContent importDataContent)
		{
			OperationResult opRes = APIDatabaseHelper.ImportData(NameSolverStrings.Default, iso, configuration, importDataContent);
			
			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Import sample data in SubscriptionDatabase
		/// </summary>
		/// <param name="iso"></param>
		/// <param name="configuration"></param>
		/// <param name="importDataContent"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/import/sample/{iso}/{configuration}")]
		public IActionResult ApiImportSampleData(string checkCode, string iso, string configuration, [FromBody] ImportDataBodyContent importDataContent)
		{
			OperationResult opRes = APIDatabaseHelper.ImportData(NameSolverStrings.Sample, iso, configuration, importDataContent);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Delete only objects (tables, views, stored procedures, triggers) in ERP database (no DMS db is involved!)
		/// </summary>
		/// <param name="subDatabase"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/deleteobjects")]
		public IActionResult ApiDeleteDatabaseObjects(string checkCode, [FromBody]SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			if (subDatabase == null)
			{
				opRes.Result = false;
				opRes.Message = Strings.NoValidInput;
				opRes.Code = (int)AppReturnCodes.InvalidData;
				return new ContentResult { StatusCode = 500, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
			}

			opRes = APIDatabaseHelper.DeleteDatabaseObjects(subDatabase);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Delete the SubscriptionDatabase row and, eventually, the database containers
		/// </summary>
		/// <param name="deleteContent"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/delete")]
		public IActionResult ApiDeleteDatabase(string checkCode, [FromBody]DeleteDatabaseBodyContent deleteContent)
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
		/// <param name="subDatabase"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/checkstructure")]
		public IActionResult ApiCheckDatabaseStructure(string checkCode, [FromBody] SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = APIDatabaseHelper.CheckDatabaseStructure(subDatabase);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}

		/// <summary>
		/// Esegue l'upgrade di un SubscriptionDatabase (per il db di ERP ed il db del DMS)
		/// Se specificata una configurazione importo anche i relativi dati di default
		/// </summary>
		/// <param name="configuration"></param>
		/// <param name="subDatabase"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		[HttpPost("api/database/upgradestructure/{configuration?}")]
		public IActionResult ApiUpgradeDatabaseStructure(string checkCode, string configuration, [FromBody] SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = APIDatabaseHelper.UpgradeDatabaseStructure(configuration, subDatabase);

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
		[HttpGet("api/database/configurations/{configType}/{iso}")]
		[Produces("application/json")]
		public IActionResult ApiGetConfigurations(string checkCode, string configType, string iso)
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

			opRes.Content = APIDatabaseHelper.GetConfigurationList(configType, iso);

			jsonHelper.AddPlainObject<OperationResult>(opRes);
			return new ContentResult { StatusCode = 200, Content = jsonHelper.WritePlainAndClear(), ContentType = "application/json" };
		}
	}
}