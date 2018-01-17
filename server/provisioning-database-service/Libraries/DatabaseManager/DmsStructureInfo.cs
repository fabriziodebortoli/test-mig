using System.Collections.Generic;
using System.Collections.Specialized;
using Microarea.Common.DiagnosticManager;
using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Interfaces;
using Microarea.ProvisioningDatabase.Libraries.DataManagerEngine;

namespace Microarea.ProvisioningDatabase.Libraries.DatabaseManager
{
	///<summary>
	/// Classe separata che si occupa di caricare le applicazioni specifiche per la gestione
	/// del database documentale.
	/// Esse sono quelle che contengono moduli il cui DatabaseObjects.xml contiene l'attributo dms="true".
	/// D'ufficio viene cmq messa anche il modulo TbOleDb, per avere in automatico anche la tabella TB_DBMark e 
	/// la relativa gestione degli aggiornamenti db.
	///</summary>
	//================================================================================
	public class DmsStructureInfo
	{
		private StringCollection applicationsList = null;
		private CheckDBStructureInfo checkDbStructInfo = null; // x il check della struttura del db

		private ApplicationDBStructureInfo appStructInfo = null; // x fare la load dei file xml di descr. database

		private BrandLoader brandLoader;
		private Diagnostic dbManagerDiagnostic;
		private BaseImportExportManager importExportManager;

		private ContextInfo contextInfo = null;
		private List<AddOnApplicationDBInfo> addOnApplicationList = null;

		//--------------------------------------------------------------------------------
		public List<AddOnApplicationDBInfo> DmsAddOnAppList { get { return addOnApplicationList; } }
		public CheckDBStructureInfo DmsCheckDbStructInfo { get { return checkDbStructInfo; } }

		///<summary>
		/// Costruttore
		///</summary>
		//--------------------------------------------------------------------------------
		public DmsStructureInfo
			(
			ContextInfo contextInfo, 
			StringCollection applicationsList, 
			BrandLoader aBrandLoader,
			Diagnostic dbManagerDiagnostic, 
			BaseImportExportManager importExportManager
			)
		{
			this.contextInfo = contextInfo;
			this.applicationsList = applicationsList;
			this.brandLoader = aBrandLoader;
			this.dbManagerDiagnostic = dbManagerDiagnostic;
			this.importExportManager = importExportManager;
		}

		///<summary>
		/// Metodo che carica le informazioni dei moduli e la situazione del database
		///</summary>
		//--------------------------------------------------------------------------------
		public void Load()
		{
			appStructInfo = new ApplicationDBStructureInfo(contextInfo.PathFinder, brandLoader, KindOfDatabase.Dms);
			appStructInfo.ReadDatabaseObjectsFiles(applicationsList, true); // N.B.: il 2° param è x caricare anche le AddColumns
			addOnApplicationList = appStructInfo.ApplicationDBInfoList;

			// richiamo la classe che si occupa di stabilire lo stato del database
			checkDbStructInfo = new CheckDBStructureInfo(KindOfDatabase.Dms, contextInfo, appStructInfo, ref dbManagerDiagnostic);
			checkDbStructInfo.OnAddDefaultDataMissingTable += new CheckDBStructureInfo.AddDefaultDataMissingTable(OnAddDefaultDataMissingTable);
			checkDbStructInfo.OnAddSampleDataMissingTable += new CheckDBStructureInfo.AddSampleDataMissingTable(OnAddSampleDataMissingTable);
			checkDbStructInfo.GetDatabaseStatus();
		}

		# region Evento per la gestione dell'ImportExport dal CheckDBStructureInfo (x le tabelle mancanti)
		/// <summary>
		/// evento intercettato dalla classe CheckDBStructureInfO quando vengono controllate le eventuali
		/// tabelle mancanti al database. Per ognuna di queste viene caricato anche il corrispondente file
		/// (se esiste) contenente i dati di default
		/// </summary>
		//---------------------------------------------------------------------
		private void OnAddDefaultDataMissingTable(string table, string application, string module)
		{
			if (importExportManager != null)
				importExportManager.AddDefaultDataTable(table, application, module);
		}
		/// <summary>
		/// evento intercettato dalla classe CheckDBStructureInfO quando vengono controllate le eventuali
		/// tabelle mancanti al database. Per ognuna di queste viene caricato anche il corrispondente file
		/// (se esiste) contenente i dati di esempio
		/// </summary>
		//---------------------------------------------------------------------
		private void OnAddSampleDataMissingTable(string table, string application, string module)
		{
			if (importExportManager != null)
				importExportManager.AddSampleDataTable(table, application, module);
		}
		# endregion
	}
}
