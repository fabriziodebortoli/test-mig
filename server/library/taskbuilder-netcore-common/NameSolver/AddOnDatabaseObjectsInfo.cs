using System;
using System.Collections;
using System.Diagnostics;

using System.Xml;

using Microarea.Common.StringLoader;
using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces;
using System.Collections.Generic;

namespace Microarea.Common.NameSolver
{
    /// <summary>
    /// Per ogni nodo di tipo <Column> parsato nei file di struttura database versione 3.0 (.dbxml)
    /// costruisco la struttura seguente
    /// </summary>
    //=========================================================================
    public class ExtraAddedColumnInfo : IAddOnDbObjectInfo
	{
		protected string columnName;	// Nome colonna
		protected string nameSpace;		// Namespace library che apporta l'AddOnColumn
		protected int release;			// release di creazione della colonna
		protected int createstep;		// createstep della colonna
		protected string tableName;		// Nome tabella modificata
		protected string tableNamespace;// Namespace completo tabella modificata

		// Properties
		//---------------------------------------------------------------------
		public string Name				{ get { return columnName; } }
		public string Namespace			{ get { return nameSpace; } }
		public int Release				{ get { return release; } }
		public int Createstep			{ get { return createstep; } }
		public string TableName			{ get { return tableName; } }
		public string TableNamespace	{ get { return tableNamespace; } }

		/// <summary>
		/// costruttore ExtraAddedColumnInfo
		/// </summary>
		//---------------------------------------------------------------------
		public ExtraAddedColumnInfo
			(
			string columnName, 
			string libNameSpace,
			int release, 
			int createstep, 
			string tableName, 
			string tableNamespace
			)
		{
			this.columnName = columnName;
			this.nameSpace = libNameSpace;
 			this.release = release;
			this.createstep = createstep;
			this.tableName = tableName;
			this.tableNamespace = tableNamespace;
		}
	}

	/// <summary>
	/// struttura del nodo AlterTable delle AdditionalColumns
	/// </summary>
	//=========================================================================
	public class AlterTableInfo
	{
		protected int		release;
		protected int		createstep;
		protected NameSpace nameSpace = null;
		
		/// <summary>
		/// release di creazione della tabella
		/// </summary>
		public int Release { get { return release; } }

		/// <summary>
		/// createstep della tabella
		/// </summary>
		public int	Createstep { get { return createstep; } }

		/// <summary>
		/// AlterTable namespace
		/// </summary>
		public NameSpace NameSpace { get { return nameSpace; } }

		/// <summary>
		/// costruttore AlterTableInfo
		/// </summary>
		/// <param name="aRelease">release di alter table</param>
		/// <param name="aCreatestep">numero di step dell'alter table</param>
		//---------------------------------------------------------------------
		public AlterTableInfo(int aRelease, int aCreatestep, NameSpace nameSpace)
		{
			release			= aRelease;
			createstep		= aCreatestep;
			this.nameSpace	= nameSpace;
		}
	}
	
	/// <summary>
	/// Struttura del nodo AdditionalColumns
	/// </summary>
	//=========================================================================
	public class AdditionalColumnTblInfo
	{
		protected string	name;
		protected NameSpace nameSpace = null;
		protected List<AlterTableInfo> addColTableInfoArray;
		
		/// <summary>
		/// namespace dell'AddColumn
		/// </summary>
		public string Name { get { return name; } }

		/// <summary>
		/// Array delle tabelle contenute nei gruppi (di tipo AlterTableInfo)
		/// </summary>
		public List<AlterTableInfo> AddColTableInfoArray { get { return addColTableInfoArray; } }

		/// <summary>
		/// Table namespace
		/// </summary>
		public NameSpace NameSpace { get { return nameSpace; } }

		/// <summary>
		/// costruttore AdditionalColumnTblInfo
		/// </summary>
		/// <param name="aName">Nome della tabella di AddColumn</param>
		//---------------------------------------------------------------------
		public AdditionalColumnTblInfo(string aName, NameSpace tableNameSpace)
		{
			nameSpace = tableNameSpace;
			name = aName;
		}

		/// <summary>
		/// parsa il singolo nodo di tipo Table
		/// </summary>
		/// <param name="addColTblNodes">array di nodi</param>
		//---------------------------------------------------------------------
		public bool ParseSingleAddColTables(XmlNodeList addColTblNodes)
		{
			if (addColTblNodes == null)
				return false;

			if (addColTableInfoArray == null)
				addColTableInfoArray = new List<AlterTableInfo>();
	
			foreach (XmlElement xCol in addColTblNodes)
			{
				string relAttribute = xCol.GetAttribute(AddOnDatabaseObjectsXML.Attribute.Release);
				string stepAttribute = xCol.GetAttribute(AddOnDatabaseObjectsXML.Attribute.Createstep);
				string nsAttribute = xCol.GetAttribute(AddOnDatabaseObjectsXML.Attribute.NameSpace);
				string virtualAttribute = xCol.GetAttribute(AddOnDatabaseObjectsXML.Attribute.Virtual);

				try
				{
					// se e' presente l'attributo virtual = true skippo la riga
					if (!string.IsNullOrWhiteSpace(virtualAttribute) && string.Compare(virtualAttribute, bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0)
						continue;

					// se uno degli attributi e' vuoto skippo
					if (string.IsNullOrWhiteSpace(relAttribute) || string.IsNullOrWhiteSpace(stepAttribute) || string.IsNullOrWhiteSpace(nsAttribute))
						continue;

					int rel = Convert.ToInt32(relAttribute);
					int step = Convert.ToInt32(stepAttribute);
					NameSpace ns = new NameSpace(nsAttribute, NameSpaceObjectType.Library);

					AlterTableInfo aAlterTableInfo = new AlterTableInfo(rel, step, ns);
					addColTableInfoArray.Add(aAlterTableInfo);
				}
				catch
				{
					continue;
				}
			}

			return true;
		}
	}

	/// <summary>
	/// classa che wrappa in memoria il file AddOnDatabaseObjects.xml
	/// contiene l'elenco delle additional columns
	/// </summary>
	//=========================================================================
	public class AddOnDatabaseObjectsInfo //: AddOnDatabaseObjectsInfo
	{
		private string		appName;
		private string		modName;
		
		private string		filePath;
		private	bool		valid;
		private	string		parsingError;

		private ModuleInfo	parentModuleInfo;
		private IList	addColumnsInfoArray;

		public  string		AppName 		{ get { return appName; }	set { appName = value; } }
		public  string		ModName			{ get { return modName; }	set { modName = value; } }

		public	string		FilePath		{ get { return filePath; } }
		public	bool		Valid			{ get { return valid; }	set { valid = value; } }
		public	string		ParsingError	{ get { return parsingError; } set { parsingError = value; } }

		public  ModuleInfo	ParentModuleInfo{  get { return parentModuleInfo; } }

		// Array delle AdditionalColumns
		public IList	AdditionalColumns { get { return addColumnsInfoArray; } }
	
		//---------------------------------------------------------------------
		public AddOnDatabaseObjectsInfo(string aFilePath, ModuleInfo aParentModuleInfo)
		{
			if (aFilePath == null || aFilePath.Length == 0)
			{
				Debug.WriteLine("Error in AddOnDatabaseObjectsInfo");
			}

			filePath		= aFilePath;
			valid			= true;
			parsingError	= string.Empty;
			parentModuleInfo= aParentModuleInfo;
		}

		/// <summary>
		/// Legge il file e crea l'array AdditionalColumns in memoria
		/// </summary>
		/// <returns>true se la lettura ha avuto successo</returns>
		//---------------------------------------------------------------------
		public bool Parse()
		{
			if	(
				!PathFinder.PathFinderInstance.ExistFile(filePath)||
				parentModuleInfo == null	|| 
				parentModuleInfo.ParentApplicationInfo == null
				)
				return false;

			LocalizableXmlDocument addOnDatabaseObjectsDocument = 
				new LocalizableXmlDocument
				(
				parentModuleInfo.ParentApplicationInfo.Name,
				parentModuleInfo.Name,
				parentModuleInfo.CurrentPathFinder
				);
			
			try
			{
				//leggo il file
				addOnDatabaseObjectsDocument.Load(filePath);
				
				//root di AddOnDatabaseObjects
				XmlElement root = addOnDatabaseObjectsDocument.DocumentElement;

				//nodo contenitore delle AdditionalColumns
				XmlNodeList additionalColElements = root.GetElementsByTagName(AddOnDatabaseObjectsXML.Element.AdditionalColumns);
				if (additionalColElements != null && additionalColElements.Count == 1)
				{
					//array di Table
					XmlNodeList tableNodeElements = ((XmlElement) additionalColElements[0]).GetElementsByTagName(AddOnDatabaseObjectsXML.Element.Table);
					if (tableNodeElements == null)
						return true;

					ParseAdditionalColumnsTbl(tableNodeElements);
				}
			}
			catch(XmlException err)
			{
				Debug.WriteLine(string.Format("Error parsing file {0} ({1})", filePath, err.Message));
				valid = false;
				parsingError = err.Message;
				return false;
			}
			catch(Exception e)
			{
				Debug.WriteLine(string.Format("Error parsing file {0} ({1})", filePath, e.Message));
				valid = false;
				parsingError = e.Message;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Parsa tutte le Tables contenute nei nodi AdditionalColumns
		/// </summary>
		/// <param name="tablesNodeElements">nodo Tables</param>
		//---------------------------------------------------------------------
		private bool ParseAdditionalColumnsTbl(XmlNodeList tablesNodeElements)
		{
			if (tablesNodeElements == null)
				return false;

			//inizializzo l'array delle AddColumns
			if (addColumnsInfoArray == null)
				addColumnsInfoArray = new List<AdditionalColumnTblInfo>();
			else
				addColumnsInfoArray.Clear();

			//scorro i nodi di tipo Tables
			foreach (XmlElement tElem in tablesNodeElements)
			{
				//namespace della Table (si compone sempre di 4 token)
				string nameSpace = tElem.GetAttribute(AddOnDatabaseObjectsXML.Attribute.NameSpace);
				if (string.IsNullOrWhiteSpace(nameSpace))
					continue;

				NameSpace tableNameSpace = new NameSpace(nameSpace, NameSpaceObjectType.Table);
				if (tableNameSpace == null || string.IsNullOrWhiteSpace(tableNameSpace.FullNameSpace))
					continue;

				// creo l'oggetto che tiene le info raccolte
				AdditionalColumnTblInfo addColTblInfo = new AdditionalColumnTblInfo(tableNameSpace.Table, tableNameSpace);

				//cerco il tag AlterTable
				XmlNodeList alterTblElem = tElem.GetElementsByTagName(AddOnDatabaseObjectsXML.Element.AlterTable);
				if (alterTblElem == null || alterTblElem.Count == 0)
					continue;

				addColTblInfo.ParseSingleAddColTables(alterTblElem);

				// aggiunto la table all'array solo se ho almeno una riga di altertable valida
				if (addColTblInfo.AddColTableInfoArray.Count > 0)
					addColumnsInfoArray.Add(addColTblInfo);
			}

			return true;
		}
	}
}
