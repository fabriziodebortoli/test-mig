using System.Collections.Generic;
using System.Collections.Specialized;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.TaskBuilderNet.Data.DataManagerEngine
{
	/// <summary>
	/// BaseImportExportManager 
	/// (entry-point per gestire il caricamento dei dati di default in modalita' silente, senza i wizard)
	/// </summary>
	//=========================================================================
	public class BaseImportExportManager
	{
		private DefaultManager defManager = null;
		private SampleManager sampleManager = null;

		// per richiamare la classe delle funzioni e datamember comuni
		protected ContextInfo contextInfo = null;
		protected BrandLoader brandLoader = null;

		public DatabaseDiagnostic DBDiagnostic = new DatabaseDiagnostic();

		/// <summary>
		/// caricamento dei dati di default silente...
		/// </summary>
		//---------------------------------------------------------------------
		public BaseImportExportManager(ContextInfo context, BrandLoader brand)
		{
			contextInfo = context;
			brandLoader = brand;
		}

		# region Funzioni per il caricamento dei dati di default delle tabelle
		/// <summary>
		/// Caricamento dell'elenco delle configurazioni disponibili per i dati di default
		/// </summary>
		//---------------------------------------------------------------------------
		public void GetDefaultConfigurationList(ref StringCollection defaultConf)
		{
			DefaultSelections defSelection = new DefaultSelections(contextInfo, brandLoader);
			defSelection.GetDefaultConfigurationList(ref defaultConf);
		}

		/// <summary>
		/// Impostazione della configurazione da caricare per i dati di default
		/// </summary>
		//---------------------------------------------------------------------------
		public void SetDefaultDataConfiguration(string configuration)
		{
			if (defManager == null)
				defManager = new DefaultManager(contextInfo, DBDiagnostic, brandLoader);
			defManager.SelectedConfiguration = configuration;
		}
		# endregion

		# region Funzione per l'importazione dei dati di default silente
		//---------------------------------------------------------------------------
		public bool ImportDefaultDataSilentMode()
		{
			if (defManager != null)
				return defManager.ImportDefaultDataSilentMode();

			return false;
		}

		/// <summary>
		/// Sto creando tutte le tabelle di un modulo (carico tutti i dati di default di quel modulo)
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddDefaultDataTable(string appName, string moduleName)
		{
			if (defManager == null)
				defManager = new DefaultManager(contextInfo, DBDiagnostic, brandLoader);

			// é l'import manager che controlla l'esistenza del\dei file di dati di default
			// associati ai moduli. Se esistono l'inserisce nella lista dei file da importare altrimenti no
			defManager.AddDefaultDataTable(appName, moduleName);
		}

		//---------------------------------------------------------------------------
		public bool ImportAppendDefaultData(List<string> missingTablesListForAppend)
		{
			if (defManager != null)
				return defManager.ImportAppendDefaultData(missingTablesListForAppend);

			return false;
		}

		/// <summary>
		/// carica i dati di default di una specifica tabella (gestione tabelle mancanti)
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddDefaultDataTable(string tableName, string appName, string moduleName)
		{
			if (defManager == null)
				defManager = new DefaultManager(contextInfo, DBDiagnostic, brandLoader);

			// é l'import manager che controlla l'esistenza del\dei file di dati di default
			// associati alla tabella. Se esiste l'inserisce nella lista dei file da importare altrimenti no
			defManager.AddDefaultDataTable(tableName, appName, moduleName);
		}

		/// <summary>
		/// carica i dati di default di Append di una specifica tabella (gestione tabelle mancanti)
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddAppendDefaultDataTable(string appName, string moduleName)
		{
			if (defManager == null)
				defManager = new DefaultManager(contextInfo, DBDiagnostic, brandLoader);

			// é l'import manager che controlla l'esistenza del\dei file di dati di default
			// associati alla tabella. Se esiste l'inserisce nella lista dei file da importare altrimenti no
			defManager.AddAppendDefaultDataTable(appName, moduleName);
		}
		# endregion

		# region Funzioni per l'importazione dei dati di esempio silente
		/// <summary>
		/// Impostazione della configurazione da caricare per i dati di esempio
		/// </summary>
		//---------------------------------------------------------------------------
		public void SetSampleDataConfiguration(string configuration, string isoState)
		{
			if (sampleManager == null)
				sampleManager = new SampleManager(contextInfo, DBDiagnostic, brandLoader);
			sampleManager.SelectedConfiguration = configuration;
			sampleManager.SelectedIsoState = isoState;
		}

		//---------------------------------------------------------------------------
		public bool ImportSampleDataSilentMode()
		{
			if (sampleManager != null)
				sampleManager.ImportSampleDataSilentMode();
			
			return false;
		}

		/// <summary>
		/// Sto creando tutte le tabelle di un modulo (carico tutti i dati di default di quel modulo)
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddSampleDataTable(string appName, string moduleName)
		{
			if (sampleManager == null)
				sampleManager = new SampleManager(contextInfo, DBDiagnostic, brandLoader);
			
			// é l'import manager che controlla l'esistenza del\dei file di dati di default
			// associati ai moduli. Se esistono l'inserisce nella lista dei file da importare altrimenti no
			sampleManager.AddSampleDataTable(appName, moduleName);
		}

		//---------------------------------------------------------------------------
		public bool ImportAppendSampleData(List<string> missingTablesListForAppend)
		{
			if (sampleManager != null)
				return sampleManager.ImportAppendSampleData(missingTablesListForAppend);

			return false;
		}

		/// <summary>
		/// carica i dati di default di una specifica tabella (gestione tabelle mancanti)
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddSampleDataTable(string tableName, string appName, string moduleName)
		{
			if (sampleManager == null)
				sampleManager = new SampleManager(contextInfo, DBDiagnostic, brandLoader);

			// é l'import manager che controlla l'esistenza del\dei file di dati di default
			// associati alla tabella. Se esiste l'inserisce nella lista dei file da importare altrimenti no
			sampleManager.AddSampleDataTable(tableName, appName, moduleName);
		}

		/// <summary>
		/// carica i dati di default di Append di una specifica tabella (gestione tabelle mancanti)
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddAppendSampleDataTable(string appName, string moduleName)
		{
			if (sampleManager == null)
				sampleManager = new SampleManager(contextInfo, DBDiagnostic, brandLoader);

			// é l'import manager che controlla l'esistenza del\dei file di dati di default
			// associati alla tabella. Se esiste l'inserisce nella lista dei file da importare altrimenti no
			sampleManager.AddAppendSampleDataTable(appName, moduleName);
		}
		# endregion
	}
}