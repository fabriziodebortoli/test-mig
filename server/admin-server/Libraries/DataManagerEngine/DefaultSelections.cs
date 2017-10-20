using System.Collections.Specialized;
using Microarea.Common.NameSolver;
using Microarea.AdminServer.Libraries.DatabaseManager;
using TaskBuilderNetCore.Interfaces;
using System.Collections;
using System.IO;
using System;
using System.Data;

namespace Microarea.AdminServer.Libraries.DataManagerEngine
{
	/// <summary>
	/// DefaultSelections (struttura in memoria con le opzioni scelte dall'utente)
	/// </summary>
	//=========================================================================
	public class DefaultSelections : DataManagerSelections
	{
		# region Variables
		public ExportSelections ExportSel;
		public ImportSelections ImportSel;
		
		public enum ModeType {IMPORT, EXPORT};
		public ModeType mode = ModeType.IMPORT; // se genero o carico i dati
		
		public string	SelectedIsoState; // Iso Stato prescelta
		public string	SelectedConfiguration = DataManagerConsts.Basic; // Configurazione prescelta di default 
		
		public bool		ForceSelections = false; // per forzare il ri-caricamento delle selezioni

		private BrandLoader brandLoader = null;
		# endregion

		# region Properties
		//---------------------------------------------------------------------
		public ModeType Mode 
		{
			get { return mode; }
			set
			{
				mode = value;
				// se l'utente ha deciso di effettuare la generazione dei dati 
				// di default (sono in modalit� di export)
				if (mode == ModeType.EXPORT)
				{
					if (ExportSel == null || ForceSelections)
						ExportSel = new ExportSelections(ContextInfo, Catalog, brandLoader);
				}
				else	
				{
					if (ImportSel == null)
					{
						ImportSel = new ImportSelections(ContextInfo, Catalog, brandLoader);
						ImportSel.UpdateExistRow = ImportSelections.UpdateExistRowType.UPDATE_ROW;
					}
				}								
			}
		}
		# endregion

		# region Costruttore
		/// <summary>
		/// costruttore
		/// </summary>
		//---------------------------------------------------------------------
		public DefaultSelections(ContextInfo context, BrandLoader brandLoader) 
			: base(context, brandLoader)
		{
			SelectedIsoState	= ContextInfo.IsoState;
			this.brandLoader	= brandLoader;

			// istanzio una connessione solo per il caricamento del catalog
			using (TBConnection tbConnection = new TBConnection(context.ConnectAzDB, DBMSType.SQLSERVER))
			{
				tbConnection.Open();
				Catalog = new CatalogInfo();
				Catalog.Load(tbConnection, true);
			}
		}
		# endregion

		# region Ricerca delle varie configurazioni	
		//---------------------------------------------------------------------------
		public void GetDefaultConfigurationList(ref StringCollection defaultConfigList)
		{
			GetDefaultConfigurationList(ref defaultConfigList, this.SelectedIsoState);
		}		

		//---------------------------------------------------------------------------
		public void GetDefaultConfigurationList(ref StringCollection defaultConfigList, string countryName)
		{
			GetConfigurationList(ref defaultConfigList, NameSolverStrings.Default, countryName);
			// se non esiste la configurazione DefaultConfiguration e sono in fase di generazione dei
			// dati, inserisco nella combo il nome
			if (mode == ModeType.EXPORT && !defaultConfigList.Contains(SelectedConfiguration))
				defaultConfigList.Add(SelectedConfiguration);
		}
		#endregion

		//---------------------------------------------------------------------------
		public void LoadDefaultData()
		{
			if (string.IsNullOrWhiteSpace(SelectedConfiguration) || string.IsNullOrWhiteSpace(SelectedIsoState))
				return;

			ArrayList moduleList = null;
			ArrayList fileList = new ArrayList();
			GetAllApplication();

			foreach (string appName in applicationList)
			{
				moduleList = new ArrayList(ContextInfo.PathFinder.GetModulesList(appName));

				foreach (Common.NameSolver.ModuleInfo modInfo in moduleList)
					AddDefaultFiles(appName, modInfo.Name, ref fileList);
			}

			StringCollection appendFiles = new StringCollection();
			foreach (FileInfo file in fileList)
			{
				// skippo i file con un nome che inizia con DeploymentManifest (visto che potrebbe essere
				// presente per esigenze di installazione di specifiche configurazioni dati di default/esempio 
				// (esigenza sorta principalmente con i partner polacchi - miglioria 3067)
				if (file.Name.StartsWith("DeploymentManifest", StringComparison.OrdinalIgnoreCase))
					continue;

				// devo inserire in coda quelli con suffisso Append
				if (Path.GetFileNameWithoutExtension(file.Name).EndsWith(DataManagerConsts.Append, StringComparison.OrdinalIgnoreCase))
					appendFiles.Add(file.FullName);
				else
					ImportSel.AddItemInImportList(file.FullName);
			}

			//inserisco quelli con suffisso Append
			foreach (string fileName in appendFiles)
				ImportSel.AddItemInImportList(fileName);
		}

		//---------------------------------------------------------------------------
		public void AddDefaultFiles(string appName, string moduleName, ref ArrayList fileList)
		{
			DirectoryInfo standardDir = new DirectoryInfo(Path.Combine(ContextInfo.PathFinder.GetStandardDataManagerDefaultPath(appName, moduleName, SelectedIsoState), SelectedConfiguration));
			DirectoryInfo customDir = new DirectoryInfo(Path.Combine(ContextInfo.PathFinder.GetCustomDataManagerDefaultPath(appName, moduleName, SelectedIsoState), SelectedConfiguration));
			ContextInfo.PathFinder.GetXMLFilesInPath(standardDir, customDir, ref fileList);
		}

		/// <summary>
		/// pre-carica le selezioni del wizard con i dati contenuti nel file di configurazione indicato dall'utente
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadSelectionsFromConfigurationFile()
		{
			if (ExportSel == null || ExportSel.ConfigInfo == null) 
				return;

			ExportSel.AllTables			= ExportSel.ConfigInfo.AllTables;
			ExportSel.SelectColumns		= !ExportSel.ConfigInfo.AllColumns;
			ExportSel.WriteQuery		= ExportSel.ConfigInfo.WhereClause;
			
			ExportSel.ExportTBCreated	= ExportSel.ConfigInfo.ColTBCreated;
			ExportSel.ExportTBModified	= ExportSel.ConfigInfo.ColTBModified;

			ExportSel.ExecuteScriptTextBeforeExport = ExportSel.ConfigInfo.ExecuteScript;
			ExportSel.ScriptTextBeforeExport		= ExportSel.ConfigInfo.Script;
		}
	}	
}
