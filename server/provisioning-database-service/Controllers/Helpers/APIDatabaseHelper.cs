using Microarea.Common.DiagnosticManager;
using Microarea.Common.Generic;
using Microarea.Common.NameSolver;
using Microarea.ProvisioningDatabase.Infrastructure;
using Microarea.ProvisioningDatabase.Infrastructure.Model;
using Microarea.ProvisioningDatabase.Libraries.DatabaseManager;
using Microarea.ProvisioningDatabase.Libraries.DataManagerEngine;
using Microarea.ProvisioningDatabase.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.ProvisioningDatabase.Controllers.Helpers
{
	/// <summary>
	/// Helper con i vari metodi richiamati dal ProvisioningDatabaseController
	/// </summary>
	//============================================================================
	public class APIDatabaseHelper
	{
		/// <summary>
		/// Try to open connection with credentials
		/// </summary>
		//---------------------------------------------------------------------
		public static OperationResult TestConnection(DatabaseCredentials dbCredentials)
		{
			OperationResult opRes = new OperationResult();

			// if databaseName is empty I use master
			bool isAzureDB = (dbCredentials.Provider == NameSolverStrings.SQLAzure);

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
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.GetErrorsStrings();

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
						opRes.Message = Strings.SQLProviderAndEditionNotCompatible;
					}
				}
			}

			return opRes;
		}

		/// <summary>
		/// Try to open connection with administrative credentials and check if dbName already exists
		/// </summary>
		/// <param name="dbName"></param>
		/// <param name="dbCredentials"></param>
		//---------------------------------------------------------------------
		public static OperationResult ExistDatabase(string dbName, DatabaseCredentials dbCredentials)
		{
			OperationResult opRes = new OperationResult();

			// I use master database to load all dbs
			bool isAzureDB = (dbCredentials.Provider == NameSolverStrings.SQLAzure);

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
				opRes.Message = dTask.Diagnostic.GetErrorsStrings();
			}
			else
				opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.GetErrorsStrings();

			return opRes;
		}

		/// <summary>
		/// Metodo per eseguire il check preventivo dei vari dati inseriti dall'utente per salvare il database
		/// </summary>
		/// <param name="extSubDatabase"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static OperationResult PrecheckSubscriptionDB(ExtendedSubscriptionDatabase extSubDatabase)
		{
			// result globale dell'operazione, imposto un numero di codice per capire se ci sono operazioni da eseguire
			OperationResult opRes = new OperationResult { Result = true };

			List<OperationResult> msgList = new List<OperationResult>();

			// I use master database to load all dbs
			bool isAzureDB = (extSubDatabase.AdminCredentials.Provider == NameSolverStrings.SQLAzure);

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
				opRes.Result = false;
				msgList.Add(new OperationResult() { Message = dTask.Diagnostic.GetErrorsStrings() });
				opRes.Content = msgList;
				opRes.Code = -1;
				// faccio return perche' si e' verificata un'eccezione sul server e, visto che il server del DMS e' uguale, non procedo ulteriormente
				return opRes;
			}

			if (existERPDb)
			{
				// il db e' azure segnalo un errore bloccante
				if (extSubDatabase.DB == DB.sqlazure)
				{
					msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.ErrorDBAlreadyExists, extSubDatabase.Database.DBName, extSubDatabase.Database.DBServer) });
					opRes.Result = false;
					opRes.Code = -1;
				}
				else
					msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningDBAlreadyExists, extSubDatabase.Database.DBName, extSubDatabase.Database.DBServer) });
			}
			else
				msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningDBNotExists, extSubDatabase.Database.DBName, extSubDatabase.Database.DBServer) });

			bool existDMSDb = dTask.ExistDataBase(extSubDatabase.Database.DMSDBName);
			if (dTask.Diagnostic.Error)
			{
				opRes.Result = false;
				opRes.Content = msgList;
				msgList.Add(new OperationResult() { Message = dTask.Diagnostic.GetErrorsStrings() });
				opRes.Code = -1;
				// faccio return perche' si e' verificata un'eccezione sul server e non procedo ulteriormente
				return opRes;
			}

			if (existDMSDb)
			{
				// il db e' azure segnalo un errore bloccante
				if (extSubDatabase.DB == DB.sqlazure)
				{
					msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.ErrorDBAlreadyExists, extSubDatabase.Database.DMSDBName, extSubDatabase.Database.DBServer) });
					opRes.Result = false;
					opRes.Code = -1;
				}
				else
					msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningDBAlreadyExists, extSubDatabase.Database.DMSDBName, extSubDatabase.Database.DMSDBServer) });
			}
			else
				msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningDBNotExists, extSubDatabase.Database.DMSDBName, extSubDatabase.Database.DMSDBServer) });
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
					opRes.Code = -1;
				}
				else
				{
					// qui devo controllare la compatibilita' delle collation con 
					// vedi vecchio metodo CompatibilitySQLDatabaseCulture

					if (erpDBInfo.ExistDBMark && dmsDBInfo.ExistDBMark)
					{
						if (erpDBInfo.UseUnicode != dmsDBInfo.UseUnicode)
						{
							opRes.Result = false;
							msgList.Add(new OperationResult() { Message = DatabaseManagerStrings.ErrorUnicodeValuesNotCompatible });
							opRes.Code = -1;
						}

						if (string.Compare(erpDBInfo.Collation, dmsDBInfo.Collation, StringComparison.InvariantCultureIgnoreCase) != 0)
						{
							opRes.Result = false;
							msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.ErrorCollationNotCompatible, erpDBInfo.Name, erpDBInfo.Collation, dmsDBInfo.Name, dmsDBInfo.Collation) });
							opRes.Code = -1;
						}
						else
						{
							//@@TODO: verifica compatibilita' collation con ISOSTATO attivazione
							// se non va bene return false
						}
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
				msgList.Add(new OperationResult() { Message = dTask.Diagnostic.GetErrorsStrings() });
				opRes.Code = -1;
			}
			else
			{
				if (!existLogin)
					msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningLoginNotExists, extSubDatabase.Database.DBOwner, extSubDatabase.Database.DBServer) });
				else
				{
					// check validita' login e password per il db di ERP (solo se la login esiste)
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

					dTask.TryToConnect(out int err);
					if (dTask.Diagnostic.Error)
					{
						// se errorNr == 916 si tratta di mancanza di privilegi per la connessione 
						// ma la coppia utente/password e' corretta (altrimenti il nr di errore ritornato sarebbe 18456)
						if (err == 916)
						{
							opRes.Result = opRes.Result && true;
							msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningLoginAlreadyExists, extSubDatabase.Database.DBOwner, extSubDatabase.Database.DBServer) });
						}
						else
						{
							if (err == 18456)
								msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.ErrorIncorrectPassword, extSubDatabase.Database.DBOwner) });

							opRes.Result = false;
							opRes.Code = -1;
						}
					}
					else
						msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningLoginAlreadyExists, extSubDatabase.Database.DBOwner, extSubDatabase.Database.DBServer) });
				}
			}
			//

			bool sameLogin = string.Compare(extSubDatabase.Database.DBOwner, extSubDatabase.Database.DMSDBOwner, StringComparison.InvariantCultureIgnoreCase) == 0;
			bool existDMSLogin = existLogin;

			// se la login e' diversa allora devo tentare di connettermi
			if (!sameLogin)
			{
				// check esistenza login
				existDMSLogin = dTask.ExistLogin(extSubDatabase.Database.DMSDBOwner);

				// se nel diagnostico c'e' un errore ritorno subito
				if (dTask.Diagnostic.Error)
				{
					opRes.Result = false;
					msgList.Add(new OperationResult() { Message = dTask.Diagnostic.GetErrorsStrings() });
					opRes.Code = -1;
				}
				else
				{
					if (!existDMSLogin)
						msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningLoginNotExists, extSubDatabase.Database.DMSDBOwner, extSubDatabase.Database.DBServer) });
					else
					{
						// check validita' login e password per il db del DMS (solo se esiste)
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

						dTask.TryToConnect(out int errorNr);

						if (dTask.Diagnostic.Error)
						{
							// se errorNr == 916 si tratta di mancanza di privilegi per la connessione 
							// ma la coppia utente/password e' corretta (altrimenti il nr di errore ritornato sarebbe 18456)
							if (errorNr == 916)
							{
								opRes.Result = opRes.Result && true;
								msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningLoginAlreadyExists, extSubDatabase.Database.DBOwner, extSubDatabase.Database.DBServer) });
							}
							else
							{
								if (errorNr == 18456)
									msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.ErrorIncorrectPassword, extSubDatabase.Database.DMSDBOwner) });

								opRes.Result = false;
								opRes.Code = -1;
							}
						}
						else
							msgList.Add(new OperationResult() { Message = string.Format(DatabaseManagerStrings.WarningLoginAlreadyExists, extSubDatabase.Database.DBOwner, extSubDatabase.Database.DBServer) });
					}
				}
			}
			//

			// check preventivo struttura dei database, se esistono
			/*if (existERPDb && existDMSDb)
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
			*/

			opRes.Content = msgList;
			return opRes;
		}

		/// <summary>
		/// Metodo generico per importazione dati in SubscriptionDatabase
		/// </summary>
		/// <param name="configType">default / sample</param>
		/// <param name="iso"></param>
		/// <param name="configuration"></param>
		/// <param name="importDataContent"></param>
		//---------------------------------------------------------------------
		public static OperationResult ImportData(string configType, string iso, string configuration, ImportDataBodyContent importDataContent)
		{
			OperationResult opRes = new OperationResult();

			DatabaseManager dbManager = CreateDatabaseManager();
			opRes.Result = dbManager.ConnectAndCheckDBStructure(importDataContent.Database, false); // il param 2 effettua il controllo solo sul db di ERP
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			if (!opRes.Result)
				return opRes;

			if (dbManager.StatusDB == DatabaseStatus.EMPTY)
			{
				opRes.Result = false;
				opRes.Message = Strings.ImportDataNotAvailable;
				return opRes;
			}

			if (dbManager.ContextInfo.MakeSubscriptionDatabaseConnection(importDataContent.Database))
			{
				BaseImportExportManager importExportManager = new BaseImportExportManager(dbManager.ContextInfo, (BrandLoader)InstallationData.BrandLoader);

				if (string.Compare(configType, NameSolverStrings.Default, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					importExportManager.SetDefaultDataConfiguration(configuration);
					opRes.Result = importExportManager.ImportDefaultDataForSubscription(importDataContent.ImportParameters);
				}
				else
				{
					importExportManager.SetSampleDataConfiguration(configuration, iso);
					opRes.Result = importExportManager.ImportSampleDataForSubscription(importDataContent.ImportParameters);
				}
			}

			return opRes;
		}

		/// <summary>
		/// Delete only objects (tables, views, stored procedures, triggers) in ERP database (no DMS db is involved!)
		/// </summary>
		/// <param name="subDatabase"></param>
		//---------------------------------------------------------------------
		public static OperationResult DeleteDatabaseObjects(SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			bool isAzureDB = (subDatabase.Provider == NameSolverStrings.SQLAzure);
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
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.GetErrorsStrings();

			return opRes;
		}

		/// <summary>
		/// Esegue il check della struttura di un SubscriptionDatabase (per il db di ERP ed il db del DMS)
		/// </summary>		
		/// <param name="subDatabase"></param>
		//---------------------------------------------------------------------
		public static OperationResult CheckDatabaseStructure(SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();
			DatabaseManager dbManager = CreateDatabaseManager();
			opRes.Result = dbManager.ConnectAndCheckDBStructure(subDatabase);
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			opRes.Content = GetMessagesList(dbManager.DBManagerDiagnostic);

			// TODO: dall'attivazione della Subscription devo sapere se provengo da una vecchia versione
			// forse questo controllo non sara' piu' necessario, dipende cosa verra' deciso a livello commerciale
			//if ((dbManager.StatusDB & DatabaseStatus.PRE_40) == DatabaseStatus.PRE_40)
			//if (!this.canMigrate) { }

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

			return opRes;
		}

		/// <summary>
		/// Esegue l'upgrade di un SubscriptionDatabase (per il db di ERP ed il db del DMS)
		/// Se specificata una configurazione importo anche i relativi dati di default
		/// </summary>
		/// <param name="configuration"></param>
		/// <param name="subDatabase"></param>
		//---------------------------------------------------------------------
		public static OperationResult UpgradeDatabaseStructure(string configuration, SubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			DatabaseManager dbManager = CreateDatabaseManager();
			opRes.Result = dbManager.ConnectAndCheckDBStructure(subDatabase);
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			if (!opRes.Result)
				return opRes;

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

			return opRes;
		}

		/// <summary>
		/// Metodo che si occupa della cancellazione dei contenitori dei database
		/// Se il provider e' Azure e' necessario effettuare una connessione con le credenziali
		/// di amministrazione
		/// </summary>
		/// <param name="deleteContent"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static OperationResult DeleteDatabase(DeleteDatabaseBodyContent deleteContent)
		{
			OperationResult opRes = new OperationResult();

			// qualcuno deve aver testato la connessione del db admin per Azure

			bool isAzureDB = (deleteContent.Database.Provider == NameSolverStrings.SQLAzure);

			DatabaseTask dTask = new DatabaseTask(isAzureDB);
			opRes.Result = dTask.DeleteDatabase(deleteContent);
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.GetErrorsStrings();
			return opRes;
		}

		//---------------------------------------------------------------------
		public static OperationResult CheckAndCreateDatabases(ExtendedSubscriptionDatabase subDatabase)
		{
			OperationResult opRes = new OperationResult();

			// I use master database to load information
			bool isAzureDB = (subDatabase.AdminCredentials.Provider == NameSolverStrings.SQLAzure);

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
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.GetErrorsStrings();

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
					// I create ERP database
					opRes.Result = dTask.CreateSQLDatabaseCompact(subDatabase.Database.DBName);
				}

				opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.GetErrorsStrings();
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
					// I create DMS database
					opRes.Result = dTask.CreateSQLDatabaseCompact(subDatabase.Database.DMSDBName);
				}

				opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.GetErrorsStrings();
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

			CultureInfo ci = new CultureInfo(subDatabase.Collation);

			subDatabase.Database.DatabaseCulture = ci.LCID.ToString(); //todo : lcid della collation
			opRes.Result = true;

			return opRes;
		}

		//---------------------------------------------------------------------
		public static OperationResult CheckAndCreateLogin(ExtendedSubscriptionDatabase subDatabase, bool isDMS = false)
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
			bool isAzureDB = (subDatabase.AdminCredentials.Provider == NameSolverStrings.SQLAzure);

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
				opRes.Message = dTask.Diagnostic.GetErrorsStrings();
				return opRes;
			}

			// se la login non esiste la creo, poi creo l'utente sul database con ruolo dbo
			if (!existLogin)
			{
				opRes.Result = dTask.CreateLogin(dbowner, password);

				if (opRes.Result)
					opRes.Result = dTask.CreateUser(dbowner, dbName);

				if (!opRes.Result)
					opRes.Message = dTask.Diagnostic.GetErrorsStrings();

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
					opRes.Message = dTask.Diagnostic.GetErrorsStrings();
			}

			return opRes;
		}

		//---------------------------------------------------------------------
		private static DBInfo LoadDBMarkInfo(string connectionString, string dbName)
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

		/// <summary>
		/// Crea un oggetto di tipo DatabaseManager per richiamare i controlli sui database
		/// </summary>
		//---------------------------------------------------------------------
		public static DatabaseManager CreateDatabaseManager()
		{
			//PathFinder pf = new PathFinder("USR-DELBENEMIC", "dev_next", "WebERP", "sa") { Edition = "Professional" };

			//@@TODO: per ora l'edition e' impostata come Professional (informazione che dovra' pervenire dal nuovo LM)
			// non e' chiaro se e quali altri tipi di edizione saranno previsti
			PathFinder.PathFinderInstance.Edition = "Professional";

			return new DatabaseManager(PathFinder.PathFinderInstance, new Diagnostic("ProvisioningDatabaseController"), (BrandLoader)InstallationData.BrandLoader, DBNetworkType.Large, "IT");
		}

		#region Metodi per caricamento configurazioni dati di default/esempio
		/// <summary>
		/// dato un tipo di configurazione (Default/Sample) restituisce un dictionary con 
		/// la lista delle possibili configurazioni suddivise per iso stato
		/// </summary>
		/// <param name="configType">tipo di configurazione (Default oppure Sample)</param>
		/// <param name="iso">iso stato</param>
		//---------------------------------------------------------------------------
		public static Dictionary<string, List<string>> GetConfigurationList(string configType, string iso)
		{
			//PathFinder pathFinder = new PathFinder("USR-DELBENEMIC", "dev_next", "WebERP", "sa") { Edition = "Professional" };

			//@@TODO: per ora l'edition e' impostata come Professional (informazione che dovra' pervenire dal nuovo LM)
			// non e' chiaro se e quali altri tipi di edizione saranno previsti
			PathFinder.PathFinderInstance.Edition = "Professional";

			List<string> isoConfigList = new List<string>();
			List<string> intlConfigList = new List<string>();

			foreach (string appName in GetApplications(PathFinder.PathFinderInstance))
			{
				foreach (ModuleInfo modInfo in PathFinder.PathFinderInstance.GetModulesList(appName))
				{
					// skippo il modulo TbOleDb per non considerare la TB_DBMark
					if (string.Compare(modInfo.Name, DatabaseLayerConsts.TbOleDbModuleName, StringComparison.OrdinalIgnoreCase) != 0)
					{
						AddConfiguration(PathFinder.PathFinderInstance, appName, modInfo.Name, ref isoConfigList, configType, iso);
						// mentre carico le configurazioni dell'isostato scelto, carico anche i dati internazionali
						AddConfiguration(PathFinder.PathFinderInstance, appName, modInfo.Name, ref intlConfigList, configType, "INTL");
					}
				}
			}

			Dictionary<string, List<string>> dictConfiguration = new Dictionary<string, List<string>>();
			dictConfiguration.Add(iso, isoConfigList);
			dictConfiguration.Add("INTL", intlConfigList); // aggiungo d'ufficio i dati INTL

			return dictConfiguration;
		}

		///<summary>
		/// Caricamento array applicazioni e moduli dal PathFinder
		///</summary>
		//---------------------------------------------------------------------
		private static List<string> GetApplications(PathFinder pf)
		{
			StringCollection applicationsList = new StringCollection();
			StringCollection supportList = new StringCollection();

			// prima guardo TaskBuilder
			pf.GetApplicationsList(ApplicationType.TaskBuilder, out supportList);
			foreach (string appName in supportList)
				applicationsList.Add(appName);

			// poi guardo le TaskBuilderApplications
			pf.GetApplicationsList(ApplicationType.TaskBuilderApplication, out supportList);
			for (int i = 0; i < supportList.Count; i++)
				applicationsList.Add(supportList[i]);

			return applicationsList.Cast<String>().ToList();
		}

		//---------------------------------------------------------------------------
		private static void AddConfiguration(PathFinder pf, string appName, string modName, ref List<string> configList, string configType, string iso)
		{
			string standardDir = string.Compare(configType, NameSolverStrings.Default, StringComparison.InvariantCultureIgnoreCase) == 0
				? pf.GetStandardDataManagerDefaultPath(appName, modName, iso)
				: pf.GetStandardDataManagerSamplePath(appName, modName, iso);

            // per ora ignoro la Custom (da capire eventualmente da dove verra' caricata)
           /* string customDir = (configType.CompareTo(NameSolverStrings.Default) == 0)
				? pf.GetCustomDataManagerDefaultPath(appName, modName, iso)
				: pf.GetCustomDataManagerSamplePath(appName, modName, iso);*/

            StringCollection tempList = new StringCollection();

			/*if (pf.ExistPath(customDir))
				foreach (TBDirectoryInfo dir in pf.GetSubFolders(customDir))
					tempList.Add(dir.direcotryInfo.Name);*/

			if (pf.ExistPath(standardDir))
				foreach (TBDirectoryInfo dir in pf.GetSubFolders(standardDir))
					if (!tempList.Contains(dir.direcotryInfo.Name))
						tempList.Add(dir.direcotryInfo.Name);

			foreach (string dirName in tempList)
				if (!configList.Contains(dirName))
					configList.Add(dirName);
		}
		#endregion

		//---------------------------------------------------------------------
		public static List<OperationResult> GetMessagesList(Diagnostic diagnostic)
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

		/// <summary>
		/// Metodo per eseguire il check preventivo dei vari dati inseriti dall'utente per salvare il database
		/// </summary>
		/// <param name="extSubDatabase"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static OperationResult QuickCreateSubscriptionDatabase(ExtendedSubscriptionDatabase extSubDatabase)
		{
			OperationResult opRes = new OperationResult();

			//************************************************************************************
			// aggiungere controlli di esistenza database - login - connessioni al server, etc.
			//************************************************************************************

			OperationResult existDbRes = ExistDatabase(extSubDatabase.Database.DBName, extSubDatabase.AdminCredentials);

			OperationResult existDmsDbRes = ExistDatabase(extSubDatabase.Database.DMSDBName, extSubDatabase.AdminCredentials);

			if (existDbRes.Result || existDmsDbRes.Result)
			{
				opRes.Result = false;
				string message = string.Empty;
				if (existDbRes.Result)
					message = string.Format(DatabaseManagerStrings.WarningDBAlreadyExists, extSubDatabase.Database.DBName, extSubDatabase.Database.DBServer);
				if (existDmsDbRes.Result)
					message += "\r\n" + string.Format(DatabaseManagerStrings.WarningDBAlreadyExists, extSubDatabase.Database.DMSDBName, extSubDatabase.Database.DBServer);
				opRes.Message = message;
				return opRes;
			}

			// istanzio la classe che incorpora i vari metodi
			DatabaseTask dTask = new DatabaseTask(extSubDatabase.AdminCredentials.Provider == NameSolverStrings.SQLAzure);
			// I need to connect to master
			dTask.CurrentStringConnection =
				string.Format
				(
				NameSolverDatabaseStrings.SQLConnection,
				extSubDatabase.AdminCredentials.Server,
				DatabaseLayerConsts.MasterDatabase,
				extSubDatabase.AdminCredentials.Login,
				extSubDatabase.AdminCredentials.Password
				);

			// creo la login dbowner
			opRes.Result = dTask.CreateLogin(extSubDatabase.Database.DBOwner, extSubDatabase.Database.DBPassword);
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.GetErrorsStrings();

			if (!opRes.Result)
				return opRes;

			// impostazione parametri creazione contenitore db su Azure
			// queste impostazioni dovranno essere definite a livello di subscription
			AzureCreateDBParameters param = new AzureCreateDBParameters();
			param.DatabaseName = extSubDatabase.Database.DBName;
			param.MaxSize = AzureMaxSize.MB100;

			// impostazione parametri creazione contenitore db su SqlServer
			SQLCreateDBParameters sqlParam = new SQLCreateDBParameters();
			sqlParam.DatabaseName = extSubDatabase.Database.DBName;

			// I create ERP database
			opRes.Result = dTask.CreateAzureDatabase(param); //dTask.CreateSQLDatabase(sqlParam); 
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.GetErrorsStrings();

			if (!opRes.Result)
				return opRes;

			// I create DMS database
			param.DatabaseName = extSubDatabase.Database.DMSDBName;
			sqlParam.DatabaseName = extSubDatabase.Database.DMSDBName;

			opRes.Result = dTask.CreateAzureDatabase(param); //dTask.CreateSQLDatabase(sqlParam); 
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.GetErrorsStrings();

			if (!opRes.Result)
				return opRes;

			// associo la login appena creata al database di ERP con il ruolo di db_owner
			opRes.Result = dTask.CreateUser(extSubDatabase.Database.DBOwner, extSubDatabase.Database.DBName);
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.GetErrorsStrings();

			if (!opRes.Result)
				return opRes;

			// associo la login appena creata al database DMS con il ruolo di db_owner
			opRes.Result = dTask.CreateUser(extSubDatabase.Database.DBOwner, extSubDatabase.Database.DMSDBName);
			opRes.Message = opRes.Result ? Strings.OperationOK : dTask.Diagnostic.GetErrorsStrings();

			if (!opRes.Result)
				return opRes;

			Debug.WriteLine("MP-LOG - ERP DB Name: " + extSubDatabase.Database.DBName);
			Debug.WriteLine("MP-LOG - DMS DB Name: " + extSubDatabase.Database.DMSDBName);
			Debug.WriteLine("MP-LOG - Login (dbo) Name: " + extSubDatabase.Database.DBOwner);
			Debug.WriteLine("MP-LOG - Password (dbo): " + extSubDatabase.Database.DBPassword);

			DatabaseManager dbManager = CreateDatabaseManager();
			opRes.Result = dbManager.ConnectAndCheckDBStructure(extSubDatabase.Database);
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();
			if (!opRes.Result)
				return opRes;

			dbManager.ImportDefaultData = false;
			dbManager.ImportSampleData = false;
			opRes.Result = dbManager.DatabaseManagement(false) && !dbManager.ErrorInRunSqlScript; // passo il parametro cosi' salvo il log
			opRes.Message = opRes.Result ? Strings.OperationOK : dbManager.DBManagerDiagnostic.ToString();

			opRes.Content = extSubDatabase;
			return opRes;
		}
	}
}
