using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

using System.Xml;

using Microarea.Common.StringLoader;
using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces;
using System.Collections.Generic;

namespace Microarea.Common.NameSolver
{
    /// <summary>
    /// Struttura del nodo Table/View/Procedure dei nodi Group
    /// </summary>
    //=========================================================================
    public class TableInfo : ITableInfo
	{
		protected string			name;
		protected string			nameSpace;
		protected int				release;
		protected int				createstep;
		protected ModuleInfo	owner;
		protected ILocalizer		localizer;
        protected bool              masterTable = false;
		
		// namespace della tabella
		//--------------------------------------------------------------------------------
		public string Name		{ get { return name; } }

		// release della tabella
		//--------------------------------------------------------------------------------
		public int Release		{ get { return release; } }

		// createstep della tabella
		//--------------------------------------------------------------------------------
		public int Createstep	{ get { return createstep; } }

		// nome localizzato della tabella
		//--------------------------------------------------------------------------------
		public string Title		{ get { return localizer.Translate(name); } }

		// NameSpace della tabella
		//--------------------------------------------------------------------------------
		public string Namespace	{ get { return nameSpace; } }


        // se è una master table
		//--------------------------------------------------------------------------------
        public bool MasterTable { get { return masterTable; } }
		/// <summary>
		/// costruttore TableInfo
		/// </summary>
		/// <param name="aOwner">Modulo proprietario della tabella</param>
		/// <param name="aName">Nome della tabella</param>
		/// <param name="aRelease">Release di creazione della tabella</param>
		/// <param name="aCreatestep">Step di creazione della tabella</param>
		//---------------------------------------------------------------------
        public TableInfo(ModuleInfo aOwner, string aName, int aRelease, int aCreatestep, string aNameSpace, bool isMaster)
		{
			name			= aName;
			nameSpace		= aNameSpace;
			release			= aRelease;
			createstep		= aCreatestep;
			owner			= aOwner;	
			localizer		= new DatabaseLocalizer(aName, aOwner.DictionaryFilePath);
            masterTable   = isMaster;
		}
	}

	//=========================================================================
	public class ViewInfo : IDbObjectInfo
	{
		protected string	name;
		protected int		release;
		protected int		createstep;
		protected string	nameSpace;
		
		// namespace della View
		//--------------------------------------------------------------------------------
		public string Name		{ get { return name; } }

		// release della View
		//--------------------------------------------------------------------------------
		public int Release		{ get { return release; } }

		// createstep della View
		//--------------------------------------------------------------------------------
		public int Createstep	{ get { return createstep; } }

		// NameSpace della tabella
		//--------------------------------------------------------------------------------
        public string Namespace { get { return nameSpace; } }
		/// <summary>
		/// costruttore ViewInfo
		/// </summary>
		/// <param name="aName">Nome della view</param>
		/// <param name="aRelease">Release di creazione della view</param>
		/// <param name="aCreatestep">Step di creazione della view</param>
		//---------------------------------------------------------------------
		public ViewInfo(string aName, int aRelease, int aCreatestep, string aNameSpace)
		{
			name		= aName;
			release		= aRelease;
			createstep	= aCreatestep;
			nameSpace	= aNameSpace;
		}
	}

	//=========================================================================
	public class ProcedureInfo : IDbObjectInfo
	{
		protected string	name;
		protected int		release;
		protected int		createstep;
		protected string	nameSpace;
		
		// namespace della stored procedure
		//--------------------------------------------------------------------------------
		public string Name		{ get { return name; } }

		// release della stored procedure
		//--------------------------------------------------------------------------------
		public int Release		{ get { return release; } }

		// createstep della stored procedure
		//--------------------------------------------------------------------------------
		public int Createstep	{ get { return createstep; } }

		// NameSpace della tabella
		//--------------------------------------------------------------------------------
		public string Namespace { get { return nameSpace; } }
		
		/// <summary>
		/// costruttore ProcedureInfo
		/// </summary>
		/// <param name="aName">Nome della procedure</param>
		/// <param name="aRelease">Release di creazione della procedure</param>
		/// <param name="aCreatestep">Step di creazione della procedure</param>
		//---------------------------------------------------------------------
		public ProcedureInfo(string aName, int aRelease, int aCreatestep, string aNameSpace)
		{
			name		= aName;
			release		= aRelease;
			createstep	= aCreatestep;
			nameSpace	= aNameSpace;
		}
	}

	//=========================================================================
	/// <summary>
	/// Classe che wrappa in memoria il file DatabaseObjects.xml.
	/// Contiene l'elenco dei groups e delle additional columns
	/// </summary>
	public class DatabaseObjectsInfo //: IDatabaseObjectsInfo
	{
		private string	filePath;
		private	bool	valid;
		private	string	parsingError;
		private string	signature;
		private int		release;
		private string	previousApplication;
		private string	previousModule;

		private bool	dms = false;
		private bool	isDevelopmentVersion = false; // gestione rewind

		private ModuleInfo	parentModuleInfo;

		protected List<TableInfo> tableInfoArray;
		protected List<IDbObjectInfo> viewInfoArray;
		protected List<ProcedureInfo> procedureInfoArray;

		//--------------------------------------------------------------------------------
		public	string		FilePath		{ get { return filePath; } }
		//--------------------------------------------------------------------------------
		public	bool		Valid			{ get { return valid; }  set { valid = value; } }
		//--------------------------------------------------------------------------------
		public	string		ParsingError	{ get { return parsingError; } set { parsingError = value; } }

		// Signature del modulo
		//--------------------------------------------------------------------------------
		public string		Signature		{ get {return signature;} }

		// Release del modulo
		//--------------------------------------------------------------------------------
		public int			Release			{ get {return release;} }

		// Attributo per la gestione del rewind di modulo
		//--------------------------------------------------------------------------------
		public bool IsDevelopmentVersion { get { return isDevelopmentVersion; } }
		
		// Oggetti per il database documentale
		//--------------------------------------------------------------------------------
		public bool			Dms				{ get { return dms; } }

		// PreviousSignature del modulo (per gestire cambi di modulo tra addon)
		//--------------------------------------------------------------------------------
		public string PreviousApplication	{ get { return previousApplication; } }
		public string PreviousModule		{ get { return previousModule; } }
		//--------------------------------------------------------------------------------
		public ModuleInfo	ParentModuleInfo {  get { return parentModuleInfo; } }

		// Array delle tabelle, view o procedure
		//--------------------------------------------------------------------------------
		public	IList TableInfoArray		{ get { return tableInfoArray; } }
		//--------------------------------------------------------------------------------
		public	IList ViewInfoArray			{ get { return viewInfoArray;  } }
		//--------------------------------------------------------------------------------
		public	IList ProcedureInfoArray	{ get { return procedureInfoArray;  } }

		/// <summary>
		/// Costruttore
		/// </summary>
		//---------------------------------------------------------------------
		public DatabaseObjectsInfo(string aFilePath, ModuleInfo aParentModuleInfo)
		{
			if (aFilePath == null || aFilePath.Length == 0)
				Debug.WriteLine("Error in DatabaseObjectsInfo");

			filePath		= aFilePath;
			valid			= true;
			parsingError	= string.Empty;
			parentModuleInfo= aParentModuleInfo;
		}

		//--------------------------------------------------------------------------------
		public ITableInfo GetTableInfoByName(string tableName)
		{
			if (TableInfoArray == null)
				return null;

			foreach (ITableInfo ti in TableInfoArray)
				if (string.Compare(ti.Name, tableName, StringComparison.OrdinalIgnoreCase) == 0)
					return ti;
			return null;
		}

		//--------------------------------------------------------------------------------
		public IDbObjectInfo GetViewInfoByName(string viewName)
		{
			if (ViewInfoArray == null)
				return null;

            foreach (IDbObjectInfo wi in ViewInfoArray)
				if (string.Compare(wi.Name, viewName, StringComparison.OrdinalIgnoreCase) == 0) 
					return wi;
			return null;
		}

		//--------------------------------------------------------------------------------
		public IDbObjectInfo GetProcedureInfoByName(string procedureName)
		{
			if (ProcedureInfoArray == null)
				return null;

            foreach (IDbObjectInfo pi in ProcedureInfoArray)
				if (string.Compare(pi.Name, procedureName, StringComparison.OrdinalIgnoreCase) == 0)
					return pi;
			return null;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Legge il file e crea gli array di Groups e AdditionalColumns in memoria
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

			LocalizableXmlDocument databaseObjectsDocument = 
				new LocalizableXmlDocument
				(
				parentModuleInfo.ParentApplicationInfo.Name,
				parentModuleInfo.Name,
				parentModuleInfo.CurrentPathFinder
                );
			
			try
			{
				//leggo il file
				databaseObjectsDocument.Load(filePath);
				
				//root di DatabaseObjects
				XmlElement root = databaseObjectsDocument.DocumentElement;

				// Signature
				signature = string.Empty;
				XmlNodeList signList = root.GetElementsByTagName(DataBaseObjectsXML.Element.Signature);
				if (signList != null && signList.Count > 0)
				{
					XmlNode node = signList[0];
					signature = node.InnerText;

					// leggo l'attributo dms dal tag <Signature>
					// se esiste ed e' impostato a true si tratta di un modulo per il documentale
					string dmsAttribute = ((XmlElement)node).GetAttribute(DataBaseObjectsXML.Attribute.Dms);
					if (!string.IsNullOrWhiteSpace(dmsAttribute) &&
						string.Compare(dmsAttribute, bool.TrueString, StringComparison.OrdinalIgnoreCase) == 0)
						dms = true;
				}

				// Release
				release = 0;
				XmlNodeList relList = root.GetElementsByTagName(DataBaseObjectsXML.Element.Release);
				if (relList != null && relList.Count > 0)
				{
					XmlNode node = relList[0];
					if (node.InnerText != null && node.InnerText.Length > 0)
						try { release = Int32.Parse(node.InnerText); } 
						catch (Exception) {}

					// leggo l'attributo development dal tag <Release> (facoltativo)
					// se esiste ed e' impostato a true si tratta di un modulo in sviluppo (per l'opzione Rewind)
					string developmentAttribute = ((XmlElement)node).GetAttribute(DataBaseObjectsXML.Attribute.Development);
					if (!string.IsNullOrWhiteSpace(developmentAttribute) &&
						string.Compare(developmentAttribute, bool.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0)
						isDevelopmentVersion = true;
				}

				// PreviousSignature
				previousApplication = string.Empty;
				previousModule = string.Empty;
				XmlNodeList previousSignatureList = root.GetElementsByTagName(DataBaseObjectsXML.Element.PreviousSignature);
				if (previousSignatureList != null && previousSignatureList.Count > 0)
				{
					XmlNode node = previousSignatureList[0];
					previousApplication = ((XmlElement)node).GetAttribute(DataBaseObjectsXML.Attribute.Application);
					previousModule = ((XmlElement)node).GetAttribute(DataBaseObjectsXML.Attribute.Module);
				}

				ParseDBObjects(root);
			}
			catch(XmlException err)
			{
                Debug.WriteLine(err.Message + '\n' + filePath);
				valid = false;
				parsingError = err.Message;
				return false;
			}
			catch(Exception e)
			{
				Debug.WriteLine(e.Message);
				valid = false;
				parsingError = e.Message;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Parsa tutti gli oggetti di database nell'xml
		/// </summary>
		/// <param name="root">root del file</param>
		/// <returns>successo della funzione</returns>
		//---------------------------------------------------------------------
		private bool ParseDBObjects(XmlElement root)
		{
			if (root == null)
				return false;
			
			//cerco il tag Tables
			XmlNodeList groupTablesElem = root.GetElementsByTagName(DataBaseObjectsXML.Element.Tables);
			if (groupTablesElem != null && groupTablesElem.Count > 0)
			{
				//cerco il tag Table
				XmlNodeList groupTableElements = ((XmlElement)groupTablesElem[0]).GetElementsByTagName(DataBaseObjectsXML.Element.Table);
				// richiamo il parse dei nodi Table
				ParseSingleTable(groupTableElements);
			}

			//cerco il tag Views
			XmlNodeList groupViewsElem = root.GetElementsByTagName(DataBaseObjectsXML.Element.Views);
			if (groupViewsElem != null && groupViewsElem.Count > 0)
			{
				//cerco il tag View
				XmlNodeList groupViewElements = ((XmlElement)groupViewsElem[0]).GetElementsByTagName(DataBaseObjectsXML.Element.View);
				// richiamo il parse dei nodi View
				ParseSingleView(groupViewElements);
			}

			//cerco il tag Procedures
			XmlNodeList groupProcsElem = root.GetElementsByTagName(DataBaseObjectsXML.Element.Procedures);
			if (groupProcsElem != null && groupProcsElem.Count > 0)
			{
				//cerco il tag Procedure
				XmlNodeList groupProcElements = ((XmlElement)groupProcsElem[0]).GetElementsByTagName(DataBaseObjectsXML.Element.Procedure);
				// richiamo il parse dei nodi Procedure
				ParseSingleProcedure(groupProcElements);
			}
			
			return true;
		}

		//---------------------------------------------------------------------
		public bool ParseSingleTable(XmlNodeList tableNodes)
		{
			if (tableNodes == null)
				return false;

			if (tableInfoArray == null)
				tableInfoArray = new List<TableInfo>();
	
			foreach (XmlElement xTable in tableNodes)
			{
				string attValue	= xTable.GetAttribute(DataBaseObjectsXML.Attribute.Namespace);

                NameSpace namespaceTable = new NameSpace(attValue, NameSpaceObjectType.Table);
				string name = namespaceTable.GetTokenValue(NameSpaceObjectType.Table);

                //verifico se la tabella è una masterTable
                attValue = xTable.GetAttribute(DataBaseObjectsXML.Attribute.Mastertable);
                bool isMasterTable = (string.IsNullOrEmpty(attValue)) ? false : Convert.ToBoolean(attValue);

				//cerco il tag Create
				XmlNodeList tableCreateElements = xTable.GetElementsByTagName(DataBaseObjectsXML.Element.Create);
				if (tableCreateElements == null || tableCreateElements.Count == 0)
					continue;

				int rel = Convert.ToInt32(((XmlElement) tableCreateElements[0]).GetAttribute(DataBaseObjectsXML.Attribute.Release));
				int step = Convert.ToInt32(((XmlElement) tableCreateElements[0]).GetAttribute(DataBaseObjectsXML.Attribute.Createstep));

                TableInfo atableInfo = new TableInfo(ParentModuleInfo, name, rel, step, namespaceTable.GetNameSpaceWithoutType(), isMasterTable);
				tableInfoArray.Add(atableInfo);
			}

			return true;
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// parsa ogni nodo di tipo View
		/// </summary>
		//---------------------------------------------------------------------
		public bool ParseSingleView(XmlNodeList viewNodes)
		{
			if (viewNodes == null)
				return false;

			if (viewInfoArray == null)
				viewInfoArray = new List<IDbObjectInfo>();
	
			foreach (XmlElement xView in viewNodes)
			{
				string name	= xView.GetAttribute(DataBaseObjectsXML.Attribute.Namespace);
				
				NameSpace namespaceView = new NameSpace(name, NameSpaceObjectType.View);
				name = namespaceView.GetTokenValue(NameSpaceObjectType.View);

				//cerco il tag Create
				XmlNodeList viewCreateElements = xView.GetElementsByTagName(DataBaseObjectsXML.Element.Create);
				if (viewCreateElements == null || viewCreateElements.Count == 0)
					continue;

				int rel = Convert.ToInt32(((XmlElement) viewCreateElements[0]).GetAttribute(DataBaseObjectsXML.Attribute.Release));
				int step = Convert.ToInt32(((XmlElement) viewCreateElements[0]).GetAttribute(DataBaseObjectsXML.Attribute.Createstep));

				ViewInfo aviewInfo = new ViewInfo(name, rel, step, namespaceView.GetNameSpaceWithoutType());
				viewInfoArray.Add(aviewInfo);
			}

			return true;
		}

		/// <summary>
		/// parsa ogni singolo nodo Procedure
		/// </summary>
		//---------------------------------------------------------------------
		public bool ParseSingleProcedure(XmlNodeList procedureNodes)
		{
			if (procedureNodes == null)
				return false;

			if (procedureInfoArray == null)
				procedureInfoArray = new List<ProcedureInfo>();
	
			foreach (XmlElement xProc in procedureNodes)
			{
				string name	= xProc.GetAttribute(DataBaseObjectsXML.Attribute.Namespace);
				
				NameSpace namespaceProcedure = new NameSpace(name, NameSpaceObjectType.Procedure);
				name = namespaceProcedure.GetTokenValue(NameSpaceObjectType.Procedure);

				//cerco il tag Create
				XmlNodeList procCreateElements = xProc.GetElementsByTagName(DataBaseObjectsXML.Element.Create);
				if (procCreateElements == null || procCreateElements.Count == 0)
					continue;

				int rel = Convert.ToInt32(((XmlElement) procCreateElements[0]).GetAttribute(DataBaseObjectsXML.Attribute.Release));
				int step = Convert.ToInt32(((XmlElement) procCreateElements[0]).GetAttribute(DataBaseObjectsXML.Attribute.Createstep));

				ProcedureInfo aprocInfo = new ProcedureInfo(name, rel, step, namespaceProcedure.GetNameSpaceWithoutType());
				procedureInfoArray.Add(aprocInfo);
			}

			return true;
		}
	}
}
