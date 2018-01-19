using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;

using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DataManagerEngine
{
	/// <summary>
	/// DataManagerSelections
	/// </summary>
	//=========================================================================
	public class DataManagerSelections
	{
		private ContextInfo contextInfo;
		private CatalogInfo catalog;
		private ApplicationDBStructureInfo appDBStructInfo;
		private BrandLoader brandLoader;
		private ConfigurationInfo configInfo;
		protected StringCollection applicationList = new StringCollection();

		//--------------------------------------------------------------------
		public ContextInfo ContextInfo { get { return contextInfo; } }
		//--------------------------------------------------------------------------------
		public CatalogInfo Catalog { get { return catalog; } set { catalog = value; } }
		//--------------------------------------------------------------------------------
		public ApplicationDBStructureInfo AppDBStructInfo { get { return appDBStructInfo; } }
		//--------------------------------------------------------------------------------
		public BrandLoader Brand { get { return brandLoader; } }
		//--------------------------------------------------------------------------------
		public ConfigurationInfo ConfigInfo { get { return configInfo; } set { configInfo = value; } }

		//--------------------------------------------------------------------
		public DataManagerSelections(ContextInfo context, BrandLoader brand)
		{
			contextInfo = context;
			brandLoader = brand;
		}

		//--------------------------------------------------------------------
		public DataManagerSelections(ContextInfo context, CatalogInfo catalogInfo, BrandLoader brand)
		{
			contextInfo = context;
			catalog = catalogInfo;
			brandLoader = brand;
		}

		///<summary>
		/// Caricamento array applicazioni e moduli dal PathFinder
		///</summary>
		//---------------------------------------------------------------------
		protected void GetAllApplication()
		{
			applicationList.Clear();
			StringCollection supportList = new StringCollection();

			// prima guardo TaskBuilder
			// Non devo esportare/importare le tabelle quali la DBMark per dati defaut/sample
			contextInfo.PathFinder.GetApplicationsList(ApplicationType.TaskBuilder, out supportList);

			foreach (string appName in supportList)
					applicationList.Add(appName);

			// poi guardo le TaskBuilderApplications
			contextInfo.PathFinder.GetApplicationsList(ApplicationType.TaskBuilderApplication, out supportList);
			for (int i = 0; i < supportList.Count; i++)
				applicationList.Add(supportList[i]);

			// per caricare le customizzazioni realizzate in EasyStudio
			contextInfo.PathFinder.GetApplicationsList(ApplicationType.Customization, out supportList);
			for (int i = 0; i < supportList.Count; i++)
				applicationList.Add(supportList[i]);
		}

		#region Caricamento tabelle suddivise per applicazione/modulo
		/// <summary>
		/// carico la lista delle tabelle suddivise per application/modulo e elimino 
		/// eventualmente le tabelle non presenti nel database
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadModuleTableInfo(bool reload)
		{
			if (reload)
				appDBStructInfo = new ApplicationDBStructureInfo(contextInfo.PathFinder, brandLoader);
			else
				if (appDBStructInfo != null)
					return; //ho giá caricato le info

			GetAllApplication();

			appDBStructInfo = new ApplicationDBStructureInfo(contextInfo.PathFinder, brandLoader);

			// permette di inserire per ogni catalog entry l'informazione
			// dell'applicazione/modulo di appartenenza
			appDBStructInfo.AddModuleInfoToCatalog(applicationList, ref catalog);
		}
		# endregion

		#region Gestione configurazioni per i dati di default e per i dati di esempio
		/// <summary>
		/// dato un tipo di configurazione (Default/Sample) restituisce la lista delle possibili configurazioni
		/// </summary>
		/// <param name="configList">lista dei nomi delle configurazioni</param>
		/// <param name="configType">tipo di configurazione (Default oppure Sample)</param>
		/// <param name="language">iso stato</param>
		//---------------------------------------------------------------------------
		public void GetConfigurationList(ref StringCollection configList, string configType, string language)
		{
			GetAllApplication();

			ICollection moduleList = new ArrayList();
			foreach (string appName in applicationList)
			{
				moduleList = contextInfo.PathFinder.GetModulesList(appName);

				foreach (ModuleInfo modInfo in moduleList)
				{
					// skippo il modulo TbOleDb per non considerare la TB_DBMark
					if (string.Compare(modInfo.Name, DatabaseLayerConsts.TbOleDbModuleName, StringComparison.InvariantCultureIgnoreCase) != 0)
						AddConfiguration(appName, modInfo.Name, ref configList, configType, language);
				}
			}
		}

		//---------------------------------------------------------------------------
		private void AddConfiguration(string appName, string modName, ref StringCollection configList, string configType, string language)
		{
			DirectoryInfo standardDir = new DirectoryInfo(
				(configType.CompareTo(NameSolverStrings.Default) == 0)
				? contextInfo.PathFinder.GetStandardDataManagerDefaultPath(appName, modName, language)
				: contextInfo.PathFinder.GetStandardDataManagerSamplePath(appName, modName, language));

			DirectoryInfo customDir = new DirectoryInfo(
				(configType.CompareTo(NameSolverStrings.Default) == 0)
				? contextInfo.PathFinder.GetCustomDataManagerDefaultPath(appName, modName, language)
				: contextInfo.PathFinder.GetCustomDataManagerSamplePath(appName, modName, language));

			StringCollection tempList = new StringCollection();

			if (customDir.Exists)
			{
				foreach (DirectoryInfo dir in customDir.GetDirectories())
					tempList.Add(dir.Name);
			}

			string tempName = string.Empty;

			if (standardDir.Exists)
			{
				foreach (DirectoryInfo dir in standardDir.GetDirectories())
				{
					if (!tempList.Contains(dir.Name))
						tempList.Add(dir.Name);
				}
			}

			foreach (string dirName in tempList)
				if (!configList.Contains(dirName))
					configList.Add(dirName);
		}
		#endregion

		# region Creazione file di log
		///<summary>
		/// Metodo generico per la creazione del file di log
		///</summary>
		//--------------------------------------------------------------------------------
		public string CreateLogFile(DatabaseDiagnostic dbDiagnostic)
		{
			//creo un file di log in xml contenente le informazioni delle aziende e degli errori
			string path = contextInfo.PathFinder.GetCustomCompanyLogPath
				(
					contextInfo.CompanyName,
					NameSolverStrings.AllUsers
				);

			if (string.IsNullOrEmpty(path))
				path = contextInfo.PathFinder.GetCustomPath();
			else
			{
				try
				{
					if (!Directory.Exists(path))
						Directory.CreateDirectory(path);
					// se incontro problemi di accesso per la creazione della cartella creo il file di log
					// nella Custom, in modo da non perdere le informazioni
				}
				catch (IOException)
				{
					path = contextInfo.PathFinder.GetCustomPath();
				}
				catch (UnauthorizedAccessException)
				{
					path = contextInfo.PathFinder.GetCustomPath();
				}
				catch (Exception)
				{
					path = contextInfo.PathFinder.GetCustomPath();
				}
			}

			string fileName = string.Format("{0}-{1}-{2}.xml", contextInfo.CompanyName, GetOperationType(), DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss"));
			string filePath = Path.Combine(path, fileName);

			DiagnosticView diagnosticView = new DiagnosticView(dbDiagnostic.Diagnostic);
			diagnosticView.FillListOfMessages(OrderType.Date, DiagnosticType.Error | DiagnosticType.Warning | DiagnosticType.Information);
			diagnosticView.WriteXmlFile(filePath, LogType.ImportExport);
			diagnosticView.Close();

			return filePath;
		}

		///<summary>
		/// Metodo da reimplementare nei figli in modo da pilotare la composizione del nome del file di log
		/// (di default mette un meno)
		///</summary>
		//--------------------------------------------------------------------------------
		protected virtual string GetOperationType()
		{
			return string.Empty;
		}
		# endregion
	}
}
