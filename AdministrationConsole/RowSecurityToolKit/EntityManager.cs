using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Microarea.Console.Plugin.RowSecurityToolKit.WizardPages;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.RowSecurityToolKit
{
	//================================================================================
	public class EntityManager
	{
		private RSSelections rsSelections = null;
		private List<string> involvedFilesList = null; // lo uso per non duplicare msg durante l'elaborazione

		// per richiamare la classe delle funzioni e datamember comuni
		protected ContextInfo contextInfo = null;
		protected BrandLoader brandLoader = null;
		protected ImageList stateImageList = null;

		//--------------------------------------------------------------------------------
		public delegate SqlDataReader GetCompanies();
		public event GetCompanies OnGetCompanies;

		public delegate DatabaseStatus CheckCompanyDBForRSLDelegate(string companyId);
		public event CheckCompanyDBForRSLDelegate CheckCompanyDBForRSL;

		// eventi utilizzati per gestire l'avanzamento dell'elaborazione
		//--------------------------------------------------------------------------------
		public EventHandler ElaborationCompleted;

		public delegate void OperationCompletedDelegate(bool success, string message, string filePath);
		public event OperationCompletedDelegate OperationCompleted;

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public EntityManager(ContextInfo context, BrandLoader brand, ImageList stateImageList)
		{
			contextInfo = context;
			brandLoader = brand;
			this.stateImageList = stateImageList;
		}

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public EntityManager(RSSelections selections)
		{
			rsSelections = selections;
		}

		/// <summary>
		/// richiama il wizard
		/// </summary>
		//---------------------------------------------------------------------------
		public void RunWizard()
		{
			RSWizard rsWizard = new RSWizard(contextInfo, brandLoader, stateImageList);
			rsWizard.OnGetCompanies += new RSWizard.GetCompanies(rsWizard_OnGetCompanies);
			rsWizard.CheckCompanyDBForRSL += new RSWizard.CheckCompanyDBForRSLDelegate(rsWizard_CheckCompanyDBForRSL);
			rsWizard.AddWizardPages();
			rsWizard.Run();
		}

		//---------------------------------------------------------------------------
		public DatabaseStatus rsWizard_CheckCompanyDBForRSL(string companyId)
		{
			DatabaseStatus dbStatus = DatabaseStatus.EMPTY;

			if (CheckCompanyDBForRSL != null)
				dbStatus = CheckCompanyDBForRSL(companyId);

			return dbStatus;
		}

		//---------------------------------------------------------------------------
		public SqlDataReader rsWizard_OnGetCompanies()
		{
			if (OnGetCompanies != null)
				return OnGetCompanies();

			return null;
		}

		//---------------------------------------------------------------------
		public Thread Execute()
		{
			Thread myThread = new Thread(new ThreadStart(InternalExecute));
			// quando si istanzia un nuovo Thread bisogna assegnargli le CurrentCulture, altrimenti le
			// traduzioni in lingue differenti da quelle del sistema operativo non funzionano!!!
			myThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			myThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			myThread.Start();
			return myThread;
		}

		/// <summary>
		/// Esecuzione del processo di Unparse dei file
		/// </summary>
		//---------------------------------------------------------------------
		public void InternalExecute()
		{
			switch (rsSelections.EntityAction)
			{
				case EntityAction.NEW:
					WriteNewEntity();
					break;
				case EntityAction.EDIT:
					ModifyEntity();
					break;
				case EntityAction.DELETE:
					DeleteEntity(); 
					break;
			}

			// alla fine di tutto eseguo un'update del valore delle priorita'
			// lo faccio in un secondo tempo perche' potrebbe coinvolgere file diversi
			// da quelli dell'entita' prescelta nel wizard
			UpdatePriorities();
		}

		///<summary>
		/// Scrittura di una nuova entita' e delle relative tabelle e colonne correlate
		///</summary>
		//---------------------------------------------------------------------
		private void WriteNewEntity()
		{
			involvedFilesList = new List<string>();

			IBaseModuleInfo modInfo;
			RowSecurityObjectsInfo rsInfo = GetRowSecurityObjectsInfo(rsSelections.MasterTable, out modInfo);
			if (rsInfo == null)
				return;

			OperationCompleted?.Invoke(true, string.Format(Strings.StartWriteEntity, rsSelections.Entity), string.Empty);

			// istanzio un oggetto di tipo RSEntity
			RSEntity rsEntity = new RSEntity
				(
				rsSelections.Entity,
				rsSelections.MasterTableNamespace,
				rsSelections.DocumentNamespace,
				rsSelections.EntityDescription,
				1, // metto il generico valore 1, successivamente viene effettuato l'update della priorita'
				modInfo
				);

			// aggiungo tutte le colonne selezionate
			foreach (CatalogColumn cc in rsSelections.MasterTblColumns)
				rsEntity.RsColumns.Add(new RSColumn(cc.Name));
			rsInfo.RSEntities.Add(rsEntity); // aggiungo alla lista delle entity esistenti quella che devo aggiungere

			// scrivo il file
			bool ok = rsInfo.Unparse();

			involvedFilesList.Add(rsInfo.FilePath);
			OperationCompleted?.Invoke(ok, string.Format(Strings.SavingFile, rsInfo.ParentModuleInfo.ModuleConfigInfo.ModuleName, rsInfo.ParentModuleInfo.ParentApplicationInfo.Name), rsInfo.FilePath);

			if (ok && rsSelections.EncryptFiles)
			{
				// eseguo l'encrypt del file
				ok = CRSFunctions.Encrypt(rsInfo.FilePath);
				if (!involvedFilesList.ContainsNoCase(rsInfo.FilePath))
				{
					OperationCompleted?.Invoke(ok, string.Format(Strings.EncryptedFile,
						rsInfo.ParentModuleInfo.ModuleConfigInfo.ModuleName, rsInfo.ParentModuleInfo.ParentApplicationInfo.Name), string.Empty);
					involvedFilesList.Add(rsInfo.FilePath);
				}
			}

			// per ogni tabella + colonne che referenziano la mastertable
			// devo identificare i singoli file da andare a creare/modificare
			foreach (RSRelatedTable relTbl in rsSelections.RelatedTablesList)
			{
				NameSpace namespaceTable = new NameSpace(relTbl.TableNamespace, NameSpaceObjectType.Table);
				string tableName = namespaceTable.GetTokenValue(NameSpaceObjectType.Table);

				IBaseModuleInfo modi;
				RowSecurityObjectsInfo rsi = GetRowSecurityObjectsInfo(tableName, out modi);

				if (rsi == null || modi == null)
					continue;

				// prima controllo se nelle tables del file e' gia' presente la tabella che devo analizzare
				RSTable rsTable = null;
				foreach (RSTable tbl in rsi.RSTables)
				{
					if (string.Compare(tbl.NameSpace, relTbl.TableNamespace, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						rsTable = tbl;
						break;
					}
				}
	
				if (rsTable == null)
				{
					rsTable = new RSTable(relTbl.TableNamespace);
					rsi.RSTables.Add(rsTable); // aggiungo la nuova RSTable
				}

				RSEntityBase rsEntityBase = null;
				foreach (List<string> cols in relTbl.ColumnsList)
				{

					for (int k = 0; k < rsTable.RsEntityBaseList.Count; k++)
					{
						// prima devo cercare se esiste gia' un nodo EntityBase
						RSEntityBase eb = rsTable.RsEntityBaseList[k];
						if (string.Compare(eb.Name, rsSelections.Entity, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							rsEntityBase = eb;
							break;
						}
					}

					if (rsEntityBase == null)
					{
						rsEntityBase = new RSEntityBase(rsSelections.Entity);
						rsTable.RsEntityBaseList.Add(rsEntityBase);
					}

					// una tabella puo' essere presente piu' volte con differenti colonne
					RSColumns rsColumnsList = new RSColumns();
					rsEntityBase.RsColumns.Add(rsColumnsList);

					// ogni colonna i-esima corrisponde alla colonna i-esima di quelle della mastertable
					int i = 0;
					foreach (string col in cols)
					{						
						rsColumnsList.RSColumnList.Add(new RSColumn(col, rsSelections.MasterTblColumns[i].Name));
						i++;
					}
				}
				

				// scrivo il file
				ok = rsi.Unparse();

				if (!involvedFilesList.ContainsNoCase(rsInfo.FilePath))
				{
					OperationCompleted?.Invoke(ok, string.Format(Strings.SavingFile,
						rsi.ParentModuleInfo.ModuleConfigInfo.ModuleName, rsi.ParentModuleInfo.ParentApplicationInfo.Name), rsi.FilePath);
					involvedFilesList.Add(rsInfo.FilePath);
				}

				if (ok && rsSelections.EncryptFiles)
				{
					// eseguo l'encrypt del file
					ok = CRSFunctions.Encrypt(rsi.FilePath);
					if (!involvedFilesList.ContainsNoCase(rsInfo.FilePath))
					{
						OperationCompleted?.Invoke(ok, string.Format(Strings.EncryptedFile,
						rsi.ParentModuleInfo.ModuleConfigInfo.ModuleName, rsi.ParentModuleInfo.ParentApplicationInfo.Name), string.Empty);
						involvedFilesList.Add(rsInfo.FilePath);
					}
				}
			}

			// sparo un evento che l'elaborazione e' completata
			ElaborationCompleted?.Invoke(null, System.EventArgs.Empty);
		}

		///<summary>
		/// Modifica di un'entita' esistente (e delle relative tabelle e colonne correlate)
		///</summary>
		//---------------------------------------------------------------------
		private void ModifyEntity()
		{ 
			// Prima elimino l'entita' pre-esistente
			DeleteEntity();
			// riscrivo da zero la nuova entita'
			WriteNewEntity();
		}

		///<summary>
		/// Eliminazione di un entity e di tutti i suoi riferimenti
		/// Faccio un loop su tutti i file RowSecurityObjects.xml e:
		/// 1. cerco nelle entita' ed elimino dalla corrispondente lista quella che devo eliminare
		/// 2. cerco in tutte le tabelle quelle che hanno tra le loro RSEntityBase l'entita' da eliminare:
		///		a. se trovo solo un'entita' devo eliminare il nodo parent Table
		///		b. se trovo piu' di un'entita' elimino solo il nodo RSEntity
		///</summary>
		//---------------------------------------------------------------------
		private void DeleteEntity()
		{
			OperationCompleted?.Invoke(true, string.Format(Strings.StartDeleteEntity, rsSelections.Entity), string.Empty);

			bool isChanged = false;

			foreach (RowSecurityObjectsInfo rsoi in rsSelections.RowSecurityObjectsList)
			{
				for (int i = rsoi.RSEntities.Count - 1; i >= 0; i--)
				{
					if (string.Compare(rsoi.RSEntities[i].Name, rsSelections.Entity, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						// elimino il nodo di tipo <Entity>
						rsoi.RSEntities.RemoveAt(i);
						isChanged = true;
						break; //faccio break perche' il nome entita' e' univoco
					}
				}

				for (int k = rsoi.RSTables.Count - 1; k >= 0; k--)
				{
					RSTable tbl = rsoi.RSTables[k];

					for (int z = tbl.RsEntityBaseList.Count - 1; z >= 0; z--)
					{
						if (string.Compare(tbl.RsEntityBaseList[z].Name, rsSelections.Entity, StringComparison.InvariantCultureIgnoreCase) == 0)
						{
							// elimino il nodo di tipo <Entity> base
							tbl.RsEntityBaseList.RemoveAt(z);
							isChanged = true;
						}
					}

					if (tbl.RsEntityBaseList.Count == 0)
					{
						// se non ci sono ulteriori sottonodi elimino anche il nodo di tipo <Table>
						rsoi.RSTables.RemoveAt(k);
						isChanged = true;
					}
				}

				// se ho effettuato dei cambiamenti alla struttura procedo nella serializzazione
				if (isChanged)
				{
					bool ok = rsoi.Unparse();
					isChanged = false;
					OperationCompleted?.Invoke(ok, string.Format(Strings.ModifiedFile, rsoi.ParentModuleInfo.ModuleConfigInfo.ModuleName, rsoi.ParentModuleInfo.ParentApplicationInfo.Name), rsoi.FilePath);
				}
			}

			// sparo un evento che l'elaborazione e' completata
			ElaborationCompleted?.Invoke(null, EventArgs.Empty);
		}

		///<summary>
		/// Esegue l'update del nodo Priority
		/// Scorro tutti i file RowSecurityObjects.xml e per ogni entita' in essa definita
		/// controllo la nuova priorita' assegnata dall'utente e, se necessario, viene aggiornata
		///</summary>
		//--------------------------------------------------------------------------------
		private void UpdatePriorities()
		{
			if (OperationCompleted != null)
				OperationCompleted(true, Strings.StartUpdatePriorities, string.Empty);

			foreach (RowSecurityObjectsInfo rsoi in rsSelections.RowSecurityObjectsList)
			{
				bool priorityIsModified = false;
				foreach (RSEntity rse in rsoi.RSEntities)
				{
					int priority;
					if (rsSelections.PrioritiesDictionary.TryGetValue(rse.Name, out priority))
					{
						if (rse.Priority != priority)
						{
							// se il valore della priorita' e' variato lo assegno alla struttura in memoria
							rse.Priority = priority;
							priorityIsModified = true;
						}
					}
				}

				if (priorityIsModified)
					rsoi.Unparse(); // vado a scrivere la variazione nel file
			}

			// sparo un evento che l'elaborazione e' completata
			ElaborationCompleted?.Invoke(null, EventArgs.Empty);
		}

		///<summary>
		/// Dato il nome di una tabella contenuta nel Catalog si occupa di individuare il path
		/// del file RowSecurityObjects.xml di uno specifico modulo + applicazione
		///</summary>
		//--------------------------------------------------------------------------------
		public RowSecurityObjectsInfo GetRowSecurityObjectsInfo(string tableName, out IBaseModuleInfo modInfo)
		{
			modInfo = null;

			// carico le info della masterTable (modulo + applicazione di appartenenza, 
			// per istanziare poi un oggetto di tipo RowSecurityObjectsInfo)
			CatalogTableEntry cte = rsSelections.GetRegisteredTableEntry(tableName);
			if (cte == null || string.IsNullOrWhiteSpace(cte.Application) || string.IsNullOrWhiteSpace(cte.Module))
				return null;

			// estrapolo le info del modulo
			modInfo = BasePathFinder.BasePathFinderInstance.GetModuleInfoByName(cte.Application, cte.Module);
			
			// calcolo il path del file
			string rslFilePath = Path.Combine
				(
				BasePathFinder.BasePathFinderInstance.GetApplicationModuleObjectsPath(cte.Application, cte.Module),
				NameSolverStrings.RowSecurityObjectsXml
				);

			if (string.IsNullOrWhiteSpace(rslFilePath) || modInfo == null)
				return null;

			RowSecurityObjectsInfo rsInfo = null;
			// se nella lista NON esiste il file istanzio una classe dummy che poi mi servira' successivamente
			if (!rsSelections.RowSecurityObjectsList.Exists(item => string.Compare(item.FilePath, rslFilePath, StringComparison.InvariantCultureIgnoreCase) == 0))
			{
				rsInfo = new RowSecurityObjectsInfo(rslFilePath, modInfo);
				rsSelections.RowSecurityObjectsList.Add(rsInfo);
			}
			else
				rsInfo = rsSelections.RowSecurityObjectsList.Find(item => string.Compare(item.FilePath, rslFilePath, StringComparison.InvariantCultureIgnoreCase) == 0);

			return rsInfo;
		}
	}
}