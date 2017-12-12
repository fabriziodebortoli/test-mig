using System;
using System.IO;
using System.Reflection;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.SerializableTypes;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

using Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator.Properties;

namespace Microarea.Tools.ProvisioningConfigurator.ProvisioningConfigurator
{
	//---------------------------------------------------------------------
	public class ProvisioningEngine
	{
		private ProvisioningData provisioningData = null;
		private PathFinder pathFinder = null;
		private DatabaseEngine dbEngine = null;

		public static string LogFilePath { get { return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ProvisioningLog.txt"); } }

		//---------------------------------------------------------------------
		public DatabaseEngine DatabaseEngine { get { return dbEngine; } }

		// eventi
		//---------------------------------------------------------------------
		public event EventHandler<DBEngineEventArgs> GenerationEvent;
		public event EventHandler<DBEngineEventArgs> GenerationMainEvent;

		//---------------------------------------------------------------------
		public ProvisioningEngine(ProvisioningData pData)
		{
			provisioningData = pData;

			// istanzio un PathFinder e gli assegno l'edizione Professional d'ufficio (anche se serviva solo per i dati di default)
			pathFinder = new PathFinder(NameSolverStrings.AllCompanies, NameSolverStrings.AllUsers);
			pathFinder.Edition = NameSolverStrings.ProfessionalEdition;

			// istanzio la classe che si occupa di creare in cascata tutto cio' che serve
			// passo sempre la DBNetworkType.Large cosi 
			dbEngine = new DatabaseEngine(pathFinder, DBNetworkType.Large, provisioningData.IsoCountry, InstallationData.BrandLoader, forProvisioningEnv: true);

			// aggancio gli eventi
			dbEngine.GenerationEvent += DbEngine_GenerationEvent;
			dbEngine.GenerationMainEvent += DbEngine_GenerationMainEvent;

			// inizializzazione proprieta' esposte
			dbEngine.SystemDbName = provisioningData.SystemDbName;
			dbEngine.CompanyDbName = provisioningData.CompanyDbName;
			dbEngine.DMSDbName = provisioningData.DMSDbName;
			dbEngine.CompanyName = provisioningData.CompanyName;
			// N.B. il caricamento dei dati di default + esempio e' sempre a false (impostato nel codice)
			
			// credenziali amministratore
			dbEngine.Server = provisioningData.Server;
			dbEngine.User = provisioningData.User;
			dbEngine.Password = provisioningData.Password;
			dbEngine.AdminLoginName = provisioningData.AdminLoginName;
			dbEngine.AdminLoginPassword = provisioningData.AdminLoginPassword;
			dbEngine.UserLoginName = provisioningData.UserLoginName;
			dbEngine.UserLoginPassword = provisioningData.UserLoginPassword;
		}

		//---------------------------------------------------------------------
		private void DbEngine_GenerationMainEvent(object sender, DBEngineEventArgs e)
		{
			// nuova sintassi C# 6.0 utilizzando l'operatore null-condizionale 
			// che consente di evitare di scrivere la seguente doppia istruzione:
			// if (GenerationMainEvent != null) GenerationMainEvent(sender, e);
			GenerationMainEvent?.Invoke(sender, e);
		}

		//---------------------------------------------------------------------
		private void DbEngine_GenerationEvent(object sender, DBEngineEventArgs e)
		{
			// nuova sintassi C# 6.0 utilizzando l'operatore null-condizionale 
			// che consente di evitare di scrivere la seguente doppia istruzione:
			// if (GenerationEvent != null) GenerationEvent(sender, e);
			GenerationEvent?.Invoke(sender, e);
		}

		//---------------------------------------------------------------------
		public bool ConfigureProvisioningEnvironment()
		{
			// lancio l'esecuzione
			// differenziando se sto configurando da zero o se sto aggiungendo un'azienda ad una configurazione esistente
			bool result =	provisioningData.AddCompanyMode 
							? dbEngine.ConfigureProvisioningEnvironmentInAddCompanyModeLITE() 
							: dbEngine.ConfigureProvisioningEnvironmentLITE();

			if (dbEngine.DbEngineDiagnostic.Error)
			{
				using (StreamWriter sw = new StreamWriter(LogFilePath, true))
				{
					sw.WriteLine("//-------------------------------------------");
					sw.WriteLine("{0} {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
					sw.WriteLine(Resources.ConfigurationEndedWithErrors);
					sw.WriteLine("\r\n");
					sw.WriteLine(provisioningData.ToString());
					sw.WriteLine(dbEngine.DbEngineDiagnostic.GetErrorsStrings());
					sw.WriteLine("//-------------------------------------------");
				}
			}
			else
				using (StreamWriter sw = new StreamWriter(LogFilePath, true))
				{
					sw.WriteLine("//-------------------------------------------");
					sw.WriteLine("{0} {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
					sw.WriteLine(Resources.ConfigurationSuccessfullyCompleted);
					sw.WriteLine("\r\n");
					sw.WriteLine(provisioningData.ToString());
					sw.WriteLine("//-------------------------------------------");
				}

			if (result) 
				SaveProvisioningDataFile();

			return result;
		}

		//---------------------------------------------------------------------
		public void SaveProvisioningDataFile()
		{
			// devo reinilizializzare LoginManager per ricaricare correttamente le info
			ReinitLoginManager();

			SaveLoggedUser();
			provisioningData.Save(); // serializzo le info nel file
		}

		/// <summary>
		/// Per reinizializzare LM togliamo il readonly al web.config, lo rinominiamo
		/// e rimettiamo tutto a posto
		/// </summary>
		//---------------------------------------------------------------------
		private void ReinitLoginManager()
		{
			string lmWebConfigPath = Path.Combine(pathFinder.LoginManagerPath, "web.config");

			try
			{
				FileInfo fileInfo = new FileInfo(lmWebConfigPath);
				bool wasRO = false;
				if (
					fileInfo.Exists &&
					((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
					)
				{
					fileInfo.Attributes -= FileAttributes.ReadOnly;
					wasRO = true;
				}

				File.Move(lmWebConfigPath, lmWebConfigPath + "x");
				File.Move(lmWebConfigPath + "x", lmWebConfigPath);

				if (wasRO)
					File.SetAttributes(lmWebConfigPath, File.GetAttributes(lmWebConfigPath) | FileAttributes.ReadOnly);
			}
			catch (Exception e)
			{
				using (StreamWriter sw = new StreamWriter(LogFilePath, true))
				{
					sw.WriteLine("//-------------------------------------------");
					sw.WriteLine("{0} {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
					sw.WriteLine(string.Format(Resources.UnableToReinitLM, e.Message));
					sw.WriteLine("\r\n");
					sw.WriteLine("//-------------------------------------------");
				}
			}
		}

		//---------------------------------------------------------------------
		private void SaveLoggedUser()
		{
			//salvo le informazioni dell'utente nel file appdata.xml di tbappmanager così quando parte mago è già impostato il basic user
			LoggedUser loggedUser = new LoggedUser(provisioningData.UserLoginName, provisioningData.CompanyName);
			loggedUser.Remember = false; //volendo potremmo decidere di impostare Flag autologin in mago
			loggedUser.SaveForMago();
		}
	}
}
