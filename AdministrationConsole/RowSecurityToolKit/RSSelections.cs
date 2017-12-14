using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseItems;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.RowSecurityToolKit
{
	//================================================================================
	public enum EntityAction { NEW, EDIT, DELETE }

	///<summary>
	/// Classe con le selezioni effettuate dall'utente nel wizard
	///</summary>
	//================================================================================
	public class RSSelections
	{
		// selezioni del wizard
		public EntityAction EntityAction = EntityAction.NEW;
		public string Entity = string.Empty;
		public string EntityDescription = string.Empty;	
		public bool EncryptFiles = false; // per criptare automaticamente i file coinvolti nelle modifiche
		
		public string MasterTable = string.Empty; // nome tabella
		public List<CatalogColumn> MasterTblColumns; // colonne selezionate nella mastertable

		public string MasterTableNamespace = string.Empty; // namespace tabella
		public string DocumentNamespace = string.Empty; // namespace documento
		//public string HKLNamespace = string.Empty; // namespace hotlink
		//public string NumbererNamespace = string.Empty; // namespace numeratore
		public int Priority = 1; // priorita'

		// mappa con tabelle e colonne selezionate nel DataGridView relative alle entita' figlie
		public Dictionary<string, List<string>> RelatedTablesDictionary = null;
		public List<RSRelatedTable> RelatedTablesList = null;

		// altre variabili
		private ContextInfo contextInfo;
		private CatalogInfo catalog;
		private ApplicationDBStructureInfo appDBStructInfo;
		private BrandLoader brandLoader;
		private StringCollection applicationList;
		
		// elenco tabelle riconosciute dall'applicazione (ovvero dichiarate nei DatabaseObjects.xml)
		public List<CatalogTableEntry> RegisteredTablesList;

		// lista oggetti caricati dai file di configurazione
		private List<RowSecurityObjectsInfo> rowSecurityObjectsList = new List<RowSecurityObjectsInfo>();

		// struttura in memoria globale con le entita' e tutte le tabelle referenziate, caricate dai file RowSecurityObjects.xml
		public Dictionary<string, RSEntityInfo> EntitiesDictionary = new Dictionary<string, RSEntityInfo>(StringComparer.OrdinalIgnoreCase);

		// elenco priorita' per entita'
		public Dictionary<string, int> PrioritiesDictionary = new Dictionary<string, int>();

		// elenco aziende presenti nel database di sistema e informazioni azienda correntemente selezionata
		public List<CompanyItem> CompaniesList = new List<CompanyItem>();
		public CompanyItem CompanyInfo;

		// Properties
		//--------------------------------------------------------------------
		public ContextInfo ContextInfo { get { return contextInfo; } }
		public ApplicationDBStructureInfo AppDBStructInfo { get { return appDBStructInfo; } }
		public List<RowSecurityObjectsInfo> RowSecurityObjectsList { get { return rowSecurityObjectsList; } }

		///<summary>
		/// Costruttore
		///</summary>
		//--------------------------------------------------------------------------------
		public RSSelections(ContextInfo context, BrandLoader brand)
		{
			contextInfo = context;
			brandLoader = brand;
		}

		///<summary>
		/// Richiamo il parse dei file RowSecurityObjects.xml e riempio una struttura
		/// globale con le informazioni
		///</summary>
		//--------------------------------------------------------------------------------
		internal void LoadRowSecurityObjectsInfo()
		{
			applicationList = new StringCollection();

			// considero solo le tabelle degli AddOn di TaskBuilderApplications
			// le tabelle di TB ed Extensions non sono previste
			contextInfo.PathFinder.GetApplicationsList(ApplicationType.TaskBuilderApplication, out applicationList);

			// per tutte le applicazioni trovate vado a leggere le informazioni nei file xml
			foreach (string appName in applicationList)
			{
				IBaseApplicationInfo appInfo = contextInfo.PathFinder.GetApplicationInfoByName(appName);
				foreach (BaseModuleInfo modInfo in appInfo.Modules)
				{
					if (!File.Exists(modInfo.GetRowSecurityObjectsPath()))
						continue;

					RowSecurityObjectsInfo rsoi = modInfo.RowSecurityObjectsInfo;
					if (rsoi == null)
						continue;

					// primo giro per memorizzare nel dictionary le entita'
					if (rsoi.RSEntities != null)
					{
						foreach (RSEntity item in rsoi.RSEntities)
						{
							RSEntityInfo rs;
							// se non la trovo la inserisco (e se ce ne fosse una duplicata con lo stesso nome?)
							if (!EntitiesDictionary.TryGetValue(item.Name, out rs))
								EntitiesDictionary.Add(item.Name, new RSEntityInfo(item));
						}
					}

					// memorizzo le info in una lista di appoggio, che mi serve dopo
					rowSecurityObjectsList.Add(rsoi);
				}
			}

			// seconda passata per assegnare le tabelle alle entita'
			foreach (RowSecurityObjectsInfo rsoi in rowSecurityObjectsList)
			{
				if (rsoi.RSTables != null)
				{
					// per ogni tabella
					foreach (RSTable rst in rsoi.RSTables)
					{
						// per ogni Entity di base
						foreach (RSEntityBase entityBase in rst.RsEntityBaseList)
						{
							// cerco nel dictionary l'entity corrispondente (se non la trovo significa che e' sbagliato il nome)
							// e aggiungo tutte le tabelle che la referenziano
							RSEntityInfo rsi;
							if (EntitiesDictionary.TryGetValue(entityBase.Name, out rsi))
							{
								RSTableInfo tbl = new RSTableInfo(rst);
								tbl.RsColumns.AddRange(entityBase.RsColumns);
								rsi.RsTablesInfo.Add(tbl);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Riempio il CatalogInfo e predispongo la lista filtrata di CatalogTableEntry
		/// registrate dall'applicazione (ovvero le tabelle dichiarate nei DatabaseObjects.xml)
		/// </summary>
		//--------------------------------------------------------------------------------
		internal bool LoadCatalogInfo()
		{
			if (!contextInfo.MakeCompanyConnection(CompanyInfo.CompanyId))
			{
				//DiagnosticViewer.ShowDiagnostic(rsSelections.ContextInfo.Diagnostic);
				return false;
			}

			// istanzio il catalog una volta sola
			if (catalog == null)
				catalog = new CatalogInfo();

			// se il nome dello schema e' vuoto, oppure e' cambiato dalla selezione precedente, ricarico le info (previo Clear)
			if (
				string.IsNullOrWhiteSpace(catalog.SchemaName) ||
				string.Compare(catalog.SchemaName, this.CompanyInfo.DbName, StringComparison.InvariantCultureIgnoreCase) != 0
				)
			{
				catalog.Clear(); // pulisco gli array per non ricaricare le cose doppie
				catalog.LoadAllInformationSchema(contextInfo.Connection, false); // carico il catalog dal db

				appDBStructInfo = new ApplicationDBStructureInfo(contextInfo.PathFinder, brandLoader);
				appDBStructInfo.ReadDatabaseObjectsFiles(applicationList, false); // leggo tutti gli oggetti registrati dall'applicazione

				RegisteredTablesList = new List<CatalogTableEntry>();

				// scorro le applicazioni/moduli/tabelle registrate e vado a riempire la lista delle tabelle registrate
				foreach (AddOnApplicationDBInfo appDBInfo in appDBStructInfo.ApplicationDBInfoList)
					foreach (ModuleDBInfo modInfo in appDBInfo.ModuleList)
						foreach (EntryDBInfo table in modInfo.TablesList)
						{
							CatalogTableEntry cte = catalog.GetTableEntry(table.Name);
							if (cte != null)
							{
								// arricchisce ogni CatalogEntry con le info dell'applicazione/modulo di appartenenza
								cte.Application = modInfo.ApplicationMember;
								cte.Module = modInfo.ModuleName;
								cte.Namespace = table.Namespace; // tengo da parte anche il namespace
								RegisteredTablesList.Add(cte);
							}
						}
			}

			// chiudo la connessione
			contextInfo.CloseConnection();

			// se non ci sono tabelle ritorno false
			if (RegisteredTablesList.Count == 0)
				return false;
			RegisteredTablesList.Sort(new CatalogEntryGenericComparer()); // ordino alfabeticamente

			return true;
		}

		///<summary>
		/// Dato un tablename, ritorna l'oggetto CatalogTableEntry dalla lista tabelle registrate
		///</summary>
		//--------------------------------------------------------------------------------
		public CatalogTableEntry GetRegisteredTableEntry(string tableName)
		{
			if (RegisteredTablesList == null || RegisteredTablesList.Count == 0)
				return null;
			return RegisteredTablesList.Find(tbl => string.Compare(tbl.TableName, tableName, StringComparison.InvariantCultureIgnoreCase) == 0);
		}
	}
}
