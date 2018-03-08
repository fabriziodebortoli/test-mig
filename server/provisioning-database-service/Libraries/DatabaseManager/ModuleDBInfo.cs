using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using Microarea.Common.NameSolver;

namespace Microarea.ProvisioningDatabase.Libraries.DatabaseManager
{
	#region Classe DefaultDataStep
	///<summary>
	/// Classe per memorizzare le informazioni degli step negli UpgradeInfo.xml 
	/// relativi agli scatti di release per i file di default
	///</summary>
	//================================================================================
	public class DefaultDataStep
	{
		public string Table { get; }
		public string Configuration { get; }
		public bool Overwrite { get; }
		public string Country { get; }

		//---------------------------------------------------------------------
		public DefaultDataStep(string table, string configuration, string country, bool overwrite = true)
		{
			this.Table = table;
			this.Configuration = configuration;
			this.Overwrite = overwrite;
			this.Country = country;
		}
	}
	#endregion

	#region Classe per memorizzare le info delle AdditionalColumns
	/// <summary>
	/// struttura x tenere traccia dei dati estratti dal parsing delle AdditionalColumns
	/// </summary>
	//=========================================================================
	public class AdditionalColumnsInfo
	{
		public  string		AppName;
		public  string		ModName;
		public  string		AppSignature;
		public  string		ModSignature;
		public  string		TableName;
		public  int			NumRelease;
		public  int			NumStep;

		//---------------------------------------------------------------------
		public AdditionalColumnsInfo()
		{ 
			AppName		= string.Empty;
			ModName		= string.Empty;
			AppSignature= string.Empty;
			ModSignature= string.Empty;
			TableName	= string.Empty;
			NumRelease	= 0;
			NumStep		= 0;
		}
	}
	#endregion

	#region Classe per memorizzare le info per il Recovery
	/// <summary>
	/// struttura x tenere traccia dello stato del recovery
	/// </summary>
	//=========================================================================
	public class RecoveryInfo : UpdateInfo
	{
		public int	NrRelease	= 0; // per avere traccia del nr. di livello, nr. step
		public int	NrLevel		= 0; // e nr. release analizzato
		public int	NrStep		= 0; // dove si è verificato un errore

		//---------------------------------------------------------------------
		public RecoveryInfo() { }

		//---------------------------------------------------------------------
		public RecoveryInfo(string app, string mod, int release, int lev, int step)
		{ 
			ApplicationSign = app;
			ModuleSign		= mod;
			NrRelease		= release;
			NrLevel			= lev;
			NrStep			= step;
		}

		#region Funzioni per il parsing dei file per il ripristino dei dati
		/// <summary>
		/// funzione che si occupa di "recuperare" i dati contenuti nel file xml passato come 
		/// parametro da un certo livello e step e costruisce la struttura da cui effettuare il
		/// ripristino dei dati in caso di stato inconsistente
		/// </summary>
		//--------------------------------------------------------------------------
		public void FindRecoveryData(string fileXML, CheckDBStructureInfo dbStructInfo)
		{
			UpdateInfoList = new List<SingleUpdateInfo>();

			this.addOnAppList = dbStructInfo.AddOnAppList;
			this.dbMarkInfo = dbStructInfo.DBMarkInfo;

			xDoc = new XmlDocument();
			xmlFile = fileXML;

			try
			{
                xDoc = PathFinder.PathFinderInstance.LoadXmlDocument(xDoc, xmlFile);
			}
			catch(XmlException e)
			{
				Debug.Fail(string.Format(DatabaseManagerStrings.ErrorDuringParsingXmlFile, fileXML, e.LineNumber, e.LinePosition));
				Debug.WriteLine(string.Format(DatabaseManagerStrings.ErrorDuringParsingXmlFile, fileXML, e.LineNumber, e.LinePosition));
				return;
			}

			ParseForCreate	= (NrRelease <= 1) ? true : false;
			ModGraphLevel1 = dbStructInfo.RecoveryGraphLevel1;
			ModGraphLevel2 = dbStructInfo.RecoveryGraphLevel2;
			ModGraphLevel3 = dbStructInfo.RecoveryGraphLevel3;

			// se il numero di livello e di step sono uguali a zero li inizializzo a 1
			// potrebbe essere il caso in cui un modulo che non è stato eseguito per nulla xchè dipendeva 
			// da un'altro di cui era fallita la creazione
			if (NrLevel == 0) NrLevel = 1;
			if (NrStep == 0) NrStep = 1;

			XmlElement root = xDoc.DocumentElement;

			if (root == null)
			{
				Debug.Fail(string.Format(DatabaseManagerStrings.ErrorXmlSyntaxError, fileXML));
				Debug.WriteLine(string.Format(DatabaseManagerStrings.ErrorXmlSyntaxError, fileXML));
				return;
			}

			if (root.HasChildNodes)
			{
				// se sto parsando il Create cerco tutti i nodi il cui nome inizia con Level (= 1 o 2 o 3)
				// altrimenti cerco i nodi di tipo DBRel
				XmlNodeList xlevels = (ParseForCreate) 
					? root.SelectNodes("node()[starts-with(name(), 'Level')]")
					: xDoc.GetElementsByTagName(Create_UpgradeInfoXML.Element.DBRel);

				if (xlevels == null) 
					return;

				oldIdxLev1 = oldIdxLev2 = oldIdxLev3 = -1;

				foreach (XmlElement xElem in xlevels)
				{
					// sto parsando il file CreateInfo.xml
					if (ParseForCreate)
					{
						CurrSingleUpdate = new SingleUpdateInfo();
						currIdxLev1 = currIdxLev2 = currIdxLev3 = -1;
						CurrSingleUpdate.DBRel = 1;
						
						// se l'errore si è verificato al livello 1 devo vedere tutti i livelli
						if (NrLevel == 1)
						{
							if (
								xElem.Name == Create_UpgradeInfoXML.Element.Level1 ||
								xElem.Name == Create_UpgradeInfoXML.Element.Level2 ||
								xElem.Name == Create_UpgradeInfoXML.Element.Level3
								)
								ParseRecoveryLevel(xElem);
						}
						else // altrimenti solo il livello 2
							if (
								xElem.Name == Create_UpgradeInfoXML.Element.Level2 ||
								xElem.Name == Create_UpgradeInfoXML.Element.Level3
								)
								ParseRecoveryLevel(xElem);
							else
								if (xElem.Name == Create_UpgradeInfoXML.Element.Level3)
									ParseRecoveryLevel(xElem);

						if (
							CurrSingleUpdate.ScriptLevel1List.Count > 0 ||
							CurrSingleUpdate.ScriptLevel2List.Count > 0 ||
							CurrSingleUpdate.ScriptLevel3List.Count > 0
							)
							UpdateInfoList.Add(CurrSingleUpdate);
						else
							UpdateInfoList.Remove(CurrSingleUpdate);
					}
					else // sto parsando il file UpgradeInfo.xml
					{
						if (xElem.Name == Create_UpgradeInfoXML.Element.DBRel)
						{
							ParseRecoveryDBRel(xElem);
							oldIdxLev1 = currIdxLev1;
							oldIdxLev2 = currIdxLev2;
							oldIdxLev3 = currIdxLev3;
						}
					}
				}
			}
		}

		/// <summary>
		/// per il parse del singolo nodo di tipo DBRel
		/// </summary>
		//---------------------------------------------------------------------------
		private void ParseRecoveryDBRel(XmlElement xDBRel)
		{
			// guardo il valore dell'intero contenuto nell'attributo del nodo DBRel in analisi
			// controllo che sia uguale al valore che sto cercando, se diverso non proseguo
			currNumRel = Convert.ToInt32(xDBRel.GetAttribute(Create_UpgradeInfoXML.Attribute.Numrel));
			if (currNumRel != NrRelease)
				return;
			
			// se trovo nell'array degli update gia' la presenza di quello scatto di release lo skippo
			foreach (SingleUpdateInfo sui in UpdateInfoList)
				if (sui.DBRel == currNumRel)
					return;

			CurrSingleUpdate = new SingleUpdateInfo(currNumRel);
		
			currIdxLev1 = currIdxLev2 = -1;

			if (xDBRel.HasChildNodes)
			{
				XmlNodeList xChild = xDBRel.SelectNodes("node()[starts-with(name(), 'Level')]");
				if (xChild == null) 
					return;

				foreach (XmlElement xElem in xChild)
				{
					// se l'errore si è verificato al livello 1 devo vedere entrambi i livelli
					if (NrLevel == 1)
					{
						if (
							xElem.Name == Create_UpgradeInfoXML.Element.Level1 || 
							xElem.Name == Create_UpgradeInfoXML.Element.Level2 ||
							xElem.Name == Create_UpgradeInfoXML.Element.Level3
							)
							ParseRecoveryLevel(xElem);
					}
					else // altrimenti solo il livello 2
						if (
							xElem.Name == Create_UpgradeInfoXML.Element.Level2 ||
							xElem.Name == Create_UpgradeInfoXML.Element.Level3
							)
							ParseRecoveryLevel(xElem);
						else
							if (xElem.Name == Create_UpgradeInfoXML.Element.Level3)
								ParseRecoveryLevel(xElem);
				}

				if (
					CurrSingleUpdate.ScriptLevel1List.Count > 0 ||
					CurrSingleUpdate.ScriptLevel2List.Count > 0 ||
					CurrSingleUpdate.ScriptLevel3List.Count > 0 ||
					CurrSingleUpdate.DefaultDataStepList.Count > 0
					)
					UpdateInfoList.Add(CurrSingleUpdate);
				else
					UpdateInfoList.Remove(CurrSingleUpdate);
			}
		}

		/// <summary>
		/// per il parse del singolo nodo di tipo Level (comune x tutti i livelli)
		/// </summary>
		//--------------------------------------------------------------------------
		private void ParseRecoveryLevel(XmlElement xLevel)
		{
			int numLevel = 0;

			switch (xLevel.Name)
			{
				case Create_UpgradeInfoXML.Element.Level1:
					numLevel = 1;
					break;
				case Create_UpgradeInfoXML.Element.Level2:
					numLevel = 2;
					break;
				case Create_UpgradeInfoXML.Element.Level3:
					numLevel = 3;
					break;
				default:
					return;
			}

			if (xLevel.HasChildNodes)
			{
				XmlNodeList xChild = xLevel.SelectNodes(Create_UpgradeInfoXML.Element.Step);
				if (xChild == null) 
					return;

				foreach (XmlElement xElem in xChild)
					ParseRecoveryStep(xElem, numLevel);
			}
		}

		/// <summary>
		/// per il parse del singolo nodo di tipo Step
		/// </summary>
		//--------------------------------------------------------------------------
		private void ParseRecoveryStep(XmlElement xStep, int numLevel)
		{
			int nrstep = Convert.ToInt32(xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Numstep));

			// se il contenuto dell'attributo numstep è >= di quello che sto cercando 
			// procedo nell'elaborazione
			if (nrstep < NrStep)
				return;

			string script = (numLevel == 3) 
							? xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Library) 
							: xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Script);

			bool isDefaultDataStep = false;
			DefaultDataStep defaultDataStep = null;

			// se l'attributo e' vuoto vado a controllare se si tratta di uno step per i dati di default
			if (string.IsNullOrWhiteSpace(script))
			{
				string tableAttribute = xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Table);
				string configurationAttribute = xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Configuration);
				string overwriteAttribute = xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Overwrite);
				bool overwriteValue = (string.IsNullOrEmpty(overwriteAttribute)) ? true : Convert.ToBoolean(overwriteAttribute); // N.B. di default vado in update
				string countryAttribute = xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Country);
				string countryValue = (string.IsNullOrWhiteSpace(countryAttribute) || countryAttribute == "*") ? string.Empty : countryAttribute;

				// se gli attributi previsti 
				if (string.IsNullOrWhiteSpace(tableAttribute) || string.IsNullOrWhiteSpace(configurationAttribute))
				{
					this.error = string.Format
								(
								DatabaseManagerStrings.MissingAttribute,
								(numLevel == 3) ? Create_UpgradeInfoXML.Attribute.Library : Create_UpgradeInfoXML.Attribute.Script,
								nrstep.ToString(),
								xmlFile
								);
					return;
				}

				isDefaultDataStep = true;
				defaultDataStep = new DefaultDataStep(tableAttribute, configurationAttribute, countryValue, overwriteValue);
			}

			switch (numLevel)
			{
				case 1:
				{
					if (isDefaultDataStep)
					{
						CurrSingleUpdate.DefaultDataStepList.Add(defaultDataStep);
					}
					else
					{
						if (CurrSingleUpdate.ScriptLevel1List.Contains(script + ";" + nrstep))
							return;

						CurrSingleUpdate.ScriptLevel1List.Add(script + ";" + nrstep);
					}

					if (ParseForCreate)
					{
						if (currIdxLev1 == -1)
						{
							currIdxLev1 = ModGraphLevel1.AddVertex(ApplicationSign + "." + ModuleSign + "." + "1");
							if (oldIdxLev1 >= 0)
								ModGraphLevel1.AddEdge(oldIdxLev1, currIdxLev1);
						}	
					}	
					else
					{
						if (currIdxLev1 == -1)
						{
							currIdxLev1 = ModGraphLevel1.AddVertex(ApplicationSign + "." + ModuleSign + "." + currNumRel);
							if (oldIdxLev1 >= 0)
								ModGraphLevel1.AddEdge(oldIdxLev1, currIdxLev1, currNumRel);
						}	
					}
					break;
				}
				
				case 2:
				{
					if (CurrSingleUpdate.ScriptLevel2List.Contains(script + ";" + nrstep))
						return;

					CurrSingleUpdate.ScriptLevel2List.Add(script + ";" + nrstep);

					if (ParseForCreate)
					{
						if (currIdxLev2 == -1)
						{
							currIdxLev2 = ModGraphLevel2.AddVertex(ApplicationSign + "." + ModuleSign + "." + "1");
							if (oldIdxLev2 >= 0)
								ModGraphLevel2.AddEdge(oldIdxLev2, currIdxLev2);
						}
					}	
					else
					{
						if (currIdxLev2 == -1)
						{
							currIdxLev2 = ModGraphLevel2.AddVertex(ApplicationSign + "." + ModuleSign + "." + currNumRel);
							if (oldIdxLev2 >= 0)
								ModGraphLevel2.AddEdge(oldIdxLev2, currIdxLev2, currNumRel);
						}
					}
					break;
				}

				case 3:
				{
					if (CurrSingleUpdate.ScriptLevel3List.Contains(script + ";" + nrstep))
						return;

					CurrSingleUpdate.ScriptLevel3List.Add(script + ";" + nrstep);

					if (ParseForCreate)
					{
						if (currIdxLev3 == -1)
						{
							currIdxLev3 = ModGraphLevel3.AddVertex(ApplicationSign + "." + ModuleSign + "." + "1");
							if (oldIdxLev3 >= 0)
								ModGraphLevel2.AddEdge(oldIdxLev3, currIdxLev3);
						}
					}
					else
					{
						if (currIdxLev3 == -1)
						{
							currIdxLev3 = ModGraphLevel3.AddVertex(ApplicationSign + "." + ModuleSign + "." + currNumRel);
							if (oldIdxLev3 >= 0)
								ModGraphLevel3.AddEdge(oldIdxLev3, currIdxLev3, currNumRel);
						}
					}
					break;
				}
			}
		
			// per le dipendenze
			if (xStep.HasChildNodes)
			{
				XmlNodeList xChild = xStep.SelectNodes(Create_UpgradeInfoXML.Element.Dependency);
				if (xChild == null) 
					return;

				foreach (XmlElement xElem in xChild)
				{
					if (ParseForCreate)
						ParseDependencyCreate(xElem, numLevel);
					else
						ParseDependencyUpgrade(xElem, numLevel);
				}
			}
		}	
		#endregion
	}
	# endregion

	#region Classe EntryDBInfo
	/// <summary>
	/// classe comune per gestire le informazioni relative al singolo entry di database
	/// (gestita a livello di Tables, Views e StoredProcedures)
	/// </summary>
	//============================================================================
	public class EntryDBInfo
	{
		private string name	= string.Empty;
		private string nameSpace = string.Empty;
		private int	rel = 0;
		private int	step = 0;
        private bool masterTable = false;

		public bool	Exist = false;
		public List<AdditionalColumnsInfo> AddColumnsList = null;

		//---------------------------------------------------------------------------
		public string Name { get { return name; } }
		public string Namespace { get { return nameSpace; } }
		public int Rel { get { return rel; } }
		public int Step { get { return step; } }
        public bool MasterTable { get { return masterTable; } set { masterTable = value; } }

		//---------------------------------------------------------------------------
		public EntryDBInfo(string name, string nameSpace, int rel, int step)
		{
			this.name	= name;
			this.nameSpace = nameSpace;
			this.rel	= rel;
			this.step	= step;
			AddColumnsList = new List<AdditionalColumnsInfo>();
		}
	}
	#endregion

	#region Classe ModuleDBInfo
	/// <summary>
	/// classe comune per gestire le informazioni relative al singolo Modulo
	/// </summary>
	//============================================================================
	public class ModuleDBInfo
	{
		public List<EntryDBInfo> TablesList = new List<EntryDBInfo>();
		public List<EntryDBInfo> ViewsList = new List<EntryDBInfo>();
		public List<EntryDBInfo> ProceduresList = new List<EntryDBInfo>();
		public List<EntryDBInfo> MissingEntryList = new List<EntryDBInfo>();

		public UpdateInfo	UpdateInfo		= null;
		public RecoveryInfo RecoveryInfo	= null;

		public bool			IsNew			= false; // gestione moduli mancanti in un db gia' esistente
		public int			NumExistTables	= 0;

		public string		ModuleName			= string.Empty; // nome modulo
		public string		ApplicationMember	= string.Empty; // nome applicazione a cui appartiene il modulo
		public string		ApplicationBrand	= string.Empty; // nome applicazione brandizzata (serve SOLO in visualizzazione!)
		public string		Title				= string.Empty; // title modulo (localizzato e tradotto in lingua)

		public string		XmlPath			= string.Empty; 
		public string		DirectoryScript	= string.Empty;
		public bool			StatusOk		= true;

		// signature e release dal DatabaseObjects
		public string		ApplicationSign	= string.Empty;
		public string		ModuleSign		= string.Empty;
		public int			DBRelease		= 0;
		//

		// PreviousSignature dal DatabaseObjects (per gestire lo spostamento dei moduli tra addon)
		public string PreviousApplication	= string.Empty;
		public string PreviousModule		= string.Empty;
		//

		// inizializzati a 1 xchè eventualmente cerco di ricrearli da zero (release 1 + step 1)
		public int			NrLevel			= 1; // per avere traccia del nr. di livello, nr. step
		public int			NrStep			= 1; // e nr. release analizzato
		public int			NrRelease		= 1; // dove si è verificato un errore

		// se nella DBMark è già presente un entry per quell'application + module + dbrelease
		// (casi di upgrade non contestuali con dipendenze da altri moduli)
		public bool			EntryOnlyInDBMark	= false; 
		
		// mi serve per tenere traccia al verificarsi di un errorenei file xml di configurazione
		// (errore di parse, il file non esiste, mancano i nodi DBRel, etc.)
		public bool			ErrorInFileXML		= false; 

		// per tenere traccia degli errori in fase di parse inviati dal pathfinder
		// (relativamente ai file di configurazione DatabaseObjects.xml)
		public string		PathErrorFile	= string.Empty;
		public string		ErrorDescription= string.Empty;
		public bool			Valid			= false;

		//---------------------------------------------------------------------------
		public ModuleDBInfo(string modName)
		{
			ModuleName = modName;
		}

		/// <summary>
		/// metodo che crea un oggetto di tipo UpdateInfo e valorizza variabili che servono 
		/// per il parsing dentro l'UpdateInfo
		/// </summary>
		//---------------------------------------------------------------------------
		public void CreateUpdateInfoInstance()
		{
			this.UpdateInfo = new UpdateInfo(this.ApplicationSign, this.ModuleSign, this.DBRelease);
		}

		/// <summary>
		/// funzione che analizza tutte le informazioni di tipo DatabaseObjectsInfo,
		/// dopo aver parsato il file DatabaseObjects.xml
		/// </summary>
		//---------------------------------------------------------------------------
		public bool LoadDatabaseObjectsInfo(DatabaseObjectsInfo databaseObjInfo)
		{		
			if (databaseObjInfo == null)
				return false;
			
			// effettuo un ulteriore controllo a livello di contenuti:
			// - Signature != empty 
			// - Release > 0
			if (!string.IsNullOrWhiteSpace(databaseObjInfo.Signature) && databaseObjInfo.Release > 0)
			{
				EntryDBInfo	entryDBInfo = null;

				if (databaseObjInfo.TableInfoArray != null)
					foreach (TableInfo table in databaseObjInfo.TableInfoArray)
					{
						entryDBInfo = new EntryDBInfo(table.Name, table.Namespace, table.Release, table.Createstep);
                        entryDBInfo.MasterTable = table.MasterTable;
						TablesList.Add(entryDBInfo);
					}

				if (databaseObjInfo.ViewInfoArray != null)
					foreach (ViewInfo view in databaseObjInfo.ViewInfoArray)
					{
						entryDBInfo = new EntryDBInfo(view.Name, view.Namespace, view.Release, view.Createstep);
						ViewsList.Add(entryDBInfo);
					}

				if (databaseObjInfo.ProcedureInfoArray != null)
					foreach (ProcedureInfo proc in databaseObjInfo.ProcedureInfoArray)
					{
						entryDBInfo = new EntryDBInfo(proc.Name, proc.Namespace, proc.Release, proc.Createstep);
						ProceduresList.Add(entryDBInfo);
					}
			}
			else
			{
				databaseObjInfo.ParsingError = string.Format(DatabaseManagerStrings.ErrorSignatureOrReleaseNotValid, this.Title, this.ApplicationBrand);
				return false;
			}

			return true;
		}
	}
	#endregion

	#region Classe AddOnApplicationDBInfo
	/// <summary>
	/// classe comune per gestire le informazioni relative alla singola AddOnApplication
	/// </summary>
	//============================================================================
	public class AddOnApplicationDBInfo
	{
		public List<ModuleDBInfo> ModuleList = new List<ModuleDBInfo>();

		public string	ApplicationName		= string.Empty;
		public string	BrandTitle			= string.Empty;
		public bool		AllTablesAbsent		= true;
		public bool		AllTablesPresent	= true;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="appName">nome applicazione</param>
		/// <param name="title">nome applicazione brandizzata</param>
		//---------------------------------------------------------------------------
		public AddOnApplicationDBInfo(string appName, string title)
		{
			ApplicationName = appName;
			BrandTitle		= title;
		}
	}
	#endregion

	#region Classe SingleUpdateInfo
	/// <summary>
	/// classe di appoggio per tenere traccia degli step e relativi script da eseguire
	/// suddivisi per modulo + numero release + lev1 + lev2
	/// </summary>
	// lo status serve per sapere se tutti gli step di un upgrade possono essere 
	// effettuati e quindi se l'intero upgrade è valido
	// i valori dello status sono i seguenti:
	// 0: errore *** 1: ok
	//============================================================================
	public class SingleUpdateInfo
	{
		public int	DBRel	= 0; // numero di release di dbrel nell'UpgradeInfo.xml
		public bool	Status	= true;

		public List<string>	ScriptLevel1List = new List<string>();
		public List<string> ScriptLevel2List = new List<string>();
		public List<string> ScriptLevel3List = new List<string>();
		public List<DefaultDataStep> DefaultDataStepList = new List<DefaultDataStep>();

		//---------------------------------------------------------------------------
		public SingleUpdateInfo()
		{ }

		/// <summary>
		/// costruttore classe SingleUpdateInfo
		/// </summary>
		/// <param name="rel">numero di release</param>
		//---------------------------------------------------------------------------
		public SingleUpdateInfo(int rel)
		{
			DBRel = rel;
		}
	}
	#endregion

	#region Classe UpdateInfo e parsing dei file (CreateInfo.xml e UpgradeInfo.xml)
	/// <summary>
	/// definizione della classe UpdateInfo
	/// </summary>
	//============================================================================
	public class UpdateInfo
	{
		protected	XmlDocument	xDoc			= null;
		public		bool		ParseForCreate	= true;
		
		// serve per rintracciare la signature dell'applicazione+modulo quando sono in fase di parsing
		// dei nodi <Dependency>... non è bello passarlo dall'esterno ma non mi viene in mente un modo più furbo
		protected List<AddOnApplicationDBInfo> addOnAppList = null;
		protected DBMarkInfo dbMarkInfo = null;
		//

		public List<SingleUpdateInfo> UpdateInfoList = null;
		public DirectGraph ModGraphLevel1 = null;
		public DirectGraph ModGraphLevel2 = null;
		public DirectGraph ModGraphLevel3 = null;

		// variabili private utilizzate durante i parse dei file xml
		protected int currNumRel = 0;  // numero di release del nodo DBRel corrente
		protected int currIdxLev1 = -1; // per ottimizzare l'inserimento dei vertici nel grafo 1
		protected int currIdxLev2 = -1; // per ottimizzare l'inserimento dei vertici nel grafo 2
		protected int currIdxLev3 = -1; // per ottimizzare l'inserimento dei vertici nel grafo 3

		protected int oldIdxLev1 = -1; // per tenere traccia della dipendenza dei vari DBRel
		protected int oldIdxLev2 = -1; // all'interno dello stesso file UpgradeInfo.xml
		protected int oldIdxLev3 = -1; // all'interno dello stesso file UpgradeInfo.xml

		private bool level1Parsed = false;
		private bool level2Parsed = false;
		private bool level3Parsed = false;

		protected string error = string.Empty;
		protected string xmlFile = string.Empty;
		//
		public SingleUpdateInfo CurrSingleUpdate= null;

		// data-member in cui memorizzo l'application name e il module name che sto analizzando
		protected string	ApplicationSign	= string.Empty;
		protected string	ModuleSign		= string.Empty;
		protected int		DbRel			= 0;

		// mi tengo da parte se il db da aggiornare e' di una versione pre 4.0
		protected DatabaseStatus DBSourceStatus = DatabaseStatus.EMPTY; 

		// data-member in cui sono memorizzati i valori ritornati dal SqlDataReader
		// dopo averli letti dalla tabella TB_DBMark
		public	string		DbMarkSignature	= string.Empty;
		public	string		DbMarkModule	= string.Empty;
		public	int			DbMarkRel		= 0;

		protected PathFinder finder = null;	
		protected string provider = string.Empty;
		
		public int CurrRel = 0;	// nr. di scatto di release che sto effettuando, x individuare gli array di script corretti

		private bool isForNewModule = false; //  per tenere traccia se si tratta di un modulo nuovo per un db esistente

		// mi serve per inserire nell'output la struttura delle dipendenze (a solo scopo debug)
		//----------------------------------------------------
		public bool WriteLogInfo = false; // di default e' a false

		//---------------------------------------------------------------------------
		public UpdateInfo() { }
		
		//---------------------------------------------------------------------------
		public UpdateInfo(string app, string mod, int rel)
		{
			ApplicationSign	= app;
			ModuleSign		= mod;
			DbRel			= rel;
		}
		
		/// <summary>
		/// effettua il parse del singolo file CreateInfo.xml / UpgradeInfo.xml
		/// </summary>
		//---------------------------------------------------------------------------
		public void Parse
			(
			CheckDBStructureInfo dbStructInfo,
			string		fileXML, 
			bool		create,
			out string	error,
			bool forNewModule
			)
		{
			UpdateInfoList = new List<SingleUpdateInfo>();

			this.DBSourceStatus = dbStructInfo.DBStatus;
			this.addOnAppList = dbStructInfo.AddOnAppList;
			this.isForNewModule = forNewModule;
			this.dbMarkInfo = dbStructInfo.DBMarkInfo;

			error = string.Empty;
			xmlFile = fileXML;
			
			xDoc = new XmlDocument();
			
			try
			{
                // leggo il file
                xDoc = PathFinder.PathFinderInstance.LoadXmlDocument(xDoc, xmlFile);
            }
			catch(XmlException e)
			{
				// errore nella Load del file
				error = string.Format(DatabaseManagerStrings.ErrorDuringParsingXmlFile, fileXML, e.LineNumber, e.LinePosition);
				return;
			}

			AppendTextToOutput("\r\n");
			AppendTextToOutput("//***********************************************************************************\\");
			AppendTextToOutput("Loading file :" + fileXML);
			AppendTextToOutput(create ? "------- FOR CREATE" : "------- FOR UPGRADE");

			ParseForCreate = create;
			ModGraphLevel1 = dbStructInfo.GraphLevel1;
			ModGraphLevel2 = dbStructInfo.GraphLevel2;
			ModGraphLevel3 = dbStructInfo.GraphLevel3;

			// root
			XmlElement root = xDoc.DocumentElement;

			if (root == null)
			{
				error = string.Format(DatabaseManagerStrings.ErrorXmlSyntaxError, fileXML);
				return;
			}

			if (root.HasChildNodes)
			{
				// solo se sono in fase di upgrade controllo prima che l'attributo NumRel
				// dell'ultimo nodo di tipo DBRel contenga un valore (intero) maggiore
				// di quello corrispondente nella tabella TB_DBMark, altrimenti non proseguo
				// nel parse completo del file, xchè non è necessario.
				if (!ParseForCreate)
				{
					XmlNodeList xNodeList = xDoc.GetElementsByTagName(Create_UpgradeInfoXML.Element.DBRel);
					if (xNodeList == null) 
						return;

					int rel = Convert.ToInt32(xNodeList[xNodeList.Count - 1].Attributes.GetNamedItem(Create_UpgradeInfoXML.Attribute.Numrel).InnerText);

					if (rel <= DbMarkRel)
					{
						error = string.Format(DatabaseManagerStrings.ErrorMissingDBRelInUpgradeInfo, rel, this.ModuleSign, this.ApplicationSign);
						return;
					}
				}
				
				// se sto parsando il Create cerco tutti i nodi il cui nome inizia con Level (= 1 o 2 o 3)
				// altrimenti cerco i nodi di tipo DBRel
				XmlNodeList xlevels = 
					(ParseForCreate)
					? root.SelectNodes("node()[starts-with(name(), 'Level')]")
					: xDoc.GetElementsByTagName(Create_UpgradeInfoXML.Element.DBRel);

				if (xlevels == null) 
					return;

				oldIdxLev1 = oldIdxLev2 = oldIdxLev3 = -1;

				foreach (XmlElement xElem in xlevels)
				{
					// sto parsando il file CreateInfo.xml
					if (ParseForCreate)
					{
						CurrSingleUpdate = new SingleUpdateInfo();
						currIdxLev1 = currIdxLev2 = currIdxLev3 = -1;
						CurrSingleUpdate.DBRel = 1;

						if (
							xElem.Name == Create_UpgradeInfoXML.Element.Level1	|| 
							xElem.Name == Create_UpgradeInfoXML.Element.Level2	||
							xElem.Name == Create_UpgradeInfoXML.Element.Level3
							)
							ParseLevel(xElem);

						if (
							CurrSingleUpdate.ScriptLevel1List.Count > 0 ||
							CurrSingleUpdate.ScriptLevel2List.Count > 0 ||
							CurrSingleUpdate.ScriptLevel3List.Count > 0
							)
							UpdateInfoList.Add(CurrSingleUpdate);
						else
							UpdateInfoList.Remove(CurrSingleUpdate);
					}
					else // sto parsando il file UpgradeInfo.xml
					{
						if (xElem.Name == Create_UpgradeInfoXML.Element.DBRel)
						{
							if (ParseDBRel(xElem))
							{
								if (level1Parsed)
								{
									oldIdxLev1 = currIdxLev1;
									level1Parsed = false;
								}

								if (level2Parsed)
								{
									oldIdxLev2 = currIdxLev2;
									level2Parsed = false;
								}

								if (level3Parsed)
								{
									oldIdxLev3 = currIdxLev3;
									level3Parsed = false;
								}
							}
						}
					}
				}
			}

			// devo assegnare l'eventuale errore riscontrato all'esterno
			error = this.error;
		}
				
		/// <summary>
		/// per il parse del singolo nodo di tipo DBRel
		/// </summary>
		/// <param name="xDBRel"></param>
		//---------------------------------------------------------------------------
		private bool ParseDBRel(XmlElement xDBRel)
		{
			// guardo il valore dell'intero contenuto nell'attributo del nodo DBRel in analisi
			// se questo è minore/uguale a quello della tabella TB_DBMark non proseguo
			// inoltre controllo di effettuare solo i passaggi di release indicati nel DatabaseObjects.xml
			// e non i successivi (in quest'ultimo caso dovrò poi segnalare che c'era un'irregolarità
			// tra quello indicato nel DatabaseObjects.xml e quello indicato nell'UpgradeInfo.xml)
			currNumRel = Convert.ToInt32(xDBRel.GetAttribute(Create_UpgradeInfoXML.Attribute.Numrel));
			if (DbMarkRel >= currNumRel || DbRel < currNumRel)
				return false;

			// se trovo nell'array degli update gia' la presenza di quello scatto di release lo skippo
			foreach (SingleUpdateInfo sui in UpdateInfoList)
				if (sui.DBRel == currNumRel)
					return false;

			if ((DBSourceStatus & DatabaseStatus.PRE_40) == DatabaseStatus.PRE_40)
			{
				string sOldNumRel = xDBRel.GetAttribute(Create_UpgradeInfoXML.Attribute.Oldnumrel);
				if (!string.IsNullOrWhiteSpace(sOldNumRel))
				{
					int oldNumRel = Convert.ToInt32(sOldNumRel);
					if (this.DbMarkRel >= oldNumRel) //skippo lo script
						return true;
				}
			}

			CurrSingleUpdate = new SingleUpdateInfo(currNumRel);

			currIdxLev1 = currIdxLev2 = currIdxLev3 = -1;

			if (xDBRel.HasChildNodes)
			{
				XmlNodeList xChild = xDBRel.SelectNodes("node()[starts-with(name(), 'Level')]");
				if (xChild == null) 
					return false;

				foreach (XmlElement xElem in xChild)
					ParseLevel(xElem);

				if (
					CurrSingleUpdate.ScriptLevel1List.Count > 0 ||
					CurrSingleUpdate.ScriptLevel2List.Count > 0 ||
					CurrSingleUpdate.ScriptLevel3List.Count > 0 ||
					CurrSingleUpdate.DefaultDataStepList.Count > 0
					)
					UpdateInfoList.Add(CurrSingleUpdate);
				else
					UpdateInfoList.Remove(CurrSingleUpdate);
			}

			return true;
		}

		/// <summary>
		/// per il parse del singolo nodo di tipo Level (comune x tutti i livelli)
		/// </summary>
		//--------------------------------------------------------------------------
		private void ParseLevel(XmlElement xLevel)
		{
			int numLevel = 0;

			switch (xLevel.Name)
			{
				case Create_UpgradeInfoXML.Element.Level1:
					numLevel = 1;
					break;
				case Create_UpgradeInfoXML.Element.Level2:
					numLevel = 2;
					break;
				case Create_UpgradeInfoXML.Element.Level3:
					numLevel = 3;
					break;
				default:
					return;
			}

			if (xLevel.HasChildNodes)
			{
				XmlNodeList xChild = xLevel.SelectNodes(Create_UpgradeInfoXML.Element.Step);
				if (xChild == null) 
					return;

				foreach (XmlElement xElem in xChild)
					ParseStep(xElem, numLevel);
			}
		}

		/// <summary>
		/// per il parse del singolo nodo di tipo Step
		/// </summary>
		//--------------------------------------------------------------------------
		private void ParseStep(XmlElement xStep, int numLevel)
		{
			int nrstep = Convert.ToInt32(xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Numstep));
			string script = (numLevel == 3) 
							? xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Library) 
							: xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Script);

			bool isDefaultDataStep = false;
			DefaultDataStep defaultDataStep = null;

			// se l'attributo e' vuoto vado a controllare se si tratta di uno step per i dati di default
			if (string.IsNullOrWhiteSpace(script))
			{
				string tableAttribute = xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Table);
				string configurationAttribute = xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Configuration);
				string overwriteAttribute = xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Overwrite);
				bool overwriteValue = (string.IsNullOrEmpty(overwriteAttribute)) ? true : Convert.ToBoolean(overwriteAttribute); // N.B. di default vado in update
				string countryAttribute = xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Country);
				string countryValue = (string.IsNullOrWhiteSpace(countryAttribute) || countryAttribute == "*") ? string.Empty : countryAttribute;

				// se gli attributi previsti 
				if (string.IsNullOrWhiteSpace(tableAttribute) || string.IsNullOrWhiteSpace(configurationAttribute))
				{
					this.error = string.Format
								(
								DatabaseManagerStrings.MissingAttribute,
								(numLevel == 3) ? Create_UpgradeInfoXML.Attribute.Library : Create_UpgradeInfoXML.Attribute.Script,
								nrstep.ToString(),
								xmlFile
								);
					return;
				}

				isDefaultDataStep = true;
				defaultDataStep = new DefaultDataStep(tableAttribute, configurationAttribute, countryValue, overwriteValue);
			}

			switch (numLevel)
			{
				case 1:
				{
					level1Parsed = true;

					if (isDefaultDataStep)
					{
						CurrSingleUpdate.DefaultDataStepList.Add(defaultDataStep);
						AppendTextToOutput(string.Format("ParseStep method default step {0}: for table: {1} - configuration: {2})", nrstep, defaultDataStep.Table, defaultDataStep.Configuration));
					}
					else
					{
						if (CurrSingleUpdate.ScriptLevel1List.Contains(script + ";" + nrstep))
							return;

						CurrSingleUpdate.ScriptLevel1List.Add(script + ";" + nrstep);

						AppendTextToOutput("ParseStep method: " + script + ";" + nrstep);
					}

					if (ParseForCreate)
					{
						if (currIdxLev1 == -1)
						{
							if (!this.isForNewModule)
							{
								currIdxLev1 = ModGraphLevel1.AddVertex(ApplicationSign + "." + ModuleSign + "." + "1");
								
								AppendTextToOutput("AddVertex for step create: " + ApplicationSign + "." + ModuleSign + "." + "1");
								AppendTextToOutput("Vertex ID = " + currIdxLev1.ToString());
								
								if (oldIdxLev1 >= 0)
								{
									ModGraphLevel1.AddEdge(oldIdxLev1, currIdxLev1);
									AppendTextToOutput("AddEdge for step create: " + oldIdxLev1.ToString() + " => " + currIdxLev1.ToString());
								}
							}
							else
							{
								int rel = -1;
								// per ottenere la release di arrivo del modulo date le signature
								GetReleaseForModuleSignature(ApplicationSign, ModuleSign, out rel);
								currIdxLev1 = ModGraphLevel1.AddVertex(ApplicationSign + "." + ModuleSign + "." + rel);

								AppendTextToOutput("AddVertex for step create: " + ApplicationSign + "." + ModuleSign + "." + rel.ToString());
								AppendTextToOutput("Vertex ID = " + currIdxLev1.ToString());
							}
						}	
					}	
					else
					{
						if (currIdxLev1 == -1)
						{
							currIdxLev1 = ModGraphLevel1.AddVertex(DbMarkSignature + "." + DbMarkModule + "." + currNumRel);
							
							AppendTextToOutput("AddVertex for step upgrade: " + DbMarkSignature + "." + DbMarkModule + "." + currNumRel.ToString());
							AppendTextToOutput("Vertex ID = " + currIdxLev1.ToString());

							if (oldIdxLev1 >= 0)
							{
								ModGraphLevel1.AddEdge(oldIdxLev1, currIdxLev1, currNumRel);
								AppendTextToOutput("AddEdge for step upgrade: " + oldIdxLev1.ToString() + " => " + currIdxLev1.ToString() + " => " + currNumRel.ToString());
							}
						}	
					}
					break;
				}
				
				case 2:
				{
					if (CurrSingleUpdate.ScriptLevel2List.Contains(script + ";" + nrstep))
						return;

					level2Parsed = true;

					CurrSingleUpdate.ScriptLevel2List.Add(script + ";" + nrstep);

					AppendTextToOutput("ParseStep method: " + script + ";" + nrstep);

					if (ParseForCreate)
					{
						if (currIdxLev2 == -1)
						{
							if (!this.isForNewModule)
							{
								currIdxLev2 = ModGraphLevel2.AddVertex(ApplicationSign + "." + ModuleSign + "." + "1");
								AppendTextToOutput("AddVertex for step create:" + ApplicationSign + "." + ModuleSign + "." + "1");
								if (oldIdxLev2 >= 0)
								{
									ModGraphLevel2.AddEdge(oldIdxLev2, currIdxLev2);
									AppendTextToOutput("AddEdge for step create:" + oldIdxLev2.ToString() + " => " + currIdxLev2.ToString());
								}
							}
							else
							{
								int rel = -1;
								// per ottenere la release di arrivo del modulo date le signature
								GetReleaseForModuleSignature(ApplicationSign, ModuleSign, out rel);
								currIdxLev2 = ModGraphLevel2.AddVertex(ApplicationSign + "." + ModuleSign + "." + rel);

								AppendTextToOutput("AddVertex for step create:" + ApplicationSign + "." + ModuleSign + "." + rel.ToString());
							}
						}
					}	
					else
					{
						if (currIdxLev2 == -1)
						{
							currIdxLev2 = ModGraphLevel2.AddVertex(DbMarkSignature + "." + DbMarkModule + "." + currNumRel);
							if (oldIdxLev2 >= 0)
								ModGraphLevel2.AddEdge(oldIdxLev2, currIdxLev2, currNumRel);
						}
					}
					break;
				}

				case 3:
				{
					if (CurrSingleUpdate.ScriptLevel3List.Contains(script + ";" + nrstep))
						return;

					level3Parsed = true;

					CurrSingleUpdate.ScriptLevel3List.Add(script + ";" + nrstep);

					AppendTextToOutput("ParseStep method: " + script + ";" + nrstep);

					if (ParseForCreate)
					{
						if (currIdxLev3 == -1)
						{
							if (!this.isForNewModule)
							{
								currIdxLev3 = ModGraphLevel3.AddVertex(ApplicationSign + "." + ModuleSign + "." + "1");
								AppendTextToOutput("AddVertex for step create:" + ApplicationSign + "." + ModuleSign + "." + "1");
								if (oldIdxLev3 >= 0)
								{
									ModGraphLevel3.AddEdge(oldIdxLev3, currIdxLev3);
									AppendTextToOutput("AddEdge for step create:" + oldIdxLev3.ToString() + " => " + currIdxLev3.ToString());
								}
							}
							else
							{
								int rel = -1;
								// per ottenere la release di arrivo del modulo date le signature
								GetReleaseForModuleSignature(ApplicationSign, ModuleSign, out rel);
								currIdxLev3 = ModGraphLevel3.AddVertex(ApplicationSign + "." + ModuleSign + "." + rel);

								AppendTextToOutput("AddVertex for step create:" + ApplicationSign + "." + ModuleSign + "." + rel.ToString());
							}
						}
					}
					else
					{
						if (currIdxLev3 == -1)
						{
							currIdxLev3 = ModGraphLevel3.AddVertex(DbMarkSignature + "." + DbMarkModule + "." + currNumRel);
							if (oldIdxLev3 >= 0)
								ModGraphLevel3.AddEdge(oldIdxLev3, currIdxLev3, currNumRel);
						}
					}
					break;
				}
			}
		
			// per le dipendenze
			if (xStep.HasChildNodes)
			{
				XmlNodeList xChild = xStep.SelectNodes(Create_UpgradeInfoXML.Element.Dependency);
				if (xChild == null) 
					return;

				foreach (XmlElement xElem in xChild)
				{
					if (ParseForCreate)
						ParseDependencyCreate(xElem, numLevel);
					else
						ParseDependencyUpgrade(xElem, numLevel);
				}
			}
		}

		/// <summary>
		/// per il parse del singolo nodo di tipo Dependency del file CreateInfo.xml
		/// </summary>
		//--------------------------------------------------------------------------
		protected void ParseDependencyCreate(XmlElement xDep, int numLevel)
		{
			string application = xDep.GetAttribute(Create_UpgradeInfoXML.Attribute.App);
			string module = xDep.GetAttribute(Create_UpgradeInfoXML.Attribute.Module);

			// se gli attributi sono vuoti non procedo
			if (string.IsNullOrEmpty(application) || string.IsNullOrEmpty(module))
			{
				this.error = string.Format(DatabaseManagerStrings.MissingAttributesForTag,Create_UpgradeInfoXML.Element.Dependency,xmlFile);
				return;
			}

			string appSignature, modSignature = string.Empty;
			int index = -1;
			// per ottenere la signature del modulo specificato nel tag Dependency
			GetSignatureForDependencyEntry(application, module, out appSignature, out modSignature);

			switch (numLevel)
			{
				case 1:
				{
					if (!this.isForNewModule)
					{
						index = ModGraphLevel1.AddVertex(appSignature + "." + modSignature + "." + "1");
						AppendTextToOutput("AddVertex for dependency create :" + appSignature + "." + modSignature + "." + "1");
						AppendTextToOutput("Vertex ID = " + index.ToString());

						ModGraphLevel1.AddEdge(index, currIdxLev1);
						AppendTextToOutput("AddEdge for dependency create:" + index.ToString() + " => " + currIdxLev1.ToString());
					}
					else
					{
						int rel = -1;
						// per ottenere la release di arrivo del modulo da cui dipendo
						GetReleaseForModuleSignature(appSignature, modSignature, out rel);

						index = ModGraphLevel1.AddVertex(appSignature + "." + modSignature + "." + rel);
						AppendTextToOutput("AddVertex for step create:" + appSignature + "." + modSignature + "." + rel.ToString());
						AppendTextToOutput("Vertex ID = " + index.ToString());

						ModGraphLevel1.AddEdge(index, currIdxLev1, rel);
						AppendTextToOutput("AddEdge for dependency create:" + index.ToString() + " => " + currIdxLev1.ToString() + " => " + rel.ToString());
					}
					break;
				}
				case 2:
				{
					if (!this.isForNewModule)
					{
						index = ModGraphLevel2.AddVertex(appSignature + "." + modSignature + "." + "1");
						AppendTextToOutput("AddVertex for dependency create :" + appSignature + "." + modSignature + "." + "1");
						ModGraphLevel2.AddEdge(index, currIdxLev2);
						AppendTextToOutput("AddEdge for dependency create:" + index.ToString() + " => " + currIdxLev2.ToString());
					}
					else
					{
						int rel = -1;
						// per ottenere la release di arrivo del modulo da cui dipendo
						GetReleaseForModuleSignature(appSignature, modSignature, out rel);

						index = ModGraphLevel2.AddVertex(appSignature + "." + modSignature + "." + rel);
						AppendTextToOutput("AddVertex for step create:" + appSignature + "." + modSignature + "." + rel.ToString());

						ModGraphLevel2.AddEdge(index, currIdxLev2, rel);
						AppendTextToOutput("AddEdge for dependency create:" + index.ToString() + " => " + currIdxLev2.ToString() + " => " + rel.ToString());
					}
					break;
				}
				case 3:
				{
					if (!this.isForNewModule)
					{
						index = ModGraphLevel3.AddVertex(appSignature + "." + modSignature + "." + "1");
						AppendTextToOutput("AddVertex for dependency create :" + appSignature + "." + modSignature + "." + "1");
						ModGraphLevel3.AddEdge(index, currIdxLev3);
						AppendTextToOutput("AddEdge for dependency create:" + index.ToString() + " => " + currIdxLev3.ToString());
					}
					else
					{
						int rel = -1;
						// per ottenere la release di arrivo del modulo da cui dipendo
						GetReleaseForModuleSignature(appSignature, modSignature, out rel);

						index = ModGraphLevel3.AddVertex(appSignature + "." + modSignature + "." + rel);
						AppendTextToOutput("AddVertex for step create:" + appSignature + "." + modSignature + "." + rel.ToString());

						ModGraphLevel3.AddEdge(index, currIdxLev3, rel);
						AppendTextToOutput("AddEdge for dependency create:" + index.ToString() + " => " + currIdxLev3.ToString() + " => " + rel.ToString());
					}
					break;
				}
			}
		}

		/// <summary>
		/// per il parse del singolo nodo di tipo Dependency del file UpgradeInfo.xml
		/// </summary>
		//--------------------------------------------------------------------------
		protected void ParseDependencyUpgrade(XmlElement xDep, int numLevel)
		{
			string app		= xDep.GetAttribute(Create_UpgradeInfoXML.Attribute.App);
			string mod		= xDep.GetAttribute(Create_UpgradeInfoXML.Attribute.Module);
			string rel		= xDep.GetAttribute(Create_UpgradeInfoXML.Attribute.Rel);
			string oldrel	= xDep.GetAttribute(Create_UpgradeInfoXML.Attribute.Oldrel);

			// se gli attributi sono vuoti non procedo
			if (string.IsNullOrEmpty(app) || string.IsNullOrEmpty(app))
			{
				this.error = string.Format(DatabaseManagerStrings.MissingAttributesForTag, Create_UpgradeInfoXML.Element.Dependency, xmlFile);
				return;
			}

			string appSignature, modSignature = string.Empty;
			int index = -1;
			int numRel = -1; 

			// per ottenere la signature del modulo specificato nel tag Dependency
			GetSignatureForDependencyEntry(app, mod, out appSignature, out modSignature);

			// se il nr di release non e' stato indicato nel nodo <Dependency>
			// devo considerare la release di arrivo nel DatabaseObjects.xml
			// ma solo per i moduli che non sono ancora stati creati!
			if (string.IsNullOrEmpty(rel))
			{
				GetReleaseForModuleSignature(appSignature, modSignature, out numRel);
				int relFromDBMark = dbMarkInfo.DBMarkTable.GetDBReleaseFromDBMark(appSignature, modSignature);
				if (relFromDBMark != -1)
					return; // significa che si tratta di modulo esistente in aggiornamento, quindi ritorno
			}
			else
			{
				numRel = Convert.ToInt32(rel);

				if ((DBSourceStatus & DatabaseStatus.PRE_40) == DatabaseStatus.PRE_40)
				{
					if (!string.IsNullOrWhiteSpace(oldrel))
					{
						int nOldRel = Convert.ToInt32(oldrel);
						if (this.DbMarkRel >= nOldRel) //skippo lo script
							return;
					}
				}
            }

			switch (numLevel)
			{
				case 1:
				{
					index = ModGraphLevel1.AddVertex(appSignature + "." + modSignature + "." + numRel);
					AppendTextToOutput("AddVertex for dependency upgrade :" + appSignature + "." + modSignature + "." + numRel.ToString());
					AppendTextToOutput("Vertex ID = " + index.ToString());

					ModGraphLevel1.AddEdge(index, currIdxLev1, numRel);
					AppendTextToOutput("AddEdge for dependency upgrade:" + index.ToString() + " => " + currIdxLev1.ToString() + " => " + numRel.ToString());
					break;
				}
				case 2:
				{
					index = ModGraphLevel2.AddVertex(appSignature + "." + modSignature + "." + numRel);
					ModGraphLevel2.AddEdge(index, currIdxLev2, numRel);
					break;
				}
				case 3:
				{
					index = ModGraphLevel3.AddVertex(appSignature + "." + modSignature + "." + numRel);
					ModGraphLevel3.AddEdge(index, currIdxLev3, numRel);
					break;
				}
			}
		}

		/// <summary>
		/// faccio la load del singolo file xml solo la prima volta
		/// utilizzato solo per trovare lo step specifico per creare un oggetto mancante
		/// </summary>
		//--------------------------------------------------------------------------
		public void LoadSingleXML(CheckDBStructureInfo dbStructInfo, string xmlPath, out string error)
		{
			if (this.dbMarkInfo == null)
				this.dbMarkInfo = dbStructInfo.DBMarkInfo;

			error = string.Empty;
			xmlFile = xmlPath;

			xDoc = new XmlDocument();

			try
			{
                xDoc = PathFinder.PathFinderInstance.LoadXmlDocument(xDoc, xmlFile);
            }
			catch (XmlException e)
			{
				error = string.Format(DatabaseManagerStrings.ErrorDuringParsingXmlFile, xmlPath, e.LineNumber, e.LinePosition);
				return;
			}
		}

		/// <summary>
		/// cerco tramite espressioni di XPath di trovare il singolo nodo dello step
		/// che mi serve. il parametro booleano mi dice se sono in create o in upgrade.
		/// utilizzato solo per trovare lo step specifico per creare un oggetto mancante
		/// </summary>
		//--------------------------------------------------------------------------
		public void ParseSingleXML
			(
			EntryDBInfo				e, 
			AdditionalColumnsInfo	eC, 
			bool					create, 
			string					fullPath, 
			PathFinder				pathFinder, 
			string					providerName,
			List<AddOnApplicationDBInfo> addOnAppList,
			out string				error
			)
		{
			error = string.Empty;

			if (xDoc == null)
				return;

			ParseForCreate		= create;
			finder				= pathFinder;
			provider			= providerName;
			this.addOnAppList	= addOnAppList;
			
			int myRel, myStep;
			string aS = string.Empty, mS = string.Empty;
			if (eC != null)
			{
				myRel			= eC.NumRelease;
				myStep			= eC.NumStep;
				// backup
				aS				= ApplicationSign;
				mS				= ModuleSign;
				//
				ApplicationSign = eC.AppSignature;
				ModuleSign		= eC.ModSignature;
			}
			else
			{
				myRel	= e.Rel;
				myStep	= e.Step;
			}

			if (ParseForCreate)
			{
				XmlNodeList nodeList;
				XmlElement root = xDoc.DocumentElement;
				if (root == null)
				{
					error = string.Format(DatabaseManagerStrings.ErrorXmlSyntaxError, fullPath);
					return;
				}
				
				nodeList = root.SelectNodes(string.Format("//Step[@numstep='" + myStep + "']"));
				if (nodeList == null || nodeList.Count == 0) 
				{
					error = string.Format(DatabaseManagerStrings.ErrorMissingStepInCreateInfo, this.ModuleSign, this.ApplicationSign, myStep);
					return;
				}

				oldIdxLev1 = oldIdxLev2 = oldIdxLev3 = -1;
				currIdxLev1 = currIdxLev2 = currIdxLev3 = -1;
				
				CurrSingleUpdate.DBRel = myRel;
				currNumRel = 1; // ha senso mettere il myRel????
				
				foreach (XmlElement elem in nodeList)
				{
					switch (elem.ParentNode.Name)
					{
						case Create_UpgradeInfoXML.Element.Level1:
							ParseSingleStep(elem, 1, fullPath);
							break;
						case Create_UpgradeInfoXML.Element.Level2:
							ParseSingleStep(elem, 2, fullPath);
							break;
						case Create_UpgradeInfoXML.Element.Level3:
							ParseSingleStep(elem, 3, fullPath);
							break;
						default:
							break;
					}
				}
			}
			else
			{
				XmlNode nodeDBRel = xDoc.SelectSingleNode(string.Format("//DBRel[@numrel='{0}']", myRel));
				if (nodeDBRel == null)
				{
					error = string.Format(DatabaseManagerStrings.ErrorMissingDBRelInUpgradeInfo, myRel, this.ModuleSign, this.ApplicationSign);
					return;
				}

				oldIdxLev1 = oldIdxLev2 = oldIdxLev3 = -1;
				currIdxLev1 = currIdxLev2 = currIdxLev3 = -1;

				CurrSingleUpdate.DBRel = myRel;
				currNumRel = myRel;

				if (nodeDBRel.HasChildNodes)
				{
					// analizzo prima gli step del nodo Level1
					XmlNode nodeLev1 = nodeDBRel.SelectSingleNode(string.Format("Level1/Step[@numstep='{0}']", myStep));
					if (nodeLev1 != null)
						ParseSingleStep((XmlElement)nodeLev1, 1, fullPath);
					else
						error = string.Format(DatabaseManagerStrings.ErrorMissingStepInUpgradeInfo, this.ModuleSign, this.ApplicationSign, myStep, myRel);

					// analizzo gli step del nodo Level2
					XmlNode nodeLev2 = nodeDBRel.SelectSingleNode(string.Format("Level2/Step[@numstep='{0}']", myStep));
					if (nodeLev2 != null)
					{
						ParseSingleStep((XmlElement)nodeLev2, 2, fullPath);
						error = string.Empty;
					}

					// analizzo gli step del nodo Level3
					XmlNode nodeLev3 = nodeDBRel.SelectSingleNode(string.Format("Level3/Step[@numstep='{0}']", myStep));
					if (nodeLev3 != null)
						ParseSingleStep((XmlElement)nodeLev3, 3, fullPath);
				}
			}

			if (eC != null)
			{
				// backup
				ApplicationSign = aS;
				ModuleSign		= mS;
				//
			}

			// per propagare all'esterno gli eventuali errori di parse
			error = this.error;
		}

		/// <summary>
		/// per il parse del singolo nodo di tipo Step, relativo ad uno specifico numero di step
		/// </summary>
		//--------------------------------------------------------------------------
		private void ParseSingleStep(XmlElement xStep, int numLevel, string fullPath)
		{
			int nrstep = Convert.ToInt32(xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Numstep));

			string script = finder.GetStandardScriptPath
				(
				fullPath,
				(numLevel == 3) ? xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Library) : xStep.GetAttribute(Create_UpgradeInfoXML.Attribute.Script),
				provider,
				ParseForCreate,
				CurrSingleUpdate.DBRel
				);	

			// se l'attributo e' vuoto non procedo
			if (string.IsNullOrEmpty(script))
			{
				this.error = string.Format
					(
					DatabaseManagerStrings.MissingAttribute,
					(numLevel == 3) ? Create_UpgradeInfoXML.Attribute.Library : Create_UpgradeInfoXML.Attribute.Script,
					nrstep.ToString(),
					fullPath
					);
				return;
			}
			
			switch (numLevel)
			{
				case 1:
				{
					if (CurrSingleUpdate.ScriptLevel1List.Contains(script + ";" + nrstep))
						return; 

					CurrSingleUpdate.ScriptLevel1List.Add(script + ";" + nrstep);

					if (ParseForCreate)
					{
						if (currIdxLev1 == -1)
						{
							currIdxLev1 = ModGraphLevel1.AddVertex(ApplicationSign + "." + ModuleSign + "." + currNumRel);
							if (oldIdxLev1 >= 0)
								ModGraphLevel1.AddEdge(oldIdxLev1, currIdxLev1);
						}	
					}	
					else
					{
						if (currIdxLev1 == -1)
						{
							//currIdxLev1 = ModGraphLevel1.AddVertex(DbMarkSignature + "." + DbMarkModule + "." + currNumRel);
							currIdxLev1 = ModGraphLevel1.AddVertex(ApplicationSign + "." + ModuleSign + "." + currNumRel);
							if (oldIdxLev1 >= 0)
								ModGraphLevel1.AddEdge(oldIdxLev1, currIdxLev1, currNumRel);
						}	
					}
					break;
				}
				
				case 2:
				{
					if (CurrSingleUpdate.ScriptLevel2List.Contains(script + ";" + nrstep))
						return;

					CurrSingleUpdate.ScriptLevel2List.Add(script + ";" + nrstep);

					if (ParseForCreate)
					{
						if (currIdxLev2 == -1)
						{
							currIdxLev2 = ModGraphLevel2.AddVertex(ApplicationSign + "." + ModuleSign + "." + currNumRel);
							if (oldIdxLev2 >= 0)
								ModGraphLevel2.AddEdge(oldIdxLev2, currIdxLev2);
						}
					}	
					else
					{
						if (currIdxLev2 == -1)
						{
							//currIdxLev2 = ModGraphLevel2.AddVertex(DbMarkSignature + "." + DbMarkModule + "." + currNumRel);
							currIdxLev2 = ModGraphLevel2.AddVertex(ApplicationSign + "." + ModuleSign + "." + currNumRel);
							if (oldIdxLev2 >= 0)
								ModGraphLevel2.AddEdge(oldIdxLev2, currIdxLev2, currNumRel);
						}
					}
					break;
				}

				case 3:
				{
					if (CurrSingleUpdate.ScriptLevel3List.Contains(script + ";" + nrstep))
						return;

					CurrSingleUpdate.ScriptLevel3List.Add(script + ";" + nrstep);

					if (ParseForCreate)
					{
						if (currIdxLev3 == -1)
						{
							currIdxLev3 = ModGraphLevel3.AddVertex(ApplicationSign + "." + ModuleSign + "." + currNumRel);
							if (oldIdxLev3 >= 0)
								ModGraphLevel3.AddEdge(oldIdxLev3, currIdxLev3);
						}
					}
					else
					{
						if (currIdxLev3 == -1)
						{
							// currIdxLev3 = ModGraphLevel3.AddVertex(DbMarkSignature + "." + DbMarkModule + "." + currNumRel);
							currIdxLev3 = ModGraphLevel3.AddVertex(ApplicationSign + "." + ModuleSign + "." + currNumRel);
							if (oldIdxLev3 >= 0)
								ModGraphLevel3.AddEdge(oldIdxLev3, currIdxLev3, currNumRel);
						}
					}
					break;
				}
			}
		
			// per le dipendenze
			if (xStep.HasChildNodes)
			{
				XmlNodeList xChild = xStep.SelectNodes(Create_UpgradeInfoXML.Element.Dependency);
				if (xChild == null) 
					return;

				foreach (XmlElement xElem in xChild)
				{
					if (ParseForCreate)
						ParseDependencyCreate(xElem, numLevel);
					else
						ParseDependencyUpgrade(xElem, numLevel);
				}
			}
		}

		///<summary>
		/// Metodo che dati i nomi di applicazione e modulo ritorna:
		/// - signature di applicazione e modulo
		///</summary>
		//---------------------------------------------------------------------------
		private void GetSignatureForDependencyEntry(string application, string module, out string applSign, out string modSign)
		{
			applSign = string.Empty;
			modSign	= string.Empty;

			foreach (AddOnApplicationDBInfo addOnAppDBInfo in addOnAppList)
			{
				if (string.Compare(addOnAppDBInfo.ApplicationName, application, StringComparison.OrdinalIgnoreCase) == 0)
				{
					foreach (ModuleDBInfo moduleDBInfo in addOnAppDBInfo.ModuleList)
					{
						if (string.Compare(moduleDBInfo.ModuleName, module, StringComparison.OrdinalIgnoreCase) == 0)
						{
							applSign = moduleDBInfo.ApplicationSign;
							modSign = moduleDBInfo.ModuleSign;
							break;
						}
					}
				}
			}
		}

		///<summary>
		/// Metodo che date le signature di applicazione/modulo ritorna il numero di release di arrivo previsto per il modulo
		///</summary>
		//--------------------------------------------------------------------------
		private void GetReleaseForModuleSignature(string applicationSign, string moduleSign, out int release)
		{
			release = -1;

			foreach (AddOnApplicationDBInfo addOnAppDBInfo in addOnAppList)
			{
				foreach (ModuleDBInfo moduleDBInfo in addOnAppDBInfo.ModuleList)
				{
					if (string.Compare(moduleDBInfo.ApplicationSign, applicationSign, StringComparison.OrdinalIgnoreCase) == 0 &&
						string.Compare(moduleDBInfo.ModuleSign, moduleSign, StringComparison.OrdinalIgnoreCase) == 0)
					{
						release = moduleDBInfo.DBRelease;
						break;
					}
				}
			}
		}	

		//---------------------------------------------------------------------------
		private void AppendTextToOutput(string text)
		{
			if (WriteLogInfo)
				Debug.WriteLine(text);
		}
	}
	#endregion

	#region Classe ModuleDBInfoList
	//============================================================================
	public class ModuleDBInfoList : List<ModuleDBInfo>
	{
		//---------------------------------------------------------------------------
		public ModuleDBInfoList() {	}

		/// <summary>
		/// funzione che ritorna un oggetto di tipo ModuleDBInfo, relativo al ModuleName 
		/// passato come parametro.
		/// </summary>
		//---------------------------------------------------------------------------
		public ModuleDBInfo GetItem(string nameSpace, bool isChild)
		{
			string [] str = nameSpace.Split(new Char[] {'.'});

			string app, module = string.Empty;
			int rel = 0;

			// prima estrapolo il nome dell'application.
			app	= str[0].ToString();

			// se la Length è > 1 vuol dire che mi hanno passato il namespace
			// in questo caso estrapolo anche il module name
			if (str.Length > 1)
				module = str[1].ToString();

			if (str.Length > 2)
				rel = Convert.ToInt32(str[2]);

			ModuleDBInfo dbInfo = Find(m => (string.Compare(m.ApplicationSign, app, StringComparison.OrdinalIgnoreCase) == 0) &&
											(string.Compare(m.ModuleSign, module, StringComparison.OrdinalIgnoreCase) == 0));

			if (dbInfo != null)
			{
				if (!isChild)
					dbInfo.UpdateInfo.CurrRel = rel;
				return dbInfo;
			}

			/*foreach (ModuleDBInfo info in this)
			{
				if ((string.Compare(info.ApplicationSign, app, StringComparison.OrdinalIgnoreCase) == 0) &&
					(string.Compare(info.ModuleSign, module, StringComparison.OrdinalIgnoreCase) == 0))
				{
					if (!isChild)
						info.UpdateInfo.CurrRel = rel;
					return info;
				}
			}*/

			return null;
		}
	}
	#endregion

	///<summary>
	/// Classe di appoggio per gestire l'aggiornamento preventivo nella TB_DBMark delle righe
	/// delle AddOnApp che dichiarano un nodo <PreviousSignature> nel DatabaseObjects.xml.
	/// Serve per gestire il cambio di signature dei moduli, quando vengono spostati in un'altra addonapp.
	/// Prima di eseguire il vero e proprio aggiornamento del database, viene effettuato
	/// un update nella TB_DBMark per aggiornare con i nuovi valori e poi costruire il grafo corretto
	/// per l'esecuzione degli script
	///</summary>
	//============================================================================
	public class PreviousSignatureInfo
	{
		public string	NewApplication				= string.Empty;
		public string	NewModule					= string.Empty;
		public string	PreviousApplication			= string.Empty;
		public string	PreviousModule				= string.Empty;
		public int		PreviousDBReleaseInDBMark	= 0;

		//---------------------------------------------------------------------------
		public PreviousSignatureInfo(string newApp, string newMod, string previousApp, string previousMod)
		{
			NewApplication		= newApp;
			NewModule			= newMod;
			PreviousApplication = previousApp;
			PreviousModule		= previousMod;
		}
	}
}