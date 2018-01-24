using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

using Microarea.Common.StringLoader;
using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.NameSolver
{
    ///<summary>
    /// Classe che mappa in memoria un nodo di tipo Entity "base", ovvero quello utilizzato 
    /// come figlio dei nodi Table
    ///</summary>
    //================================================================================
    public class RSEntityBase
	{
		private string name;
		private List<RSColumn> rsColumns = new List<RSColumn>();

		//--------------------------------------------------------------------------------
		public string Name { get { return name; } }
		public List<RSColumn> RsColumns { get { return rsColumns; } set { rsColumns = value; } }

		//--------------------------------------------------------------------------------
		public RSEntityBase (string name)
		{
			this.name = name;
		}
	}

	///<summary>
	/// Classe che mappa in memoria un nodo di tipo Entity "full", ovvero quello che identifica
	/// la struttura in memoria del nodo di definizione dell'entita'
	///</summary>
	//================================================================================
	public class RSEntity
	{
		private ModuleInfo parentModuleInfo;
		
		private string name;
		private string masterTableNamespace;
		private string documentNamespace;
		private string hklNamespace;
		private string numbererNamespace;
		private string description;
		private int priority;

		private List<RSColumn> rsColumns = new List<RSColumn>();

		// Properties
		//--------------------------------------------------------------------------------
		public ModuleInfo ParentModuleInfo { get { return parentModuleInfo; } }
		
		public string Name { get { return name; } }
		public string MasterTableNamespace { get { return masterTableNamespace; } }
		public string DocumentNamespace { get { return documentNamespace; }	}
		public string HKLNamespace { get { return hklNamespace; } }
		public string NumbererNamespace { get { return numbererNamespace; } }
		public string Description { get { return description; } }
		public int Priority { get { return priority; } set { priority = value; } }

		public List<RSColumn> RsColumns { get { return rsColumns; } set { rsColumns = value; } }

		public string MasterTableName
		{
			get
			{
				NameSpace namespaceTable = new NameSpace(masterTableNamespace, NameSpaceObjectType.Table);
				return namespaceTable.GetTokenValue(NameSpaceObjectType.Table); // ritorna il nome della tabella
			}
		}

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public RSEntity(string name, string masterTableNs, string documentNs, string hklNs, string numbererNs, string description, int priority, ModuleInfo parentModuleInfo)
		{
			this.name = name;
			this.masterTableNamespace = masterTableNs;
			this.documentNamespace = documentNs;
			this.hklNamespace = hklNs;
			this.numbererNamespace = numbererNs;
			this.description = description;
			this.priority = priority;
			this.parentModuleInfo = parentModuleInfo;
		}
	}

	///<summary>
	/// Classe che mappa in memoria un nodo di tipo Table
	///</summary>
	//================================================================================
	public class RSTable
	{
		private string nameSpace;
		private List<RSEntityBase> rsEntityBaseList = new List<RSEntityBase>();

		// Properties
		//--------------------------------------------------------------------------------
		public string NameSpace { get { return nameSpace; } }
		public List<RSEntityBase> RsEntityBaseList { get { return rsEntityBaseList; } set { rsEntityBaseList = value; } }

		public string Name
		{
			get
			{
				NameSpace namespaceTable = new NameSpace(nameSpace, NameSpaceObjectType.Table);
				return namespaceTable.GetTokenValue(NameSpaceObjectType.Table);
			}
		}

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public RSTable(string nameSpace)
		{
			this.nameSpace = nameSpace;
		}
	}

	///<summary>
	/// Classe che mappa in memoria un nodo di tipo RSColumn
	///</summary>
	//================================================================================
	public class RSColumn
	{
		private string name;
		private string entityColumn;

		// Properties
		//--------------------------------------------------------------------------------
		public string Name { get { return name; } }
		public string EntityColumn { get { return entityColumn; } }

		///<summary>
		/// Constructors
		///</summary>
		//--------------------------------------------------------------------------------
		public RSColumn(string name)
			: this(name, string.Empty)
		{ }
		
		//--------------------------------------------------------------------------------
		public RSColumn(string name, string entity)
		{
			this.name = name;
			this.entityColumn = entity;
		}
	}

	/// <summary>
	/// Parse / unparse file RowSecurityObjectsInfo
	/// </summary>
	//================================================================================
	public class RowSecurityObjectsInfo
	{
		private string filePath;
		private ModuleInfo parentModuleInfo;

		private List<RSEntity> rsEntitiesList = new List<RSEntity>();
		private List<RSTable> rsTablesList = new List<RSTable>();

		//--------------------------------------------------------------------------------
		public string FilePath { get { return filePath; } }
		public ModuleInfo ParentModuleInfo { get { return parentModuleInfo; } }

		public List<RSEntity> RSEntities { get { return rsEntitiesList; } }
		public List<RSTable> RSTables { get { return rsTablesList; } }

		/// <summary>
		/// constructor
		/// </summary>
		//---------------------------------------------------------------------
		public RowSecurityObjectsInfo(string aFilePath, ModuleInfo aParentModuleInfo)
		{
			if (string.IsNullOrWhiteSpace(aFilePath))
				Debug.Fail("Error in RowSecurityObjectsInfo file");

			filePath = aFilePath;
			parentModuleInfo = aParentModuleInfo;
		}

		# region Metodi di Parse
		/// <summary>
		/// Legge il file e crea la struttura in memoria con le informazioni in esso contenute
		/// </summary>
		/// <returns>true se la lettura ha avuto successo</returns>
		//---------------------------------------------------------------------
		public bool Parse()
		{
			if (
				!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(filePath) ||
				parentModuleInfo == null ||
				parentModuleInfo.ParentApplicationInfo == null
				)
				return false;

			LocalizableXmlDocument xmlDoc = new LocalizableXmlDocument
				(
				parentModuleInfo.ParentApplicationInfo.Name,
				parentModuleInfo.Name,
				parentModuleInfo.CurrentPathFinder
                );

			try
			{
				//leggo il file
				xmlDoc.Load(filePath);

				//root di RowSecurityObjectsInfo.xml
				XmlElement root = xmlDoc.DocumentElement;

				// cerco il nodo Entities
				XmlNodeList entitiesElem = root.GetElementsByTagName(RowSecurityObjectsXML.Element.Entities);
				if (entitiesElem != null && entitiesElem.Count > 0)
				{
					//cerco il tag Entity
					XmlNodeList entityNodes = ((XmlElement)entitiesElem[0]).GetElementsByTagName(RowSecurityObjectsXML.Element.Entity);
					// richiamo il parse dei nodi Entity
					ParseEntityNodes(entityNodes);
				}

				// cerco il nodo Tables
				XmlNodeList tablesElem = root.GetElementsByTagName(RowSecurityObjectsXML.Element.Tables);
				if (tablesElem != null && tablesElem.Count > 0)
				{
					//cerco il tag Table
					XmlNodeList tableNodes = ((XmlElement)tablesElem[0]).GetElementsByTagName(RowSecurityObjectsXML.Element.Table);
					// richiamo il parse dei nodi Table
					ParseTableNodes(tableNodes);
				}
			}
			catch (XmlException err)
			{
				Debug.Fail(err.Message);
				return false;
			}
			catch (Exception e)
			{
				Debug.Fail(e.Message);
				return false;
			}

			return true;
		}

		///<summary>
		/// Parse dei nodi di tipo Entity
		///</summary>
		//---------------------------------------------------------------------
		public bool ParseEntityNodes(XmlNodeList entityNodes)
		{
			if (entityNodes == null)
				return false;

			string name, masterTableNs = string.Empty, documentNs = string.Empty, hklNs = string.Empty, numbererNs = string.Empty, description = string.Empty;
			int priority = 0;

			try
			{
				foreach (XmlElement xEntity in entityNodes)
				{
					name = xEntity.GetAttribute(RowSecurityObjectsXML.Attribute.Name);

					// tag MasterTableNamespace
					XmlNodeList masterTableNsNode = xEntity.GetElementsByTagName(RowSecurityObjectsXML.Element.MasterTableNamespace);
					if (masterTableNsNode != null && masterTableNsNode.Count > 0)
						masterTableNs = ((XmlElement)masterTableNsNode[0]).InnerText;

					// tag DocumentNamespace
					XmlNodeList documentNsNode = xEntity.GetElementsByTagName(RowSecurityObjectsXML.Element.DocumentNamespace);
					if (documentNsNode != null && documentNsNode.Count > 0)
						documentNs = ((XmlElement)documentNsNode[0]).InnerText;

					// tag HKLNamespace
					XmlNodeList hklNsNode = xEntity.GetElementsByTagName(RowSecurityObjectsXML.Element.HKLNamespace);
					if (hklNsNode != null && hklNsNode.Count > 0)
						hklNs = ((XmlElement)hklNsNode[0]).InnerText;

					// tag NumbererNamespace
					XmlNodeList numbererNsNode = xEntity.GetElementsByTagName(RowSecurityObjectsXML.Element.NumbererNamespace);
					if (numbererNsNode != null && numbererNsNode.Count > 0)
						numbererNs = ((XmlElement)numbererNsNode[0]).InnerText;

					// tag Description
					XmlNodeList descriptionNode = xEntity.GetElementsByTagName(RowSecurityObjectsXML.Element.Description);
					if (descriptionNode != null && descriptionNode.Count > 0)
						description = ((XmlElement)descriptionNode[0]).InnerText;

					// tag priority
					XmlNodeList priorityNode = xEntity.GetElementsByTagName(RowSecurityObjectsXML.Element.Priority);
					if (priorityNode != null && priorityNode.Count > 0)
					{
						try
						{
							priority = Int32.Parse(((XmlElement)priorityNode[0]).InnerText);
						}
						catch
						{ }
					}


					RSEntity rsEntity = new RSEntity(name, masterTableNs, documentNs, hklNs, numbererNs, description, priority, this.parentModuleInfo);
					rsEntitiesList.Add(rsEntity);

					rsEntity.RsColumns = ParseColumns(xEntity);
				}
			}
			catch (XmlException xmlEx)
			{
				throw(xmlEx);
			}
			catch (Exception ex)
			{
				throw (ex);
			}

			return true;
		}

		///<summary>
		/// Parse dei nodi di tipo Table
		///</summary>
		//---------------------------------------------------------------------
		public bool ParseTableNodes(XmlNodeList tableNodes)
		{
			if (tableNodes == null)
				return false;

			try
			{
				foreach (XmlElement xTable in tableNodes)
				{
                    string nameSpace = xTable.GetAttribute(RowSecurityObjectsXML.Attribute.Namespace);
		
					RSTable rsTable = new RSTable(nameSpace);
					
					// cerco i nodi Entity
					XmlNodeList rsEntityNodes = xTable.GetElementsByTagName(RowSecurityObjectsXML.Element.Entity);
					if (rsEntityNodes != null && rsEntityNodes.Count > 0)
					{
						foreach (XmlElement xEntityBase in rsEntityNodes)
							rsTable.RsEntityBaseList.Add(ParseEntityBaseNodes(xEntityBase));
					}

					rsTablesList.Add(rsTable);
				}
			}
			catch (XmlException xmlEx)
			{
				throw (xmlEx);
			}
			catch (Exception ex)
			{
				throw (ex);
			}

			return true;
		}

		///<summary>
		/// Parse dei nodi di tipo Entity "base", ovvero figli dei nodi Table
		///</summary>
		//---------------------------------------------------------------------
		private RSEntityBase ParseEntityBaseNodes(XmlElement xEntityBase)
		{
			RSEntityBase rseb = new RSEntityBase(xEntityBase.GetAttribute(RowSecurityObjectsXML.Attribute.Name));

			XmlNodeList rsColumnList = xEntityBase.GetElementsByTagName(RowSecurityObjectsXML.Element.RSColumn);

			if (rsColumnList != null && rsColumnList.Count > 0)
			{
				foreach (XmlElement xCol in rsColumnList)
				{
					string colName = xCol.InnerText;
					string colEntity = xCol.GetAttribute(RowSecurityObjectsXML.Attribute.EntityColumn);

					RSColumn rsCol = new RSColumn(colName, colEntity);
					rseb.RsColumns.Add(rsCol);
				}
			}

			return rseb;
		}

		///<summary>
		/// Parse dei nodi di tipo RSColumn, figli dei nodi Entity
		///</summary>
		//---------------------------------------------------------------------
		private List<RSColumn> ParseColumns(XmlElement xElement)
		{
			List<RSColumn> columns = new List<RSColumn>();

			//cerco il tag RSColumns (attenzione che e' presente solo per i nodi Entity)
			XmlNodeList rsColumnsNode = xElement.GetElementsByTagName(RowSecurityObjectsXML.Element.RSColumns);
			if (rsColumnsNode != null && rsColumnsNode.Count > 0)
			{
				// cerco i nodi Column
				foreach (XmlElement xColumn in rsColumnsNode)
				{
					XmlNodeList rsColumnList = xColumn.GetElementsByTagName(RowSecurityObjectsXML.Element.RSColumn);

					if (rsColumnList != null && rsColumnList.Count > 0)
					{
						foreach (XmlElement xCol in rsColumnList)
						{
							string colName = xCol.InnerText;
							string colEntity = xCol.GetAttribute(RowSecurityObjectsXML.Attribute.EntityColumn);

							RSColumn rsCol = new RSColumn(colName, colEntity);
							columns.Add(rsCol);
						}
					}
				}
			}

			return columns;
		}
		# endregion

		# region Metodi di Unparse
		///<summary>
		/// Metodo per scrivere nodi nel file
		/// <Entities>
		///		<Entity name="Client">
		///			<MasterTableNamespace>OFM.Masters.Dbl.OM_Masters</MasterTableNamespace>
		///			<DocumentNamespace>OFM.Masters.Documents.OfficeClients</DocumentNamespace>
		///			<HKLNamespace>HotKeyLink.OFM.Masters.Dbl.Masters</HKLNamespace>
		///			<NumbererNamespace>OFM.Masters.Documents.Masters</NumbererNamespace>
		///			<Description>Entity row security level for OfficeClient</Description>
		///			<Priority>1</Priority>
		///			<RSColumns>
		///				<RSColumn>ClientCode</RSColumn>
		///				<RSColumn>Type</RSColumn>
		///			</RSColumns>
		///		</Entity>
		/// </Entities>
		/// <Tables>	
		///    <Table name="OM_Commitments">
		///      <Entity name="Client">
		///        <RSColumn entitycolumn="ClientCode">ClientCode</RSColumn>
		///        <RSColumn entitycolumn="Type">CommitType</RSColumn>
		///      </Entity>
		///    </Table>
		///  </Tables>
		///</summary>
		//---------------------------------------------------------------------
		public bool Unparse()
		{
			try
			{
				// XMLDocument + XmlDeclaration
				XmlDocument rowSecurityObjectsXmlDoc = new XmlDocument();
				XmlDeclaration xmlDeclaration = rowSecurityObjectsXmlDoc.CreateXmlDeclaration(NameSolverStrings.XmlDeclarationVersion, NameSolverStrings.XmlDeclarationEncoding, "yes");
				rowSecurityObjectsXmlDoc.AppendChild(xmlDeclaration);
				// root dell'xmldoc
				XmlElement rootElement = rowSecurityObjectsXmlDoc.CreateElement(RowSecurityObjectsXML.Element.RowSecurityObjects);
				rowSecurityObjectsXmlDoc.AppendChild(rootElement);
				// nodo <Entities>
				XmlElement entitiesElement = rowSecurityObjectsXmlDoc.CreateElement(RowSecurityObjectsXML.Element.Entities);
				rootElement.AppendChild(entitiesElement);

				// ri-scrivo i nodi <Entity> 
				foreach (RSEntity e in this.rsEntitiesList)
					UnparseEntityNode(e, entitiesElement, rowSecurityObjectsXmlDoc);

				// nodo <Tables>
				XmlElement tablesElement = rowSecurityObjectsXmlDoc.CreateElement(RowSecurityObjectsXML.Element.Tables);
				rootElement.AppendChild(tablesElement);
				// vado a ri-scrivere i nodi <Table> 
				foreach (RSTable t in this.rsTablesList)
					UnparseTableNode(t, tablesElement, rowSecurityObjectsXmlDoc);

				// salvataggio del file (sovrascrivendo il contenuto del file se gia' esistente)
				if (PathFinder.PathFinderInstance.FileSystemManager.ExistFile(this.filePath) && (File.GetAttributes(this.filePath) & FileAttributes.ReadOnly)== FileAttributes.ReadOnly)
					File.SetAttributes(this.filePath, FileAttributes.Normal);

				rowSecurityObjectsXmlDoc.Save(File.Open(filePath, FileMode.Open));

				File.SetAttributes(this.filePath, FileAttributes.ReadOnly);
				//rowSecurityObjectsFileInfo.Attributes = rowSecurityObjectsFileInfo.Attributes | FileAttributes.ReadOnly; // ri-assegno il readonly

				// avendo effettuato l'Unparse devo forzare il reload del file, quindi metto il RowSecurityObjectsInfo a null
				ModuleInfo modInfo = parentModuleInfo as ModuleInfo;
				modInfo.RowSecurityObjectsInfo = null;
			}
			catch (XmlException xe)
			{
				Debug.Fail(xe.ToString());
				return false;
			}
			catch (Exception err)
			{
				Debug.Fail(err.ToString());
				return false;
			}

			return true;
		}

		///<summary>
		/// Scrive un nodo di tipo Entity e le sue RSColumns
		///</summary>
		//---------------------------------------------------------------------
		private void UnparseEntityNode(RSEntity e, XmlElement parentElement, XmlDocument xmlDoc)
		{
			try
			{
				// nodo <Entity>
				XmlElement entity = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.Entity);
				entity.SetAttribute(RowSecurityObjectsXML.Attribute.Name, e.Name);

				// tag <MasterTableNamespace>
				XmlElement masterTableNs = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.MasterTableNamespace);
				masterTableNs.InnerText = e.MasterTableNamespace;
				entity.AppendChild(masterTableNs);

				// tag <DocumentNamespace>
				XmlElement documentNs = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.DocumentNamespace);
				documentNs.InnerText = e.DocumentNamespace;
				entity.AppendChild(documentNs);

				// tag <HKLNamespace>
				XmlElement hklNs = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.HKLNamespace);
				hklNs.InnerText = e.HKLNamespace;
				entity.AppendChild(hklNs);

				// <tag NumbererNamespace>
				XmlElement numbererNs = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.NumbererNamespace);
				numbererNs.InnerText = e.NumbererNamespace;
				entity.AppendChild(numbererNs);

				// nodo <Description>
				XmlElement entityDescri = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.Description);
				entityDescri.InnerText = e.Description;
				entity.AppendChild(entityDescri);

				// nodo <Priority>
				XmlElement priority = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.Priority);
				priority.InnerText = e.Priority.ToString();
				entity.AppendChild(priority);

				// nodo <RSColumns>
				XmlElement rsCols = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.RSColumns);
				foreach (RSColumn col in e.RsColumns)
				{
					// nodo <RSColumn>
					XmlElement rsCol = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.RSColumn);
					rsCol.InnerText = col.Name;
					rsCols.AppendChild(rsCol);
				}
				entity.AppendChild(rsCols);
				parentElement.AppendChild(entity);
			}
			catch (XmlException xe)
			{
				throw(xe);
			}
		}

		///<summary>
		/// Scrive un nodo di tipo Table e le sue RSColumns
		///</summary>
		//---------------------------------------------------------------------
		private void UnparseTableNode(RSTable t, XmlElement parentElement, XmlDocument xmlDoc)
		{
			try
			{
				// nodo <Table>
				XmlElement table = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.Table);
				table.SetAttribute(RowSecurityObjectsXML.Attribute.Namespace, t.NameSpace);

				foreach (RSEntityBase rse in t.RsEntityBaseList)
				{
					// nodo <Entity>
					XmlElement entity = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.Entity);
					entity.SetAttribute(RowSecurityObjectsXML.Attribute.Name, rse.Name);

					foreach (RSColumn col in rse.RsColumns)
					{
						// nodo <RSColumn>
						XmlElement rsCol = xmlDoc.CreateElement(RowSecurityObjectsXML.Element.RSColumn);
						rsCol.SetAttribute(RowSecurityObjectsXML.Attribute.EntityColumn, col.EntityColumn);
						rsCol.InnerText = col.Name;
						entity.AppendChild(rsCol);
					}
					table.AppendChild(entity);
				}
				parentElement.AppendChild(table);
			}
			catch (XmlException xe)
			{
				throw(xe);
			}
		}
		# endregion
	}
}
