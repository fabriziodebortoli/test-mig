using System.Collections.Specialized;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DataManagerEngine
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
				// di default (sono in modalitá di export)
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
			SelectedIsoState		= ContextInfo.IsoState;
			this.brandLoader	= brandLoader;

			Catalog	= new CatalogInfo();
			Catalog.Load(ContextInfo.Connection, true);
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
		# endregion

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
