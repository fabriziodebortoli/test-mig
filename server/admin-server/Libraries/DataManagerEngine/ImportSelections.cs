using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

using TaskBuilderNetCore.Interfaces;
using Microarea.AdminServer.Libraries.DatabaseManager;
using Microarea.Common.NameSolver;

namespace Microarea.AdminServer.Libraries.DataManagerEngine
{
	/// <summary>
	/// ImportSelections (struttura in memoria con le opzioni scelte dall'utente)
	/// </summary>
	//=========================================================================
	public class ImportSelections : DataManagerSelections
	{
		public List<ImportItemInfo> ImportList = new List<ImportItemInfo>();

		public bool InsertExtraFieldsRow = true;  // se true posso inserire la riga anche se non tutti i campi sono presenti sul DB
		public bool DeleteTableContext = false; // se cancellare o meno il contenuto della tabella prima di importare i dati
		public bool DisableCheckFK = true;  // disabilita il controllo dei constraints di ForeignKey
		public bool NoOptional = true;
        public bool disabledFKConstraintPostgre = false; //controlo di presenza di fk in postgre

		public bool LoadXmlToFileSystem = false; // se la directory contenente i file xml sono letti da File System
		public string PathFolderXml = string.Empty; // path della directory contenente i file xml

		// per capire se l'importazione è silente (ovvero contestuale alla creazione del database)
		// oppure se si tratta di elaborazione partita dal wizard vero e proprio
		public bool IsSilent = false;

		public bool ImportTBCreated = true;	 // importazione della colonna base TBCreated
		public bool ImportTBModified = false; // importazione della colonna base TBModified

		// per le colonne tipo DateTime usa il formato Utc (yyyy-MM-ddTHH:mm:sszzzzzz+02.00) oppure senza il +02.00
		public bool UseUtcDateTimeFormat = false;

		// variabili di comodo x i vari controlli da effettuare nelle pagine
		public string Company = string.Empty;
		public string OldCompany = string.Empty;
		public bool ClearItems = false;

		// se sovrascrivere o meno i record giá presenti
		public enum UpdateExistRowType { SKIP_ROW, UPDATE_ROW, SKIP_ROW_ERROR };
		public UpdateExistRowType UpdateExistRow = UpdateExistRowType.SKIP_ROW;

		// ErrorRecovery : comportamento da tenere in caso di errore:
		//		1) interrompere il processo ed effettuare il roolback delle informazioni 
		//		giá inserite:
		//			a) relative all'ultima tabella	STOP_LAST_FILE_ROLLBACK
		//			b) oppure a tutte le tabelle	STOP_ALL_FILE_ROLLBACK
		//		2) effettuare il roolback x la tabella che ha causato l'errore  e continuare 
		//			CONTINUE_LAST_FILE_ROLLBACK
		//		3) continuare CONTINUE
		public enum TypeRecovery { CONTINUE, CONTINUE_LAST_FILE_ROLLBACK, STOP_LAST_FILE_ROLLBACK, STOP_ALL_FILE_ROLLBACK };
		public TypeRecovery ErrorRecovery = TypeRecovery.CONTINUE_LAST_FILE_ROLLBACK;

		/// <summary>
		/// costruttore con assegnazione member della classe ContextInfo
		/// </summary>
		//---------------------------------------------------------------------
		public ImportSelections(ContextInfo context, BrandLoader brandLoader)
			: base(context, brandLoader)
		{
			Catalog = new CatalogInfo();
			Catalog.Load(ContextInfo.Connection, true);
		}

		/// <summary>
		/// costruttore con assegnazione member della classe ContextInfo e del Catalog
		/// viene istanziato dal wizard x la gestione dei dati di default
		/// </summary>
		//---------------------------------------------------------------------
		public ImportSelections(ContextInfo context, CatalogInfo catInfo, BrandLoader brandLoader)
			: base(context, catInfo, brandLoader)
		{
		}

		/// <summary>
		/// ripristina il default dei flag utilizzati dal wizard
		/// </summary>
		//---------------------------------------------------------------------
		public void Clear()
		{
			ImportList.Clear();
			InsertExtraFieldsRow = true;
			UpdateExistRow = UpdateExistRowType.SKIP_ROW;
			DeleteTableContext = false;
			DisableCheckFK = true;
			ClearItems = true;
			ErrorRecovery = TypeRecovery.CONTINUE_LAST_FILE_ROLLBACK;
		}

		# region Funzioni per gestire l'array dei file da importare, in base alle selezioni dell'utente
		/// <summary>
		/// quando faccio la Add alla ImportList, devo controllare se esiste o meno un
		/// ImportItemInfo associato a quel path.
		/// se non esiste lo creo, se esiste già aggiungo solo il nome del file alla
		/// stringcollection dei selectedfiles.
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddItemInImportList(string pathName, string fileName)
		{
			StringCollection allFilesList = null;
			ImportItemInfo item = null;
			item = GetImportItemInfo(pathName);

			if (item != null)
			{
				if (fileName.Length > 0)
					item.Add(fileName);
				else
				{
					// se la stringa relativa al nome del file è vuota significa che devo
					// caricare tutti i file contenuti in quella directory.
					// quindi li carico da file system e li inserisco
					LoadAndGetAllFiles(pathName, out allFilesList);
					foreach (string file in allFilesList)
						item.Add(file);
				}
			}
			else
			{
				item = new ImportItemInfo();
				item.PathName = pathName;

				if (fileName.Length > 0)
					item.Add(fileName);
				else
				{
					// se la stringa relativa al nome del file è vuota significa che devo
					// caricare tutti i file contenuti in quella directory.
					// quindi li carico da file system e li inserisco
					LoadAndGetAllFiles(pathName, out allFilesList);
					foreach (string file in allFilesList)
						item.Add(file);
				}

				// aggiungo l'item solo se l'ho creato nuovo
				ImportList.Add(item);
			}
		}

		/// <summary>
		/// serve per poter inserire un elemento nella lista di importazione a partire
		/// da un nome completo di file
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddItemInImportList(string fullName)
		{
			AddItemInImportList(Path.GetDirectoryName(fullName), Path.GetFileName(fullName));
		}

		/// <summary>
		/// quando faccio la Add alla ImportList per quanto riguarda i file in Append,
		/// devo creare cmq un oggetto di tipo ImportItemInfo associato a quel path, in modo
		/// da forzare l'esecuzione dei file in Append dopo tutti gli altri.
		/// Infatti se viene eseguito un file di Append (che inserisce 2 righe) e poi dopo 
		/// viene eseguito il file specifico per quella tabella, quest'ultimo pulisce la tabella, 
		/// perdendo così le 2 righe inserite precedentemente dal file di Append.
		/// </summary>
		//---------------------------------------------------------------------------
		public void AddAppendItemInImportList(string pathName, string fileName)
		{
			StringCollection allFilesList = null;
			ImportItemInfo item = new ImportItemInfo();
			item.PathName = pathName;

			if (fileName.Length > 0)
				item.Add(fileName);
			else
			{
				// se la stringa relativa al nome del file è vuota significa che devo
				// caricare tutti i file contenuti in quella directory.
				// quindi li carico da file system e li inserisco
				LoadAndGetAllFiles(pathName, out allFilesList);
				foreach (string file in allFilesList)
					item.Add(file);
			}

			ImportList.Add(item);
		}

		/// <summary>
		/// rimuovo dalla collection dei SelectedFiles corrispondente al path passato come
		/// parametro. se il nome del file è vuoto significa che devo pulire tutti
		/// gli items la collection
		/// </summary>
		//---------------------------------------------------------------------------
		public void RemoveItemFromImportList(string pathName, string fileName)
		{
			ImportItemInfo item = GetImportItemInfo(pathName);

			if (item != null && item.SelectedFiles != null)
			{
				// se è stato specificato un solo file elimino solo quello
				// altrimenti svuoto completamente la StringCollection
				if (fileName.Length > 0)
					item.SelectedFiles.Remove(fileName);
				else
					item.SelectedFiles.Clear();

				// se l'array è vuoto significa che non devo portare neppure l'item, e lo levo dall'array
				if (item.SelectedFiles.Count == 0)
					ImportList.Remove(item);
			}
		}

		/// <summary>
		/// scorro il file system relativamente al path passato come parametro
		/// e aggiungo i soli file con estensione xml ad una stringcollection
		/// </summary>
		//---------------------------------------------------------------------------
		private void LoadAndGetAllFiles(string pathName, out StringCollection strings)
		{
			strings = new StringCollection();

			DirectoryInfo dir = new DirectoryInfo(pathName);

			foreach (FileInfo file in dir.GetFiles())
			{
				// memorizzo solo i file con estensione xml
				if (string.Compare(file.Extension, NameSolverStrings.XmlExtension, StringComparison.OrdinalIgnoreCase) == 0)
					strings.Add(file.Name);
			}
		}

		/// <summary>
		/// metodo che cerca un path (specificato nel parametro passato) nell'array 
		/// dei files da importare
		/// </summary>
		//---------------------------------------------------------------------------
		public bool ExistencePathInImportList(string path)
		{
			foreach (ImportItemInfo item in ImportList)
			{
				if (string.Compare(item.PathName, path, StringComparison.OrdinalIgnoreCase) == 0)
					return true;
			}
			return false;
		}

		/// <summary>
		/// metodo che cerca il nome di un file (specificato nel parametro passato) 
		/// nell'array dei files da importare
		/// </summary>
		//---------------------------------------------------------------------------
		public bool ExistenceFileInImportList(string path, string file)
		{
			ImportItemInfo item = GetImportItemInfo(path);

			if (item != null)
			{
				if (item.SelectedFiles.Contains(file))
					return true;
			}
			return false;
		}

		/// <summary>
		/// metodo che ritorna l'oggetto di tipo ImportItemInfo corrispondente
		/// al path passato come parametro.
		/// </summary>
		//---------------------------------------------------------------------------
		public ImportItemInfo GetImportItemInfo(string path)
		{
			foreach (ImportItemInfo item in ImportList)
			{
				if (string.Compare(item.PathName, path, StringComparison.OrdinalIgnoreCase) == 0)
					return item;
			}

			return null;
		}
		# endregion

		//--------------------------------------------------------------------------------
		protected override string GetOperationType()
		{
			return "Import";
		}
	}

	# region Classe ImportItemInfo
	//=========================================================================
	public class ImportItemInfo
	{
		// deve contenere l'intero path!
		public string PathName = string.Empty;
		public StringCollection SelectedFiles = null;

		//---------------------------------------------------------------------
		public ImportItemInfo() { }

		//---------------------------------------------------------------------------
		public void Add(string file)
		{
			if (SelectedFiles == null)
				SelectedFiles = new StringCollection();

			// se l'array contiene già quella stringa non la inserisco una seconda volta
			if (!SelectedFiles.Contains(file))
				SelectedFiles.Add(file);
		}
	}
	# endregion
}
